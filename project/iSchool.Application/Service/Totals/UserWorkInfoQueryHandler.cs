using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Infrastructure.Dapper;
using iSchool.Domain.Modles;
using Microsoft.Extensions.Options;
using static iSchool.Infrastructure.ObjectHelper;
using iSchool.Domain.Enum;

namespace iSchool.Application.Service.Totals
{
    public class UserWorkInfoQueryHandler : IRequestHandler<UserWorkInfoQuery, PagedList<UserWorkInfoQueryResult>>
    {
        UnitOfWork unitOfWork;
        IMediator mediator;

        public UserWorkInfoQueryHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.mediator = mediator;
        }

        public async Task<PagedList<UserWorkInfoQueryResult>> Handle(UserWorkInfoQuery request, CancellationToken cancellationToken)
        {
            var (total, items) = request.Type == 1 ? await GetType_1(request) : await GetType_2(request);

            var pg = new PagedList<UserWorkInfoQueryResult>();
            pg.CurrentPageIndex = request.PageIndex;
            pg.PageSize = request.PageSize;
            pg.TotalItemCount = total;

            var ns = AdminInfoUtil.GetUsers(items.Select(p => p.EditorId));
            items.ForEach(p => p.Editor = ns[p.EditorId]);

            ns = AdminInfoUtil.GetUsers(items.Where(p => p.AuditorId != null).Select(p => p.AuditorId.Value));
            items.ForEach(p => p.Auditor = p.AuditorId != null ? ns[p.AuditorId.Value] : null);

            pg.CurrentPageItems = items;
            return pg;
        }

        //编辑
        async Task<(int, List<UserWorkInfoQueryResult>)> GetType_1(UserWorkInfoQuery req)
        {
            Guid gid = default;
            if (!string.IsNullOrEmpty(req.SidOrName))
            {
                if (!Guid.TryParse(req.SidOrName, out gid) || gid == default)
                    return await fn_es();
            }

            return await fn0();

            async Task<(int, List<UserWorkInfoQueryResult>)> fn0()
            {
                var sql = $@"
select s.id as sid,s.name,s.Creator as EditorId,a.Modifier as AuditorId,s.Completion,s.Status as SchoolStatus,a.Status as AuditStatus,s.CreateTime,a.id as aid,
ROW_NUMBER()over(order by s.CreateTime desc)_i
from dbo.School s 
left join (select a.* from (select * from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1)a
	inner join (select sid,max(CreateTime)as CreateTime from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1
		group by a.sid) a0 on a.sid=a0.sid and a.CreateTime=a0.CreateTime
	inner join dbo.School s on s.id=a.sid and s.IsValid=1
) a on s.id=a.sid
where DATEDIFF(dd,@date,s.CreateTime)<=0 and s.status<>@sgInitial and s.IsValid=1 
and s.Creator=@UserId {"and s.id=@sid".If(gid != default)}
{"and s.name like @SidOrName".If(!string.IsNullOrEmpty(req.SidOrName) && gid == default)}
{"and s.Status=@AuditStatus".If(req.AuditStatus == -1)} {"and a.Status=@AuditStatus".If(req.AuditStatus != null && req.AuditStatus > -1)}
";
                sql = $@"
select count(1) from ({sql}) _t ;

SELECT * FROM ({sql}) _t where _i between (@PageIndex-1)*@PageSize+1 and @PageIndex*@PageSize ;
";

                var dy = new DynamicParameters(req)
                    .Set("sid", gid)
                    .Set("SidOrName", "%" + req.SidOrName + "%")
                    .Set("date", DateTime.Now.Date)
                    .Set("sgInitial", SchoolStatus.Initial.ToInt());

                var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, dy);

                return (gr.ReadFirst<int>(), gr.Read<UserWorkInfoQueryResult>().AsList());
            }

