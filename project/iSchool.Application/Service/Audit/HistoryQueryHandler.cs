using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Infrastructure.Dapper;
using iSchool.Domain.Enum;

namespace iSchool.Application.Service.Audit
{
    public class HistoryQueryHandler : IRequestHandler<HistoryQuery, PagedList<HistoryQueryResult>>
    {
        UnitOfWork unitOfWork;

        public HistoryQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
        }

        public async Task<PagedList<HistoryQueryResult>> Handle(HistoryQuery request, CancellationToken cancellationToken)
        {
            var sql = $@"
select AuditMessage,[Status],Modifier as ModifierId,ModifyDateTime as ModifyTime,ROW_NUMBER() OVER(ORDER BY ModifyDateTime desc)as _rowid
from dbo.SchoolAudit 
where [Status] in ({SchoolAuditStatus.Failed.ToInt()},{SchoolAuditStatus.Success.ToInt()}) and IsValid=1 and sid=@SchoolId
";
            var sb = new StringBuilder();

            sql = string.Format(sql, sb);
            sql = $@"
select count(1) from ({sql}) _t ;

SELECT * FROM (SELECT * from ({sql}) _t0) _t where _rowid between (@PageIndex-1)*@PageSize+1 and @PageIndex*@PageSize ;
";
            var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, request);
            var pg = new PagedList<HistoryQueryResult>();
            pg.PageSize = request.PageSize;
            pg.CurrentPageIndex = request.PageIndex;
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();
            pg.CurrentPageItems = await gr.ReadAsync<HistoryQueryResult>();

            var ms = AdminInfoUtil.GetNames(pg.CurrentPageItems.Select(_ => _.ModifierId));
            pg.CurrentPageItems.AsList().ForEach(p => p.Modifier = ms[p.ModifierId]);

            return pg;
        }
    }
}
