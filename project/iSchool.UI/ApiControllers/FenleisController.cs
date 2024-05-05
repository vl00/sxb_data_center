using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service;
using iSchool.Organization.Appliaction.Service.Organization;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.UI.ApiControllers
{
    /// <summary>
    /// 商城分类
    /// </summary>
    [Route("api/bg/[controller]/[action]")]
    [ApiController]
    public class MallFenleisController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration config;

        public MallFenleisController(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator;
            this.config = config;
        }

        /// <summary>
        /// 常规分类页面数据
        /// </summary>
        /// <param name="code">分类code,可以不传</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BgMallFenleisLoadQueryResult), 200)]
        public async Task<Res2Result> GetNomals(int? code)
        {
            Guid userId = default;
            try { userId = HttpContext.GetUserId(); } catch { }

            var r = await _mediator.Send(new BgMallFenleisLoadQuery { Code = code, UserId = userId, ExpandMode = 2 });
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 常规分类页面数据(隐藏没3级)
        /// </summary>
        /// <param name="code">分类code,可以不传</param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(BgMallFenleisLoadQueryResult), 200)]
        public async Task<Res2Result> GetNomals2(int? code)
        {
            Guid userId = default;
            try { userId = HttpContext.GetUserId(); } catch { }

            var r = await _mediator.Send(new BgMallFenleisLoadQuery2 { Code = code, UserId = userId });
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 同级分类排序 drag drop
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BgMallFenleiDragDropCmdResult), 200)]
        public async Task<Res2Result> DragDrop(BgMallFenleiDragDropCmd cmd)
        {
            cmd.UserId = HttpContext.GetUserId();
            var r = await _mediator.Send(cmd);
            return Res2Result.Success(r);
        }

        /// <summary>
        /// (新增+修改)保存分类项
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BgMallFenleiSaveCmdResult), 200)]
        public async Task<Res2Result> Save(BgMallFenleiSaveCmd cmd)
        {
            cmd.UserId = HttpContext.GetUserId();
            var r = await _mediator.Send(cmd);
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 删除一个分类
        /// </summary>
        /// <param name="code">分类code.</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<Res2Result> Delete(int code)
        {
            var r = await _mediator.Send(new BgMallFenleisDeleteCmd { Code = code, UserId = HttpContext.GetUserId() });
            return Res2Result.Success(r);
        }

        /// <summary>
        /// 用于商品详情页商品分类查下级项s
        /// </summary>
        /// <param name="code">
        /// 分类code,可以不传. 只返回直接下级
        /// </param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BgMallFenleisLoadQueryResult), 200)]
        public async Task<Res2Result> GetChildren(int? code)
        {
            Guid userId = default;
            try { userId = HttpContext.GetUserId(); } catch { }

            var r = await _mediator.Send(new BgMallFenleisLoadQuery { Code = code, UserId = userId, ExpandMode = 1 });
            return Res2Result.Success(r);
        }


        /// <summary>
        /// 热门分类
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BgHotMallFenleisLoadQueryResult), 200)]
        public async Task<Res2Result> GetHotLs()
        {
            var r = await _mediator.Send(new BgHotMallFenleisLoadQuery { });
            return Res2Result.Success(r);
        }

        /// <summary>
        /// (修改+删除)编辑热门分类项
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<Res2Result> ModifyHot(BgModifyHotMallFenleiCmd cmd)
        {
            cmd.UserId = HttpContext.GetUserId();
            var r = await _mediator.Send(cmd);
            return Res2Result.Success(r);
        }
    }
}
