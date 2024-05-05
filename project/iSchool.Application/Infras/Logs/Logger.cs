using iSchool.Domain.Modles;
using MicroKnights.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Infrastructure
{
    public class Logger : ILog
    {
        readonly IHttpContextAccessor httpContextAccessor;
        readonly Logs.BufferLogsWatcher watcher;
        readonly log4net.ILog log;
        readonly string applicationName;

        HttpContext HttpContext => httpContextAccessor.HttpContext;

        public Logger(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, Logs.BufferLogsWatcher watcher)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.watcher = watcher;
            log = watcher.Logger;
            applicationName = configuration["logs:applicationName"];
        }

        public void Debug(object msg)
        {
            var o = getMsg(nameof(Debug), msg);
            LogMsg(o);
        }

        public void Error(Exception ex, int errCode = -1) => Error(null, ex, errCode);

        public void Error(string msg, Exception ex, int errCode = -1)
        {
            var o = getMsg(nameof(Error), msg, ex, errCode);
            LogMsg(o);
        }

        public void Fatal(Exception ex, int errCode = -1) => Fatal(null, ex, errCode);

        public void Fatal(string msg, Exception ex, int errCode = -1)
        {
            var o = getMsg(nameof(Fatal), msg, ex, errCode);
            LogMsg(o);
        }

        public void Info(object msg)
        {
            var o = getMsg(nameof(Info), msg);
            LogMsg(o);
        }

        public void Warn(object msg)
        {
            var o = getMsg(nameof(Warn), msg);
            LogMsg(o);
        }

        public void Error(object msg)
        {
            var o = getMsg(nameof(Error), msg);
            LogMsg(o);
        }

        public void Fatal(object msg)
        {
            var o = getMsg(nameof(Fatal), msg);
            LogMsg(o);
        }

        public void LogMsg(LogMessage msg)
        {
            switch (msg.Level)
            {
                case nameof(Debug):
                    watcher.TryToRun();
                    log.Debug(msg);
                    return;
                case nameof(Info):
                    watcher.TryToRun();
                    log.Info(msg);
                    return;
                case nameof(Warn):
                    watcher.TryToRun();
                    log.Warn(msg);
                    return;
                case nameof(Error):
                    watcher.TryToRun();
                    log.Error(msg);
                    return;
                case nameof(Fatal):
                    watcher.TryToRun();
                    log.Fatal(msg);
                    return;
                default:
                    throw new NotImplementedException($"logger did not support for log level={msg.Level}");
            }
        }

        public LogMessage NewMsg(LogLevel logLevel = LogLevel.Info)
        {
            return getMsg(Enum.GetName(typeof(LogLevel), logLevel), null, null, 0);
        }

        LogMessage getMsg(string lv, object msg)
        {
            return msg is LogMessage m ? m
                : msg is FnResultException fex ? getMsg(lv, fex.Message, fex, fex.Code)
                : msg is CustomResponseException crex ? getMsg(lv, crex.Message, crex, crex.ErrorCode)
                : msg is Exception ex ? getMsg(lv, ex.Message, ex, 1)
                : getMsg(lv, msg, null, 0);
        }

        LogMessage getMsg(string lv, object _msg, Exception ex, int errCode)
        {
            var routeData = HttpContext?.GetRouteData();

            var msg = new LogMessage();

            if (routeData != null)
            {
                msg.Class = routeData.Values["controller"].ToString();
                msg.Method = routeData.Values["action"].ToString();
            }
            else
            {
                var trace = new StackTrace();
                msg.Method = trace.GetFrame(2).GetMethod().Name;
                msg.Class = trace.GetFrame(2).GetMethod().DeclaringType.FullName;
            }

            msg.Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            msg.Url = getDisplayPartUrl(HttpContext?.Request)?.ToLower();
            msg.Ip = $"{HttpContext?.Connection.RemoteIpAddress}:{HttpContext?.Connection.RemotePort}";
            msg.Host = HttpContext?.Request.Host.ToString();
            msg.ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();

            msg.Level = lv;
            msg.Content = _msg?.ToString();
            msg.StackTrace = ex?.StackTrace;
            msg.ErrorCode = errCode.ToString();
            msg.Error = ex?.Message;

            msg.BusinessId = HttpContext?.Items["business-id"]?.ToString();
            msg.Application = applicationName;
            msg.Caption = HttpContext?.Items["mvc-action-desc"]?.ToString();

            var str = Tryv(() => HttpContext?.Items["mvc-action-params"]?.ToJsonString(), "{}");
            msg.Params = str;
            msg.Sid = str == null ? null : Regex.Match(str, @"""(sid|SchoolId)"":""(?<sid>[\d\w-]+)""", RegexOptions.IgnoreCase).Groups["sid"]?.Value;
            msg.Sid = string.IsNullOrEmpty(msg.Sid) ? null : msg.Sid;

            var userInfo = HttpContext?.GetUserInfo();
            msg.UserId = HttpContext?.User?.Identity.Name;
            msg.Operator = userInfo?.Displayname;   //
            msg.Role = userInfo?.Character == null ? null : string.Join('|', userInfo.Character.Select(_ => _.Name));   //

            return msg;
        }

        static string getDisplayPartUrl(HttpRequest request)
        {
            if (request == null) return null;

            return new StringBuilder()
                .Append(request.PathBase.Value ?? string.Empty)
                .Append(request.Path.Value ?? string.Empty)
                .Append(request.QueryString.Value ?? string.Empty)
                .ToString();
        }
    }
}
