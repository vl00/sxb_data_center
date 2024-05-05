using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.ApiService.Schools
{
    public class OESchoolQuery : IRequest<OESchoolQueryResult[]>
    {
        public Guid[] Ids { get; set; }
    }

    public class OESchoolQueryResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EName { get; set; }
        public string Website { get; set; }
        public string Intro { get; set; }
        public string Logo { get; set; }
        public byte Status { get; set; }
        public bool IsValid { get; set; }
    }
}
