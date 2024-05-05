using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using iSchool.Application.Service;
using iSchool.Application.Service.Audit;
using iSchool.Application.Service.OnlineSchool;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.UI.Controllers
{
    /// <summary>
    ///  数据录入模块
    /// </summary>
    public class AuditController : Controller
    {
        readonly IMediator _mediator;
        IValidator<AuditCommand> auditCmdValidator;
        AppSettings _appSettings;

        public AuditController(IMediator mediator, IValidator<AuditCommand> auditCmdValidator, AppSettings appSettings)
        {
            _mediator = mediator;
            this.auditCmdValidator = auditCmdValidator;
            this._appSettings = appSettings;
        }

        /// <summary>
        /// index页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //学校类型
            ViewBag.SchoolType = EnumUtil.GetDescs<SchoolType>();
            //招生年级
            ViewBag.SchoolGrade = EnumUtil.GetDescs<SchoolGrade>();
            //审核状态
            ViewBag.SchoolAuditStatus = EnumUtil.GetDescs<SchoolAuditStatus>();

            //编辑员
            var editors = new Authorization.Account().GetAdmins(_appSettings.GidEditor, Authorization.Account.IDType.CharacterID).Select(_ => new Domain.Total_User
            {
                Id = _.Id,
                Account = _.Name,
                Username = _.Displayname,
                RoleId = _appSettings.GidEditor,
            })
            .OrderBy(_ => _.Username)
            .ToArray();

            //审核员
            var auditors = new Authorization.Account().GetAdmins(_appSettings.GidQxAudit, Authorization.Account.IDType.FunctionID).Select(_ => new Domain.Total_User
            {
                Id = _.Id,
                Account = _.Name,
                Username = _.Displayname,
                QxId = _appSettings.GidQxAudit,
            })
            .OrderBy(_ => _.Username)
            .ToArray();

            var canSeeAll = HttpContext.HasCtrlActQyx(("audit", nameof(PageList)), "seeall");

            ViewBag.Editors = editors;
            if (!canSeeAll) ViewBag.Auditors = auditors.Where(_ => _.Id == HttpContext.GetUserId()).ToArray();
            else ViewBag.Auditors = auditors;

            var pg = await _mediator.Send(new SearchQuery
            {
                AuditorId = HttpContext.GetUserId(),
                PageIndex = 1,
                PageSize = 10,
                SearchType = 0,
                CanSeeAll = canSeeAll,
            });

            return View(pg);
        }

        /// <summary>
        /// 审核列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PageList([FromBody]SearchQuery searchQuery)
        {
            searchQuery.AuditorId = HttpContext.GetUserId();
            searchQuery.CanSeeAll = HttpContext.HasCtrlActQyx("audit", nameof(PageList), "seeall");

            var pg = await _mediator.Send(searchQuery);

            return Json(FnResult.OK(pg));
        }

        /// <summary>
        /// 审批界面
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public IActionResult Audit(Guid id)
        {
            var isAjaxRequest = HttpContext.Request.IsAjaxRequest();
            try
            {
                var req = new GetAnAuditQuery
                {
                    Id = id,
                    IsRead = false,
                    AuditorId = HttpContext.GetUserId(),
                    IsPreGet = isAjaxRequest,
                    CanDoAll = HttpContext.HasCtrlActQyx("audit", nameof(DoAudit), "candoall"),
                };
                var data = _mediator.Send(req).GetAwaiter().GetResult();

                ViewBag.isRead = false;
                if (isAjaxRequest) return Json(FnResult.OK(data));
                else return View(data);
            }
            catch (FnResultException ex) when (ex.Code == AuditOption.Errcode_Audited)
            {
                if (isAjaxRequest) throw ex;
                return RedirectToAction(nameof(Index));
            }
            catch (FnResultException ex) when (ex.Code == AuditOption.Errcode_Handing)
            {
                if (isAjaxRequest) throw ex;

                ViewBag.Msg = ex.Message;
                ViewBag.Url = Url.Action(nameof(Index));
                return View("Redirect");
            }
        }

        /// <summary>
        /// 审批界面(查看)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Look(Guid sid)
        {
            var req = new PreviewSchoolQuery { Sid = sid };
            var res = _mediator.Send(req).Result;

            ViewBag.eid = req.Eid;
            ViewBag.isRead = true;
            return View("../School/Preview", res);
        }

        /// <summary>
        /// 审批历史
        /// </summary>
        [HttpGet]
        public IActionResult AuditHistorys(HistoryQuery req)
        {
            var res = _mediator.Send(req).GetAwaiter().GetResult();

            ViewBag.ModalName = "审核历史";

            return PartialView(res);
        }

        /// <summary>
        /// 学部内容--type
        /// </summary>
        /// <param name="extId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetSchoolExtInfo(string extId, string type)
        {
            var req = new SchoolExtQuery { ExtId = Guid.Parse(extId), Type = type };

            var res = _mediator.Send(req).GetAwaiter().GetResult();
            ViewBag.Eid = Guid.Parse(extId);

            return PartialView($"ext_{type}", res);
        }

        /// <summary>
        /// 根据学校获取文章
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetSchoolArticlePageList([FromBody]SearchSchoolArticleQuery req)
        {
            var pg = await _mediator.Send(req);
            return PartialView(pg);
        }

        /// <summary>
        /// 获取学校升学成绩年份s
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetSchoolAchievementYear([FromQuery]GetSchoolAchievementYearQuery req)
        {
            var res = _mediator.Send(req).Result;
            return PartialView("Ext_AchievementYear", res);
        }

        /// <summary>
        /// 根据学校的年级查询某年升学成绩info
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetSchoolAchievementInfo([FromBody]SchoolAchievementInfoQuery req)
        {
            var res = _mediator.Send(req).Result;
            return PartialView(res);
        }

        /// <summary>
        /// 毕业去向分页列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult PageYearAchievementDestination([FromBody]GetYearAchievementDestinationQuery req)
        {
            ViewBag.Year = req.Year;
            ViewBag.Eid = req.Eid;
            if (req.PageIndex == 0)
            {
                return PartialView(new PagedList<YearAchievementDestinationQueryResult>
                {
                    CurrentPageIndex = 0,
                    PageSize = 0,
                    CurrentPageItems = Enumerable.Empty<YearAchievementDestinationQueryResult>(),                 
                });
            }
            else
            {
                var res = _mediator.Send(req).Result;
                return PartialView(res);
            }
        }

        /// <summary>
        /// 提交审核（发布/不通过）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DoAudit([FromBody]AuditCommand cmd)
        {
            var errors = auditCmdValidator.Validate(cmd).Errors;
            if (errors.Any())
            {
                throw new AggregateException(errors.Select(_ => new Exception(_.ErrorMessage)));
            }

            cmd.AuditorId = HttpContext.GetUserId();
            cmd.CanDoAll = HttpContext.HasCurrQyx("candoall");

            _ = _mediator.Send(cmd).Result;

            return Json(new
            {
                isOk = true,
            });
        }

        /// <summary>
        /// 下线已发布的学校
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OfflineSchool(Guid sid)
        {
            var cmd = new OfflineSchoolCommand { Id = sid, UserId = HttpContext.GetUserId() };
            _ = _mediator.Send(cmd).Result;
            return Json(FnResult.OK<object>(null));
        }

        /// <summary>
        /// 删除已发布的学校
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DelSchool(Guid sid)
        {
            var cmd = new DelSchoolCommand { Id = sid, UserId = HttpContext.GetUserId() };
            _ = _mediator.Send(cmd).Result;
            return Json(FnResult.OK<object>(null));
        }

        /// <summary>
        /// 根据输入的词查相应学校（智能匹配?）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetSchoolTypeahead([FromQuery]SearchSchool4TypeaheadQuery req)
        {
            var res = _mediator.Send(req).Result;
            return Json(FnResult.OK(res));
        }

        /// <summary>
        /// 批量审核
        /// </summary>
        [HttpPost]
        [AllowGoThroughMvcFilter]
        public IActionResult BatchDoAudit([FromBody]AuditCommand[] cmds)
        {
            if (!HttpContext.HasCtrlActQyx("audit", nameof(DoAudit)))
            {
                throw new FnResultException(403, "无权限");
            }

            var uid = HttpContext.GetUserId();
            var candoall = HttpContext.HasCtrlActQyx(("audit", nameof(DoAudit)), "candoall");

            foreach (var cmd in cmds)
            {
                var errors = auditCmdValidator.Validate(cmd).Errors;
                if (errors.Any())
                {
                    throw new AggregateException(errors.Select(_ => new Exception(_.ErrorMessage)));
                }

                if (!cmd.Fail)
                {
                    throw new FnResultException(2333, "批量审核不支持修改为已发布");
                }

                cmd.AuditorId = uid;
                cmd.CanDoAll = candoall;

                _ = _mediator.Send(cmd).Result;
            }

            return Json(FnResult.OK(cmds.Length));
        }

        [HttpPost]
        [AllowGoThroughMvcFilter]
        public async Task<IActionResult> ForBatchDoAuditOk([FromBody]AuditCommand cmd)
        {
            await _mediator.Send(cmd);

            return Json(FnResult.OK(1));
        }

        [HttpPost]
        [AllowGoThroughMvcFilter]
        public IActionResult ForBatchOfflineSchool([FromBody]OfflineSchoolCommand cmd)
        {
            _ = _mediator.Send(cmd).Result;
            return Json(FnResult.OK(1));
        }

        /// <summary>
        /// 删除学部点评重关联
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DpwdRecorrelate([FromQuery]DpwdRecorrQuery req)
        {
            var res = await _mediator.Send(req);
            ViewBag.req = req;
            return View(res);
        }
        /// <summary>
        /// 导出删除学部点评重关联到excel
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ExportDpwdRecorr([FromQuery]ExportDeletedSchextDpwdCommand cmd)
        {
            var bys = await _mediator.Send(cmd);
            return File(bys, "application/ms-excel", $"未重关联点评的被删除学部.xlsx");
        }
    }
}