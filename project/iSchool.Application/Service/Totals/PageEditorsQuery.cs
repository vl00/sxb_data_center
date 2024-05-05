using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Totals
{
    public class PageEditorsQuery : IRequest<PagedList<PageEditorsQueryResult>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        //public DateTime Date { get; set; } = DateTime.Now.AddDays(-1).Date;
        public string OrderBy { get; set; } = "SchoolEntryCount";
        public string OrderBy_sc { get; set; } = "desc";
    }

    public class PageEditorsQueryResult
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Account { get; set; }
        public long SchoolEntryCount { get; set; }
        public long? SchoolExtEntryCount { get; set; }
        public long AuditSuccessCount { get; set; }
        public long UnAuditCount { get; set; }
        public long AuditFailCount { get; set; }
    }
}
