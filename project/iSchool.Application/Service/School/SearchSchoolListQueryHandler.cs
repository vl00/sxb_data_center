using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Infrastructure;
using System.Linq;
using MediatR;
using Dapper;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service
{
    public class SearchSchoolListQueryHandler : IRequestHandler<SearchSchoolListQuery, SearchSchoolListDto>
    {
        private UnitOfWork UnitOfWork { get; set; }
        private IMediator _mediator;

        public SearchSchoolListQueryHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            UnitOfWork = (UnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public Task<SearchSchoolListDto> Handle(SearchSchoolListQuery request, CancellationToken cancellationToken)
        {
            request.Index = request.Index < 1 ? 1 : request.Index;

            Guid gid = default;
            if (!string.IsNullOrEmpty(request.Search))
            {
                if (!Guid.TryParse(request.Search, out gid) || gid == default)
                    return Handle_es(request);
            }

            SearchSchoolListDto dto = new SearchSchoolListDto();            

            var sql = @"
SELECT DISTINCT   dbo.School.id AS Sid,dbo.School.name AS Name,dbo.School.Creator AS Creator,dbo.School.Status as SchoolStatus,
    audit.Modifier AS AuditUserId,dbo.School.CreateTime,
    --(SELECT SUM(Completion)/COUNT(*) FROM dbo.SchoolExtension WHERE sid=dbo.School.id AND IsValid=1) AS Completion,
    dbo.School.Completion,
    dbo.School.ModifyDateTime,audit.Status  
    FROM dbo.School
    LEFT JOIN dbo.SchoolExtension ON dbo.School.id=dbo.SchoolExtension.sid and dbo.SchoolExtension.IsValid=1
    LEFT JOIN dbo.SchoolExtContent ON dbo.SchoolExtension.id=dbo.SchoolExtContent.eid and dbo.SchoolExtContent.IsValid=1
    --LEFT JOIN dbo.SchoolAudit ON dbo.School.id=dbo.SchoolAudit.sid 
    left join (
        select a.* from [dbo].SchoolAudit a with(nolock)
        inner join (select sid,MAX(CreateTime)mt,(case when Min(CreateTime)=Max(CreateTime) then 1 else 0 end)_isnew from [dbo].SchoolAudit with(nolock) where IsValid=1 group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
        where a.IsValid=1
    ) audit on school.id=audit.sid
WHERE dbo.School.IsValid=1 --and school.status in @schoolstatus 
{0}
order by dbo.School.ModifyDateTime desc
OFFSET (@pageIndex-1)*@pageSize ROWS FETCH NEXT @pageSize ROWS ONLY
";

            var countSql = @"SELECT COUNT(*) FROM(	SELECT DISTINCT dbo.School.id 
                FROM dbo.School
                LEFT JOIN dbo.SchoolExtension ON dbo.School.id=dbo.SchoolExtension.sid and dbo.SchoolExtension.IsValid=1
                LEFT JOIN dbo.SchoolExtContent ON dbo.SchoolExtension.id=dbo.SchoolExtContent.eid and dbo.SchoolExtContent.IsValid=1
                --LEFT JOIN dbo.SchoolAudit ON dbo.School.id=dbo.SchoolAudit.sid 
                left join (
                 select a.* from [dbo].SchoolAudit a with(nolock)
                 inner join (select sid,MAX(CreateTime)mt,(case when Min(CreateTime)=Max(CreateTime) then 1 else 0 end)_isnew from [dbo].SchoolAudit with(nolock) where IsValid=1 group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
                 where a.IsValid=1
                ) audit on school.id=audit.sid
               WHERE dbo.School.IsValid=1 --and school.status in @schoolstatus 
                --  (dbo.SchoolAudit.Id= (SELECT TOP 1 Id 
                --   FROM dbo.SchoolAudit WHERE (dbo.SchoolAudit.sid=dbo.School.id) 
                --     ORDER BY ModifyDateTime DESC) OR dbo.SchoolAudit.sid IS NULL) 
            {0}) T";

            var where = new StringBuilder();
            if (gid != default)
                where.Append($" and (School.id=@gid or SchoolExtension.id=@gid) ");
            if (request != null && request.Grade > 0)
                where.Append($" and SchoolExtension.Grade={request.Grade}");
            if (request != null && request.Type > 0)
                where.Append($" and SchoolExtension.Type={request.Type}");
            if (request != null && request.Province > 0)
                where.Append($" and SchoolExtContent.Province={request.Province}");
            if (request != null && request.City > 0)
                where.Append($" and SchoolExtContent.City={request.City}");
            if (request != null && request.Area > 0)
                where.Append($" and SchoolExtContent.Area={request.Area}");
            if (request != null && request.Status > 0)
                where.Append($" and audit.Status={request.Status}");
            if (request != null && request.StartTime != null)
                where.Append($" and School.CreateTime>='{request.StartTime.Value.ToString("yyyy-MM-dd")}'");
            if (request != null && request.EndTime != null)
                where.Append($" and School.CreateTime<'{request.EndTime.Value.AddDays(1).Date.ToString("yyyy-MM-dd")}'");
            if (request?.Editors != null)
                where.Append($" and school.Creator='{request.Editors.Value}'");


            var result = UnitOfWork.DbConnection.Query<SearchSchoolItem>(string.Format(sql, where.ToString()),
                new
                {
                    gid,
                    start = (request.Index - 1) * request.PageSize,
                    end = request.PageSize * request.Index,
                    pageIndex = request.Index,
                    pageSize = request.PageSize,
                    schoolstatus =
                    new int[] {
                        (int)SchoolStatus.Edit,
                        (int)SchoolStatus.Failed,
                        (int)SchoolStatus.Initial,
                        (int)SchoolStatus.InAudit,
                        (int)SchoolStatus.Success
                    }
                });
            var count = UnitOfWork.DbConnection.QueryFirst<int>(
                string.Format(countSql, where.ToString()),
                new
                {
                    gid,
                    schoolstatus = new int[] {
                        (int)SchoolStatus.Edit,
                        (int)SchoolStatus.Failed,
                        (int)SchoolStatus.Initial,
                        (int)SchoolStatus.InAudit,
                        (int)SchoolStatus.Success
                    }
                });

            dto.list = result.ToList();
            dto.PageIndex = request.Index;
            dto.PageSize = request.PageSize;
            dto.PageCount = count;
            return Task.FromResult(dto);
        }

        async Task<SearchSchoolListDto> Handle_es(SearchSchoolListQuery request)
        {
            var dto = new SearchSchoolListDto();

            var req = new Searchs.ESearchLikeQuery();
            req.PageIndex = request.Index;
            req.PageSize = request.PageSize;
            req.Grade = request.Grade;
            req.Type = request.Type;
            if (request.Province > 0) req.Province = request.Province;
            if (request.City > 0) req.City = request.City;
            if (request.Area > 0) req.Area = request.Area;
            req.AuditStatus = request.Status > 0 ? new[] { (int)request.Status.Value } : null;
            req.CreateTime1 = request.StartTime;
            req.CreateTime2 = request.EndTime;
            if (request.Editors != null) req.EidtorIds = new[] { request.Editors.Value };

            if (Guid.TryParse(request.Search, out var _gid)) req.Sid = _gid;
            else req.Name = request.Search;

            // invoke http-api
            var fr = await _mediator.Send(req);
            if (!fr.IsOk) throw new FnResultException(fr.Code, fr.Msg);
            var totalPageCount = fr.Data.TotalPageCount;
            var sids = fr.Data.CurrentPageItems.AsArray();

            if (sids?.Length < 1)
            {
                dto.PageIndex = req.PageIndex;
                dto.PageSize = req.PageSize;
                dto.PageCount = totalPageCount;
                return dto;
            }

            var sql = @"
select s.id as sid,s.name as Name,s.Creator,s.status as schoolstatus,a.Modifier as AuditUserId,s.CreateTime,a.ModifyDateTime,a.Status,s.Completion
from school s
left join (
    select a.* from [dbo].SchoolAudit a with(nolock)
    inner join (select sid,MAX(CreateTime)mt,(case when Min(CreateTime)=Max(CreateTime) then 1 else 0 end)_isnew from [dbo].SchoolAudit with(nolock) where IsValid=1 group by sid) a0 on a0.sid=a.sid and a.CreateTime=a0.mt
    where a.IsValid=1
) a on s.id=a.sid 
where s.IsValid=1 and s.id in @sids
";
            var items = await UnitOfWork.DbConnection.QueryAsync<SearchSchoolItem>(sql, new { sids });

            dto.list = items.OrderBy(_ => Array.IndexOf(sids, _.Sid)).ToList();
            dto.PageIndex = request.Index;
            dto.PageSize = request.PageSize;
            dto.PageCount = totalPageCount;
            return dto;
        }
    }

}
