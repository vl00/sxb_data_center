using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.Course;
using iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager;
using iSchool.Organization.Appliaction.OrgService_bg.Order;
using iSchool.Organization.Appliaction.OrgService_bg.Orders;
using iSchool.Organization.Appliaction.OrgService_bg.RedeemCodes;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Appliaction.RequestModels.Courses;
using iSchool.Organization.Appliaction.RequestModels.Orders;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Appliaction.Service.Organization;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 机构后台--课程管理
    /// </summary>
    public class CoursesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration config;

        public CoursesController(IMediator mediator, IWebHostEnvironment hostingEnvironment, IConfiguration config)
        {
            _mediator = mediator;
            _hostingEnvironment = hostingEnvironment;
            this.config = config;

        }

        #region 课程管理

        /// <summary>
        /// 课程管理-视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(SearchCoursesQuery query)
        {

            ViewBag.UploadUrl = config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];//上传图片
            if (query.SearchType == 1)//视图
            {
                var data = _mediator.Send(new SearchCoursesQuery() { PageIndex = 1, PageSize = 10 }).Result;
                //查询下拉框
                ViewBag.CourseStatusList = EnumUtil.GetSelectItems<CourseStatusEnum>();//课程状态枚举  

                //机构下拉框 
                var orgs = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;
                orgs.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "" });
                ViewBag.OrgList = orgs;

                //供应商下拉框 
                var suppliers = _mediator.Send(new SelectItemsQuery() { Type = 9 }).Result;
                suppliers.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "" });
                ViewBag.SupplierList = suppliers;

                return View(data);
            }
            else//分页json
            {
                var data = _mediator.Send(query).Result;
                return Json(new { data = data, isOk = true });
            }

        }

        [HttpGet]
        public async Task<CreateMpQrcodeCmdResult> CreateMpQrcode(string id_s)
        {
            var result = (await _mediator.Send(new CreateMpQrcodeCmd() { AppName = "app_org", Page = "pagesA/pages/course_detail/course_detail", Scene = $"id/{id_s}" }));
            return result;
        }

        /// <summary>
        /// 根据科目Id获取课程信息
        /// </summary>
        /// <param name="subjectid"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetCoursesBySubjectId(int subjectid)
        {
            try
            {
                var response = _mediator.Send(new CoursesBySubjectIdQuery() { SubjectId = subjectid }).Result;
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(null);
            }

        }


        #region 属性-选项
        /// <summary>
        /// 商品列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetGoodsInfoList(SearchGoodsInfoQuery query)
        {
            var data = _mediator.Send(query).Result;//商品列表
            var propItems = _mediator.Send(new QueryPropertyInfoByCourseId() { CourseId = query.CourseId }).Result;  //属性-选项信息

            var listSuppliers = _mediator.Send(new SelectItemsQuery() { Type = 9 }).Result;//供应商下拉框

            return Json(new { data = data, propItems = JsonSerializationHelper.Serialize(propItems), listSuppliers = JsonSerializationHelper.Serialize(listSuppliers), isOk = true });
        }

        /// <summary>
        /// 保存属性、选项、商品的信息及关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SavePropertyInfo(SavePropertyInfoCommand request)
        {
            var result = _mediator.Send(request).Result;
            return Json(result);
        }

        /// <summary>
        /// 导出所有SKU明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult ExportSKUData(ExportGoodsInfoList request)
        {
            var data = _mediator.Send(request).Result.Data as IEnumerable<dynamic>;
            string filepath = AppContext.BaseDirectory + "wwwroot" + @"\SKU.xlsx";
            Infrastructure.Excel.ExcelHelper.DataToExcel(filepath, data, "SKU详情", true);
            return File(System.IO.File.ReadAllBytes(filepath), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SKU详情.xlsx");
        }


        #endregion

        #region 新增/编辑课程~展示vs保存


        /// <summary>
        /// 新增/编辑页面
        /// </summary>
        /// <param name="id">课程Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddUpdateShow(Guid? id)
        {
            var isadd = id == null;
            var dto = new AddCoursesShowDto();
            if (isadd)//新增
            {
                dto.Id = Guid.NewGuid();
                dto.IsAdd = true;
                dto.DrpInfo = new CourseDrpInfo();
                dto.ListNotices = new List<CourseNotices>();
                //dto.Type = CourseTypeEnum.Course.ToInt();//默认是网课
            }
            else//编辑--获取课程旧数据并赋给dto
            {
                var oldCourse = (Course)(_mediator.Send(new QueryCourseById() { CourseId = (Guid)id, IgnoreStatus = true }).Result.Data);
                dto.Id = (Guid)id;
                dto.Name = oldCourse.Name;
                dto.Title = oldCourse.Title;
                dto.SubTitle = oldCourse.Subtitle;
                dto.OrgId = oldCourse.Orgid;
                dto.Price = oldCourse.Price;
                dto.Stock = oldCourse.Stock;
                dto.Duration = oldCourse.Duration;
                dto.ListOldModes = JsonSerializationHelper.JSONToObject<List<int>>(oldCourse.Mode ?? "[]");
                dto.ListOldGoodthingTypes = JsonSerializationHelper.JSONToObject<List<string>>(oldCourse.GoodthingTypes ?? "[]");
                dto.ListOldSubjects = JsonSerializationHelper.JSONToObject<List<string>>(oldCourse.Subjects ?? "[]");
                dto.Type = oldCourse.Type;
                dto.IsExplosions = oldCourse.IsExplosions;
                dto.IsInvisibleOnline = oldCourse.IsInvisibleOnline;
                dto.Subject = oldCourse.Subject;
                dto.MinAge = oldCourse.Minage;
                dto.MaxAge = oldCourse.Maxage;
                dto.No = oldCourse.No;
                dto.IsSystemCourse = oldCourse.Type == CourseTypeEnum.Goodthing.ToInt() ? null : oldCourse.IsSystemCourse;
                dto.Videos = string.IsNullOrEmpty(oldCourse.Videos) ? null : JsonSerializationHelper.JSONToObject<List<string>>(oldCourse.Videos);
                dto.VideoCovers = string.IsNullOrEmpty(oldCourse.Videocovers) ? null : JsonSerializationHelper.JSONToObject<List<string>>(oldCourse.Videocovers);
                #region banner-url
                var bannerUrls = oldCourse.Banner;
                StringBuilder sBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(bannerUrls))
                {
                    //var list = JsonSerializationHelper.JSONToObject<List<string>>(bannerUrls);
                    //var count = list.Count <= 9 ? list.Count + 1 : 10;
                    //sBuilder.AppendLine($@"<div class={'"'}row img-margin-left{'"'} >");
                    //for (int i = 1; i <= count; i++)
                    //{
                    //    if (i == count)
                    //    {
                    //        //上传按钮
                    //        sBuilder.AppendLine($@"<div class={'"'}col-md-2{'"'}>");
                    //        sBuilder.AppendLine($@"    <input type = {'"'}file{'"'} id={'"'}{id}{'"'} hidden={'"'}hidden{'"'} class={'"'}c_ignore updateFile{'"'} name={'"'}files{'"'} multiple accept = {'"'}jpg,png{'"'} title={'"'}只允许上传Mp4格式的视频!视频大小不能超过40M{'"'} />");
                    //        sBuilder.AppendLine($@"    <input type = {'"'}button{'"'} id={'"'}uploadlogo{'"'} style={'"'}width: 100px; height: 100px; font-size: 50px;{'"'} class={'"'}uploadvideo-btn btn  btn-info btn-block c_ignore updateBtn{'"'} data-video={'"'}{id}{'"'} data-input={'"'}InterviewVideos{'"'} value={'"'}+{'"'} />");
                    //        sBuilder.AppendLine($@"</div>");
                    //    }
                    //    else
                    //    {
                    //        //图片
                    //        sBuilder.AppendLine($@" <div class={'"'}col-md-2{'"'}>");
                    //        sBuilder.AppendLine($@"    <div class={'"'}form-inline{'"'}>");
                    //        sBuilder.AppendLine($@"        <a class={'"'}delrankbtn fa fa-minus-circle deletebutten  text-danger{'"'} data-input={'"'}{list[i - 1]}{'"'}></a>");
                    //        sBuilder.AppendLine($@"        <img style = {'"'}width:100px;height:100px;{'"'} src={'"'}{list[i - 1]}{'"'} />");
                    //        sBuilder.AppendLine($@"    </div>");
                    //        sBuilder.AppendLine($@"    <div class={'"'}form-inline{'"'}>");
                    //        sBuilder.AppendLine($@"        <div class={'"'}col-md-7{'"'} style={'"'}text-align:center;{'"'}>");
                    //        sBuilder.AppendLine($@"            <a href = {'"'}javascript:void(0){'"'} data-id={'"'}{'"'} class={'"'}downloadpic text-info{'"'} data-input={'"'}{list[i - 1]}{'"'} >下载</a> ");
                    //        sBuilder.AppendLine($@"        </div>");
                    //        sBuilder.AppendLine($@"        <div class={'"'}col-md-5{'"'}></div>");
                    //        sBuilder.AppendLine($@"    </div>");
                    //        sBuilder.AppendLine($@"</div>");
                    //    }
                    //    if (i % 4 == 0 && i > 1)
                    //    {
                    //        sBuilder.AppendLine($@"</div><div class={'"'}row img-margin-left{'"'}>");
                    //    }
                    //}
                    //sBuilder.AppendLine($@"</div>");
                }
                dto.Banner = sBuilder.ToString();
                #endregion                
                dto.BannerUrls = oldCourse.Banner;
                dto.BannerUrls_s = oldCourse.Banner_s ?? oldCourse.Banner;
                dto.Detail = oldCourse.Detail;
                dto.NewUserExclusive = oldCourse.NewUserExclusive;
                dto.LimitedTimeOffer = oldCourse.LimitedTimeOffer;
                dto.SetTop = oldCourse.SetTop ?? false;

                var propList = _mediator.Send(new QueryPropertyInfoByCourseId() { CourseId = (Guid)id }).Result;
                dto.PropInfoList = propList;

                var listSuppliers = _mediator.Send(new SelectItemsQuery() { Type = 9 }).Result;//供应商下拉框
                dto.ListSuppliers = listSuppliers;

                //课程分销信息
                dto.DrpInfo = _mediator.Send(new QueryDrpInfoByCourseId() { CourseId = dto.Id }).Result ?? new CourseDrpInfo();
                dto.LastOffShelfTime = oldCourse.LastOffShelfTime;
                dto.LastOnShelfTime = oldCourse.LastOnShelfTime;

                //购前须知
                dto.ListNotices = _mediator.Send(new QueryCourseNoticesByCId() { CourseId = dto.Id }).Result;
                dto.ListNoticesJson = JsonSerializationHelper.Serialize(dto.ListNotices);
                //商品分类
                var CommodityData = new List<Organization.Appliaction.OrgService_bg.ResponseModels.BgMallFenleisLoadQueryResult>();
                var CommodityTypes = string.IsNullOrEmpty(oldCourse.CommodityTypes) ? new List<int>() : JsonSerializationHelper.JSONToObject<List<int>>(oldCourse.CommodityTypes);
                foreach (var item in CommodityTypes)
                {
                    var data = await _mediator.Send(new BgMallFenleisLoadQuery { Code = item, ExpandMode = 2 });
                    if (data != null)
                    {
                        CommodityData.Add(data);
                    }
                }
                dto.ListOldCommodityTypes = CommodityData;
            }
            //大课信息            
            dto.BigCoursesList = _mediator.Send(new QueryBigCourseInfoByCourseId() { CourseId = dto.Id }).Result;
            ViewBag.OrgeEvltCrawlerUploadUrl = config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];
            ViewBag.CashbackTypeList = EnumUtil.GetSelectItems<CashbackTypeEnum>();//返现类型枚举  
            ////平级佣金只有元

            //ViewBag.PJCashbackTypeList = EnumUtil.GetSelectItems<CashbackTypeEnum>();//返现类型枚举  
            dto.ListOrgs = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;//机构下拉框  

            dto.ListSubjects = _mediator.Send(new SelectItemsQuery() { Type = 8, OtherCondition = "1" }).Result;//科目分类
            dto.ListGoods = _mediator.Send(new SelectItemsQuery() { Type = 8, OtherCondition = "14" }).Result;//好物分类


            // 运费
            if (!isadd && dto.Type == CourseTypeEnum.Goodthing.ToInt()) dto.Freights = await _mediator.Send(new QueryFreightByCourseId { CourseId = dto.Id });
            dto.Freights ??= new FreightItemDto[0];
            ViewBag.FreightCitys = await _mediator.Send(new QueryFreightCitys4Course { });

            // 不发货地区
            if (!isadd && dto.Type == CourseTypeEnum.Goodthing.ToInt())
            {
                var bls = await _mediator.Send(new QueryFreightBlackListByCourseId { CourseId = dto.Id });
                ViewBag.FreightBlackList = bls?.Citys?.Length > 0 ? new[] { bls } : new FreightBlackListDto[0];
            }
            else ViewBag.FreightBlackList = new FreightBlackListDto[0];

            ViewBag.isadd = isadd;

            //一级分类
            ViewBag.Setp1 = await _mediator.Send(new BgMallFenleisLoadQuery { ExpandMode = 2 });

            return View(dto);
        }

        /// <summary>
        /// 获取供应商的(多)地址s
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSupplierAddresses(Guid suppid)
        {
            var r = await _mediator.Send(new SelectItemsQuery { Type = 10, SupplierId = suppid });
            return Json(Res2Result.Success(r));
        }

        /// <summary>
        /// 保存(新增/编辑)课程信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveCourse(SaveCourseCommand0 request)
        {
            var userId = HttpContext.GetUserId();
            request.UserId = userId;
            var result = await _mediator.Send(request);
            return Json(result);
        }

        #endregion

        #region 机构分类~机构联动

        /// <summary>
        /// 机构分类、机构联动数据
        /// </summary>
        /// <param name="orgTypeId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ChangeCourseData(Guid orgTypeId)
        {
            var result = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;//机构        
            return Json(result);
        }

        #endregion

        #region 商品分类--具体分类联动
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeId">分类Id(1-课程；2-好物)</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GoodsOrSubjectSelectList(int typeId)
        {

            string otherCondition = typeId == 1 ? "1" : "14";//KeyValue.type=1-课程科目分类、KeyValue.type=14-好物分类
            var result = _mediator.Send(new SelectItemsQuery() { Type = 8, OtherCondition = otherCondition }).Result;//机构下拉框             
            return Json(result);
        }

        #endregion

        /// <summary>
        /// 课程上下架
        /// </summary>
        /// <param name="id">课程Id</param>
        /// <param name="orgId">机构Id</param>
        /// <param name="status">订单状态</param>
        /// <param name="subject">科目Id</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OffOrOnTheShelf(Guid id, Guid orgId, string status, int? subject)
        {
            Guid userId = HttpContext.GetUserId();
            int status_ = (int)(status == "上架" ? CourseStatusEnum.Ok : CourseStatusEnum.Fail);
            var oldCourse = (Course)(_mediator.Send(new QueryCourseById() { CourseId = id, IgnoreStatus = true }).Result.Data);
            var result = _mediator.Send(new ChangeSingleFieldCommand()
            {
                Id = id,
                FieldName = "Status",
                FieldValue = status_,
                UserId = userId,
                TableName = "Course",
                BatchDelCache = new List<string>() {
                    $"org:course:courseid:{id}"
                    ,$"org:course:courseid:{id}:*"

                    ,$"org:organization:orgid:{orgId}"
                    ,$"org:organization:orgid:{orgId}:*"//pc单个机构计数-课程数量

                    ,$"org:course:courseid:{id}:pc:relatedcourses"//pc课程详情-机构(相关)课程s
                    ,$"org:organization:orgid:{orgId}:pc:relatedcourses"//pc机构详情-机构(相关)课程s

                    ,"org:courses:*"
                    ,"org:*:relatedEvlts:*"//pc评测详情-相关评测s、pc课程详情-相关评测s、pc机构详情-相关评测s
                    ,"org:evlt:info:*"//评测详情有机构信息、课程信息、专题信息
                }
            }).Result;
            if (result.Succeed)
            {
                //上下架成功，则更新对应机构下的科目
                _mediator.Send(new UpdateOrgSubjectByCourseCommand() { CourseId = id, OrgId = orgId, NewSubject = subject, OperationType = status == "上架" ? 1 : 3 });

                // add user log
                HttpContext.RequestServices.GetService<SmLogUserOperation>()
                    .SetUserId(userId).SetClass(nameof(CoursesController)).SetMethod(nameof(OffOrOnTheShelf))
                    .SetParams("_", new { id, orgId, status })
                    .SetOldata("course", oldCourse)
                    .SetTime(DateTime.Now);
            }
            return Json(result);
        }

        #endregion

        #region 课程详情
        /// <summary>
        /// 根据guid-id获取课程信息
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetCourseById(Guid courseId)
        {
            var result = _mediator.Send(new QueryCourseById() { CourseId = courseId }).Result;
            return Json(result);
        }
        #endregion

        #region 专用于已有课程展示
        ///// <summary>
        ///// 根据guid-id获取课程信息
        ///// </summary>
        ///// <param name="courseId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public IActionResult ShowCourseById(Guid courseId)
        //{
        //    var r = (Course)_mediator.Send(new QueryCourseById() { CourseId = courseId }).Result.Data;
        //    var model = new ShowDBCourseModel();
        //    model.AgeRange = r.Minage + "-" + r.Maxage + " 岁";
        //    model.Subject = EnumUtil.GetDesc(((SubjectEnum)r.Subject));
        //    model.Duration = (r.Duration==null || r.Duration<=0)?"未收录":r.Duration + " 分钟"; 
        //    var listModes = JsonSerializationHelper.JSONToObject<List<int>>(r.Mode);
        //    if (listModes.Any())
        //    {
        //        var strModes = new List<string>();
        //        foreach (var item in listModes)
        //        {
        //            strModes.Add(EnumUtil.GetDesc(((TeachModeEnum)item)));
        //        }
        //        model.Modes = string.Join(',', strModes);
        //    }
        //    else
        //    {
        //        model.Modes = "未收录";
        //    }

        //    return Json(model);
        //}
        #endregion

        #region 课程--留资

        /// <summary>
        /// 课程留资--列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CoursePurchase(SearchCoursesOrderQuery query)
        {
            if (query.SearchType == 1)//视图
            {
                query.PageIndex = 1; query.PageSize = 10;
                var data = _mediator.Send(query).Result;

                #region 查询下拉框

                //订单状态枚举  
                var orderstatus = EnumUtil.GetSelectItems<OrderStatus>();
                orderstatus.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.OrderStatusList = orderstatus;

                //约课状态枚举
                var bookingCourse = EnumUtil.GetSelectItems<BookingCourseStatusEnum>();
                bookingCourse.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.BookingCourse = bookingCourse;

                //机构下拉框  
                var orglist = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;
                orglist.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.OrgList = orglist;

                //科目下拉框
                var listSubject = EnumUtil.GetSelectItems<SubjectEnum>();
                listSubject.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.SubjectList = listSubject;

                //机构类型下拉框
                var listOrgType = EnumUtil.GetSelectItems<OrgCfyEnum>();
                listOrgType.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.listOrgType = listOrgType;

                #endregion

                return View(data);
            }
            else//分页json
            {
                var data = _mediator.Send(query).Result;
                return Json(new { data = data, isOk = true });
            }
        }

        /// <summary>
        /// 【留资】关闭课程订单
        /// </summary>
        /// <param name="courseid">课程Id</param>
        /// <param name="id">订单Id</param>
        /// <param name="phonenumber">留资用户手机号码</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CloseOrder(Guid courseid, Guid id, string phonenumber)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new ChangeSingleFieldCommand()
            {
                Id = id,
                FieldName = "IsValid",
                FieldValue = 0,
                UserId = userId,
                TableName = "Order",
                BatchDelCache = new List<string>()
                {
                    $"org:course:coursebuy:courseid:{courseid}:phonenumber:{phonenumber}"
                }
            }).Result;
            //var result = _mediator.Send(new CloseOrderCommand(){ Id = id , UserId=userId }).Result;
            return Json(result);
        }

        /// <summary>
        /// 【留资】课程订单状态变更
        /// </summary>
        /// <param name="courseid">课程Id</param>
        /// <param name="id">订单Id</param>
        /// <param name="nextstatus">订单状态</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeOrderStatus(Guid courseid, Guid id, string nextstatus)
        {
            Guid userId = HttpContext.GetUserId();
            int? status = nextstatus == "发货" ? (int)OrderStatus.Delivered : nextstatus == "退货" ? (int)OrderStatus.Returning : nextstatus == "已退货" ? (int)OrderStatus.Returned : -1;
            var result = _mediator.Send(new ChangeOrderStatusCommand()
            {
                Id = id,
                CourseId = courseid,
                Status = (int)status,
                UserId = userId
            }).Result;
            return Json(result);
        }

        #endregion

        #region 课程--order

        #region Query
        /// <summary>
        /// 根据机构Id，获取机构下的课程列表
        /// </summary>
        /// <param name="orgId">机构Id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CourseSelectList(Guid orgId)
        {
            var result = _mediator.Send(new SelectItemsQuery() { Type = 3, Id = orgId }).Result;//机构级联课程下拉框             
            return Json(result);
        }

        /// <summary>
        /// 根据机构Id，获取机构下的课程列表Json格式
        /// </summary>
        /// <param name="orgId">机构Id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CourseSelectListJson(Guid orgId)
        {
            var result = _mediator.Send(new SelectItemsQuery() { Type = 3, Id = orgId }).Result;//机构级联课程下拉框             
            return Json(ResponseResult.Success(result));
        }



        /// <summary>
        /// 课程订单--列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetOrders(SearchOrdersQuery query)
        {
            if (query.SearchType == 1)//视图
            {
                query.PageIndex = 1; query.PageSize = 10;
                var data = _mediator.Send(query).Result;

                #region 查询下拉框

                //订单状态枚举  
                var orderstatus = EnumUtil.GetSelectItems<OrderStatusV2>();
                orderstatus.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.OrderStatusList = orderstatus;

                //约课状态枚举
                var bookingCourse = EnumUtil.GetSelectItems<BookingCourseStatusEnum>();
                bookingCourse.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.BookingCourse = bookingCourse;

                //机构下拉框  
                var orglist = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;
                orglist.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.OrgList = orglist;

                //科目下拉框
                var listSubject = EnumUtil.GetSelectItems<SubjectEnum>();
                listSubject.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.SubjectList = listSubject;

                //机构类型下拉框
                var listOrgType = EnumUtil.GetSelectItems<OrgCfyEnum>();
                listOrgType.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.listOrgType = listOrgType;

                //商品分类枚举  
                var courseType = EnumUtil.GetSelectItems<CourseTypeEnum>();
                courseType.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.CourseTypeList = courseType;

                //供应商下拉框  
                var supplierlist = _mediator.Send(new SelectItemsQuery() { Type = 9 }).Result;
                orglist.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
                ViewBag.SupplierList = supplierlist;

                //快递公司下拉框
                var coms = GetCompanys().Result;
                ViewBag.Companys = coms;

                #endregion

                return View(data);
            }
            else//分页json
            {
                var data = _mediator.Send(query).Result;
                return Json(new { data = data, isOk = true });
            }
        }


        /// <summary>
        /// 课程订单--列表Json
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetOrdersJson(SearchOrdersQuery query)
        {
            var data = _mediator.Send(query).Result;
            return Json(ResponseResult.Success(data));
        }


        /// <summary>
        /// 课程订单--筛选项
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetOrdersSelectJson()
        {
            //订单状态枚举  
            var orderstatus = EnumUtil.GetSelectItems<OrderStatusV2>();
            orderstatus.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });

            //约课状态枚举
            var bookingCourse = EnumUtil.GetSelectItems<BookingCourseStatusEnum>();
            bookingCourse.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });

            //机构下拉框  
            var orglist = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;
            orglist.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });

            //供应商
            var supplierlist = _mediator.Send(new SelectItemsQuery() { Type = 9 }).Result;
            supplierlist.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });

            //科目下拉框
            var listSubject = EnumUtil.GetSelectItems<SubjectEnum>();
            listSubject.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });



            //机构类型下拉框
            var listOrgType = EnumUtil.GetSelectItems<OrgCfyEnum>();
            listOrgType.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });


            //商品分类枚举  
            var courseType = EnumUtil.GetSelectItems<CourseTypeEnum>();
            courseType.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });



            //快递公司下拉框
            var coms = GetCompanys().Result;


            return Json(ResponseResult.Success(
                new
                {
                    OrderStatus = orderstatus,
                    BookingCourse = bookingCourse,
                    OrgList = orglist,
                    ListSubject = listSubject,
                    ListOrgType = listOrgType,
                    CourseType = courseType,
                    Coms = coms,
                    Supplierlist = supplierlist
                }));
        }


        /// <summary>
        /// 根据订单Id，获取订单详情
        /// </summary>
        /// <param name="ordId">订单Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(Guid ordId)
        {
            var result = await _mediator.Send(new OrderDetailsByOrdIdQuery { OrderId = ordId });
            var refundResult = await _mediator.Send(new GetOrderRefundDetilsByOrderIdQuery { OrderId = ordId });

            return Json(ResponseResult.Success(new
            {
                Send = result,
                Refund = refundResult
            }));
        }


        /// <summary>
        /// 获取订单快递
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IActionResult GetOrderLogistics(Guid orderId)
        {
            return Json(ResponseResult.Success());
        }

        #endregion

        #region Update
        /// <summary>
        /// 批量发货
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BatchSendGoods(BatchSendGoodsCommand request)
        {
            Guid userId = HttpContext.GetUserId();
            request.UserId = userId;
            var result = _mediator.Send(request).Result;
            return Json(result);
        }

        /// <summary>
        /// 取消/关闭课程订单
        /// </summary>
        /// <param name="ordId">订单id</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CacleOrCloseOrder(Guid ordId)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new ChangeSingleFieldCommand()
            {
                Id = ordId,
                FieldName = "status",
                FieldValue = OrderStatusV2.Cancelled.ToInt(),
                UserId = userId,
                TableName = "Order",
                BatchDelCache = new List<string>()
                {
                    //todo
                }
            }).Result;
            return Json(result);
        }

        /// <summary>
        /// 更新订单--机构反馈
        /// </summary>
        /// <param name="ordId">订单id</param>
        /// <param name="systemRemark">机构反馈</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateOrderInfo(Guid ordId, string systemRemark)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new UpdateOrderInfoCommand()
            {
                OrderId = ordId,
                SystemRemark = systemRemark,
                UserId = userId
            }).Result;
            return Json(result);
        }

        /// <summary>
        /// 更新订单--机构反馈
        /// </summary>
        /// <param name="ordId">订单id</param>
        /// <param name="systemRemark">机构反馈</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateOrderInfoJson(Guid ordId, string systemRemark)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new UpdateOrderInfoCommand()
            {
                OrderId = ordId,
                SystemRemark = systemRemark,
                UserId = userId
            }).Result;

            return Json(result ? ResponseResult.Success() : ResponseResult.Failed());
        }



        /// <summary>
        /// 订单约课状态变更
        /// </summary>
        /// <param name="ordid">订单Id</param>
        /// <param name="appointmentStatus">约课状态</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeAppointmentStatus(Guid ordid, int appointmentStatus)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new ChangeAppointmentStatusCommand()
            {
                OrdId = ordid,
                AppointmentStatus = appointmentStatus,
                UserId = userId
            }).Result;
            return Json(result);
        }


        /// <summary>
        /// 退款并更新订单状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ResponseResult RefundAndChangeOrderStatus(BackGroundRefundCommand request)
        {

            request.Auditor = HttpContext.GetUserId();
            //request.RefundApiUrl = $"{config["AppSettings:wxpay:baseUrl"]}/api/PayOrder/Refund";
            //var paykey = config["AppSettings:wxpay:paykey"];
            //var system = config["AppSettings:wxpay:system"];
            //request.UserId = HttpContext.GetUserId();
            //request.PayKey = paykey;
            //request.System = system;
            var result = _mediator.Send(request).Result;
            if (result)
            {
                return ResponseResult.Success("退款成功");
            }
            else
            {
                return ResponseResult.Failed("退款失败");
            }
        }


        /// <summary>
        /// 保存发货
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveSendGoodsJson([FromBody] SendGoodsCommand request)
        {
            Guid userId = HttpContext.GetUserId();
            request.Creator = userId;

            var result = _mediator.Send(request).Result;
            return Json(result);
        }


        /// <summary>
        /// 保存发货旧后台发货
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveSendGoods(SendGoodsCommand request)
        {
            Guid userId = HttpContext.GetUserId();
            request.Creator = userId;

            var result = _mediator.Send(request).Result;
            return Json(result);
        }


        #region old
        ///// <summary>
        ///// 物流详情
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public IActionResult LogisticsDetails(QueryLogisticsDetails query)
        //{
        //    query.LogisticeApi = config[Consts.LogisticsDetailsUrl];
        //    var result = _mediator.Send(query).Result;
        //    ViewBag.Succeed = result.Succeed;
        //    if (result.Succeed)
        //    {
        //        var data =(LogisticeInfo) result.Data;
        //        ViewBag.LogisticsDetails = data;
        //        ViewBag.LogisticsItems = data.Items;
        //    }
        //    else
        //    {
        //        ViewBag.ErrMesg = result.Msg;
        //    }

        //    return PartialView();
        //} 
        #endregion


        #endregion

        #endregion

        #region 课程下的商品库存销量查看
        /// <summary>
        /// 库存销量查看
        /// </summary>
        /// <param name="courseid">课程Id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CourseGoodsInfo(Guid courseid)
        {
            ViewBag.Ths = 0;
            ViewBag.Trs = null;
            var response = _mediator.Send(new QueryGoodsStockByCId() { CourseId = courseid }).Result;
            if (response.Any())
            {
                ViewBag.Ths = response[0].Items.Count;
                ViewBag.Trs = response;
            }
            return PartialView();
        }
        #endregion

        #region 快递公司信息        
        /// <summary>
        /// 获取快递公司s和编码s
        /// </summary>
        /// <returns></returns>       
        public async Task<KeyValuePair<string, string>[]> GetCompanys()
        {
            var r = (await _mediator.Send(KuaidiServiceArgs.GetCompanyCodes())).GetResult<KeyValuePair<string, string>[]>();
            return r;
        }

        /// <summary>
        /// 物流详情
        /// </summary>
        /// <dhCode>兑换码</dhCode>
        /// <nu>快递单号</nu>  
        /// <companyCode>快递公司编号</companyCode>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LogisticsDetails(string dhCode, string nu, string companyCode)
        {
            var result = _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery() { Com = companyCode, Nu = nu }).Result;

            ViewBag.Succeed = false;
            ViewBag.DHCode = dhCode;
            if (result.Errcode == 0)
            {
                var data = result;
                ViewBag.LogisticsDetails = data;
                ViewBag.LogisticsItems = data?.Items?.ToList();
                ViewBag.Succeed = true;
            }
            else
            {
                ViewBag.ErrMesg = result.Errmsg;
            }

            return PartialView();
        }





        /// <summary>
        /// 检测物流单号是否正确
        /// </summary>
        /// <nu>快递单号</nu>  
        /// <companyCode>快递公司编号</companyCode>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> CheckNu(string nu, string companyCode)
        {
            var com = (await _mediator.Send(KuaidiServiceArgs.CheckNu(nu, companyCode))).Result;
            if (com == null) return false;
            else return true;

            //var result =  _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery() { Com = companyCode, Nu = nu }).Result;

            //return result.Errcode==0? Json(true): Json(false);
        }


        /// <summary>
        /// 检测物流单号是否正确
        /// </summary>
        /// <nu>快递单号</nu>  
        /// <companyCode>快递公司编号</companyCode>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseResult> CheckNuJson(string nu, string companyCode)
        {
            var com = (await _mediator.Send(KuaidiServiceArgs.CheckNu(nu, companyCode))).Result;
            if (com == null) return ResponseResult.Failed();
            else return ResponseResult.Success();

            //var result =  _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery() { Com = companyCode, Nu = nu }).Result;

            //return result.Errcode==0? Json(true): Json(false);
        }


        /// <summary>
        /// 获取订单物流详情
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ResponseResult> OrderLogistics([FromQuery] OrderLogisticsByOrderIdQuery query)
        {
            var res = await _mediator.Send(query);
            res.Companys = GetCompanys().Result;

            return ResponseResult.Success(res);
        }


        [HttpPost]
        public ResponseResult AddOrderLogistics([FromBody] AddOrderLogisticsCommand command)
        {
            Guid userId = HttpContext.GetUserId();
            command.UserId = userId;
            var res = _mediator.Send(command).Result;

            return res ? ResponseResult.Success() : ResponseResult.Failed();
        }



        #endregion

        #region 兑换码管理

        /// <summary>
        /// 随机获取一个兑换码
        /// </summary>
        /// <param name="courseid"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetDHCode(Guid orderid, Guid courseid)
        {
            try
            {
                var response = _mediator.Send(new QuerySingleRedeemCode() { CourseId = courseid, OrderId = orderid })?.Result;
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(null);
            }

        }
        /// <summary>
        /// 随机获取兑换码
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="courseid"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetDHCodeJson(Guid orderid, Guid courseid)
        {
            try
            {
                var response = _mediator.Send(new QuerySingleRedeemCode() { CourseId = courseid, OrderId = orderid })?.Result;
                return Json(ResponseResult.Success(response));
            }
            catch (Exception ex)
            {
                return Json(ResponseResult.Success(null));
            }

        }


        /// <summary>
        /// 兑换码管理-视图
        /// </summary>
        /// <id>课程Id(页面初始条件)</id>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DHCodeManage(Guid id)
        {
            ViewBag.CourseId = id;
            return View();
        }

        /// <summary>
        /// 【3、兑换记录分页api】根据课程Id，获取兑换码兑换记录
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ExchangeList(Guid courseId, int pageIndex, int pageSize)
        {
            var request = new SearchExchangesQuery()
            {
                CourseId = courseId,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            var result = _mediator.Send(request).Result;
            return Json(result);

        }

        /// <summary>
        /// 4.1、展示模板内容及兑换码信息 
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ShowMsgTemplateAndDHCodeInfo(Guid courseId)
        {

            var request = new ShowMsgDHCodeQuery()
            {
                CourseId = courseId
            };
            var result = _mediator.Send(request).Result;
            return Json(result);

        }

        /// <summary>
        /// 4.1、(新增/编辑)保存模板action 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveMsgTemplate(SaveMsgTemplateCommand cmd)
        {

            var result = _mediator.Send(cmd).Result;
            return Json(result);

        }


        /// <summary>
        /// 导出兑换记录列表到excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportExchange(Guid cid)
        {
            var id = await _mediator.Send(new ExportExchangesCommand() { CourseId = cid });
            return Json(string.IsNullOrEmpty(id) ? ResponseResult.Failed("系统繁忙") : ResponseResult.Success(id, null));
        }

        /// <summary>
        /// excel导入兑换码
        /// </summary>
        /// <cid>课程Id</cid>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddRedeemCodeFromExcel(Guid cid)
        {
            var excelfile = HttpContext.Request.Form.Files[0];

            if (excelfile.Length > 1024.0 * 1024 * 100) // > 100M
            {
                return Json(new
                {
                    isOk = false,
                    msg = "excel超过100M了, 请分割",
                });
            }
            try
            {
                var uid = HttpContext.GetUserId();

                using (var excel = new MemoryStream((int)excelfile.Length))
                {
                    await excelfile.CopyToAsync(excel);

                    await _mediator.Send(new ExcelImportRedeemCodeCommand { Excel = excel, UserId = uid, CourseId = cid });

                    return Json(new { isOk = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isOk = false,
                    msg = ex.Message,
                });
            }
        }

        /// <summary>
        /// 释放订单【占用】的兑换码
        /// 【占用:订单只是绑定了兑换码，尚未发送】
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ReleaseOccupiedCode(Guid cid)
        {
            var result = _mediator.Send(new ReleaseOccupiedCodeCommand() { CourseId = cid, }).Result;
            return Json(result);
        }


        #endregion


        /// <summary>
        /// 统计页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Total()
        {
            return View();
        }
    }
}
