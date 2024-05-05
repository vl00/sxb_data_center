using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using iSchool.Organization.Appliaction.OrgService_bg.Exports;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 导出
    /// </summary>
    public class ExportController : Controller
    {
        IMediator _mediator;

        public ExportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region 微信合作院校库页面底部广告-成交数据统计

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SchextAndOrder()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SchextAndOrder_P(ExportSchextAndOrderCommand cmd)
        {
            var id = await _mediator.Send(cmd);
            return Json(string.IsNullOrEmpty(id) ? ResponseResult.Failed("系统繁忙") : ResponseResult.Success(id, null));
        }

        #endregion

        #region RwInviteActivity 微信群顾问拉新

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RwInviteActivity()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RwInviteActivity_P(ExportRwInviteActivityCmd cmd)
        {
            var id = await _mediator.Send(cmd);
            return Json(string.IsNullOrEmpty(id) ? ResponseResult.Failed("系统繁忙") : ResponseResult.Success(id, null));
        }

        #endregion
    }
}