            async Task<(int, List<UserWorkInfoQueryResult>)> fn_es()
            {
                var q = new Searchs.ESearchLikeQuery();
                q.PageIndex = req.PageIndex;
                q.PageSize = req.PageSize;
                q.Name = req.SidOrName;
                q.EidtorIds = new[] { req.UserId };
                if (req.AuditStatus != null) q.AuditStatus = new[] { (int)req.AuditStatus.Value };

                var fr = await mediator.Send(q);
                if (!fr.IsOk) throw new FnResultException(fr.Code, fr.Msg);
                var total = fr.Data.TotalItemCount;
                var sids = fr.Data.CurrentPageItems.AsArray();

                if (sids?.Length < 1)
                    return (total, new List<UserWorkInfoQueryResult>());

                var sql = @"
select s.id as sid,s.name,s.Creator as EditorId,a.Modifier as AuditorId,s.Completion,s.Status as SchoolStatus,a.Status as AuditStatus,s.CreateTime,a.id as aid
from dbo.School s 
left join (select a.* from (select * from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1)a
	inner join (select sid,max(CreateTime)as CreateTime from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1
		group by a.sid) a0 on a.sid=a0.sid and a.CreateTime=a0.CreateTime
	inner join dbo.School s on s.id=a.sid and s.IsValid=1
) a on s.id=a.sid
where DATEDIFF(dd,@date,s.CreateTime)<=0 and s.status<>@sgInitial and s.IsValid=1 and s.id in @sids
";
                var dy = new DynamicParameters()
                    .Set("sids", sids)
                    .Set("date", DateTime.Now.Date)
                    .Set("sgInitial", SchoolStatus.Initial.ToInt());

                var items = await unitOfWork.DbConnection.QueryAsync<UserWorkInfoQueryResult>(sql, dy);
                return (total, items.OrderBy(_ => Array.IndexOf(sids, _.Sid)).ToList());
            }
        }       

        //审核
        async Task<(int, List<UserWorkInfoQueryResult>)> GetType_2(UserWorkInfoQuery req)
        {
            Guid gid = default;
            if (!string.IsNullOrEmpty(req.SidOrName))
            {
                if (!Guid.TryParse(req.SidOrName, out gid) || gid == default)
                    return await fn_es();
            }

            return await fn0();

            async Task<(int, List<UserWorkInfoQueryResult>)> fn0()
            {
                var sql = $@"
select s.id as sid,s.name,s.Creator as EditorId,a.Modifier as AuditorId,s.Completion,s.Status as SchoolStatus,a.Status as AuditStatus,s.CreateTime,a.id as aid,
ROW_NUMBER()over(order by s.CreateTime desc)_i
from (select * from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1)a
inner join (select sid,max(CreateTime)as CreateTime from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1
	group by a.sid) a0 on a.sid=a0.sid and a.CreateTime=a0.CreateTime
inner join dbo.School s on s.id=a.sid and s.IsValid=1
where a.Status in (@aSuccess,@aFailed) and a.Modifier=@UserId
{"and s.id=@sid".If(gid != default)}
{"and s.name like @SidOrName".If(!string.IsNullOrEmpty(req.SidOrName) && !Guid.TryParse(req.SidOrName, out _))}
{"and a.Status=@AuditStatus".If(req.AuditStatus != null && req.AuditStatus > -1)}
";
                sql = $@"
select count(1) from ({sql}) _t ;

SELECT * FROM ({sql}) _t where _i between (@PageIndex-1)*@PageSize+1 and @PageIndex*@PageSize ;
";

                var dy = new DynamicParameters(req)
                    .Set("sid", req.SidOrName)
                    .Set("SidOrName", "%" + req.SidOrName + "%")
                    .Set("date", DateTime.Now.Date)
                    .Set("aSuccess", SchoolAuditStatus.Success.ToInt())
                    .Set("aFailed", SchoolAuditStatus.Failed.ToInt());

                var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, dy);

                return (gr.ReadFirst<int>(), gr.Read<UserWorkInfoQueryResult>().AsList());
            }

            async Task<(int, List<UserWorkInfoQueryResult>)> fn_es()
            {
                var q = new Searchs.ESearchLikeQuery();
                q.PageIndex = req.PageIndex;
                q.PageSize = req.PageSize;
                q.Name = req.SidOrName;
                q.AuditorIds = new[] { req.UserId };
                if (req.AuditStatus != null) q.AuditStatus = new[] { (int)req.AuditStatus.Value };

                var fr = await mediator.Send(q);
                if (!fr.IsOk) throw new FnResultException(fr.Code, fr.Msg);
                var total = fr.Data.TotalItemCount;
                var sids = fr.Data.CurrentPageItems.AsArray();

                if (sids?.Length < 1)
                    return (total, new List<UserWorkInfoQueryResult>());

                var sql = @"
select s.id as sid,s.name,s.Creator as EditorId,a.Modifier as AuditorId,s.Completion,s.Status as SchoolStatus,a.Status as AuditStatus,s.CreateTime,a.id as aid
from (select * from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1)a
inner join (select sid,max(CreateTime)as CreateTime from dbo.SchoolAudit a where DATEDIFF(dd,@date,a.CreateTime)<=0 and a.IsValid=1
	group by a.sid) a0 on a.sid=a0.sid and a.CreateTime=a0.CreateTime
inner join dbo.School s on s.id=a.sid and s.IsValid=1
where a.Status in (@aSuccess,@aFailed) and s.id in @sids
";
                var dy = new DynamicParameters()
                    .Set("sids", sids)
                    .Set("date", DateTime.Now.Date)
                    .Set("aSuccess", SchoolAuditStatus.Success.ToInt())
                    .Set("aFailed", SchoolAuditStatus.Failed.ToInt());

                var items = await unitOfWork.DbConnection.QueryAsync<UserWorkInfoQueryResult>(sql, dy);
                return (total, items.OrderBy(_ => Array.IndexOf(sids, _.Sid)).ToList());
            }
        }
    }
}
