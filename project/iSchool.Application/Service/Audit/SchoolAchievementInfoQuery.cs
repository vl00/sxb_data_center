using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class SchoolAchievementInfoQuery : IRequest<SchoolAchievementInfoQueryResult>
    {
        public Guid Eid { get; set; }
        public int Year { get; set; }
    }

    public class SchoolAchievementInfoQueryResult
    {
        public Guid Eid { get; set; }
        public int Year { get; set; }
        public string SchFtype { get; set; }
        public Domain.Enum.SchoolGrade SchoolGrade { get; set; }
        public Domain.Enum.SchoolType SchoolType { get; set; }
    }

    public class HighSchoolAchievementInfoQueryResult : SchoolAchievementInfoQueryResult
    {
        public HighSchoolAchievement Data { get; set; }
    }

    public class MiddleSchoolAchievementInfoQueryResult : SchoolAchievementInfoQueryResult
    {
        public MiddleSchoolAchievement Data { get; set; }
    }

    public class PrimarySchoolAchievementInfoQueryResult : SchoolAchievementInfoQueryResult
    {
        public PrimarySchoolAchievement[] Data { get; set; }
    }

    public class KindergartenAchievementInfoQueryResult : SchoolAchievementInfoQueryResult
    {
        public KindergartenAchievement[] Data { get; set; }
    }
}
