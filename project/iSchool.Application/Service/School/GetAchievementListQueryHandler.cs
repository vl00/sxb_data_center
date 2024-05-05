using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System.Text;
using Dapper;
using iSchool.Infrastructure;

namespace iSchool.Application.Service
{
    public class GetAchievementListQueryHandler : IRequestHandler<GetAchievementListQuery, List<KeyValueDto<string>>>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private UnitOfWork UnitOfWork { get; set; }

        public GetAchievementListQueryHandler(IRepository<SchoolExtension> schoolExtensionRepository, IUnitOfWork unitOfWork)
        {
            _schoolExtensionRepository = schoolExtensionRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
        }

        public Task<List<KeyValueDto<string>>> Handle(GetAchievementListQuery request, CancellationToken cancellationToken)
        {
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            if (ext == null)
                return Task.FromResult(new List<KeyValueDto<string>>());
            else
            {
                StringBuilder sql = new StringBuilder();
                if (ext.Grade == (byte)iSchool.Domain.Enum.SchoolGrade.SeniorMiddleSchool)
                    sql.Append(@"SELECT ach.schoolId AS [key],college.name AS [value],ach.count AS [message]  FROM dbo.SchoolAchievement AS ach 
                LEFT JOIN dbo.College AS college ON ach.schoolId=college.id
                WHERE ach.IsValid=1 AND college.IsValid=1 AND ach.extId=@extId AND ach.year=@year ORDER BY ach.CreateTime");
                else
                    sql.Append(@"SELECT ach.schoolId AS [key],sch.name+'_'+ext.name AS [value],ach.count AS [message]
                FROM dbo.SchoolAchievement AS ach 
                LEFT  JOIN  dbo.SchoolExtension AS ext ON ach.schoolId =ext.id
                LEFT JOIN dbo.School AS sch ON sch.id=ext.sid
               WHERE ach.extId=@extId AND ach.IsValid=1  AND ext.IsValid=1 
                AND ach.year=@year ORDER BY ach.CreateTime");

                var result = UnitOfWork.DbConnection.Query<Guid, string, Double, KeyValueDto<string>>(sql.ToString(),
                    (key, value, count) =>
                {
                    return new KeyValueDto<string> { Key = key.ToString(), Value = value.ToString(), Message = Convert.ToInt32(count).ToString() };
                }, new { extId = request.ExtId, year = request.Year, grade = request.Grade }, UnitOfWork.DbTransaction, true, "value,message");


                return Task.FromResult(result.ToList());
            }
        }
    }
}
