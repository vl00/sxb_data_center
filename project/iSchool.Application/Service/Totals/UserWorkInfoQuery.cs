using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Authorization.Models;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Totals
{
    public class UserWorkInfoQuery : IRequest<PagedList<UserWorkInfoQueryResult>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid UserId { get; set; }
        public int Type { get; set; } = 1;
        public string SidOrName { get; set; } 
        public int? AuditStatus { get; set; }
    }

    public class UserWorkInfoQueryResult
    {
        public Guid Sid { get; set; }
        public string Name { get; set; }
        public Guid EditorId { get; set; }
        public AdminInfo Editor { get; set; }
        public Guid? AuditorId { get; set; }
        public AdminInfo Auditor { get; set; }
        public double Completion { get; set; }
        public byte SchoolStatus { get; set; }
        public byte? AuditStatus { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid? Aid { get; set; }
    }
}
