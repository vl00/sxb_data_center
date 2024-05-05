using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using MediatR;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.Service.Audit
{
    public class SearchQueryHandler : IRequestHandler<SearchQuery, PagedList<SearchResult>>
    {
        UnitOfWork unitOfWork;
        IMediator mediator;
        AuditOption auditOption;

        public SearchQueryHandler(IUnitOfWork unitOfWork, IMediator mediator, AuditOption auditOption)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.mediator = mediator;
            this.auditOption = auditOption;
        }

        public async Task<PagedList<SearchResult>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            Guid gid = default;
            var (total, items) = !string.IsNullOrEmpty(request.IdOrName) 
                && (!Guid.TryParse(request.IdOrName, out gid) || gid == default) 
                ? await Handle_es(request) : await Handle_0(request, gid);

            var pg = new PagedList<SearchResult>();
            pg.PageSize = request.PageSize;
            pg.CurrentPageIndex = request.PageIndex;
            pg.TotalItemCount = total;

            var ns = AdminInfoUtil.GetNames(items.Select(p => p.CreatorId));
            items.ForEach(p => p.Creator = ns[p.CreatorId]);

            ns = AdminInfoUtil.GetNames(items.Where(p => p?.ModifierId != null).Select(p => p.ModifierId.Value));
            items.ForEach(p => p.Modifier = Tryv(() => ns[p.ModifierId ?? Guid.Empty]));
            pg.CurrentPageItems = items;

            //_ = mediator.Send(new CheckInAuditExpireCommand());

            return pg;
        }

        async Task<(int Total, List<SearchResult> Items)> Handle_0(SearchQuery request, Guid gid)
        {
            var sql = @"select * from (
select *,(case when _isnew=1 and AuditStatus=@asUnAudit then null else _ModifierId end)as ModifierId,
(case when (AuditStatus=@asInAudit and _ModifierId=@AuditorId) then -1 when AuditStatus=@asFailed then 0 when AuditStatus=@asUnAudit then 1 when AuditStatus=@asSuccess then 2 else 3 end)as '_order'
from(
    select a.Id,a.sid as SchoolId,s.name as SchoolName,s.CreateTime,a.ModifyDateTime as ModifyTime,s.Creator as CreatorId,a.Modifier as _ModifierId,a0._isnew,
      (case when (a.Status=@asInAudit and datediff(minute,a.ModifyDateTime,getdate())>@atmin) then @asUnAudit else a.Status end)as AuditStatus
    from [dbo].SchoolAudit a with(nolock)
    inner join (select sid,MAX(CreateTime)mt,(case when Min(CreateTime)=Max(CreateTime) then 1 else 0 end)_isnew from [dbo].SchoolAudit with(nolock) group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
    left join dbo.School s with(nolock) on s.id=a.sid
    where --a.Status<>@asAuditing and (a.Status<>@asInAudit or (a.Modifier=@AuditorId and datediff(minute,a.ModifyDateTime,getdate())>@atmin)) and 
        a.IsValid=1 and s.IsValid=1 {0}
)as T 
)as T where AuditStatus<>@asAuditing {1}
";
            var sb0 = new StringBuilder();
            var sb1 = new StringBuilder();

            if (gid != default)
            {
                sb0.Append(" and (s.id=@gid or exists(select 1 from dbo.SchoolExtension where IsValid=1 and sid=a.sid and id=@gid)) ");
            }
            if (request.SchoolGrade != null)
            {
                sb0.Append(" and exists (select 1 from dbo.SchoolExtension where IsValid=1 and sid=a.sid and grade=@SchoolGrade) ");
                //sb0.Append(" and e.grade=@SchoolGrade");
            }
            if (request.SchoolType != null)
            {
                sb0.Append(" and exists (select 1 from dbo.SchoolExtension where IsValid=1 and sid=a.sid and type=@SchoolType) ");
                //sb0.Append(" and e.type=@SchoolType");
            }
            if (request.AuditStatus != null)
            {
                sb1.Append(" and AuditStatus=@AuditStatus ");
            }
            if (request.Area1 > -1)
            {
                sb0.Append(" and exists (select 1 from dbo.SchoolExtContent c inner join dbo.SchoolExtension e on e.id=c.eid where e.IsValid=1 and c.IsValid=1 and e.sid=a.sid and c.province=@Area1) ");
                //sb0.Append(" and e.province=@Area1");
            }
            if (request.Area2 > -1)
            {
                sb0.Append(" and exists (select 1 from dbo.SchoolExtContent c inner join dbo.SchoolExtension e on e.id=c.eid where e.IsValid=1 and c.IsValid=1 and e.sid=a.sid and c.city=@Area2) ");
                //sb0.Append(" and e.city=@Area2");
            }
            if (request.Area3 > -1)
            {
                sb0.Append(" and exists (select 1 from dbo.SchoolExtContent c inner join dbo.SchoolExtension e on e.id=c.eid where e.IsValid=1 and c.IsValid=1 and e.sid=a.sid and c.area=@Area3) ");
                //sb0.Append(" and e.area=@Area3");
            }
            if (request.StartTime != null)
            {
                sb0.Append(" and s.CreateTime>=@StartTime ");
            }
            if (request.EndTime != null)
            {
                request.EndTime = request.EndTime?.AddDays(1).Date;
                sb0.Append(" and s.CreateTime<@EndTime ");
            }
            if (request.Editors != null)
            {
                sb0.Append(" and s.creator=@Editors ");
            }
            if (request.Auditors != null)
            {
                sb1.Append(" and ModifierId=@Auditors ");
            }

            if (!request.CanSeeAll)
            {
                sb1.Append(" and (AuditStatus not in (@asSuccess,@asFailed) or _ModifierId=@AuditorId)");
                sb1.Append(" and (AuditStatus<>@asUnAudit or _isnew=1 or exists(select 1 from dbo.SchoolAuditorInfo ai where ai.Sid=T.SchoolId and ai.AuditorUid=@AuditorId))");
            }

            sql = string.Format(sql, sb0, sb1);
            sql = $@"
select count(1) from ({sql}) _t ;

{sql}
order by _order,ModifyTime desc
OFFSET (@pageIndex-1)*@pageSize ROWS FETCH NEXT @pageSize ROWS ONLY
";
            var dy = new DynamicParameters(request)
                .Set("@gid", gid)
                .Set("@asFailed", Domain.Enum.SchoolAuditStatus.Failed)
                .Set("@asSuccess", Domain.Enum.SchoolAuditStatus.Success)
                .Set("@asInAudit", Domain.Enum.SchoolAuditStatus.InAudit)
                .Set("@asUnAudit", Domain.Enum.SchoolAuditStatus.UnAudit)
                .Set("@asAuditing", Domain.Enum.SchoolAuditStatus.Auditing)
                .Set("@atmin", auditOption.AtAuditMin)
                //.Set("@nid", Math.Abs(request.AuditorId.GetHashCode()))
                ;

            var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, dy);

            return (await gr.ReadFirstAsync<int>(), (await gr.ReadAsync<SearchResult>()).AsList());
        }

        async Task<(int Total, List<SearchResult> Items)> Handle_es(SearchQuery request)
        {
            var req = new Searchs.ESearchLikeQuery();
            req.PageIndex = request.PageIndex;
            req.PageSize = request.PageSize;
            if (request.SchoolGrade != null) req.Grade = (byte)request.SchoolGrade.Value;
            if (request.SchoolType != null) req.Type = (byte)request.SchoolType.Value;
            if (request.Area1 > 0) req.Province = request.Area1;
            if (request.Area2 > 0) req.City = request.Area2;
            if (request.Area3 > 0) req.Area = request.Area3;
            if (request.AuditStatus != null) req.AuditStatus = new[] { (int)request.AuditStatus.Value };
            else req.AuditStatus = EnumUtil.GetDescs<Domain.Enum.SchoolAuditStatus>().Where(_ => _.Value != Domain.Enum.SchoolAuditStatus.Auditing).Select(_ => (int)_.Value).ToArray();
            req.CreateTime1 = request.StartTime;
            req.CreateTime2 = request.EndTime;
            if (request.Editors != null) req.EidtorIds = new[] { request.Editors.Value };
            if (request.Auditors != null) req.AuditorIds = new[] { request.Auditors.Value };
            if (!request.CanSeeAll) req.AuditorIds = new[] { request.AuditorId };

            if (Guid.TryParse(request.IdOrName, out var _gid)) req.Sid = _gid;
            else req.Name = request.IdOrName;
            
            // invoke http-api
            var fr = await mediator.Send(req);
            if (!fr.IsOk) throw new FnResultException(fr.Code, fr.Msg);
            var total = fr.Data.TotalItemCount;
            var sids = fr.Data.CurrentPageItems.AsArray();

            if (sids?.Length < 1) return (total, new List<SearchResult>());

            var sql = @"select * from (
select *,(case when _isnew=1 and AuditStatus=@asUnAudit then null else _ModifierId end)as ModifierId
from(
    select a.Id,a.sid as SchoolId,s.name as SchoolName,s.CreateTime,a.ModifyDateTime as ModifyTime,s.Creator as CreatorId,a.Modifier as _ModifierId,a0._isnew,
      (case when (a.Status=@asInAudit and datediff(minute,a.ModifyDateTime,getdate())>@atmin) then @asUnAudit else a.Status end)as AuditStatus
    from [dbo].SchoolAudit a with(nolock)
    inner join (select sid,MAX(CreateTime)mt,(case when Min(CreateTime)=Max(CreateTime) then 1 else 0 end)_isnew from [dbo].SchoolAudit with(nolock) group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
    left join dbo.School s with(nolock) on s.id=a.sid
    where a.IsValid=1 and s.IsValid=1 and s.id in @sids
)as T 
)as T where AuditStatus<>@asAuditing
";
            var items = await unitOfWork.DbConnection.QueryAsync<SearchResult>(sql, new
            {
                asFailed = Domain.Enum.SchoolAuditStatus.Failed.ToInt(),
                asSuccess = Domain.Enum.SchoolAuditStatus.Success.ToInt(),
                asInAudit = Domain.Enum.SchoolAuditStatus.InAudit.ToInt(),
                asUnAudit = Domain.Enum.SchoolAuditStatus.UnAudit.ToInt(),
                asAuditing = Domain.Enum.SchoolAuditStatus.Auditing.ToInt(),
                atmin = auditOption.AtAuditMin,
                sids,
            });
            return (total, items.OrderBy(_ => Array.IndexOf(sids, _.SchoolId)).ToList());
        }
    }
}
