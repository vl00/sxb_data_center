using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.Service.Evaluations;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.OrgService_bg.Evaluations;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Domain;
using Microsoft.Extensions.Configuration;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.DependencyInjection;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.Course;
using iSchool.Organization.Appliaction.Service;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 评测管理
    /// </summary>
    public class EvaluationController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration config;        
        string evltTableName = "Evaluation";

        public EvaluationController(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator;
            this.config = config;    
        }

        #region 评测内容管理

        /// <summary>
        /// 评测列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="query">查询条件</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(SearchEvalListQuery query, int page=1)
        {           
            int pageSize = 10;
            query.PageSize = pageSize;
            query.PageIndex = page < 1 ? 1 : page;
            var data = _mediator.Send(query).Result;
            var datasAsIPagedList = new StaticPagedList<EvalItem>(data.list, data.PageIndex, data.PageSize, data.PageCount);

            #region ViewBag

            ViewBag.SubjectList = EnumUtil.GetSelectItems<SubjectEnum>();//相关科目

            ViewBag.AuditStatusList = EnumUtil.GetSelectItems<EvltAuditStatusEnum>();//审核状态

            ViewBag.WhetherList = EnumUtil.GetSelectItems<CooperationStatus>(); // 是否类型下拉框

            //查询条件
            string queryJson = query == null ? null : JsonSerializationHelper.Serialize(query);
            ViewBag.queryJson = queryJson;
            ViewBag.query = query;
            #endregion

            return View(datasAsIPagedList);
        }

        /// <summary>
        /// 上下架
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="status">1:上架;2:下架</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OffOrOnTheShelf(Guid id, int status)
        {
            Guid userId = HttpContext.GetUserId();
            status = status == 1 ? (int)EvaluationStatusEnum.Fail : (int)EvaluationStatusEnum.Ok;//如果是上架，则变为下架；否则反之      
            var result = _mediator.Send(new ChangeSingleFieldCommand() 
            { 
                TableName= evltTableName
                , UserId=userId
                , Id = id
                , BatchDelCache = null
                , FieldName ="status"
                , FieldValue = status }).Result;
            //var result = _mediator.Send(new SetOffOrOnTheShelfCommand() { BatchDelCache=null, Id=id, Status=status, TableName= evltTableName, UserId= userId }).Result;
            _mediator.Send(new ClearEvltCachesCommand() { Id=id, Type=(int)EvltOperationType.Update });
            return Json(result);

            #region 此方法清缓存慢
            //var result = _mediator.Send(new SetOffOrOnTheShelfCommand() { BatchDelCache=null, Id=id, Status=status, TableName= evltTableName, UserId= userId }).Result;
            //if (result.Succeed)
            //{
            //    _mediator.Send(new ClearEvltCachesCommand() { Id=id, Type=4  });
            //}
            //return Json(result); 
            #endregion
        }

        /// <summary>
        /// 单字段更新--是否加精|是否纯文字图片
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="fieldandvalue">字段名和值，用&隔开</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditFiel(Guid id, string fieldandvalue)
        {
            Guid userId = HttpContext.GetUserId();
            var strArr = fieldandvalue.Split('&');
            var result = _mediator.Send(new ChangeSingleFieldCommand() { TableName = evltTableName, UserId = userId, Id = id, BatchDelCache=null, FieldName= strArr[0] , FieldValue= strArr[1] == "true" ? true : false }).Result;
            if(result.Succeed) _mediator.Send(new ClearEvltCachesCommand() { Id = id, Type = (int)EvltOperationType.Update });
            return Json(result);
        }

        /// <summary>
        /// 更新科目
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="subject"></param> 
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateSubject(Guid id, int subject)
        {
            var result = _mediator.Send(new UpdateSubjectCommand() { EvltId = id, Subject = subject }).Result;
            if (result.Succeed) _mediator.Send(new ClearEvltCachesCommand() { Id = id, Type = (int)EvltOperationType.Update });
            return Json(result);
        }

        /// <summary>
        /// [评测内容管理-弹窗]--查看
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId">用户Id</param>
        /// <param name="nickName">用户昵称</param>
        /// <param name="mobile">用户手机号</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Indexwin(Guid id,Guid userId, string nickName,string mobile)
        {           
            var result = _mediator.Send(new EvaluationDetailsQuery() {  Id=id, UserId= userId, UserCenterBaseUrl = config[Consts.BaseUrl_usercenter] }).Result;

            result.UserId = userId;
            result.NickName = nickName;
            result.Mobile = mobile;
            
            return PartialView(result);
        }

        /// <summary>
        /// 更新评测官方点赞数
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="shamLikes">官方点赞数</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateEvltShamLikes(Guid id, int shamLikes)
        {          
            var result = _mediator.Send(new UpdateEvltShamLikesByIdCommand() { Id = id, ShamLikes= shamLikes }).Result;
            if (result.Succeed) _mediator.Send(new ClearEvltCachesCommand() { Id = id, Type = (int)EvltOperationType.LikesUpdate });
            return Json(result);
        }


        #region 编辑评测
        /// <summary>
        /// [编辑评测]--展示待编辑内容
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="queryJson"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]

        public IActionResult EvltEditShow(Guid id, string queryJson, int page)
        {
            page=page < 1 ? 1 : page;
            var dto = _mediator.Send(new EvaluationUpdateShow() { Id = id }).Result;
            if (dto == null)
            {
                return RedirectToAction("Index");
            }

            dto.ListSpecials = _mediator.Send(new SelectItemsQuery() { Type = 2 }).Result;//专题下拉框 
            dto.ListOrgs = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;//机构下拉框 

            Guid? orgid = null;
            if (dto.ListEvltBind?.Any() == true) //绑定课程
                orgid = dto.ListEvltBind[0].Orgid;

            dto.ListCourses = orgid != null ? _mediator.Send(new SelectItemsQuery() { Type = 3, Id = orgid }).Result : null;//课程下拉框 
            dto.ListEvltComments = _mediator.Send(new EvltCommentByEvltIdQuery() { EvltId = id, PageIndex = page, PageSize = 5 }).Result;

            #region  old 已有课程
            //if (dto.Courseid != null)//
            //{
            //    try
            //    {
            //        var r = (Course)_mediator.Send(new QueryCourseById() { CourseId = (Guid)dto.Courseid }).Result.Data;
            //        var model = new ShowDBCourseModel();
            //        model.AgeRange = r.Minage + "-" + r.Maxage + " 岁";
            //        model.Subject = r.Subject != null ? EnumUtil.GetDesc((SubjectEnum)r.Subject) : "未收录";
            //        model.Duration = (r.Duration == null || r.Duration <= 0) ? "未收录" : r.Duration + " 分钟";
            //        var listModes = JsonSerializationHelper.JSONToObject<List<int>>(r.Mode);
            //        if (listModes.Any())
            //        {
            //            var strModes = new List<string>();
            //            foreach (var item in listModes)
            //            {
            //                strModes.Add(EnumUtil.GetDesc(((TeachModeEnum)item)));
            //            }
            //            model.Modes = string.Join(',', strModes);
            //        }
            //        else
            //        {
            //            model.Modes = "未收录";
            //        }
            //        dto.ShowDBCourse = model;
            //    }
            //    catch (Exception ex)
            //    {
            //        dto.ShowDBCourse = null;
            //    }

            //} 
            #endregion

            #region url
            if (dto.ListEvaluationItems.Any() == true)
            {
                dto.DicUrlHtml = new Dictionary<int, string>();
                foreach (var item in dto.ListEvaluationItems)
                {
                    StringBuilder sBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(item.Pictures))
                    {
                        var list = JsonSerializationHelper.JSONToObject<List<string>>(item.Pictures);
                        var count = list.Count <= 9 ? list.Count + 1 : 10;
                        sBuilder.AppendLine($@"<div class={'"'}row img-margin-left{'"'} >");
                        for (int i = 1; i <= count; i++)
                        {
                            if (i == count)
                            {
                                //上传按钮
                                sBuilder.AppendLine($@"<div class={'"'}col-md-2{'"'}>");
                                sBuilder.AppendLine($@"    <input type = {'"'}file{'"'} id={'"'}{id}-{item.Type}{'"'} hidden={'"'}hidden{'"'} class={'"'}c_ignore updateFile{'"'} name={'"'}files{'"'} multiple accept = {'"'}jpg,png{'"'} title={'"'}只允许上传Mp4格式的视频!视频大小不能超过40M{'"'} />");
                                sBuilder.AppendLine($@"    <input type = {'"'}button{'"'} id={'"'}uploadlogo-{item.Type}{'"'} style={'"'}width: 100px; height: 100px; font-size: 50px;{'"'} class={'"'}uploadvideo-btn btn  btn-info btn-block c_ignore updateBtn{'"'} data-video={'"'}{id}{'"'} data-input={'"'}InterviewVideos{'"'} value={'"'}+{'"'} />");
                                sBuilder.AppendLine($@"</div>");
                            }
                            else
                            {
                                //图片
                                sBuilder.AppendLine($@" <div class={'"'}col-md-2{'"'}>");
                                sBuilder.AppendLine($@"    <div class={'"'}form-inline{'"'}>");
                                sBuilder.AppendLine($@"        <a id={'"'}deletebutten-{item.Type}{'"'} class={'"'}delrankbtn fa fa-minus-circle deletebutten  text-danger{'"'} data-input={'"'}{list[i - 1]}{'"'}></a>");
                                sBuilder.AppendLine($@"        <img style = {'"'}width:100px;height:100px;{'"'} src={'"'}{list[i - 1]}{'"'} />");
                                sBuilder.AppendLine($@"    </div>");
                                sBuilder.AppendLine($@"    <div class={'"'}form-inline{'"'}>");
                                sBuilder.AppendLine($@"        <div class={'"'}col-md-7{'"'} style={'"'}text-align:center;{'"'}>");
                                sBuilder.AppendLine($@"            <a href = {'"'}javascript:void(0){'"'} data-id={'"'}{'"'} class={'"'}downloadpic text-info{'"'} data-input={'"'}{list[i - 1]}{'"'} >下载</a> ");
                                sBuilder.AppendLine($@"        </div>");
                                sBuilder.AppendLine($@"        <div class={'"'}col-md-5{'"'}></div>");
                                sBuilder.AppendLine($@"    </div>");
                                sBuilder.AppendLine($@"</div>");
                            }
                            if (i % 4 == 0 && i > 1)
                            {
                                sBuilder.AppendLine($@"</div><div class={'"'}row img-margin-left{'"'}>");
                            }
                        }
                        sBuilder.AppendLine($@"</div>");
                    }
                    dto.DicUrlHtml.Add(item.Type, sBuilder.ToString());
                    //ViewBag.UrlHtml = sBuilder.ToString();
                }
            }
            #endregion
            ViewBag.queryJson = queryJson;
            ViewBag.page = 1;
            ViewBag.OrgeEvltCrawlerUploadUrl = config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];
          
            return View(dto);
        }

        /// <summary>
        /// 保存编辑内容SaveEditEvltCommand request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveEditEvlt(UpdateEvaluationCommand request)
        {
            Guid userId = HttpContext.GetUserId();
            request.Modifier = userId;
            var result = _mediator.Send(request).Result;
            if (result.Succeed) _mediator.Send(new ClearEvltCachesCommand() { Id =request.Id, Type = (int)EvltOperationType.BigUpdate });
            return Json(result);
        } 
        #endregion

        #region 新增评测


        /// <summary>
        /// 新增评测界面
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryJson"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]

        public IActionResult AddShow(string queryJson, int page)
        {
            var dto = new AddEvaluationShowDto();

            dto.Id = Guid.NewGuid();

            dto.ListSpecials=_mediator.Send(new SelectItemsQuery() { Type = 2 }).Result;//专题下拉框 
            dto.ListOrgs = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;//机构下拉框 
            //dto.ListCourses= _mediator.Send(new SelectItemsQuery() { Type = 3, Id = dto.Orgid }).Result : null;//课程下拉框 

            #region ViewBag           
            ViewBag.queryJson = queryJson;
            ViewBag.page = page < 1 ? 1 : page;
            ViewBag.OrgeEvltCrawlerUploadUrl = config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];
           
            #endregion

            return View(dto);
        }

        /// <summary>
        /// 新增评测--只有自由模式
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddEvlt(AddEvltCommand request)
        {
            Guid userId = HttpContext.GetUserId();
            request.Creator = userId;
            var result = _mediator.Send(request).Result;
            if (result.Succeed) _mediator.Send(new ClearEvltCachesCommand() { Id = request.Id, Type = (int)EvltOperationType.Add });
            return Json(result);
        }


        #endregion



        #endregion

        #region 审核

        /// <summary>
        /// 审核不通过
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="userId">种草用户Id</param>
        /// <param name="auditRecord"></param>      
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EvltRewardUnPassAudit(Guid id, Guid userId, string auditRecord)
        {
            var _operator = HttpContext.GetUserId();
            var result = await _mediator.Send(new EvltRewardUnPassAuditCommand() { Id = id, Operator = _operator, UserId = userId, AuditRecord = auditRecord });
            return Json(result);
        }

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="userId">种草用户Id</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EvltRewarPassAudit(Guid id, Guid userId, decimal reward)
        {
            Guid _operator = HttpContext.GetUserId();
            var result = _mediator.Send(new EvltRewardPassAuditCommand() { Id = id, Operator = _operator, UserId = userId, Reward = reward }).Result;
            return Json(result);           
        }

        #endregion

        #region 评论-回复管理
        /// <summary>
        /// 根据评测Id，获取评论列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EvaluationCommentList([FromBody] EvltCommentByEvltIdQuery request)
        {           
            var data = _mediator.Send(request).Result;
            return Json(new { data=data, isOk=true });
        }        
        /// <summary>
        /// 更新单条评论
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="fieldandvalue">字段名和值，用&隔开</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateCommentById([FromBody] EvltCommentCommand request)
        {            
            var result = _mediator.Send(request).Result;
            return Json(result);
        }

        /// <summary>
        ///【回复列表】 根据评论Id，获取回复列表(不分页)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ReplyList([FromBody] ReplyByComIdQuery request)
        {
            var data = _mediator.Send(request).Result;
            return Json(new { data = data, isOk = true });
        }

        /// <summary>
        /// 【批量更新回复】
        /// </summary>
        /// <param name="id">评测Id</param>
        /// <param name="fieldandvalue">字段名和值，用&隔开</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BatchUpdateRelpsById([FromBody] BatchReplyCommand request)
        {
            var result = _mediator.Send(request).Result;
            return Json(result);
        }
        #endregion


    }
}
