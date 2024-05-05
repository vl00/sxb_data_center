using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Authorization;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Domain.Enum;
using System.Text;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service.Jobs
{
    public class ComputeSchoolDataCommandHandler : IRequestHandler<ComputeSchoolDataCommand>
    {
        public ComputeSchoolDataCommandHandler(IServiceProvider sp, ILog log, AppSettings appSettings,
            IMediator mediator,
            IUnitOfWork unitOfWork,
            IRepository<Total_User> resp_tuser)
        {
            this.sp = sp;
            this.log = log;
            this.mediator = mediator;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.appSettings = appSettings;
            this.resp_tuser = resp_tuser;
        }

        IServiceProvider sp;
        ILog log;
        IMediator mediator;
        UnitOfWork unitOfWork;
        AppSettings appSettings;
        IRepository<Total_User> resp_tuser;

        Guid gidEditor => appSettings.GidEditor;
        Guid gidAuditor => appSettings.GidAuditor;
        Guid gidQxEdit => appSettings.GidQxEdit;
        Guid gidQxAudit => appSettings.GidQxAudit;

        public async Task<Unit> Handle(ComputeSchoolDataCommand req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            log.Info($"working 获取所有编辑+所有有审核权限的user");
            await mediator.Send(new ComputeUserCommand { Date = req.Date });
            log.Info($"work ok 获取所有编辑+所有有审核权限的user");

            log.Info($"before working 生成每日编辑数据");
            Compute_DateUserEditSchool(req);
            log.Info($"after work ok 生成每日编辑数据");

            log.Info($"before working 生成每日审核数据");
            Compute_DateUserAuditSchool(req);
            log.Info($"after work ok 生成每日审核数据");

            log.Info($"before working 录入数据记录");
            Compute_Date_school_All(req);
            log.Info($"after work ok 录入数据记录");

            log.Info($"before working 审核数据记录");
            Compute_Date_audit_all(req);
            log.Info($"after work ok 审核数据记录");

            log.Info($"before working 生成数据概况");
            Compute_TotalDataboard(req);
            log.Info($"after work ok 生成数据概况");

            return Unit.Value;
        }

        private string get_sql_declare(ComputeSchoolDataCommand cmd)
        {
            return $@"
declare @gidEditor uniqueidentifier = '{gidEditor}'
declare @gidQxAudit uniqueidentifier = '{gidQxAudit}'
declare @date date = '{cmd.Date.ToString("yyyy-MM-dd")}'
declare @gid0 uniqueidentifier = '{Guid.Empty}'
declare @sgInitial tinyint = {SchoolStatus.Initial.ToInt()}
declare @sgInAudit tinyint = {SchoolStatus.InAudit.ToInt()}
declare @sgSuccess tinyint = {SchoolStatus.Success.ToInt()}
declare @sgFailed tinyint = {SchoolStatus.Failed.ToInt()}
declare @aSuccess tinyint = {SchoolAuditStatus.Success.ToInt()}
declare @aFailed tinyint = {SchoolAuditStatus.Failed.ToInt()}
";
        }

        private void Compute_DateUserEditSchool(ComputeSchoolDataCommand cmd)
        {
            var sql = $@"{get_sql_declare(cmd)}

delete from dbo.TotalDateUserEditSchool where [Date]=@date ;

insert dbo.TotalDateUserEditSchool ([Date],UserId,SchoolEntryCount,SchoolExtEntryCount,AuditSuccessCount,UnAuditCount,AuditFailCount,IsValid)
select @date,u.Id,isnull(t.SchoolEntryCount,0),isnull(t.SchoolExtEntryCount,0),isnull(t.AuditSuccessCount,0),isnull(t.UnAuditCount,0),isnull(t.AuditFailCount,0),1 
from Total_user u left join (
    select s.*,e.SchoolExtEntryCount,a.AuditSuccessCount,a.UnAuditCount,a.AuditFailCount
    from (select s.Creator,count(1)as SchoolEntryCount from dbo.School s 
	    where DATEDIFF(dd,@date,s.CreateTime)<=0 and s.Creator<>@gid0 and s.status<>@sgInitial and s.IsValid=1 
	    group by s.Creator
    ) s 
	left join (select s.Creator,sum(case when e.id is null then 0 else 1 end)as SchoolExtEntryCount from dbo.School s 
		left join dbo.SchoolExtension e on e.sid=s.id and e.allowedit=0 and e.IsValid=1 and DATEDIFF(dd,@date,e.CreateTime)<=0 --and e.Creator<>@gid0
	    where DATEDIFF(dd,@date,s.CreateTime)<=0 and s.Creator<>@gid0 and s.status<>@sgInitial and s.IsValid=1 
	    group by s.Creator
	) e on s.Creator=e.Creator
    left join (select a.editor,sum(case when a.Status not in(@aSuccess,@aFailed) then 1 else 0 end)as UnAuditCount,
	    sum(case when a.Status=@aSuccess then 1 else 0 end)as AuditSuccessCount,
	    sum(case when a.Status=@aFailed then 1 else 0 end)as AuditFailCount
	    from (select s.Creator as editor,a.* from (select * from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1)a
	    inner join (select sid,max(CreateTime)as CreateTime from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1
	        group by a.sid) a0 on a.sid=a0.sid and a.CreateTime=a0.CreateTime
	    inner join dbo.School s on s.id=a.sid and s.IsValid=1
	    )a group by a.editor
    ) a on s.Creator=a.editor
)t on t.Creator=u.id
where u.RoleId=@gidEditor
";

            try
            {
                unitOfWork.BeginTransaction();

                unitOfWork.DbConnection.Execute(sql, null, unitOfWork.DbTransaction);

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw ex;
            }
        }

        private void Compute_DateUserAuditSchool(ComputeSchoolDataCommand cmd)
        {
            var sql = $@"{get_sql_declare(cmd)}

delete from dbo.TotalDateUserAuditSchool where [Date]=@date ;

insert dbo.TotalDateUserAuditSchool([Date],UserId,AuditCount,AuditExtCount,IsValid)
select @date,u.Id,isnull(a.AuditCount,0),isnull(a.AuditExtCount,0),1 
from Total_user u left join (
	select a.Modifier,count(1)as AuditCount,sum(isnull(e.AuditExtCount,0))as AuditExtCount
	from (select * from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.ModifyDateTime)<=0 and a.IsValid=1)a
	inner join (select sid,max(ModifyDateTime)as ModifyDateTime from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.ModifyDateTime)<=0 and a.IsValid=1
		group by a.sid) a0 on a.sid=a0.sid and a.ModifyDateTime=a0.ModifyDateTime
	inner join dbo.School s on s.id=a.sid and s.IsValid=1
	left join (
		select e.sid,count(1)as AuditExtCount from dbo.SchoolExtension e where e.allowedit=0 and e.IsValid=1 and DATEDIFF(dd,@date,e.CreateTime)<=0 
		group by e.sid
	) e on s.id=e.sid
	where a.Status in(@aSuccess,@aFailed)
	group by a.Modifier
)a on a.Modifier=u.id
where u.QxId=@gidQxAudit
";

            try
            {
                unitOfWork.BeginTransaction();

                unitOfWork.DbConnection.Execute(sql, null, unitOfWork.DbTransaction);

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw ex;
            }
        }

        private void Compute_Date_school_All(ComputeSchoolDataCommand cmd)
        {
            var sql = $@"{get_sql_declare(cmd)}
select count(1) from dbo.School s where DATEDIFF(dd,@date,s.CreateTime)=0 and s.status>@sgInitial and s.IsValid=1  ;
select count(1) from dbo.School s where DATEDIFF(dd,@date,s.CreateTime)<=0 and s.status>@sgInitial and s.IsValid=1  ;

select count(1) from SchoolExtension e where e.allowedit=0 and DATEDIFF(dd,@date,e.CreateTime)=0 and e.IsValid=1 
    and exists(select 1 from School s where s.IsValid=1 and s.status>@sgInitial and s.id=e.sid) ;
select count(1) from SchoolExtension e where e.allowedit=0 and DATEDIFF(dd,@date,e.CreateTime)<=0 and e.IsValid=1 
    and exists(select 1 from School s where s.IsValid=1 and s.status>@sgInitial and s.id=e.sid) ;
";
            var gr = unitOfWork.DbConnection.QueryMultiple(sql);
            var m = new TotalDateSchoolEdit { Date = cmd.Date };
            m.SchoolEntryCount = gr.ReadFirst<int>();
            m.AllSchoolEntryCount = gr.ReadFirst<long>();
            m.SchoolExtEntryCount = gr.ReadFirst<int?>();
            m.AllSchoolExtEntryCount = gr.ReadFirst<long?>();

            sql = @"
if not exists(select 1 from TotalDateSchoolEdit where Date=@Date) begin
    insert dbo.TotalDateSchoolEdit(Date,SchoolEntryCount,AllSchoolEntryCount,SchoolExtEntryCount,AllSchoolExtEntryCount)
    values(@Date,@SchoolEntryCount,@AllSchoolEntryCount,@SchoolExtEntryCount,@AllSchoolExtEntryCount) ;
end else begin
    update dbo.TotalDateSchoolEdit set SchoolEntryCount=@SchoolEntryCount,AllSchoolEntryCount=@AllSchoolEntryCount,SchoolExtEntryCount=@SchoolExtEntryCount,AllSchoolExtEntryCount=@AllSchoolExtEntryCount
    where Date=@Date ;
end";
            unitOfWork.DbConnection.Execute(sql, m);
        }

        private void Compute_Date_audit_all(ComputeSchoolDataCommand cmd)
        {
            var sql = $@"{get_sql_declare(cmd)}

select a.*,ISNULL(e.ExtCount,0)as ExtCount into #aa 
from (select * from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.ModifyDateTime)<=0 and a.IsValid=1)a
inner join (select sid,max(ModifyDateTime)as ModifyDateTime from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.ModifyDateTime)<=0 and a.IsValid=1
	group by a.sid) a0 on a.sid=a0.sid and a.ModifyDateTime=a0.ModifyDateTime
