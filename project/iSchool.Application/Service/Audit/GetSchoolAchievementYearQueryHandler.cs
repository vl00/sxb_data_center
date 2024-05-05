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
    public class GetSchoolAchievementYearQueryHandler : IRequestHandler<GetSchoolAchievementYearQuery, SchoolAchievementYearQueryResult[]>
    {
        UnitOfWork unitOfWork;
        IRepository<SchoolExtension> schoolExtensionRepository;

        public GetSchoolAchievementYearQueryHandler(IRepository<SchoolExtension> schoolExtensionRepository, 
            IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.schoolExtensionRepository = schoolExtensionRepository;
        }

        public Task<SchoolAchievementYearQueryResult[]> Handle(GetSchoolAchievementYearQuery req, CancellationToken cancellationToken)
        {
            var sgext = schoolExtensionRepository.GetAll(_ => _.IsValid == true && _.Id == req.Eid).FirstOrDefault();
            if (sgext == null) throw new Exception($"不存在的学校eid={sgext.Id}");

            var schoolGrade = (Domain.Enum.SchoolGrade)sgext.Grade;
            var schoolType = (SchoolType)sgext.Type;

            var sql = $@"
{@"select extid as eid,year 
from [dbo].SchoolAchievement
where extid=@Eid and IsValid=1
union 
select extid as eid,year from [dbo].HighSchoolAchievement
where extid=@Eid and IsValid=1"
.If(schoolGrade == Domain.Enum.SchoolGrade.SeniorMiddleSchool)}

{$@"select extid as eid,year 
from [dbo].{(
schoolGrade == Domain.Enum.SchoolGrade.SeniorMiddleSchool ? "" :
schoolGrade == Domain.Enum.SchoolGrade.JuniorMiddleSchool ? "[MiddleSchoolAchievement]" :
schoolGrade == Domain.Enum.SchoolGrade.PrimarySchool ? "PrimarySchoolAchievement" :
schoolGrade == Domain.Enum.SchoolGrade.Kindergarten ? "KindergartenAchievement" :
throw new ArgumentException("该学校类型与升学成绩无关"))} 
where extid=@Eid and IsValid=1"
.If(schoolGrade != Domain.Enum.SchoolGrade.SeniorMiddleSchool)}

group by extid,year
order by year desc
";
            var res = unitOfWork.DbConnection.Query<SchoolAchievementYearQueryResult>(sql, req).ToArray();

            return Task.FromResult(res);
        }
    }
}
