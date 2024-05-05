using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace iSchool.Application.Service
{
    public class UpSchoolExtCompletionCommandHandler : IRequestHandler<UpSchoolExtCompletionCommand>
    {
        UnitOfWork unitOfWork;
        IServiceProvider sp;
        IHttpContextAccessor httpContextAccessor;

        HttpContext HttpContext => httpContextAccessor.HttpContext;

        public UpSchoolExtCompletionCommandHandler(IUnitOfWork unitOfWork, IServiceProvider sp, IHttpContextAccessor httpContextAccessor)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.sp = sp;
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<Unit> Handle(UpSchoolExtCompletionCommand cmd, CancellationToken cancellationToken)
        {
            var sql = @"
update e set e.Completion=c.Completion --,e.Modifier=@UserId,e.ModifyDateTime=getdate()
from dbo.SchoolExtension e, (
    select e.id,((isnull(c1.Completion,0)+isnull(c2.Completion,0)+isnull(c3.Completion,0)+isnull(c4.Completion,0)+isnull(c5.Completion,0)+isnull(c6.Completion,0))/6)as Completion
    from dbo.SchoolExtension e
    left join dbo.SchoolExtContent c1 on e.id=c1.eid and c1.IsValid=1 
    left join dbo.SchoolExtRecruit c2 on e.id=c2.eid and c2.IsValid=1 
    left join dbo.SchoolExtCourse c3 on e.id=c3.eid and c3.IsValid=1 
    left join dbo.SchoolExtCharge c4 on e.id=c4.eid and c4.IsValid=1 
    left join dbo.SchoolExtQuality c5 on e.id=c5.eid and c5.IsValid=1 
    left join dbo.SchoolExtlife c6 on e.id=c6.eid and c6.IsValid=1 
    where e.IsValid=1 and e.id=@Eid
) c
where e.id=c.id and e.id=@Eid ;

update dbo.School set Completion=(
    select sum(Completion)/count(1) from dbo.SchoolExtension where sid=(select sid from SchoolExtension where id=@Eid) and IsValid=1
) where id=(select sid from SchoolExtension where id=@Eid) ;
";
            unitOfWork.DbConnection.Execute(sql, cmd);//执行并返回受影响行数

            var uid = HttpContext.GetUserId();

            SimpleQueue.Default.EnqueueThenRunOnChildScope(sp,
                (_sp, e) => _sp.GetService<IMediator>().Publish(e),
                new SchoolExtCompletionUpdatedEvent { Eid = cmd.Eid, Time = DateTime.Now, UserId = uid }
            );

            return Task.FromResult(Unit.Value);
        }
    }

    public class SchoolExtCompletionUpdatedEventHandler : INotificationHandler<SchoolExtCompletionUpdatedEvent>
    {
        UnitOfWork unitOfWork;

        public SchoolExtCompletionUpdatedEventHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
        }

        public async Task Handle(SchoolExtCompletionUpdatedEvent e, CancellationToken cancellationToken)
        {
            var sql = $@"
update dbo.SchoolExtension set ModifyDateTime=@Time,Modifier=(case when Modifier='{Guid.Empty}' then @UserId else Modifier end) where id=@Eid and ModifyDateTime<@Time ;
update dbo.School set ModifyDateTime=@Time,Modifier=(case when Modifier='{Guid.Empty}' then @UserId else Modifier end) where id=(select sid from SchoolExtension e where e.id=@Eid) and ModifyDateTime<@Time ;

update s set s.Creator=@UserId,s.CreateTime=e.ModifyDateTime,s.status={SchoolStatus.Edit.ToInt()}
from School s, SchoolExtension e
where s.id=e.sid and s.Creator='{Guid.Empty}' and s.status={SchoolStatus.Initial.ToInt()} and s.IsValid=1 and e.id=@Eid ;
";
            await unitOfWork.DbConnection.ExecuteAsync(sql, e);
        }
    }
}
