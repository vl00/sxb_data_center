using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class GetSchoolAchievementYearQuery : IRequest<SchoolAchievementYearQueryResult[]>
    {
        public Guid Eid { get; set; }
    }

    public class SchoolAchievementYearQueryResult
    {
        public Guid Eid { get; set; }
        public int Year { get; set; }
    }
}
