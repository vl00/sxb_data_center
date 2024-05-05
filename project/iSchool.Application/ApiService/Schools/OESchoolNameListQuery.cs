using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.ApiService.Schools
{
    public class OESchoolNameListQuery : IRequest<OESchoolQueryResult[]>
    {
        public string[] Names { get; set; }
        public int? Status { get; set; }
    }
}
