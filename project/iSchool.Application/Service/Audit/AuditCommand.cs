using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class AuditCommand : IRequest
    {
        public Guid Id { get; set; }
        public string Msg { get; set; }
        public bool Fail { get; set; }

        public Guid AuditorId { get; set; }
        public bool CanDoAll { get; set; }
    }
}
