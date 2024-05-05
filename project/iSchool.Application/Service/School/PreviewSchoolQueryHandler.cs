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

namespace iSchool.Application.Service
{
    public class PreviewSchoolQueryHandler : IRequestHandler<PreviewSchoolQuery, PreviewSchoolQueryResult>
    {
        UnitOfWork unitOfWork;
        IRepository<School> schoolRepository;

        public PreviewSchoolQueryHandler(IRepository<School> schoolRepository, IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.schoolRepository = schoolRepository;
        }

        public async Task<PreviewSchoolQueryResult> Handle(PreviewSchoolQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var res = new PreviewSchoolQueryResult();
            res.Sid = req.Sid;

            var school = schoolRepository.Get(req.Sid);
            if (school == null) throw new Exception($"找不到对应的学校school id={req.Sid}");

            res.SchoolName = school.Name;
            res.SchoolName_e = school.Name_e;
            res.Logo = school.Logo;
            res.WebSite = school.Website;
            res.Info = school.Intro;
            res.EduSysType = school.EduSysType;

            var sql = @"
                        select t.name from dbo.GeneralTag t 
                        inner join GeneralTagBind b on t.id=b.tagID
                        where b.dataID=@schoolId and b.dataType=2
                        ";
            res.Tags = unitOfWork.DbConnection.Query<string>(sql, new { schoolId = school.Id }).ToArray();

            sql = @"
                    select e.id as ExtId,e.name as ExtName,
                    e.Completion
                    --((isnull(c1.Completion,0)+isnull(c2.Completion,0)+isnull(c3.Completion,0)+isnull(c4.Completion,0)+isnull(c5.Completion,0)+isnull(c6.Completion,0))/6)as Completion
                    from dbo.SchoolExtension e
                    --left join dbo.SchoolExtContent c1 on e.id=c1.eid and c1.IsValid=1 
                    --left join dbo.SchoolExtRecruit c2 on e.id=c2.eid and c2.IsValid=1 
                    --left join dbo.SchoolExtCourse c3 on e.id=c3.eid and c3.IsValid=1 
                    --left join dbo.SchoolExtCharge c4 on e.id=c4.eid and c4.IsValid=1 
                    --left join dbo.SchoolExtQuality c5 on e.id=c5.eid and c5.IsValid=1 
                    --left join dbo.SchoolExtlife c6 on e.id=c6.eid and c6.IsValid=1 
                    where e.IsValid=1 
                    and e.sid=@schoolId
                    ";
            res.Exts = unitOfWork.DbConnection.Query<PreviewSchoolExt>(sql, new { schoolId = school.Id }).ToArray();

            sql = @"
                    select top 1 * from dbo.SchoolAudit a where a.Sid=@Sid and a.IsValid=1 
                    order by CreateTime desc
                    ";
            var a = unitOfWork.DbConnection.QueryFirstOrDefault<SchoolAudit>(sql, new { Sid = school.Id });
            res.CurrAuditMessage = a?.AuditMessage;
            res.CurrAuditStatus = a == null ? (Domain.Enum.SchoolAuditStatus?)null : (Domain.Enum.SchoolAuditStatus)a.Status;

            return res;
        }
    }
}
