using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Download;
using iSchool.Infrastructure.Upload;
using iSchool.Organization.Appliaction.OrgService_bg.Evaluations;
using iSchool.Organization.Appliaction.Service;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using X.PagedList;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 抓取评测管理
    /// </summary>
    public class EvaluationCrawlerController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration config;
       
        readonly AppSettings _appSettings;

        public EvaluationCrawlerController(IMediator mediator, IWebHostEnvironment hostingEnvironment, IConfiguration config,
            IOptions<AppSettings> appSettings)
        {
            _mediator = mediator;
            _hostingEnvironment = hostingEnvironment;
            this.config = config;
            this._appSettings = appSettings.Value;
            
        }

        #region 抓取评测管理
        /// <summary>
        /// 抓取评测管理-列表
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(CaptureEvaluationListQuery query, int page)
        {
            int pageSize = 10;
            query.PageSize = pageSize;
            query.PageIndex = page < 1 ? 1 : page;
            var data = _mediator.Send(query).Result;
            var datasAsIPagedList = new StaticPagedList<CrawlerItem>(data.list, data.PageIndex, data.PageSize, data.PageCount);

            #region ViewBag

            ViewBag.GrabTypeList = EnumUtil.GetSelectItems<GrabTypeEnum>();//类型            

            //查询条件
            string queryJson = query == null ? null : JsonSerializationHelper.Serialize(query);
            ViewBag.queryJson = queryJson;
            ViewBag.query = query;


            ViewBag.page = page;
            #endregion

            return View(datasAsIPagedList);
        }

        /// <summary>
        /// 抓取评测编辑页面
        /// </summary>
        /// <param name="id">抓取评测Id</param>
        /// <param name="queryJson">用于返回列表</param>
        /// <param name="page">用于返回列表</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EditShow(Guid id, string queryJson,int page)
        {
            var dto = _mediator.Send(new CaptureEvaluationDetailsQuery() { Id = id }).Result;
            if (dto == null)
            {
                return RedirectToAction("Index");
            }

            #region url
            StringBuilder sBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(dto.Url))
            {
                var list = JsonSerializationHelper.JSONToObject<List<string>>(dto.Url);
                var count = list.Count <= 9 ? list.Count+1 : 10;
                sBuilder.AppendLine($@"<div class={'"'}row img-margin-left{'"'} >");
                for (int i = 1; i <= count; i++)
                {
                    if (i == count)
                    {
                        //上传按钮
                        sBuilder.AppendLine($@"<div class={'"'}col-md-2{'"'}>");
                        sBuilder.AppendLine($@"    <input type = {'"'}file{'"'} id={'"'}{id}{'"'} hidden={'"'}hidden{'"'} class={'"'}c_ignore updateFile{'"'} name={'"'}files{'"'} multiple accept = {'"'}jpg,png{'"'} title={'"'}只允许上传Mp4格式的视频!视频大小不能超过40M{'"'} />");
                        sBuilder.AppendLine($@"    <input type = {'"'}button{'"'} id={'"'}uploadlogo{'"'} style={'"'}width: 100px; height: 100px; font-size: 50px;{'"'} class={'"'}uploadvideo-btn btn  btn-info btn-block c_ignore updateBtn{'"'} data-video={'"'}{id}{'"'} data-input={'"'}InterviewVideos{'"'} value={'"'}+{'"'} />");
                        sBuilder.AppendLine($@"</div>");
                    }
                    else
                    {
                        //图片
                        sBuilder.AppendLine($@" <div class={'"'}col-md-2{'"'}>");
                        sBuilder.AppendLine($@"    <div class={'"'}form-inline{'"'}>");
                        sBuilder.AppendLine($@"        <a class={'"'}delrankbtn fa fa-minus-circle deletebutten  text-danger{'"'} data-input={'"'}{list[i-1]}{'"'}></a>");
                        sBuilder.AppendLine($@"        <img style = {'"'}width:100px;height:100px;{'"'} src={'"'}{list[i-1]}{'"'} />");
                        sBuilder.AppendLine($@"    </div>");
                        sBuilder.AppendLine($@"    <div class={'"'}form-inline{'"'}>");
                        sBuilder.AppendLine($@"        <div class={'"'}col-md-7{'"'} style={'"'}text-align:center;{'"'}>");
                        sBuilder.AppendLine($@"            <a href = {'"'}javascript:void(0){'"'} data-id={'"'}{'"'} class={'"'}downloadpic text-info{'"'} data-input={'"'}{list[i-1]}{'"'} >下载</a> ");                    
                        sBuilder.AppendLine($@"        </div>");
                        sBuilder.AppendLine($@"        <div class={'"'}col-md-5{'"'}></div>");
                        sBuilder.AppendLine($@"    </div>");
                        sBuilder.AppendLine($@"</div>");
                    }
                    if (i % 4 == 0 && i>1)
                    {
                        sBuilder.AppendLine($@"</div><div class={'"'}row img-margin-left{'"'}>");
                    }
                }
                sBuilder.AppendLine($@"</div>");
            }
            ViewBag.UrlHtml = sBuilder.ToString();

            #endregion

            #region ViewBag

            ViewBag.OrgList = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;//机构下拉框 
            ViewBag.SpecialList = _mediator.Send(new SelectItemsQuery() { Type=2 }).Result;//专题下拉框 
            ViewBag.CourseList = dto.OrgId!=null? _mediator.Send(new SelectItemsQuery() { Type = 3, Id=dto.OrgId }).Result:null;//课程下拉框 

            ViewBag.AgeGroupList = EnumUtil.GetSelectItems<AgeGroup>();//年龄段
            ViewBag.TeachModeList = EnumUtil.GetSelectItems<TeachModeEnum>();//教学模式、上课方式            

            ViewBag.queryJson = queryJson;
            ViewBag.page = page;
            ViewBag.OrgeEvltCrawlerUploadUrl = config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];
            #endregion

            return View(dto);            
        }

        /// <summary>
        /// 发布 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(ReleaseCaptureEvaluationCommand request)
        {           
            var result = _mediator.Send(request).Result;
            if (result.Succeed)
            {
                _mediator.Send(new ClearEvltCachesCommand() { Id = request.Id, Type = (int)EvltOperationType.Add});//影响评论内容
            }
            return Json(result);
            
        }

        /// <summary>
        /// 机构、课程联动数据
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ChangeCourseData(Guid orgId)
        {
            var result=  _mediator.Send(new SelectItemsQuery() { Type = 3, Id = orgId }).Result;//课程下拉框             
            return Json(result);
        }

        /// <summary>
        /// 删除抓取评测
        /// </summary>
        /// <param name="id">抓取评测Id</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DelEvltCrawler(Guid id)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new ChangeSingleFieldCommand()
            {
                Id = id,
                FieldName = "IsValid",
                FieldValue = 0,
                UserId = userId,
                TableName = "EvaluationCrawler",
                BatchDelCache =null
            }).Result;           
            return Json(result);
        }




        #endregion

        #region 图片操作


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Upload(int type, Guid id)
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

                ////获取裁剪参数
                //var json = Request.Form["avatar_data"].ToString();
                //JObject jo = JObject.Parse(json);
                //var result = UploadHelper.TransportImage(file, "logo", id.ToString(),
                //   Convert.ToInt32((float)jo["width"]), Convert.ToInt32((float)jo["height"]), Convert.ToInt32((float)(jo["x"])), Convert.ToInt32((float)(jo["y"])),
                //    out extension, _appSettings.UploadUrl, _hostingEnvironment.WebRootPath + "\\images\\temp\\");
                var url = config[Consts.OrgBaseUrl_UploadUrl].FormatWith($"eval/{id}", $"{Guid.NewGuid()}.png");

                var result = UploadHelper.TransportImage(file, "logo", id.ToString(),
                    out extension,
                    _appSettings.UploadUrl, _hostingEnvironment.WebRootPath + "\\images\\temp\\");

                if (string.IsNullOrEmpty(result.url))
                    return Json(new HttpResponse<string> { State = 500 });
                else
                {
                    ////把图片返回的url入库                    
                    //var list = JsonSerializationHelper.JSONToObject<List<string>>(oldUrls);
                    //if (list==null) list = new List<string>();
                    //list.Add(result);
                    //var dy = new DynamicParameters();
                    //dy.Add("@url", JsonSerializationHelper.Serialize(list));
                    //_mediator.Send(new EvalCrawlerEditByIdWithFieldCommand() { Id = id, Parameters = dy, UpdateSql = " url=@url " });

                    return Json(new HttpResponse<string> { State = 200, Result = result + "?t=" + DateTime.Now.Ticks.ToString() });
                }

            }
            return Json("");
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="picUrl"></param>
        /// <param name="savePath"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Download(string picUrl, string savePath, int timeOut=-1)
        {
            var result= DownloadHelper.DownloadPicture(picUrl, savePath, timeOut);
            if (result)
                return Json(new HttpResponse<string> { State = 200, Message ="下载成功"});
            else
                return Json("");
        }


        #endregion
    }
}
