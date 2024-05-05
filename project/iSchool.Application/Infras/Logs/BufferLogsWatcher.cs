using log4net.Core;
using MicroKnights.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;

namespace iSchool.Infrastructure.Logs
{
    public class BufferLogsWatcher : IDisposable, IErrorHandler
    {
        readonly object _sync = new object();
        Timer _timer;
        readonly AdoNetAppender appender;
        readonly int delay;
        readonly object _errlock = new object();
        readonly string errlogPath;

        public log4net.ILog Logger { get; }

        public BufferLogsWatcher(IConfiguration configuration)
        {
            Logger = log4net.LogManager.GetLogger("NETCoreRepository", this.GetType());

            appender = Logger.Logger.Repository.GetAppenders().FirstOrDefault(_ => _ is AdoNetAppender) as AdoNetAppender;
            if (appender != null) appender.ErrorHandler = this;

            errlogPath = configuration["logs:errlogPath"];
            init_logs();

            delay = configuration.GetValue<int>("logs:write-db-delay");
            if (delay > -1) _timer = new Timer(DoWork, this, -1, -1);
        }

        public void TryToRun()
        {
            if (_timer == null) return;
            lock (_sync)
            {
                _timer.Change(-1, -1);
                _timer.Change(delay, -1);
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer?.Change(-1, -1);
                _timer?.Dispose();
                _timer = null;
                DoWork(this);
            }
        }

        static TimerCallback DoWork = delegate(object o)
        {
            var _this = o as BufferLogsWatcher;

            _this.appender?.Flush();
        };

        void IErrorHandler.Error(string message, Exception e, ErrorCode errorCode)
        {
            errorHandler_log(message, e, errorCode);
        }

        void IErrorHandler.Error(string message, Exception e)
        {
            errorHandler_log(message, e, ErrorCode.WriteFailure);
        }

        void IErrorHandler.Error(string message)
        {
            errorHandler_log(message, null, ErrorCode.WriteFailure);
        }

        void init_logs()
        {
            try
            {
                var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), string.Format(errlogPath, "--")));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            catch { }
        }

        void errorHandler_log(string message, Exception e, ErrorCode errorCode)
        {
            lock (_errlock)
            {
                File.AppendAllText(
                    Path.Combine(Directory.GetCurrentDirectory(), string.Format(errlogPath, DateTime.Now.ToString("yyyy-MM-dd"))),
                    $@"[error] {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}  ------------------------------
$errorCode={Enum.GetName(errorCode.GetType(), errorCode)}
$msg={message}, $exception={e?.Message} 
{e?.StackTrace}
-------------------------------------------------------------------------------------------------------------------------------------{"\n"}"
                );
            }
        }
    }
}