inner join dbo.School s on s.id=a.sid and s.IsValid=1
left join (
		select e.sid,count(1)as ExtCount from dbo.SchoolExtension e where e.allowedit=0 and e.IsValid=1 
		group by e.sid
	) e on s.id=e.sid
where a.Status in(@aSuccess,@aFailed) ;

select count(1)as AuditCount from #aa a where DATEDIFF(dd,@date,a.ModifyDateTime)=0
select count(1)as AllAuditCount from #aa a where DATEDIFF(dd,@date,a.ModifyDateTime)<=0
select sum(a.ExtCount)as AuditExtCount from #aa a where DATEDIFF(dd,@date,a.ModifyDateTime)=0
select sum(a.ExtCount)as AllAuditExtCount from #aa a where DATEDIFF(dd,@date,a.ModifyDateTime)<=0

drop table #aa
";
            var gr = unitOfWork.DbConnection.QueryMultiple(sql);
            var m = new TotalDateSchoolAudit { Date = cmd.Date };
            m.AuditCount = gr.ReadFirst<int?>() ?? 0;
            m.AllAuditCount = gr.ReadFirst<long?>() ?? 0;
            m.AuditExtCount = gr.ReadFirst<int?>();
            m.AllAuditExtCount = gr.ReadFirst<long?>();

            sql = @"
