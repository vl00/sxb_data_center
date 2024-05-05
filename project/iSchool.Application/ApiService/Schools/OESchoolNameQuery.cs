using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.ApiService.Schools
{
    public class OESchoolNameQuery : IRequest<OESchoolQueryResult[]>
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int? Status { get; set; }
    }
}
