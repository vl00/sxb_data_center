using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 机构后台--素材管理
    /// </summary>
    public class MaterialLibrarysController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration config;

        public MaterialLibrarysController(IMediator mediator, IWebHostEnvironment hostingEnvironment, IConfiguration config)
        {
            _mediator = mediator;
            _hostingEnvironment = hostingEnvironment;
            this.config = config;

        }

        /// <summary>
        /// 素材管理index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pgp = await _mediator.Send(new MeterialPgListQuery { });
            ViewBag.MeterialPgList = pgp.PageInfo;
            return View();
        }
        /// <summary>
        /// 素材管理index分页
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> IndexPage(MeterialPgListQuery query)
        {
            var pg = await _mediator.Send(query);
            return PartialView("Index.page", pg?.PageInfo);
        }

        /// <summary>
        /// 上架or下架
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpStatus(UpdateMaterialStatusCommand cmd)
        {
            cmd.Userid = HttpContext.GetUserId();
            var b = await _mediator.Send(cmd);
            return Json(ResponseResult.Success(b));
        }

        /// <summary>
        /// 素材新增or编辑
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Detail(Guid? id)
        {
            ViewBag.OrgImgUploadUrl = $"{config["BaseUrls:org-api"]}/api/home/img1";
            ViewBag.OnVideoUploadUrl = $"{config["BaseUrls:org-api"]}/api/home/video1";
            var model = id == null ? new MeterialDetailDto() : await _mediator.Send(new GetMeterialDetailQuery { Id = id.Value });
            ViewBag.id = model == null ? (object)null : model.Id == default ? (object)null : model.Id;
            return View(model);
        }
        /// <summary>
        /// 模糊搜索商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FindCourses(string txt)
        {
            var r = await _mediator.Send(new GetCourses4MeterialQuery { Txt = txt });
            return Json(ResponseResult.Success(r));
        }
        /// <summary>
        /// 素材库中 获取商品图片视频s
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCourseMedias(Guid courseId)
        {
            var r = await _mediator.Send(new GetCourseMedias4MeterialQuery { CourseId = courseId });
            return Json(ResponseResult.Success(r));
        }
        /// <summary>
        /// 保存素材
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] AddorEditMaterialCommand cmd)
        { 
            cmd.Userid = HttpContext.GetUserId();
            var r = await _mediator.Send(cmd);
            return Json(r);
        }
    }
}
