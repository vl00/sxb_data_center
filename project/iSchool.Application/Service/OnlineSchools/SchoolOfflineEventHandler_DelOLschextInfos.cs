using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Application.Service.OnlineSchools;

namespace iSchool.Application.Service
{
    public class SchoolOfflineEventHandler_DelOLschextInfos : INotificationHandler<SchextNoValidEvent>
    {
        ILog log;
        UnitOfWork unitOfWork;
        IServiceProvider sp;

        public SchoolOfflineEventHandler_DelOLschextInfos(IServiceProvider sp, ILog log, IUnitOfWork unitOfWork)
        {
            this.sp = sp;
            this.log = log;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.unitOfWork.DbConnection.Close();
        }

        public async Task Handle(SchextNoValidEvent e, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            if ((e.Exts?.Length ?? 0) < 1) return;

            var sql = new StringBuilder();
            foreach (var (sid, eid) in e.Exts)
            {
                sql.AppendLine($"delete from Lyega_OLschextSimpleInfo where sid='{sid}' and eid='{eid}';");
            }

            var _ex = await Retry_to_do(async () => 
            {
                try
                {
                    unitOfWork.DbConnection.TryOpen();
                    unitOfWork.BeginTransaction();

                    unitOfWork.DbConnection.Execute(sql.ToString(), null, unitOfWork.DbTransaction);

                    unitOfWork.CommitChanges();
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    throw ex;
                }
            }, 3, 500);

            if (_ex != null)
            {
                log.Error(_ex, 198607);
            }
        }

        static async Task<Exception> Retry_to_do(Func<Task> func, int? c, int ms, Action<Exception> onerror = null)
        {
            var cc = c == null ? c : c < 1 ? 1 : c;
            for (var i = 1; (cc == null || i <= cc); i++)
            {
                try
                {
                    await func();
                    break;
                }
                catch (FnResultException ex)
                {
                    onerror?.Invoke(ex);
                    return ex;
                }
                catch (Exception ex)
                {
                    onerror?.Invoke(ex);
                    if (cc != null && i == cc.Value) return ex;
                    else if (ms > 0) await Task.Delay(ms);
                }
            }
            return null;
        }
    }
}
