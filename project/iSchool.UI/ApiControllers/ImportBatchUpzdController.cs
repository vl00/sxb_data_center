using CSRedis;
using iSchool.Api.ModelBinders;
using iSchool.BgServices;
using iSchool.Domain.Modles;
using iSchool.Infras;
using iSchool.Infrastructure;
using iSchool.Application.Service.ImportBatchUpzd;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
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
using System.Threading.Tasks;
using System.Web;

namespace iSchool.UI.ApiControllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ImportBatchUpzdController : ControllerBase
    {
        IMediator _mediator;

        public ImportBatchUpzdController(IMediator _mediator)
        {
            this._mediator = _mediator;
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
        [ProducesResponseType(typeof(UploadVideoResult), 1444200)]
        [DisableRequestSizeLimit]
        public async Task<Res2Result> Upload([FromForm(Name = "id")] string fid, [FromForm] string p, [FromForm] long index, [FromForm] long total, [FromForm(Name = "size")] long blockSize,
            [BindFormFile(0)] IFormFile file, [FromForm] string fileName, [FromForm] string ext,
            [FromForm] string _email, [FromForm] string _id, [FromForm] string _ver,
            [FromServices] IConfiguration config)
        {
            try { _ = HttpContext.GetUserId(); }
            catch
            {
                return Res2Result.Fail("请先登录", 401);
            }

            if (file == null || file.Length == 0L)
            {
                return Res2Result.Fail("上传文件不能为空");
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

            using (var fs = new FileStream(x.Path, FileMode.Open, FileAccess.Read))
            {
                _id = await Upload_to_hushushu(HttpContext.RequestServices, fs, fileName, _ver, _id, _email);
            }
            try { System.IO.File.Delete(x.Path); } catch { }
            var result = new UploadImgResult() { Id = _id };
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
            if (index == 1 && string.IsNullOrEmpty(fid)) fid = Guid.NewGuid().ToString("n");
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

        [NonAction]
        static async Task<string> Upload_to_hushushu(IServiceProvider services, FileStream fs, string fileName, string _ver, string _id, string _email)
        {
            var rabbit = services.GetService<RabbitMQConnectionForPublish>();
            await default(ValueTask);
            _id = !string.IsNullOrEmpty(_id) ? _id : Guid.NewGuid().ToString();
            var hashMd5 = HashAlgmUtil.EncryptStream(fs, "md5");
            fs.Position = 0;
            var (blockSize, blockCount) = ReadFileInfo(fs);

            await foreach (var (i, mm) in ReadFile(fs, 1, blockSize, blockCount))
            {
                while (true)
                {
                    try
                    {
                        var args = new object[] { i, fs.Length, fileName ?? Path.GetFileName(fs.Name), hashMd5, _ver, mm.ToArray(), _email };
                        rabbit.OpenChannel().ConfirmPublish(
                            new RabbitMessage("amq.topic", "zd70.")
                            .SetHeader("hu-actor-id", $"/huactor/recv_file/{_id}")
                            .SetBody(args.ToJsonString())
                        );
                        break;
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(1000 * 2);
                    }
                }
            }

            return _id;
        }

        static (int blockSize, int blockCount) ReadFileInfo(Stream fs)
        {
            var totalSize = fs.Length; //文件大小
            var blockSize = (int)(1024 * 1024 * 0.5); //块大小512k
            var blockCount = (int)Math.Ceiling(totalSize / (double)blockSize); //总块数
            return (blockSize, blockCount);
        }

        static async IAsyncEnumerable<(int i, Memory<byte>)> ReadFile(FileStream fs, int i, int blockSize, int blockCount)
        {
            using var mo = MemoryPool<byte>.Shared.Rent(blockSize);
            for (; i <= blockCount; i++)
            {
                var bi = await fs.ReadAsync(mo.Memory);
                yield return (i, bi == blockSize ? mo.Memory : mo.Memory[0..bi]);
            }
        }

        #endregion upload


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<Res2Result> GetTopN(int pi, int ps)
        {
            var r = (await _mediator.Send(new ImportBatchUpzdReqArgs
            {
                Search = new ImportBatchUpzdReqArgs.SearchArgs
                {
                    PageIndex = pi,
                    PageSize = ps,
                }
            })).SearchResult;
            return Res2Result.Success(r);
        }


    }
}
