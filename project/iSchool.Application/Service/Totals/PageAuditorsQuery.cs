using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Totals
{
    public class PageAuditorsQuery : IRequest<PagedList<PageAuditorsQueryResult>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        //public DateTime Date { get; set; } = DateTime.Now.AddDays(-1).Date;
        public string OrderBy { get; set; } = "AuditCount";
        public string OrderBy_sc { get; set; } = "desc";
    }

    public class PageAuditorsQueryResult
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Account { get; set; }
		public long AuditCount { get; set; }
        public long? AuditExtCount { get; set; }
    }
}
