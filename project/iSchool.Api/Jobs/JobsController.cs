using iSchool.Application.Service;
using iSchool.Application.Service.Jobs;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Jobs.Controllers
{
    /// <summary>
    /// jobs
    /// </summary>
    [AllowGoThroughMvcFilter]
    [Authorize("jobs")]
    public partial class JobsController : Controller
    {
        IMediator _mediator;

        /// <summary>
        /// ctor
        /// </summary>
        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 每日计算编辑和审核员s
        /// </summary>
        /// <returns></returns>        
        public IActionResult GetEditorsAndAuditors([FromQuery]DateTime? date)
        {
            _mediator.Send(new ComputeUserCommand { Date = date ?? DateTime.Now.AddDays(-1).Date }).GetAwaiter().GetResult();

            return Ok();
        }

        /// <summary>
        /// 每日计算学校数据概况
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        public IActionResult ComputeSchoolData([FromQuery]DateTime? date)
        {
            var t = date;
            if (t == null)
            {
                var n = DateTime.Now;
                t = n.Hour >= 12 ? n : n.AddDays(-1).Date;
            }

            _mediator.Send(new ComputeSchoolDataCommand { Date = t.Value.Date }).GetAwaiter().GetResult();

            return Ok();
        }

        /// <summary>
        /// 每日计算学校数据概况(now)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ComputeSchoolDataNow()
        {
            _mediator.Send(new ComputeSchoolDataCommand { Date = DateTime.Now.Date }).GetAwaiter().GetResult();

            return Ok();
        }

        /// <summary>
        /// 检测录入里学部step3-7有无内容
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CmptCheckOnlineSchoolS3to7Empty([FromBody]CheckOnlineSchoolS3to7Empty_Command cmd)
        {
            if (cmd.Sid == null)
            {
                if (cmd.Now == null) cmd.Now = DateTime.Now;
                if (cmd.Now.Value.Hour < 4) cmd.Now = cmd.Now.Value.AddHours(-6);
            }
            if (!cmd.IsBckgd) await _mediator.Send(cmd);
            else
            {
                var sp = HttpContext.RequestServices.CreateScope().ServiceProvider;
                _ = Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await sp.GetService<IMediator>().Send(cmd);
                    }
                    finally
                    {                        
                        sp.GetService<ILog>().Info("end");
                        sp.Dispose();
                    }
                });
            }
            return Ok();
        }

        /// <summary>
        /// 审核单被获取后超过了XX分钟还未变成成功or失败, 变回未审核
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CheckInAuditExpire()
        {
            await _mediator.Send(new Application.Service.Audit.CheckInAuditExpireCommand());
            return Ok();
        }

        /// <summary>
        /// FixSync_Lyega_OLschextSimpleInfo
        /// </summary>
        [HttpPost]
        [Obsolete("use ExecSql1 instead")]
        public async Task<IActionResult> FixSync_Lyega_OLschextSimpleInfo([FromBody]ExecSqlFileCommand cmd)
        {
            await _mediator.Send(cmd);
            return Ok();
        }

        /// <summary>
        /// exec simple sql
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExecSql1([FromBody]ExecSqlFileCommand cmd)
        {
            await _mediator.Send(cmd);
            return Json(FnResult.OK(true));
        }

        /// <summary>
        /// BaiduTuiGuang_realtime
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BaiduTuiGuang_realtime([FromBody] BaiduTuiGuang_realtime_Command cmd)
        {
            await _mediator.Send(cmd);
            return Ok();
        }

        /// <summary>
        /// 重发事件 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Republishevent([FromQuery] string type, [FromBody] dynamic json)
        {
            await _mediator.Publish(JsonExtensions.ToObject(json?.ToString(), Type.GetType(type)));
            return Json(FnResult.OK(1));
        }
    }
}