if not exists(select 1 from TotalDateSchoolAudit where Date=@Date) begin
    insert dbo.TotalDateSchoolAudit(Date,AuditCount,AllAuditCount,AuditExtCount,AllAuditExtCount)
    values(@Date,@AuditCount,@AllAuditCount,@AuditExtCount,@AllAuditExtCount) ;
end else begin
    update dbo.TotalDateSchoolAudit set AuditCount=@AuditCount,AllAuditCount=@AllAuditCount,AuditExtCount=@AuditExtCount,AllAuditExtCount=@AllAuditExtCount
    where Date=@Date ;
end";
            unitOfWork.DbConnection.Execute(sql, m);
        }

        private void Compute_TotalDataboard(ComputeSchoolDataCommand cmd)
        {
            var board = unitOfWork.DbConnection.QueryFirstOrDefault<TotalDataboard>("select * from dbo.TotalDataboard") ?? new TotalDataboard();

            var sql = $@"{get_sql_declare(cmd)}

select sum(case when u.RoleId=@gidEditor then 1 else 0 end)as EditorCount,sum(case when u.QxId=@gidQxAudit then 1 else 0 end)as AuditorCount from dbo.Total_User u ;

select sum(case when a.Status not in(@aSuccess,@aFailed) then 1 else 0 end)as UnAuditCount,
sum(case when a.Status=@aFailed then 1 else 0 end)as AuditFailedCount,
sum(case when a.Status=@aSuccess then 1 else 0 end)as AuditSuccessCount
from (select * from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.ModifyDateTime)<=0 and a.IsValid=1)a
inner join (select sid,max(ModifyDateTime)as ModifyDateTime from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.ModifyDateTime)<=0 and a.IsValid=1
	group by a.sid) a0 on a.sid=a0.sid and a.ModifyDateTime=a0.ModifyDateTime
