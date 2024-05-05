using Autofac;
using AutoMapper;
using Enyim.Caching;
using iSchool.Authorization;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Cache;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.Service.Aftersales;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace iSchool.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiTestController : Controller
    {
        AppSettings _appSettings;
        IWebHostEnvironment _webHostEnvironment;

        public ApiTestController(IOptions<AppSettings> appSettings, IWebHostEnvironment webHostEnvironment)
        {
            _appSettings = appSettings.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("/Env")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEnv()
        {
            return new JsonResult(new
            {
                _webHostEnvironment.ApplicationName,
                _webHostEnvironment.EnvironmentName,
                _webHostEnvironment.ContentRootPath
            });
        }

        [HttpGet("{id=123}")]
        [HttpPost("a2")]
        public IActionResult A1(int id) => throw new NotSupportedException();

        [HttpPost(nameof(UploadImgWithidx1986))]
        [AllowAnonymous]
        public async Task<IFnResult> UploadImgWithidx1986()
        {
            try { Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"images/temp")); } catch { }

            var file = Request.Form.Files.FirstOrDefault();
            if (file == null)
            {
                return FnResult.Fail("上传文件不能为空");
            }

            var fid = Request.Form["id"].ToString();
            var index = Convert.ToInt32(Request.Form["index"]);
            var total = Convert.ToInt32(Request.Form["total"]);
            var blockSize = Convert.ToInt32(Request.Form["size"]);
            var ext = Request.Form["ext"];

            if (index == 1 && string.IsNullOrEmpty(fid)) fid = Guid.NewGuid().ToString("n");

            var path = Path.Combine(Directory.GetCurrentDirectory(), $"images/temp/{fid}.{ext}");
            var steam = file.OpenReadStream();
            using (var fs = System.IO.File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek((index - 1) * blockSize, SeekOrigin.Begin);
                await steam.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            if (index < total)
            {
                return FnResult.OK(new
                {
                    id = fid
                    //src = 
                });
            }

            using (var fs = System.IO.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(0, SeekOrigin.Begin);

                var r = upload_img(_appSettings.UploadUrl.FormatWith(fid, $"{fid}.{ext}"), fs);
                if (r.IsOk)
                {
                    return FnResult.OK(new
                    {
                        id = fid,
                        src = r.Data,
                    });
                }
                else return r;
            }

            FnResult<string> upload_img(string url, Stream fs)
            {                
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.Timeout = 60000;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
                req.ContentLength = fs.Length;
                var sr_s = req.GetRequestStream();
                fs.CopyTo(sr_s);
                var res = (HttpWebResponse)req.GetResponse();
                if (req.HaveResponse)
                {
                    if (!((int)res.StatusCode).In(200, 201, 202, 204, 206))
                    {
                        return FnResult.Fail<string>("上传失败", (int)res.StatusCode);
                    }
                    else
                    {
                        var st = res.GetResponseStream();
                        var re = new StreamReader(st);
                        var rez = JToken.Parse(re.ReadToEnd());
                        re.Close();
                        if ((int?)rez["status"] == 0) return FnResult.OK(rez["cdnUrl"].ToString());
                        else return FnResult.Fail<string>("上传失败", 400);
                    }
                }
                else return FnResult.Fail<string>("上传失败", -1);
            }
        }
    }

    [AllowAnonymous]
    public class TestController : Controller
    {
        readonly IMediator _mediator, mediator2;
        readonly IServiceProvider serviceProvider;
        readonly CacheManager cacheManager;
        readonly IMemcachedClient memcachedClient;
        readonly IContainer container;
        readonly ILifetimeScope lifetimeScope;

        public TestController(IMediator mediator, IMediator mediator2,
            IServiceProvider serviceProvider, CacheManager cacheManager, IMemcachedClient memcachedClient,
            IContainer container = null, ILifetimeScope lifetimeScope = null)
        {
            this._mediator = mediator;
            this.mediator2 = mediator2;
            this.serviceProvider = serviceProvider;
            this.cacheManager = cacheManager;
            this.memcachedClient = memcachedClient;
            this.container = container; //null
            this.lifetimeScope = lifetimeScope;
        }

        public IActionResult Gc()
        {
            GC.Collect();
            return new EmptyResult();
        }

        [HttpGet]
        public IActionResult Index([FromServices]ILifetimeScope lifetimeScope2 = null)
        {
            bool? b = null;
            var sp = this.HttpContext.RequestServices;

            b = sp != serviceProvider; 

            b = cacheManager != sp.GetService<CacheManager>();

            b = container == sp.GetService<IContainer>(); //null
            b = lifetimeScope == lifetimeScope2;
            b = lifetimeScope == sp.GetService<ILifetimeScope>();
            b = sp.GetService<IComponentContext>() == lifetimeScope;

            b = _mediator != sp.GetService<IMediator>();
            b = _mediator != lifetimeScope.Resolve<IMediator>();
            b = _mediator != mediator2;

            b = lifetimeScope.Resolve<IUnitOfWork>() == lifetimeScope.Resolve<IUnitOfWork>();

            return View(b);
        }

        /// <summary>
        /// test for throw error
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Error()
        {
            async Task _f()
            {
                await Task.Delay(200);
                throw new Exception("test to throw a error");
            }

            await _f();
            return Json(new { });
        }

        public async Task<IActionResult> Cache()
        {
            var key = @"kkk";
            var t = 60 * 60;

            memcachedClient.Set(key, "123", t);
            await Task.Delay(1000);
            object val = memcachedClient.Get(key);

            cacheManager.Add(key, @"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
");
            await Task.Delay(1000);
            val = cacheManager.Get(key);

            return Json(new { val });
        }

        public IActionResult Log1()
        {
            var strsb = new StringBuilder();
            var log = serviceProvider.GetService<Infrastructure.ILog>();
            log.Debug("test try log"); _ = nameof(FileStream);

            var idy = HttpContext.User?.Identity;
            strsb.AppendLine($"HttpContext.User?.Identity.IsAuthenticated={idy.IsAuthenticated},name={idy.Name}");

            var options = serviceProvider.GetService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var dr = options.TicketDataFormat.Unprotect(HttpContext.Request.Cookies["iSchoolConsoleAuth"]);

            var userinfo = HttpContext.GetUserInfo();
            //var afns = HttpContext.GetUserPlatAllFunctions();
            //var qys = afns.SelectMany(_ => _.Query.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(_ => _.Trim());
            strsb.AppendLine($"userinfo={userinfo.ToJsonString(indented: false)}");

            strsb.AppendLine($"cookie PageQuery={HttpContext.Request.Cookies["PageQuery"]}");
            strsb.AppendLine($"item PageQuery={HttpContext.Items["PageQuery"]}");

            strsb.AppendLine($"has-qx /audit/OfflineSchool = {HttpContext.HasCtrlActQyx("audit", "OfflineSchool")}");
            strsb.AppendLine($"has-qx /audit/OfflineSchool [do] = {HttpContext.HasCtrlActQyx("audit", "OfflineSchool", "do")}");

            return Content(strsb.ToString());
        }

        public IActionResult Log2()
        {
            //var a = Directory.GetFiles(@"C:\Users\vl00\Desktop\");
            var a = Directory.GetFiles(@"\\10.1.0.8\shared-auth-ticket-keys");
            //var a = Directory.GetFiles(@"e:\RG\1");

            return Content(nameof(Log2));
        }

        public IActionResult QX(string cll, string act, [FromQuery]string[] qx)
        {
            var b = HttpContext.HasCtrlActQyx(cll, act, qx);
            var str = $"cll={cll}\nact={act}\nqx={qx.ToJsonString()}\nhas={b}";
            return Content(str);
        }

        public IActionResult nologin()
        {
            return Unauthorized("mmmm");
        }

        public IActionResult GetObj(string type)
        {
            var b = lifetimeScope.TryResolve(Type.GetType(type, true), out var instance);
            if (b) return Json(new { b, instance = instance.GetType() });
            else return Json(new { b });
        }

        public IActionResult map()
        {
            var sp = HttpContext.RequestServices;
            var mapper = sp.GetService<IMapper>();
            var sg0 = new OnlineSchool();
            sg0.Id = Guid.NewGuid();
            sg0.Intro = "qwerty";
            var sg1 = mapper.Map<School>(sg0);
            return Json(sg1);
        }

        //public IActionResult route()
        //{
        //    var routedata = RouteUtils.GetRouteData("/audit/audit");
        //    routedata = RouteUtils.GetRouteData("/test/loG1");
        //    routedata = RouteUtils.GetRouteData("/api/apitest/555"); //
        //    routedata = RouteUtils.GetRouteData("/api/apitest");
        //    routedata = RouteUtils.GetRouteData("/api/apitest/a1");
        //    routedata = RouteUtils.GetRouteData("/api/apitest/a2?id=666", "post");
        //    return Json(routedata);
        //}

        public IActionResult roles(Guid gid, int cty)
        {
            // 数据编辑 1934e1a3-214d-427b-97d6-e1fa95c7b9a8  //0 Account.IDType.CharacterID
            // 数据审核 9ecc51e3-be8f-4412-974d-994b9e088e94
            var rus = new Account().GetAdmins(gid, (Account.IDType)cty);
            // 编辑权限 ac121abc-7e13-4dfc-ad3d-571ae23859e9
            // 审核权限 cde70a87-150f-4454-92d9-46171ec1cdcb  //1 Account.IDType.FunctionID

            return Json(rus.Select(_ => new { _.Id, _.Name, _.Displayname }));
        }

        public IActionResult Log4([FromServices] ILoggerFactory logf, [FromServices] ILogger<CancelFxInfoCommandHandler> log)
        {
            logf.CreateLogger("aaa").LogInformation("log4444");
            log.LogInformation("log4444");
            return Json(1);
        }
    }
}
