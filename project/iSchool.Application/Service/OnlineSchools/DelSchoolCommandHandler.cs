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
using System.Linq;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service.OnlineSchool
{
    public class DelSchoolCommandHandler : IRequestHandler<DelSchoolCommand>
    {
        UnitOfWork unitOfWork;
        IMediator mediator;
        IServiceProvider sp;

        public DelSchoolCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IServiceProvider sp)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.sp = sp;
        }

        public async Task<Unit> Handle(DelSchoolCommand cmd, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var sql = @"
select count(1) from dbo.OnlineSchool o with(nolock)
--left join dbo.School s with(nolock) on o.Id=s.Id --and s.IsValid=1
left join (select a.* from [dbo].SchoolAudit a with(nolock)
    inner join (select sid,MAX(CreateTime)mt from [dbo].SchoolAudit with(nolock) group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
    where a.Sid=@Id
)a on a.Sid=o.Id and a.Status=@Astatus and a.IsValid=1
where o.Id=@Id and o.IsValid=1 
";
            var c = unitOfWork.DbConnection.ExecuteScalar<int>(sql,
                    new DynamicParameters(cmd)
                        .Set("Astatus", SchoolAuditStatus.Success));

            if (c <= 0)
            {
                throw new Exception("不能删除不是已发布的学校");
            }

            sql = @"
update dbo.OnlineSchool set [Status]=@status,show=0,IsValid=0,ModifyDateTime=GETDATE(),Modifier=@UserId where Id=@Id ;
update dbo.OnlineSchoolExtension set IsValid=0,ModifyDateTime=GETDATE(),Modifier=@UserId where Sid=@Id ;

update dbo.School set [Status]=@status,show=0,IsValid=0,ModifyDateTime=GETDATE(),Modifier=@UserId where Id=@Id ;
update dbo.SchoolExtension set IsValid=0,ModifyDateTime=GETDATE(),Modifier=@UserId where Sid=@Id ;

update a set a.IsValid=0 --a.Status=@Astatus,a.ModifyDateTime=GETDATE(),a.Modifier=@UserId,
from dbo.SchoolAudit a
where a.Sid=@Id ;
";
            try
            {
                unitOfWork.BeginTransaction();

                unitOfWork.DbConnection.Execute(sql,
                    new DynamicParameters(cmd)
                        .Set("status", SchoolStatus.Failed)
                        .Set("Astatus", SchoolAuditStatus.Failed),
                    unitOfWork.DbTransaction);

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw new FnResultException(400, ex);
            }

            SimpleQueue.Default.EnqueueThenRunOnChildScope( 
                (_sp, e) => _sp.GetService<IMediator>().Publish(e),
                new AfterSchoolOffDelEvent { Sid = cmd.Id, IsOff = false });

            return Unit.Value;
        }
    }
}
