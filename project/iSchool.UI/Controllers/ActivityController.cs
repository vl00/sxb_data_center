using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg;
using iSchool.Organization.Appliaction.OrgService_bg.Activitys;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 后台--活动审核模块
    /// </summary>
    public class ActivityController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _config;

        public ActivityController(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator;            
            _config = config;
        }
        
        /// <summary>
        /// 活动管理--列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(SearchActivitysQuery query)
        {
            ViewBag.SpecialList = _mediator.Send(new SelectItemsQuery() { Type = 2 }).Result;
            query.ActivityUrl = _config[Consts.ActivityUrl];
            if (query.SearchType == 1)//视图
            {                
                query.PageIndex = 1; query.PageSize = 10;
                var data = _mediator.Send(query).Result;
                ViewBag.ActivityStatusList = EnumUtil.GetSelectItems<ActivityStatus>();//活动状态枚举  
                return View(data);
            }
            else//分页json
            {
                var data = _mediator.Send(query).Result;
                return Json(new { data = data, isOk = true });
            }

        }

        #region 新增/编辑--展示vs保存


        /// <summary>
        /// 新增/编辑页面
        /// </summary>
        /// <param name="id">活动Id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AddUpdateShow(Guid? id)
        {

            var dto = new AddUpdateActivityShowDto();
            if (id == null)//新增
            {
                dto.Id = Guid.NewGuid();
                dto.IsAdd = true;
                dto.ListSpecials = _mediator.Send(new SelectItemsQuery() { Type = 5 }).Result;
                dto.StartTime = DateTime.Now;
                dto.EndTime = DateTime.Now.AddDays(10);
            }
            else//编辑--获取课程旧数据并赋给dto
            {
                dto = _mediator.Send(new QueryActivityById() { ActivityId=(Guid)id}).Result;
                dto.ListSpecials = _mediator.Send(new SelectItemsQuery() { Type = 5,ActivityId=(Guid)id }).Result;
            }
            ViewBag.OrgeEvltCrawlerUploadUrl = _config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];
             
            return View(dto);
        }

        /// <summary>
        /// 保存(新增/编辑)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveCourse(SaveActivityCommand request)
        {
            Guid userId = HttpContext.GetUserId();
            request.UserId = userId;
            var result = _mediator.Send(request).Result;
            return Json(result);
        }


        #endregion

        #region 活动上下架
        /// <summary>
        /// 活动上/下架
        /// </summary>
        /// <param name="id">活动Id</param>
        /// <param name="status">1:上架;2:下架;</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OffOrOnTheShelf(Guid id, int status)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new UpdateActivityStatusCommand() { Id = id, Status = status, UserId = userId }).Result;
            return Json(result);
        }
        #endregion




        /// <summary>
        /// 活动审核页面
        /// </summary>        
        [HttpGet]
        public async Task<IActionResult> Audit()
        {
            ViewBag.Activitys = await _mediator.Send(new AuditLsActiQuery { PageIndex = 1, PageSize = 99, Type = ActivityType.Hd2.ToInt() });
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AuditLsActi([FromBody] AuditLsActiQuery query)
        {
            var r = await _mediator.Send(query);
            return Json(r);
        }
        [HttpGet]
        public async Task<IActionResult> AuditLsSpcl(string activityId)
        {
            var id = Guid.TryParse(activityId, out var _id) ? _id : default;
            if (id == default) return Json(ResponseResult.Success(new AuditLsSpclQueryResult()));
            var r = await _mediator.Send(new AuditLsSpclQuery { ActivityId = id });
            return Json(ResponseResult.Success(r));
        }
        [HttpPost]
        public async Task<IActionResult> AuditLsPager([FromBody] AuditLsPagerQuery query, [FromQuery] int t = 0)
        {
            var r = await _mediator.Send(query);
            if (t == 1) return Json(ResponseResult.Success(r));
            else return PartialView("Audit.List", r);
        }

        /// <summary>
        /// 查询活动预算和支出
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetActivityPayMoney(Guid activityId)
        {
            var r = await _mediator.Send(new ActivityPayMoneyQuery { ActivityId = activityId });
            return Json(ResponseResult.Success(r));
        }

        /// <summary>
        /// 审核post请求
        /// </summary>                
        [HttpPost]
        public async Task<IActionResult> Audit([FromBody] ActiAuditCommand cmd)
        {
            cmd.AuditorId = HttpContext.GetUserId();
            var r = await _mediator.Send(cmd);
            return Json(ResponseResult.Success(r));
        }

        /// <summary>
        /// 导出活动审核到excel
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExportXlsxP([FromBody] ExportAuditLsCommand cmd)
        {
            var id = await _mediator.Send(cmd);
            return Json(string.IsNullOrEmpty(id) ? ResponseResult.Failed("系统繁忙") : ResponseResult.Success(id, null));
        }

    }
}

