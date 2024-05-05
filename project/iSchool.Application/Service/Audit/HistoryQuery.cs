using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class HistoryQuery : IRequest<PagedList<HistoryQueryResult>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string SchoolId { get; set; }
    }

    public class HistoryQueryResult
    {
        public Guid ModifierId { get; set; }
        public string Modifier { get; set; }
        public DateTime ModifyTime { get; set; }
        public string AuditMessage { get; set; }
        public byte Status { get; set; }
    }
}
