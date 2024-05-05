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
using iSchool.Application.Service.OnlineSchools;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.BgServices;
using MediatR;
using Dapper;
using Dapper.Contrib.Extensions;

namespace iSchool.Application.Service
{
    public class SchoolOnlinedEventHandler_ComputeOLschextInfos : INotificationHandler<SchoolOnlinedEvent>
    {
        IServiceProvider _sp;

        public SchoolOnlinedEventHandler_ComputeOLschextInfos(IServiceProvider sp)
        {
            this._sp = sp;
        }

        public Task Handle(SchoolOnlinedEvent e, CancellationToken cancellationToken)
        {
            return Handle_core(_sp, e);
        }

        static async Task Handle_core(IServiceProvider sp, SchoolOnlinedEvent e)
        {
            var log = sp.GetService<ILog>();
            UnitOfWork unitOfWork = null;

            var _ex = await Retry_to_do(async () => 
            {
                unitOfWork = sp.GetService<IUnitOfWork>() as UnitOfWork;
                unitOfWork.DbConnection.Close();

                var sql = $@"
select e.id as eid,e.sid,s.name as Schname,e.name as Extname,e.grade,e.type,e.discount,e.diglossia,e.chinese,e.SchFtype,
c.province,c.city,c.area,c.address,c.latitude,c.longitude,c.LatLong,c.lodging,c.sdextern,
CONVERT(float,null) as TotalScore,CONVERT(bit,null) as IsAuthedByOpen,@aid as AuditId,@ModifyTime as ModifyTime
from dbo.OnlineSchool s with(nolock)
inner join dbo.OnlineSchoolExtension e with(nolock) on e.sid=s.id and e.IsValid=1
left join dbo.OnlineSchoolExtContent c with(nolock) on c.eid=e.id and c.IsValid=1
--left join (select a.* from dbo.SchoolAudit a with(nolock)
--    inner join (select sid,MAX(CreateTime)mt,(case when Min(CreateTime)=Max(CreateTime) then 1 else 0 end)_isnew from [dbo].SchoolAudit with(nolock) where sid=@sid group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
--    where a.sid=@sid
--)a on s.id=a.sid
where s.status={SchoolStatus.Success.ToInt()} and s.IsValid=1 and s.id=@sid
";
                var ls = await unitOfWork.DbConnection.QueryAsync<Lyega_OLschextSimpleInfo>(sql, new { e.Sid, e.Aid, ModifyTime = e.T });
                if (ls?.Any() != true) return;
                try
                {
                    unitOfWork.DbConnection.TryOpen();
                    unitOfWork.BeginTransaction();

                    sql = @"delete from Lyega_OLschextSimpleInfo where sid=@sid ";
                    await unitOfWork.DbConnection.ExecuteAsync(sql, new { e.Sid }, unitOfWork.DbTransaction);

                    sql = @"
insert Lyega_OLschextSimpleInfo (eid,sid,Schname,Extname,grade,type,discount,diglossia,chinese,SchFType,
    province,city,area,address,latitude,longitude,LatLong,lodging,sdextern,
    TotalScore,IsAuthedByOpen,AuditId,ModifyTime)
values (@eid,@sid,@Schname,@Extname,@grade,@type,@discount,@diglossia,@chinese,@SchFType,
    @province,@city,@area,@address,@latitude,@longitude,@LatLong,@lodging,@sdextern,
    @TotalScore,@IsAuthedByOpen,@AuditId,@ModifyTime)
";
                    await unitOfWork.DbConnection.ExecuteAsync(sql, ls, unitOfWork.DbTransaction);

                    unitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    unitOfWork?.Rollback();
                    throw ex;
                }
            }, 3, 500);

            if (_ex != null)
            {
                log.Error("error on ComputeOLschextInfos then retry it again.", _ex, 17789);
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
