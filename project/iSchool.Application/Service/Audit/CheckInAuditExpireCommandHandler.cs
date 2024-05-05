using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Domain.Enum;
using System.Text;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service.Audit
{
    public class CheckInAuditExpireCommandHandler : IRequestHandler<CheckInAuditExpireCommand>
    {
        IServiceProvider _sp;

        static DateTime? _time;
        static int fc;

        public CheckInAuditExpireCommandHandler(IServiceProvider sp)
        {
            this._sp = sp;
        }

        public Task<Unit> Handle(CheckInAuditExpireCommand req, CancellationToken cancellationToken)
        {
            /// 审核单被获取后超过了XX分钟还未变成成功or失败, 变回未审核

            SimpleQueue.Default.EnqueueThenRunOnChildScope(_sp, dowork);
            void dowork(IServiceProvider sp, object o)
            {
                var auditOption = sp.GetService<AuditOption>();
                var log = sp.GetService<ILog>();

                if (_time == null) _time = DateTime.Now;
                else
                {
                    var now = DateTime.Now;
                    if ((now - _time.Value) < TimeSpan.FromMinutes(3))
                        return;

                    _time = now;
                }

                var sql = @"
update dbo.SchoolAudit set [Status]=@sUnAudit,ModifyDateTime=CreateTime,Modifier=@empty,Creator=@empty,AuditMessage=''
where [Status]=@sInAudit and IsValid=1 
and datediff(minute,ModifyDateTime,getdate())>@atmin
";
                try
                {
                    log.Info("后台线程执行'正在处理的审核单过期状态变回未审核'");

                    var unitOfWork = sp.GetRequiredService<IUnitOfWork>() as UnitOfWork;

                    //lock (auditOption.Sync)
                    {
                        unitOfWork.DbConnection.Execute(sql, new
                        {
                            sUnAudit = SchoolAuditStatus.UnAudit,
                            sInAudit = SchoolAuditStatus.InAudit,
                            atmin = auditOption.AtAuditMin,
                            empty = Guid.Empty,
                        });
                    }

                    Interlocked.Exchange(ref fc, 0);
                }
                catch (Exception ex)
                {
                    var msg = log.NewMsg(LogLevel.Warn);
                    msg.Content = "正在处理的审核单过期状态变回未审核失败";
                    msg.Error = ex.Message;
                    msg.ErrorCode = "-1555";
                    msg.StackTrace = ex.StackTrace;
                    log.LogMsg(msg);

                    if (Interlocked.Increment(ref fc) < 2)
                        _time = null;
                }
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
