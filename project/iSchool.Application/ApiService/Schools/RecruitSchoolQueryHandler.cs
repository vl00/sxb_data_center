using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Dapper;
using System.Linq;

namespace iSchool.Application.ApiService.Schools
{
    public class RecruitSchoolQueryHandler : IRequestHandler<RecruitSchoolQuery, RecruitSchoolQueryResult[]>
    {
        UnitOfWork unitOfWork;

        public RecruitSchoolQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<RecruitSchoolQueryResult[]> Handle(RecruitSchoolQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = $@"
select a.year,a.schoolId,ea.name as schoolname,
a.count,a.extId,e.name as extname,e.sid,s.name as sname
from [dbo].OnlineSchoolAchievement a 
{(req.IsCollege ? "left join dbo.College ea on ea.id=a.schoolId and ea.IsValid=1" : 
    "left join dbo.OnlineSchoolExtension ea on ea.id=a.schoolId and ea.IsValid=1")}
left join dbo.OnlineSchoolExtension e on e.id=a.extId and e.IsValid=1
inner join dbo.OnlineSchool s on s.id=e.sid and s.IsValid=1
where a.IsValid=1 and a.schoolId in @SchoolIds and a.year=@Year
";

            var r = unitOfWork.DbConnection.Query<RecruitSchoolQueryResult>(sql, req);
            return r.ToArray();
        }
    }
}
