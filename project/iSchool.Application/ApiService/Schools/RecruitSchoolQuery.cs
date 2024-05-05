using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.ApiService.Schools
{
    public class RecruitSchoolQuery : IRequest<RecruitSchoolQueryResult[]>
    {
        public int Year { get; set; }
        public bool IsCollege { get; set; }
        public Guid[] SchoolIds { get; set; }
    }

    public class RecruitSchoolQueryResult
    {
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public int Count { get; set; }
        public int Year { get; set; }
        public Guid ExtId { get; set; }
        public string ExtName { get; set; }
        public Guid Sid { get; set; }
        public string SName { get; set; }
    }
}
