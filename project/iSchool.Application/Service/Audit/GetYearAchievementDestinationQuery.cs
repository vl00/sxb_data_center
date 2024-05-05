using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class GetYearAchievementDestinationQuery : IRequest<PagedList<YearAchievementDestinationQueryResult>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public Guid Eid { get; set; }
        public int Year { get; set; }
    }

    public class YearAchievementDestinationQueryResult
    {
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public int Count { get; set; }
    }
}
