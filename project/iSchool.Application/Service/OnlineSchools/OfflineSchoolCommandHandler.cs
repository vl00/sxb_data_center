using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Linq;
using iSchool.Application.Service.OnlineSchool;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service
{
    public class OfflineSchoolCommandHandler : IRequestHandler<OfflineSchoolCommand>
    {
        UnitOfWork unitOfWork;
        IMediator mediator;
        IServiceProvider sp;

        public OfflineSchoolCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IServiceProvider sp)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.sp = sp;
        }

        public async Task<Unit> Handle(OfflineSchoolCommand cmd, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var sql = @"
select count(1) from dbo.OnlineSchool o with(nolock)
inner join dbo.School s with(nolock) on o.Id=s.Id
inner join (select a.* from [dbo].SchoolAudit a with(nolock)
    inner join (select sid,MAX(CreateTime)mt from [dbo].SchoolAudit with(nolock) group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
    where a.Sid=@Id
)a on a.Sid=o.Id
where a.Status=@Astatus and s.Id=@Id and
o.IsValid=1 and s.IsValid=1 and a.IsValid=1
";
            var c = unitOfWork.DbConnection.ExecuteScalar<int>(sql,
                    new DynamicParameters(cmd)
                        .Set("Astatus", SchoolAuditStatus.Success));

            if (c <= 0)
            {
                throw new Exception("学校未发布");
            }

            sql = @"
select a.* from [dbo].SchoolAudit a
inner join (select sid,MAX(CreateTime)mt from [dbo].SchoolAudit group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
where a.Sid=@Id ;
";
            var a = unitOfWork.DbConnection.QueryFirstOrDefault<SchoolAudit>(sql, new DynamicParameters(cmd));

            if (a == null)
            {
                throw new Exception("学校未发布");
            }

            try
            {
                unitOfWork.BeginTransaction();

                sql = @"
update dbo.OnlineSchool set [Status]=@status,show=0,ModifyDateTime=GETDATE(),Modifier=@UserId where Id=@Id ;

update dbo.School set [Status]=@status,show=0,IsValid=1,ModifyDateTime=GETDATE(),Modifier=@UserId where Id=@Id ;

--update a set a.Status=@Astatus,a.ModifyDateTime=GETDATE(),a.Modifier=@UserId,a.IsValid=1 //...
";
                unitOfWork.DbConnection.Execute(sql,
                    new DynamicParameters(cmd)
                        .Set("status", SchoolStatus.Failed)
                        .Set("Astatus", SchoolAuditStatus.Failed),
                    unitOfWork.DbTransaction);

                a.Id = Guid.NewGuid();
                a.AuditMessage = "需修改";
                a.Status = (byte)SchoolAuditStatus.Failed;
                //a.Creator = cmd.UserId;
                a.Modifier = cmd.UserId;
                a.CreateTime = DateTime.Now;
                a.ModifyDateTime = DateTime.Now;
                a.IsValid = true;

                unitOfWork.DbConnection.Insert(a, unitOfWork.DbTransaction);

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw new FnResultException(400, ex);
            }

            SimpleQueue.Default.EnqueueThenRunOnChildScope(sp, 
                (_sp, e) => _sp.GetService<IMediator>().Publish(e),
                new AfterSchoolOffDelEvent { Sid = cmd.Id, IsOff = true });

            return Unit.Value;
        }
    }
}
