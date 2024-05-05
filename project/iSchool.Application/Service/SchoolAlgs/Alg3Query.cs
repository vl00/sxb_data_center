using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service
{
    public class Alg3Query : IRequest<Alg3QyRstDto>
    {
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
    }

    public class Alg3QyRstDto
    {
        public ExtInfoEntity Ext { get; set; } = new ExtInfoEntity();
        public double? ExtamAvgscore { get; set; }
        public int? No1Count { get; set; }
        public int? CmstuCount { get; set; }
        public int? RecruitCount { get; set; }
    }
}
