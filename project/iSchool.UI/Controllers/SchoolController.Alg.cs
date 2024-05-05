using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Application.Service;
using iSchool.Application.Service.Audit;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Upload;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace iSchool.UI.Controllers
{
    /// <summary>
    ///  数据录入模块
    /// </summary>
    public partial class SchoolController : Controller
    {
        /// <summary>
        /// 算法-社会 页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Alg1(Guid sid, Guid extId)
        {
            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction(nameof(Main));
            }
            var userId = HttpContext.GetUserId();

            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            ViewBag.sid = sid;
            ViewBag.eid = extId;

            ViewBag.Subjs = _mediator.Send(new KvsQuery { Type = 3, ParentId = "f3xk" }).Result;
            ViewBag.Arts = _mediator.Send(new KvsQuery { Type = 3, ParentId = "f3ys" }).Result;
            ViewBag.Sports = _mediator.Send(new KvsQuery { Type = 3, ParentId = "f3ty" }).Result;
            ViewBag.Science = _mediator.Send(new KvsQuery { Type = 3, ParentId = "f3kx" }).Result;

            var r = _mediator.Send(new Alg1Query { Sid = sid, Eid = extId }).Result;           
            return View(r);
        }
        /// <summary>
        /// 算法-社会 页面save
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Alg1([FromBody]Alg1QyRstDto dto, [FromQuery]Guid sid)
        {
            var uid = HttpContext.GetUserId();
            var cmd = new Alg1Command { Sid = sid, Dto = dto, UserId = uid };
            _mediator.Send(cmd).GetAwaiter().GetResult();

            return Json(FnResult.OK(1));
        }

        /// <summary>
        /// 算法-经济 页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Alg2(Guid sid, Guid extId)
        {
            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction(nameof(Main));
            }
            var userId = HttpContext.GetUserId();

            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            ViewBag.sid = sid;
            ViewBag.eid = extId;

            var r = _mediator.Send(new Alg2Query { Sid = sid, Eid = extId }).Result;
            return View(r);
        }
        /// <summary>
        /// 算法-经济 页面save
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Alg2([FromBody]Alg2QyRstDto dto)
        {
            var uid = HttpContext.GetUserId();
            var cmd = new Alg2Command { Dto = dto, UserId = uid };
            _mediator.Send(cmd).GetAwaiter().GetResult();

            return Json(FnResult.OK(1));
        }

        /// <summary>
        /// 算法-个人
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Alg3(Guid sid, Guid extId)
        {
            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction(nameof(Main));
            }
            var userId = HttpContext.GetUserId();

            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            ViewBag.sid = sid;
            ViewBag.eid = extId;

            var r = _mediator.Send(new Alg3Query { Sid = sid, Eid = extId }).Result;
            return View(r);
        }
        /// <summary>
        /// 算法-个人 页面save
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Alg3(Alg3Command cmd)
        {
            var uid = HttpContext.GetUserId();
            cmd.UserId = uid;
            _mediator.Send(cmd).GetAwaiter().GetResult();

            return Json(FnResult.OK(1));
        }
    }
}