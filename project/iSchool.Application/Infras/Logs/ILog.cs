using MicroKnights.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace iSchool.Infrastructure
{
    public interface ILog
    {
        void Debug(object msg);
        void Info(object msg);
        void Warn(object msg);
        void Error(object msg);
        void Error(Exception ex, int errCode = -1);
        void Error(string msg, Exception ex, int errCode = -1);
        void Fatal(object msg);
        void Fatal(Exception ex, int errCode = -1);
        void Fatal(string msg, Exception ex, int errCode = -1);
        void LogMsg(LogMessage msg);
        LogMessage NewMsg(LogLevel logLevel = LogLevel.Info);
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
    }

    public class LogMessage
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// 执行时间
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// 网站
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 业务编码
        /// </summary>
        public string BusinessId { get; set; }
        /// <summary>
        /// 应用程序
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// 类名
        /// </summary>
        public string Class { get; set; }
        /// <summary>
        /// 方法名
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string Params { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 主机
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 线程号
        /// </summary>
        public string ThreadId { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 操作人姓名
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 操作人角色
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 错误码
        /// </summary>
        public string ErrorCode { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 堆栈跟踪
        /// </summary>
        public string StackTrace { get; set; }
        /// <summary>
        ///  学校sid
        /// </summary>
        public string Sid { get; set; }

        //public override string ToString()
        //{
        //    var result = new StringBuilder();
        //    result.AppendLine("应用程序：{0}", Application);
        //    result.AppendLine("日志级别：{0}", Level);
        //    result.AppendLine("操作时间：{0}", Time);
        //    if (!BusinessId.IsNullOrEmpty())
        //    {
        //        result.AppendLine("业务号：{0}", BusinessId);
        //    }
        //    if (!Caption.IsNullOrEmpty())
        //    {
        //        result.AppendLine("标题：{0}", Caption);
        //    }
        //    if (!Content.IsNullOrEmpty())
        //    {
        //        result.AppendLine("内容：{0}", Content);
        //    }
        //    if (!Params.IsNullOrEmpty())
        //    {
        //        result.AppendLine("附加内容：{0}", Params);
        //    }
        //    if (!UserId.IsNullOrEmpty())
        //    {
        //        result.AppendLine("用户编号：{0}", UserId);
        //    }
        //    if (!Operator.IsNullOrEmpty())
        //    {
        //        result.AppendLine("操作人姓名：{0}", Operator);
        //    }
        //    if (!Role.IsNullOrEmpty())
        //    {
        //        result.AppendLine("操作人角色：{0}", Role);
        //    }
        //    if (!Url.IsNullOrEmpty())
        //    {
        //        result.AppendLine("Url：{0}", Url);
        //    }
        //    if (!Ip.IsNullOrEmpty())
        //    {
        //        result.AppendLine("IP：{0}", Ip);
        //    }
        //    if (!Host.IsNullOrEmpty())
        //    {
        //        result.AppendLine("主机：{0}", Ip);
        //    }
        //    if (!ThreadId.IsNullOrEmpty())
        //    {
        //        result.AppendLine("线程号：{0}", ThreadId);
        //    }
        //    if (!Class.IsNullOrEmpty())
        //    {
        //        result.AppendLine("类名：{0}", Class);
        //    }
        //    if (!Method.IsNullOrEmpty())
        //    {
        //        result.AppendLine("方法名：{0}", Method);
        //    }
        //    if (!ErrorCode.IsNullOrEmpty())
        //    {
        //        result.AppendLine("错误码：{0}", ErrorCode);
        //    }
        //    if (!Error.IsNullOrEmpty())
        //    {
        //        result.AppendLine("错误消息：{0}", Error);
        //    }
        //    if (!StackTrace.IsNullOrEmpty())
        //    {
        //        result.AppendLine("堆栈跟踪：{0}", StackTrace);
        //    }
        //    result.AppendLine("-------------------------------------------------------");
        //    result.AppendLine();

        //    return result.ToString();
        //}
    }
}
