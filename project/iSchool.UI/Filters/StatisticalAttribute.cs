using iSchool.Infrastructure;
using iSchool.UI.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.UI.Filters
{
    public class StatisticalAttribute : ActionFilterAttribute, IExceptionFilter
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext; 
            var log = httpContext.RequestServices.GetService<ILog>();
            var routeData = httpContext.GetRouteData();

            var controllerName = routeData.Values["controller"].ToString().ToLower();
            var actionName = routeData.Values["action"].ToString().ToLower();

            var desc = StatisticalLogsMiddleware.ActionDescDict.TryGetValue((controllerName, actionName), out var _desc) ? _desc : null;

            httpContext.Items["is-mvc-hander"] = true;
            httpContext.Items["mvc-action-desc"] = desc;
            if (context.ActionArguments.Any()) httpContext.Items["mvc-action-params"] = context.ActionArguments;

            log.Info($"before call [{httpContext.Request.Method}] /{controllerName}/{actionName}");

            await base.OnActionExecutionAsync(context, next);
        }

        public void OnException(ExceptionContext context)
        {
            //if (context.Exception == null) return;

            ///
            /// 这里只捕捉controller的error
            /// mvc page cshtml 里面的错误不会被这里捕捉
            /// see '~/Middlewares/StatisticalLogsMiddleware.cs'
            /// 

            #region
            /* var log = context.HttpContext.RequestServices.GetService<ILog>();

            log.Error(context.ExceptionDispatchInfo.SourceException);

            context.ExceptionHandled = true;
            context.Result = new JsonResult(new
            {
                isOk = false,
                errCode = -1,
                msg = "系统错误: " + context.Exception.Message,
            }); */
            #endregion
        }
    }
}
