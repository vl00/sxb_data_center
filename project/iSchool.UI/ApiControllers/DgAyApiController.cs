using CSRedis;
using iSchool.Api.ModelBinders;
using iSchool.BgServices;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infras;
using iSchool.Infrastructure;
using iSchool.Application.Service.DgAy;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace iSchool.UI.ApiControllers
{
    [Route("api/dgay/[Action]")]
    [ApiController]
    public class DgAyApiController : ControllerBase
    {
        readonly IMediator _mediator;
        readonly CSRedisClient redis;

        public DgAyApiController(CSRedisClient redis,
            IMediator _mediator)
        {
            this._mediator = _mediator;
            this.redis = redis;
        }


        #region upload

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="fid">文件id.当`index=1时为null`,后续`值=之前的返回id`</param>
        /// <param name="p">服务文件夹</param>
        /// <param name="index">当前上传的块下标,不能小于1</param>
        /// <param name="total">总块数</param>
        /// <param name="blockSize">块大小</param>
        /// <param name="file">块数据,就是前端的file的分割</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext"></param>
        /// <param name="_email"></param>
        /// <param name="_id"></param>
        /// <param name="_ver"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadImgResult), 200)]
        [DisableRequestSizeLimit]
        public async Task<Res2Result> Upload([FromForm(Name = "id")] string fid, [FromForm] string p, [FromForm] long index, [FromForm] long total, [FromForm(Name = "size")] long blockSize,
            [BindFormFile(0)] IFormFile file, [FromForm] string fileName, [FromForm] string ext,
            [FromForm] string _email, [FromForm] string _id, [FromForm] string _ver,
            [FromServices] IConfiguration config)
        {
            Guid userid = default;
            try { userid = HttpContext.GetUserId(); }
            catch
            {
                return Res2Result.Fail("请先登录", 401);
            }

            if (file == null || file.Length == 0L)
            {
                return Res2Result.Fail("上传文件不能为空");
            }
            if (string.IsNullOrEmpty(fid))
            {
                if (index == 1) fid = $"{DateTime.Now:yyMMddHHmmssfff}{Guid.NewGuid().ToString().Replace("-", "")}";
                else return Res2Result.Fail("无效的上传", 500);
            }

            // 一开始
            if (index == 1 && await redis.ExistsAsync(BgCacheKeys.DgAy_import))
            {
                return Res2Result.Fail("已经有人在上传了.");
            }

            var x = await Save_bys_to_tmp(config, file, fid, p, index, blockSize, fileName, ext);
            fid = x.fid;

            if (index < total)
            {
                return Res2Result.Success(new UploadImgResult
                {
                    Id = fid,                    
                });
            }

            // 刚上传完
            if (!await redis.SetAsync(BgCacheKeys.DgAy_import, fid, 60 * 10, RedisExistence.Nx))
            {
                return Res2Result.Fail("已经有人在上传了.");
            }

            AsyncUtils.StartNew(new ImportDgAy2022ReqArgs
            {
                FilePath = x.Path,
                UserId = userid,
                Gid = fid,
            });

            var result = new UploadImgResult { Id = fid };
            return Res2Result.Success(result);
        }
                
        [NonAction]
        static async Task<(string fid, string Path, string UploadUrl)> Save_bys_to_tmp(IConfiguration config, IFormFile file,
            string fid, string p, long index, long blockSize, string fileName, string ext)
        {
            ext ??= "png";
            p ??= "eval";
#if DEBUG
            p = "test/" + p;
#endif
            index = index >= 1 ? index : throw new CustomResponseException("index不能小于1");
            ext = (ext[0] == '.' ? ext[1..] : ext).ToLower();

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), $"images/temp")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), $"images/temp"));

            var path = Path.Combine(Directory.GetCurrentDirectory(), $"images/temp/{fid}.{ext}");
            var steam = file.OpenReadStream();
            using (var fs = System.IO.File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Seek((index - 1) * blockSize, SeekOrigin.Begin);
                await steam.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            return (fid, path, null);
        }

        #endregion upload

        /// <summary>
        /// get import result
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(ImportDgAy2022ResResult), 200)]
        public async Task<Res2Result> GetImportResult(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Res2Result.Fail(404);
            }
            var cts = new CancellationTokenSource(1000 * 5);
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var r = await redis.GetAsync<ImportDgAy2022ResResult>(BgCacheKeys.DgAy_import_result.FormatWith(id));
                    if (r != null) return Res2Result.Success(r);
                }
                catch { }
                await Task.Delay(1000, cts.Token).AwaitNoErr();
            }
            return Res2Result.Success();
        }


        /// <summary>
        /// 获取最后一次导入的xlsx
        /// </summary>                
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ExportDgAy2022Cmd), 200)]        
        public async Task<Res2Result> GetLast()
        {
            Guid userid = default;
            try { userid = HttpContext.GetUserId(); }
            catch
            {
                return Res2Result.Fail("请先登录", 401);
            }

            var id = $"{DateTime.Now:yyMMddHHmmss}{Guid.NewGuid():n}";

            if (!await redis.SetAsync(BgCacheKeys.DgAy_import, id, 60 * 10, RedisExistence.Nx))
            {
                return Res2Result.Fail("已经有人在操作了.");
            }

            var cmd = new ExportDgAy2022Cmd { Id = id };
            AsyncUtils.StartNew(cmd);

            return Res2Result.Success(cmd);
        }

        /// <summary>
        /// get export result
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(ExportDgAy2022CmdResult), 200)]
        public async Task<Res2Result> GetExportResult(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Res2Result.Fail(404);
            }
            var cts = new CancellationTokenSource(1000 * 5);
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var r = await redis.GetAsync<ExportDgAy2022CmdResult>(BgCacheKeys.DgAy_export_result.FormatWith(id));
                    if (r != null) return Res2Result.Success(r);
                }
                catch { }
                await Task.Delay(1000, cts.Token).AwaitNoErr();
            }
            return Res2Result.Success();
        }

    }
}
