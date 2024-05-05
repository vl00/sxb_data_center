using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.Course;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iSchool.UI.ApiControllers
{
    /// <summary>
    /// 机构后台--课程管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class Courses2Controller : Controller
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration config;

        public Courses2Controller(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator;
            this.config = config;

        }

        #region 商品详情(对应旧mvc页/course/AddUpdateShow)
        /// <summary>
        /// 商品详情(对应旧mvc页/course/AddUpdateShow)
        /// </summary>
        /// <param name="id">spu id</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiRes_Courses_GetCourseDetail), 200)]
        public async Task<Res2Result> GetCourseDetail(Guid? id)
        {
            var result = new ApiRes_Courses_GetCourseDetail();
            var isadd = result.isadd = id == null;
            var dto = result.Model = new AddCoursesShowDto();
            await default(ValueTask);

            Course oldCourse = null;
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
                oldCourse = (Course)(_mediator.Send(new QueryCourseById() { CourseId = (Guid)id, IgnoreStatus = true }).Result.Data);
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
                #endregion                
                dto.BannerUrls = oldCourse.Banner;
                dto.BannerUrls_s = oldCourse.Banner_s ?? oldCourse.Banner;
                dto.Detail = oldCourse.Detail;
                dto.NewUserExclusive = oldCourse.NewUserExclusive;
                dto.LimitedTimeOffer = oldCourse.LimitedTimeOffer;
                dto.SetTop = oldCourse.SetTop ?? false;
                dto.SpuLimitedBuyNum = oldCourse.LimitedBuyNum;

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
            }

            //大课信息            
            dto.BigCoursesList = _mediator.Send(new QueryBigCourseInfoByCourseId() { CourseId = dto.Id }).Result;
            result.OrgeEvltCrawlerUploadUrl = config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];
            result.CashbackTypeList = EnumUtil.GetSelectItems<CashbackTypeEnum>();//返现类型枚举  
            ////平级佣金只有元
            //ViewBag.PJCashbackTypeList = EnumUtil.GetSelectItems<CashbackTypeEnum>();//返现类型枚举  

            dto.ListOrgs = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;//机构下拉框  

            dto.ListSubjects = _mediator.Send(new SelectItemsQuery() { Type = 8, OtherCondition = "1" }).Result;//科目分类
            dto.ListGoods = _mediator.Send(new SelectItemsQuery() { Type = 8, OtherCondition = "14" }).Result;//好物分类

            // 运费
            if (!isadd && dto.Type == CourseTypeEnum.Goodthing.ToInt()) dto.Freights = await _mediator.Send(new QueryFreightByCourseId { CourseId = dto.Id });
            dto.Freights ??= new FreightItemDto[0];
            result.FreightCitys = await _mediator.Send(new QueryFreightCitys4Course { });

            // 不发货地区
            if (!isadd && dto.Type == CourseTypeEnum.Goodthing.ToInt())
            {
                var bls = await _mediator.Send(new QueryFreightBlackListByCourseId { CourseId = dto.Id });
                result.FreightBlackList = bls?.Citys?.Length > 0 ? new[] { bls } : new FreightBlackListDto[0];
            }
            else result.FreightBlackList = new FreightBlackListDto[0];

            // 商品3级分类            
            {
                var ctys = string.IsNullOrEmpty(oldCourse?.CommodityTypes) ? null : oldCourse.CommodityTypes.ToObject<int[]>();
                ctys = ctys?.Length > 0 ? ctys : new int[] { 0 };
                dto.ListOldCommodityTypes ??= new List<Organization.Appliaction.OrgService_bg.ResponseModels.BgMallFenleisLoadQueryResult>();
                foreach (var cty in ctys)
                {
                    var data = await _mediator.Send(new BgMallFenleisLoadQuery2 { Code = cty });
                    if (data != null) dto.ListOldCommodityTypes.Add(data);
                }
                // 之前没数据
                if (string.IsNullOrEmpty(oldCourse?.CommodityTypes) && dto.ListOldCommodityTypes.Count > 0)
                {
                    dto.ListOldCommodityTypes[0].SetLs(2, null);
                    dto.ListOldCommodityTypes[0].SetLs(3, null);
                    dto.ListOldCommodityTypes[0].Selected_d2 = null;
                    dto.ListOldCommodityTypes[0].Selected_d3 = null;
                }
            }

            return Res2Result.Success(result);
        }
        //
        class ApiRes_Courses_GetCourseDetail
        {
            /// <summary>true=新增, false=修改</summary>
            public bool isadd;
            /// <summary>主要数据</summary>
            public AddCoursesShowDto Model;
            /// <summary></summary>
            public string OrgeEvltCrawlerUploadUrl;
            /// <summary>返现类型枚举</summary>
            public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> CashbackTypeList;
            /// <summary>运费地区s</summary>
            public (int Code, string Name)[] FreightCitys;
            /// <summary>不发货地区</summary>
            public FreightBlackListDto[] FreightBlackList;
        }
        #endregion

        /// <summary>
        /// 保存(新增/编辑)课程信息
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ResponseResult> SaveCourse(SaveCourseCommand cmd)
        {
            var userId = HttpContext.GetUserId();
            cmd.UserId = userId;
            var result = await _mediator.Send(cmd);
            return result;
        }

        #region sku列表
        /// <summary>
        /// sku列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(Res_GetGoodsInfoList), 200)]
        public async Task<Res2Result> GetGoodsInfoList([FromQuery] SearchGoodsInfoQuery query)
        {
            var data = await _mediator.Send(query); //商品sku列表

            var propItems = await _mediator.Send(new QueryPropertyInfoByCourseId { CourseId = query.CourseId });  //属性-选项信息
            var listSuppliers = await _mediator.Send(new SelectItemsQuery { Type = 9 });//供应商下拉框
            //return Json(new { data = data, propItems = JsonSerializationHelper.Serialize(propItems), listSuppliers = JsonSerializationHelper.Serialize(listSuppliers), isOk = true });

            return Res2Result.Success(new { data, propItems, listSuppliers });
        }
        //
        class Res_GetGoodsInfoList
        {
            public Organization.Appliaction.ViewModels.Courses.CourseGoodsInfo data;
            public List<Organization.Appliaction.ViewModels.Courses.PropertyAndItems> propItems;
            public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> listSuppliers;
        }
        #endregion

        /// <summary>
        /// 保存属性、选项、商品的信息及关系
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ResponseResult> SavePropertyInfo(SavePropertyInfoCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            return result;
        }

        /// <summary>
        /// 品牌分类与品牌下拉框联动
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.Rendering.SelectListItem[]), 200)]
        public async Task<Res2Result> OrgSelectList(int v)
        {
            var result = await _mediator.Send(new SelectItemsQuery { Type = 1, OtherCondition = $"1-{v}" }); //机构下拉框             
            return Res2Result.Success(result);
        }
    }
}
