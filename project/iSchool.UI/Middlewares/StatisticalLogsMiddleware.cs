using iSchool.Authorization;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace iSchool.UI.Middlewares
{
    public class StatisticalLogsMiddleware
    {
        readonly RequestDelegate next;

        public static readonly ConcurrentDictionary<(string, string), string> ActionDescDict;

        static StatisticalLogsMiddleware()
        {
            ActionDescDict = new ConcurrentDictionary<(string, string), string>();

            //获取xml的路径
            var xmlpath = string.Format(@"{0}/{1}.xml", Directory.GetCurrentDirectory(), typeof(StatisticalLogsMiddleware).Assembly.GetName().Name);

            //判断文件是否存在
            if (File.Exists(xmlpath))
            {
                var xmldoc = XElement.Load(new FileStream(xmlpath, FileMode.Open, FileAccess.Read));

                foreach (var node in xmldoc.DescendantsAndSelf().Where(_ => _.Name == "member"))
                {
                    var type = node.Attribute("name")?.Value ?? "";
                    if (type.IndexOf(".Controllers.") > 0 || type.IndexOf(".ApiControllers.") > 0)
                    {
                        if (type.StartsWith("M:"))
                        {
                            //action
                            var controllerName = "Controller.";
                            var start = type.IndexOf(controllerName) + controllerName.Length;
                            var end = type.IndexOf("(");
                            var actionName = (end == -1 ? type.Substring(start) : type.Substring(start, end - start)).ToLower();

                            //controller
                            start = type.IndexOf(".Controllers.") + ".Controllers.".Length;
                            end = type.IndexOf("Controller.", start);
                            end = end != -1 ? end : type.IndexOf(".", start);
                            controllerName = type.Substring(start, end - start).ToLower();

                            //获取action的注释
                            var summaryNode = node.Element("summary");
                            if (summaryNode != null && !string.IsNullOrEmpty(summaryNode.Value) && !ActionDescDict.ContainsKey((controllerName, actionName)))
                            {
                                ActionDescDict.TryAdd((controllerName, actionName), summaryNode.Value.Trim());
                            }
                        }
                    }
                }
            }
        }

        public StatisticalLogsMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, ILog log, IConfiguration configuration)
        {
            try
            {
                //context.Request.Path = "/test/log2";
                //var rd = new RouteValueDictionary();
                //rd.Add("controller", "test");
                //rd.Add("action", "log2");
                //context.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = new RouteData(rd) });

                context.Items["business-id"] = $"{configuration["logs:applicationName"]}_{Guid.NewGuid().ToString("n").Substring(0, 13)}";

                await next(context);

                if (Equals(context.Items["is-mvc-hander"], true))
                {
                    log.Info($"after call and response ok");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                var _ex = ex;
                while (_ex is AggregateException exs)
                {
                    _ex = exs.InnerException;
                    if (_ex == null) break;
                    ex = _ex;
                }

                var code = ex is FnResultException fex ? fex.Code :
                    ex is CustomResponseException cex ? cex.ErrorCode :
                    ex is NoPermissionException nopex ? nopex.Code :
                    ex is NoLoginException ? 401 :
                    context.RequestAborted.IsCancellationRequested ? -1005 :
                    -1;

                if (Equals(context.Items["is-mvc-hander"], true))
                {
                    log.Error(ex ?? _ex, code);
                }
                else
                {
                    log.Error("error may happen on middleware", ex ?? _ex, code);
                }

                if (code == 401)
                {
                    if (context.Request.IsAjaxRequest())
                    {
                        //context.Response.StatusCode = 401;
                        //await context.Response.WriteAsync((ex as NoLoginException)?.LoginUrl);

                        var LoginUrl = ((ex as NoLoginException)?.LoginUrl ?? "").Contains("https://") ? (ex as NoLoginException)?.LoginUrl : (context.Request.Scheme + ":" + (ex as NoLoginException)?.LoginUrl);
                        //var refererUrl = context.Request.Headers["referer"].ToString();
                        LoginUrl = LoginUrl.Remove(LoginUrl.ToLower().IndexOf("?redirect_uri=")) + "?redirect_uri=";

                        var responseResult = new ResponseResult
                        {
                            Msg = "你还未登录，请登录后再来吧",
                            status = ResponseCode.NoLogin,
                            Data = new { LoginUrl = LoginUrl }
                        };
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        var responseText = JsonConvert.SerializeObject(responseResult,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                        await context.Response.WriteAsync(responseText);
                        return;
                    }
                    else if (ex is NoLoginException noLoginException)
                    {
                        context.Response.Redirect(noLoginException.LoginUrl);
                        return;
                    }
                    else
                    {
                        context.Response.Redirect(
                            context.RequestServices.GetService<IOptions<AppSettings>>().Value.LoginUrl +
                            Uri.EscapeDataString(context.Request.GetDisplayUrl()));
                        return;
                    }
                }

                if (code == 403)
                {
#if !DEBUG
                    if (!context.Request.IsAjaxRequest())
                    {
                        await context.Response.WriteAsync(redirect_page("403 没有权限", "/home"), Encoding.UTF8);
                        return;
                    }
#endif
                }

                var o = new { isOk = false, errCode = code, msg = $"{(ex ?? _ex).Message}", stackTrace = $"{(ex ?? _ex).StackTrace}", status = code, succeed = false };
                if (context.Request.IsAjaxRequest()) context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json"; 
                await context.Response.WriteAsync(o.ToJsonString(true), Encoding.UTF8);
            }
        }

        public static IActionResult InvalidModelStateResponse(ActionContext context)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = 400,
            };

            //log error on invalid modelstate
            var log = context.HttpContext.RequestServices.GetService<ILog>();
            log.Fatal("invalid modelstate", new Exception(problemDetails.Errors.ToJsonString(indented: true)), 400);

            var error = string.Join("\r\n\t", problemDetails.Errors.Select(_ => $"{_.Key}: {string.Join("\r\n\t\t", _.Value)}"));
            var result = new BadRequestObjectResult(new { isOk = false, errCode = 400, status = 400, succeed = false, msg = "参数绑定时出错:\r\n\t" + error });

            result.ContentTypes.Add("application/problem+json");
            result.ContentTypes.Add("application/problem+xml");

            return result;
        }

        static string redirect_page(string msg, string url)
        {
            var str = $@"
<html>
<head>
<meta charset=""utf-8"">
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
</head>
<body>
<div>{msg}</div>
<script>
    window.onload = function () {{
        setTimeout(function() {{
            window.location.replace('{url}');
        }}, 5000);
    }};
</script>
</body>
</html>
";
            return str;
        }
    }
}