inner join dbo.School s on s.id=a.sid and s.IsValid=1 ;

select count(1)as SchoolCount from School s where s.Creator<>@gid0 and s.status>@sgInitial and DATEDIFF(dd,@date,s.CreateTime)<=0 and s.IsValid=1 ;

select count(1)as SchoolExtCount from SchoolExtension e where e.allowedit=0 and DATEDIFF(dd,@date,e.CreateTime)<=0 and e.IsValid=1 
    and exists(select 1 from School s where s.IsValid=1 and s.status>@sgInitial and s.id=e.sid) ;

select count(1)as SchoolExtAuditSuccessCount from OnlineSchoolExtension e where DATEDIFF(dd,@date,e.CreateTime)<=0 and e.IsValid=1 
    and exists(select 1 from OnlineSchool s where s.IsValid=1 and s.status=@sgSuccess and s.id=e.sid) ;
";

            var g = unitOfWork.DbConnection.QueryMultiple(sql);
            var r = g.ReadFirst();
            board.EditorCount = Convert.ToInt32(r.EditorCount);
            board.AuditorCount = Convert.ToInt32(r.AuditorCount);
            r = g.ReadFirst();
            board.UnAuditCount = Convert.ToInt64(r.UnAuditCount);
            board.AuditFailedCount = Convert.ToInt64(r.AuditFailedCount);
            board.AuditSuccessCount = Convert.ToInt64(r.AuditSuccessCount);
            r = g.ReadFirst();
            board.SchoolCount = Convert.ToInt64(r.SchoolCount);
            r = g.ReadFirst();
            board.SchoolExtCount = Convert.ToInt64(r.SchoolExtCount);
            r = g.ReadFirst();
            board.SchoolExtAuditSuccessCount = Convert.ToInt64(r.SchoolExtAuditSuccessCount);

            try
            {
                sql = @"
delete from dbo.TotalDataboard ;
insert TotalDataboard(EditorCount,AuditorCount,UnAuditCount,AuditFailedCount,AuditSuccessCount,SchoolCount,SchoolExtCount,SchoolExtAuditSuccessCount)
    values(@EditorCount,@AuditorCount,@UnAuditCount,@AuditFailedCount,@AuditSuccessCount,@SchoolCount,@SchoolExtCount,@SchoolExtAuditSuccessCount) ;
";
                unitOfWork.BeginTransaction();
                unitOfWork.DbConnection.Execute(sql, board, unitOfWork.DbTransaction);
                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw ex;
            }
        }
    }
}
