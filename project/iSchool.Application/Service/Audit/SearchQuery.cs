using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class SearchQuery : IRequest<PagedList<SearchResult>>
    {
        public Guid AuditorId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int SearchType { get; set; } = 0;

        public string IdOrName { get; set; }

        public SchoolGrade? SchoolGrade { get; set; }
        public SchoolType? SchoolType { get; set; }       
        public SchoolAuditStatus? AuditStatus { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Area1 { get; set; } = -1;
        public int Area2 { get; set; } = -1;
        public int Area3 { get; set; } = -1;
        public Guid? Editors { get; set; }
        public Guid? Auditors { get; set; }

        public bool CanSeeAll { get; set; } = false;
    }

    public class SearchResult
    {
        public Guid Id { get; set; }
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public Guid CreatorId { get; set; }
        public string Creator { get; set; }
        public Guid? ModifierId { get; set; }
        public string Modifier { get; set; }
        public int AuditStatus { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? ModifyTime { get; set; }
    }
}
