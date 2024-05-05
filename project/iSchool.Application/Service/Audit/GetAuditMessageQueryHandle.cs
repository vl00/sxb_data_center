using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service.Audit
{
    public class GetAuditMessageQueryHandle : IRequestHandler<GetAuditMessageQuery, string>
    {
        private UnitOfWork UnitOfWork { get; set; }

        public GetAuditMessageQueryHandle(IUnitOfWork unitOfWork)
        {
            UnitOfWork = (UnitOfWork)unitOfWork;
        }

        public Task<string> Handle(GetAuditMessageQuery request, CancellationToken cancellationToken)
        {
            var QueryAuditSql = @"select top 1 * from dbo.SchoolAudit a where a.Sid=@Sid and a.IsValid=1 order by CreateTime desc";
            var a = UnitOfWork.DbConnection.QueryFirstOrDefault<SchoolAudit>(QueryAuditSql, new { Sid = request.Sid });
            return Task.FromResult(a?.AuditMessage);
        }
    }
}
