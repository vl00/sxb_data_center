using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service
{
    public class Alg3Command : IRequest
    {
        public Guid UserId { get; set; }
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
        public double? ExtamAvgscore { get; set; }
        public int? No1Count { get; set; }
        public int? CmstuCount { get; set; }
        public int? RecruitCount { get; set; }
    }
}
