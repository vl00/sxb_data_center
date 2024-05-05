using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Infrastructure.Dapper;
using iSchool.Domain.Enum;

namespace iSchool.Application.Service.Audit
{
    public class DpwdRecorrQueryHandler : IRequestHandler<DpwdRecorrQuery, PagedList<DpwdRecorrQueryResult>>
    {
        UnitOfWork unitOfWork;

        public DpwdRecorrQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
        }

        async Task<PagedList<DpwdRecorrQueryResult>> ignore_Handle(DpwdRecorrQuery req, CancellationToken cancellationToken)
        {
            var sql = $@"
select * from (
select s.id as sid,e.id as eid,s.name as sname,e.name as ename,e.ModifyDateTime as deltime,dp.id as dwid,dp.Content collate Chinese_PRC_90_CI_AI as Content,1 as dtype
from dbo.school s
left join dbo.SchoolExtension e on s.id=e.sid
left join [iSchoolProduct].[dbo].SchoolComments dp on dp.SchoolId=s.id and dp.SchoolSectionId=e.id and dp.State<>@dpNotokState
where not(s.IsValid=1 and e.IsValid=1) and dp.id is not null
union all
select s.id as sid,e.id as eid,s.name as sname,e.name as ename,e.ModifyDateTime as deltime,q.id as dwid,q.Content collate Chinese_PRC_90_CI_AI as Content,2 as dtype
from dbo.school s
left join dbo.SchoolExtension e on s.id=e.sid
left join [iSchoolProduct].[dbo].QuestionInfos q on q.SchoolId=s.id and q.SchoolSectionId=e.id and q.State<>@dpNotokState
where not(s.IsValid=1 and e.IsValid=1) and q.id is not null
) T where 1=1 {(req.DelTime1 != null ? "and deltime>=@DelTime1" : "")} {(req.DelTime2 != null ? "and deltime<@DelTime2" : "")} 
{(req.Txt.IsNullOrEmpty() ? "" : req.Sty.In(null, 0) ? "and (sid=@txt or eid=@txt)" : req.Sty == 1 ? "and sid=@txt" : req.Sty == 2 ? "and eid=@txt" : "")}
";
            sql = $@"
{(req.PageIndex == -1 && req.PageSize == -1 ? "select -1;" : $"select count(1) from ({sql}) _t ;")}

{sql}
order by deltime desc
{(req.PageIndex == -1 && req.PageSize == -1 ? "" : "OFFSET (@pageIndex-1)*@pageSize ROWS FETCH NEXT @pageSize ROWS ONLY")}
";

            var dy = new DynamicParameters(req)
                .Set("@dpNotokState", 4)
                .Set(nameof(req.DelTime1), req.DelTime1?.Date)
                .Set(nameof(req.DelTime2), req.DelTime2?.Date.AddDays(1))
                ;
            var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, dy);
            var pg = new PagedList<DpwdRecorrQueryResult>();
            pg.CurrentPageIndex = req.PageIndex;
            pg.PageSize = req.PageSize;
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();
            pg.CurrentPageItems = await gr.ReadAsync<DpwdRecorrQueryResult>();

            return pg;
        }

        public async Task<PagedList<DpwdRecorrQueryResult>> Handle(DpwdRecorrQuery req, CancellationToken cancellationToken)
        {
            var sql = $@"
select * from (
select s.id as sid,e.id as eid,s.name as sname,e.name as ename,e.ModifyDateTime as deltime,dp.id as dwid,dp.Content collate Chinese_PRC_90_CI_AI as Content,1 as dtype
from dbo.school s
left join dbo.SchoolExtension e on s.id=e.sid
left join [iSchoolProduct].[dbo].SchoolComments dp on dp.SchoolId=s.id and dp.SchoolSectionId=e.id and dp.State<>@dpNotokState
where not(s.IsValid=1 and e.IsValid=1) and dp.id is not null
union all
select s.id as sid,e.id as eid,s.name as sname,e.name as ename,e.ModifyDateTime as deltime,q.id as dwid,q.Content collate Chinese_PRC_90_CI_AI as Content,2 as dtype
from dbo.school s
left join dbo.SchoolExtension e on s.id=e.sid
left join [iSchoolProduct].[dbo].QuestionInfos q on q.SchoolId=s.id and q.SchoolSectionId=e.id and q.State<>@dpNotokState
where not(s.IsValid=1 and e.IsValid=1) and q.id is not null
) T where 1=1 {(req.DelTime1 != null ? "and deltime>=@DelTime1" : "")} {(req.DelTime2 != null ? "and deltime<@DelTime2" : "")} 
{(req.Txt.IsNullOrEmpty() ? "" : req.Sty.In(null, 0) ? "and (sid=@txt or eid=@txt)" : req.Sty == 1 ? "and sid=@txt" : req.Sty == 2 ? "and eid=@txt" : "")}
";
            sql = $@"
{(req.PageIndex == -1 && req.PageSize == -1 ? "select -1;" : $"select count(1) from ({sql}) _t ;")}

{sql}
order by deltime desc
{(req.PageIndex == -1 && req.PageSize == -1 ? "" : "OFFSET (@pageIndex-1)*@pageSize ROWS FETCH NEXT @pageSize ROWS ONLY")}
";

            var dy = new DynamicParameters(req)
                .Set("@dpNotokState", 4)
                .Set(nameof(req.DelTime1), req.DelTime1?.Date)
                .Set(nameof(req.DelTime2), req.DelTime2?.Date.AddDays(1))
                ;
            var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, dy, commandTimeout: 60 * 1000);
            var pg = new PagedList<DpwdRecorrQueryResult>();
            pg.CurrentPageIndex = req.PageIndex;
            pg.PageSize = req.PageSize;
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();
            pg.CurrentPageItems = await gr.ReadAsync<DpwdRecorrQueryResult>();

            return pg;
        }
    }
}
