using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Upload;
using iSchool.Organization.Appliaction.OrgService_bg.Organization;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Appliaction.Service.Organization;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetTopologySuite.Index.Bintree;
using X.PagedList;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 机构后台--机构管理
    /// </summary>
    public class OrganizationController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration config;
        readonly AppSettings _appSettings;

        public OrganizationController(IMediator mediator, IWebHostEnvironment hostingEnvironment, IConfiguration config,
            IOptions<AppSettings> appSettings)
        {
            _mediator = mediator;
            _hostingEnvironment = hostingEnvironment;
            this.config = config;
            this._appSettings = appSettings.Value;
        }



        #region 机构管理

        /// <summary>
        /// 机构管理
        /// </summary>
        /// <param name="page"></param>
        /// <param name="query">查询条件</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(SearchOrgListQuery query, int page)
        {
            int pageSize = 10;
            query.PageSize = pageSize;
            query.PageIndex = page < 1 ? 1 : page;
            var data = _mediator.Send(query).Result;
            var datasAsIPagedList = new StaticPagedList<OrgItem>(data.list, data.PageIndex, data.PageSize, data.PageCount);

            #region ViewBag

            ViewBag.OrgCfyList = EnumUtil.GetSelectItems<OrgCfyEnum>();//机构分类
            ViewBag.AgeGroupList = EnumUtil.GetSelectItems<AgeGroup>();//年龄段
            ViewBag.TeachModeList = EnumUtil.GetSelectItems<TeachModeEnum>();//教学模式
            ViewBag.CooperationStatusList = EnumUtil.GetSelectItems<CooperationStatus>(); // 是否合作
            ViewBag.OrgeEvltCrawlerUploadUrl = config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];
            //查询条件
            string queryJson = query == null ? null : JsonSerializationHelper.Serialize(query);
            ViewBag.queryJson = queryJson;
            ViewBag.query = query;
            #endregion

            return View(datasAsIPagedList);
        }

        /// <summary>
        /// 更新机构Type
        /// </summary>        
        /// <param name="type">机构分类Id</param>
        /// <param name="orgId">机构Id</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateOrgType(string types, Guid orgId)
        {
            Guid userId = HttpContext.GetUserId();
            var dy = new DynamicParameters();
            //var p = JsonSerializationHelper.Serialize(new List<int>() { type });
            dy.Add("@Type", types);
            dy.Add("@userid", userId);
            var result = _mediator.Send(new UpdateSpecialStatusCommand() { OrgId = orgId, Parameters = dy, UpdateSql = " [Types]=@Type,Modifier=@userid " }).Result;
            return Json(result);
        }

        /// <summary>
        /// 更新机构Logo
        /// </summary>  
        /// <param name="orgId">机构Id</param>
        /// <param name="logo">机构Logo</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateOrgLogo(Guid orgId, string logo)
        {
            Guid userId = HttpContext.GetUserId();
            var dy = new DynamicParameters();
            dy.Add("@logo", logo);
            var result = _mediator.Send(new UpdateSpecialStatusCommand() { OrgId = orgId, Parameters = dy, UpdateSql = " logo=@logo " }).Result;
            return Json(result);
        }

        /// <summary>
        /// 机构上/下架
        /// </summary>
        /// <param name="orgId">机构Id</param>
        /// <param name="status">1:上架;0:下架</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OffOrOnTheShelf(Guid orgId, int status)
        {
            Guid userId = HttpContext.GetUserId();
            var org = _mediator.Send(new OrgInfoByIdQuery { Id = orgId }).Result;

            var dy = new DynamicParameters();
            dy.Add("@status", status == 1 ? OrganizationStatusEnum.Ok : OrganizationStatusEnum.Fail);
            var result = _mediator.Send(new UpdateSpecialStatusCommand() { OrgId = orgId, Parameters = dy, UpdateSql = " status=@status " }).Result;

            if (result.Succeed)
            {
                // add user log
                HttpContext.RequestServices.GetService<SmLogUserOperation>()
                    .SetUserId(userId).SetClass(nameof(OrganizationController)).SetMethod(nameof(OffOrOnTheShelf))
                    .SetParams("_", new { orgId, status })
                    .SetOldata("organization", org)
                    .SetTime(DateTime.Now);
            }

            return Json(result);
        }

        /// <summary>
        /// 删除机构
        /// </summary>
        /// <param name="orgId">机构Id</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DelOrg(Guid orgId)
        {
            Guid userId = HttpContext.GetUserId();
            var dy = new DynamicParameters();
            dy.Add("@IsValid", false);
            var result = _mediator.Send(new UpdateSpecialStatusCommand() { OrgId = orgId, Parameters = dy, UpdateSql = " IsValid=@IsValid " }).Result;
            return Json(result);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="type">, Guid id</param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Upload(int type)
        {

            if (!Directory.Exists(_hostingEnvironment.WebRootPath + @"\images\temp"))
            {
                try { Directory.CreateDirectory(_hostingEnvironment.WebRootPath + @"\images\temp"); }
                catch { }
            }

            var files = Request.Form.Files;

            //如果上传文件为null
            if (files == null || files.Count() == 0)
            {
                return Json(new HttpResponse<string> { State = 200, Message = "上传文件不能为空" });
            }
            //logo
            if (type == (int)UploadType.Logo)
            {

                var file = files[0];
                var extension = string.Empty;

                var url = config[Consts.OrgBaseUrl_UploadUrl].FormatWith($"orglogo/{file.Name}", $"{Guid.NewGuid()}.png");
                var result = UploadHelper.TransportImage(file, "logo", Guid.NewGuid().ToString(),
                    out extension,
                    _appSettings.UploadUrl, _hostingEnvironment.WebRootPath + "\\images\\temp\\");

                if (string.IsNullOrEmpty(result.url))
                    return Json(new HttpResponse<string> { State = 500 });
                else
                {
                    //把logo返回的url入库
                    var dy = new DynamicParameters();
                    dy.Add("@logo", result);
                    _mediator.Send(new UpdateSpecialStatusCommand() { OrgId = new Guid(file.Name), Parameters = dy, UpdateSql = " logo=@logo " });
                    return Json(new HttpResponse<string> { State = 200, Result = result + "?t=" + DateTime.Now.Ticks.ToString() });
                }

            }
            return Json("");
        }

        #endregion        

        #region 认领机构
        /// <summary>
        /// 认领机构-列表
        /// </summary>
        /// <param name="page">页码</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ClaimOrgList(int page)
        {
            int pageSize = 10;
            var data = _mediator.Send(new ClaimOrganizationsListQuery() { PageIndex = page < 1 ? 1 : page, PageSize = pageSize }).Result;

            var datasAsIPagedList = new StaticPagedList<ClaimOrgItem>(data.list, data.PageIndex, data.PageSize, data.PageCount);
            return View(datasAsIPagedList);
        }

        /// <summary>
        /// 认领状态修改
        /// </summary>
        /// <param name="id"></param>
        /// <param name="orgId">机构Id</param>
        /// <param name="stats">true:确认认领;false:拒绝</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ClaimOrgOrNot(Guid id, Guid orgId, int stats)
        {
            var result = _mediator.Send(new ClaimOrgCommand() { Id = id, OrgId = orgId, Stats = stats }).Result;

            return Json(result);
        }

        #endregion

        #region 新增-编辑机构

        /// <summary>
        /// 展示机构新增-编辑页面
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddOrEditOrg(Guid? orgId)
        {
            OrgAddOrEditShowDto dto = new OrgAddOrEditShowDto();
            if (orgId == default || orgId == null)//新增
            {
                dto.Id = Guid.NewGuid();
                dto.IsAdd = true;
            }
            else
            {
                var org = _mediator.Send(new OrgInfoByIdQuery() { Id = (Guid)orgId }).Result;
                dto.Id = org.Id;
                dto.Name = org.Name;
                dto.Desc = org.Desc;
                dto.SubDesc = org.Subdesc;
                dto.LOGO = org.Logo;
                dto.MinAge = org.Minage;
                dto.MaxAge = org.Maxage;
                dto.Modes = org.Modes;
                dto.Types = org.Types;
                dto.GoodthingTypes = org.GoodthingTypes;

                //dto.OldTypes = JsonSerializationHelper.JSONToObject<List<string>>(org.Types);
                //dto.OldAges = JsonSerializationHelper.JSONToObject<List<SelectListItem>>(org.Ages);
                //dto.OldModes = JsonSerializationHelper.JSONToObject<List<string>>(org.Modes);
                dto.Intro = org.Intro;

                var brandData = new List<Organization.Appliaction.OrgService_bg.ResponseModels.BgMallFenleisLoadQueryResult>();
                var brandTypes = string.IsNullOrEmpty(org.BrandTypes) ? new List<int>() : JsonSerializationHelper.JSONToObject<List<int>>(org.BrandTypes);
                foreach (var item in brandTypes)
                {
                    var data = await _mediator.Send(new BgMallFenleisLoadQuery { Code = item, ExpandMode = 2 });
                    if (data != null)
                    {
                        brandData.Add(data);
                    }
                }
                dto.BrandTypes = brandData;
            }


            dto.ListSelectTypes = EnumUtil.GetSelectItems<OrgCfyEnum>();//机构分类

            //dto.ListSelectGoodthingTypes = EnumUtil.GetSelectItems<GoodthingCfyEnum>();//好物分类
            dto.ListSelectGoodthingTypes = _mediator.Send(new SelectItemsQuery() { Type = 8, OtherCondition = "14" }).Result;//好物分类

            //dto.ListSelectAges = EnumUtil.GetSelectItems<AgeGroup>();//年龄段
            dto.ListSelectModes = EnumUtil.GetSelectItems<TeachModeEnum>();//教学模式

            ////查询条件
            //string queryJson = query == null ? null : JsonSerializationHelper.Serialize(query);
            //ViewBag.queryJson = queryJson;
            //ViewBag.query = query;
            ViewBag.OrgeEvltCrawlerUploadUrl = config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];
            ViewBag.Setp1 = await _mediator.Send(new BgMallFenleisLoadQuery { ExpandMode = 2 });
            return View(dto);
        }

        /// <summary>
        /// 保存（新增/编辑）机构信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveOrg(AddOrEditOrgCommand request)
        {
            Guid userId = HttpContext.GetUserId();
            request.UserId = userId;
            var result = _mediator.Send(request).Result;
            return Json(result);
        }
        #endregion

        #region 根据机构分类，获取机构下拉框数据源

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgType">机构分类</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> OrgSelectList(int orgType)
        {
            var result = await _mediator.Send(new SelectItemsQuery { Type = 1, OtherCondition = $"1-{orgType}" }); //机构下拉框             
            return Json(result);
        }

        #endregion
    }
}
