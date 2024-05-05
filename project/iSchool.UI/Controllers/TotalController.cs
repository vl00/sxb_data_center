using Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using iSchool.Application.Service;
using iSchool.Application.Service.Totals;
using iSchool.Authorization;
using iSchool.Infrastructure;
using iSchool.UI.Models;

namespace iSchool.UI.Controllers
{
    public class TotalController : Controller
    {
        private readonly IMediator _mediator;

        public TotalController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        /// <summary>
        /// 人员管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.TotalDataboard = _mediator.Send(new GetBoardQuery()).Result;

            ViewBag.Editors = _mediator.Send(new PageEditorsQuery()).Result;

            ViewBag.SchoolEdit = _mediator.Send(new PageSchoolEditQuery()).Result;

            ViewBag.Auditors = _mediator.Send(new PageAuditorsQuery()).Result;

            ViewBag.SchoolAudit = _mediator.Send(new PageSchoolAuditQuery()).Result;

            return View();
        }

        [HttpPost]
        public IActionResult PageEditors(PageEditorsQuery req)
        {
            var r = _mediator.Send(req).Result;
            return PartialView("P_TbEditors", r);
        }

        [HttpPost]
        public IActionResult PageSchoolEdit(PageSchoolEditQuery req)
        {
            var r = _mediator.Send(req).Result;
            return PartialView("P_TbSchoolEdit", r);
        }

        [HttpPost]
        public IActionResult PageAuditors(PageAuditorsQuery req)
        {
            var r = _mediator.Send(req).Result;
            return PartialView("P_TbAuditors", r);
        }

        [HttpPost]
        public IActionResult PageSchoolAudit(PageSchoolAuditQuery req)
        {
            var r = _mediator.Send(req).Result;
            return PartialView("P_TbSchoolAudit", r);
        }

        /// <summary>
        /// 人员管理 - 详细
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult UserWorkInfo(Guid uid, int type)
        {
            var pg = _mediator.Send(new UserWorkInfoQuery { UserId = uid, Type = type }).Result;

            ViewBag.Schools = pg;

            return View();
        }

        [HttpPost]
        public IActionResult PageUserWorkInfo(UserWorkInfoQuery req)
        {
            var r = _mediator.Send(req).Result;
            return PartialView("P_TbUserWorkInfo", r);
        }
    }
}
