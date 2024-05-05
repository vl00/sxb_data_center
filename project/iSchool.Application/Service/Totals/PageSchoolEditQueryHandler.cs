using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Infrastructure.Dapper;
using iSchool.Domain.Modles;
using Microsoft.Extensions.Options;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.Service.Totals
{
    public class PageSchoolEditQueryHandler : IRequestHandler<PageSchoolEditQuery, PagedList<TotalDateSchoolEdit>>
    {
        UnitOfWork unitOfWork;

        public PageSchoolEditQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
        }

        public async Task<PagedList<TotalDateSchoolEdit>> Handle(PageSchoolEditQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var sql = $@"
select *,row_number()over(order by Date desc) as _i
from TotalDateSchoolEdit 
where 1=1 {$"and Date='{req.Date?.ToString("yyyy-MM-dd")}'".If(req.Date != null)}
";
            sql = $@"
select count(1) from ({sql}) _t ;

SELECT * FROM ({sql}) _t where _i between (@PageIndex-1)*@PageSize+1 and @PageIndex*@PageSize ;
";

            var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, req);
            var pg = new PagedList<TotalDateSchoolEdit>();
            pg.PageSize = req.PageSize;
            pg.CurrentPageIndex = req.PageIndex;
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();
            pg.CurrentPageItems = await gr.ReadAsync<TotalDateSchoolEdit>();
            return pg;   
        }
    }
}
