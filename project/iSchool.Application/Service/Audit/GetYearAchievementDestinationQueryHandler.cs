using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Infrastructure.Dapper;

namespace iSchool.Application.Service.Audit
{
    public class GetYearAchievementDestinationQueryHandler : IRequestHandler<GetYearAchievementDestinationQuery, PagedList<YearAchievementDestinationQueryResult>>
    {
        UnitOfWork unitOfWork;

        public GetYearAchievementDestinationQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
        }

        public async Task<PagedList<YearAchievementDestinationQueryResult>> Handle(GetYearAchievementDestinationQuery req, CancellationToken cancellationToken)
        {
            var sql = $@"
select a.schoolId,(case when e0.grade=@grade then c.name else (s.name+'-'+e.name) end) as SchoolName,a.count,row_number()over(order by a.ModifyDateTime desc) as _i
from [dbo].[SchoolAchievement] a 
inner join dbo.[SchoolExtension] e0 on a.extId=e0.id
left join [dbo].[OnlineSchoolExtension] e on e.id=a.schoolId and e.IsValid=1
left join [dbo].[OnlineSchool] s on e.sid=s.id and s.IsValid=1
left join [dbo].[College] c on c.id=a.schoolId and c.IsValid=1
where e0.IsValid=1 and a.year=@Year and a.extId=@Eid and a.IsValid=1
";
            sql = $@"
select count(1) from ({sql}) _t ;

SELECT * FROM ({sql}) _t where _i between (@PageIndex-1)*@PageSize+1 and @PageIndex*@PageSize ;
";
            var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, new DynamicParameters(req)
                    .Set("grade", Domain.Enum.SchoolGrade.SeniorMiddleSchool.ToInt())
                );
            var pg = new PagedList<YearAchievementDestinationQueryResult>();
            pg.PageSize = req.PageSize;
            pg.CurrentPageIndex = req.PageIndex;
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();
            pg.CurrentPageItems = await gr.ReadAsync<YearAchievementDestinationQueryResult>();
            return pg;
        }
    }
}
