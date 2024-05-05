using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using iSchool.Application.Service;
using iSchool.Application.Service.Totals;
using iSchool.Authorization;
using iSchool.Infrastructure;
using iSchool.UI.Models;
using iSchool.Domain.Modles;
using iSchool.Authorization.Models;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.UI.Modules;

namespace iSchool.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;
        private const int iSchoolDataPlatformId = 1;



        public HomeController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        /// <summary>
        /// index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[AllowAnonymous] 
        public IActionResult Index()
        {
            if (HttpContext.HasCtrlActQyx("total", "index"))
            {
                //管理员
                return Redirect("/total/index");
            }
            if (HttpContext.HasCtrlActQyx("school", "index"))
            {
                //数据录入
                return Redirect("/school/index");
            }
            if (HttpContext.HasCtrlActQyx("audit", "index"))
            {
                //数据审核
                return Redirect("/audit/index");
            }

            throw new NoPermissionException(403, "没有权限");
        }

        /// <summary>
        /// 数据管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Data(int tagType = -1, int? cgy2 = 0)
        {
            tagType = tagType > 1 ? tagType : 2;

            //标签列表
            var tagList = await _mediator.Send(new GetTagListQuery(false) { TagType = tagType });
            ViewBag.TagsJson = JsonSerializationHelper.Serialize(tagList);
            //标签分类
            var tag2cgyList = await _mediator.Send(new GetTag2CgysQuery { Tag1Cgy = tagType });
            ViewBag.Tag2cgyList = tag2cgyList;

            ViewBag.TagType = tagType;
            ViewBag.cgy2 = cgy2;
            return View();
        }

        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="type">详情类型</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Detail(string userId, int type)
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Test()
        {
            return View();
        }

        /// <summary>
        /// 根据parentid和type获取keyvalue的数据
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetKV(int pid = 0, int type = 1)
        {
            try
            {
                var qy = new KVQuery { ParentId = pid, Type = type };

                var vor = this.HttpContext.RequestServices.GetService<IValidator<KVQuery>>();
                if (vor != null)
                {
                    var errors = vor.Validate(qy).Errors;
                    if (errors.Any())
                    {
                        throw new AggregateException(errors.Select(_ => new Exception(_.ErrorMessage)));
                    }
                }

                var o = _mediator.Send(qy).Result;

                return Json(new
                {
                    isOk = true,
                    data = o,
                });
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
        /// 获取生成的xlsx文件
        /// </summary>                
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetXlsx([FromQuery] string id, [FromServices] IConfiguration config)
        {
            var path = Path.Combine(AppContext.BaseDirectory, config["AppSettings:XlsxDir"], $"{id}.xlsx");
            if (!System.IO.File.Exists(path)) return NoContent();
            return this.File(new FileStream(path, FileMode.Open, FileAccess.Read), "application/ms-excel", $"{id}.xlsx");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> DelExpiredFiles([FromBody] string[] paths)
        {
            var now = DateTime.UtcNow;
            var rms = new List<string>();
            await default(ValueTask);

            foreach (var path0 in paths)
            {
                var path = Path.Combine(AppContext.BaseDirectory, path0);
                if (!Directory.Exists(path)) continue;

                foreach (var fp in Directory.EnumerateFiles(path))
                {
                    if ((now - new FileInfo(fp).LastWriteTimeUtc) > TimeSpan.FromHours(5))
                        rms.Add(fp);
                }
            }
            if (rms.Count > 0)
            {
                foreach (var fp in rms)
                {
                    try { System.IO.File.Delete(fp); }
                    catch { }
                }
            }

            return Json(FnResult.OK());
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetCurrentUser()
        {
            var admin = GetAdmin();
            return Json(ResponseResult.Success(admin));
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult Menus()
        {
            var functions = GetAdmin().Character.SelectMany((s, funs) =>
            {
                return s.Function.Where(f => f.PlatformId == iSchoolDataPlatformId);
            });
            var points = MenuPoints();
            var menus = functions.Select(s => new MenuRespDto()
            {
                MenuId = s.Id,
                MenuName = s.Name,
                MenuCode = s.Controller,
                Points = points.Where(point => point.MenuId == s.Id).ToList()
            }).ToList();
            return Json(ResponseResult.Success(menus));

        }


        public List<MenuPointDto> MenuPoints()
        {
            var permission = new Permission();
            var userid = HttpContext.GetUserId();
            var querys = permission.GetAllQueryByPlatformID(iSchoolDataPlatformId, userid);

            var points = querys.Where(s => s.Id != Guid.Empty).Select(s => new MenuPointDto
            {
                MenuId = s.FunctionId,
                PointName = s.Name,
                PointCode = s.Selector
            }).ToList();

            return points;
        }

        public AdminInfo GetAdmin()
        {
            var admin = new iSchool.Authorization.Account().Info(HttpContext) ??
         new AdminInfo()
         {
             Id = Guid.Parse("3c7bd740-086a-534a-a192-2cc6b410bf40"),
             Name = "DevTester",
             Character = new List<Character>()
         };
            return admin;
        }


    }
}
