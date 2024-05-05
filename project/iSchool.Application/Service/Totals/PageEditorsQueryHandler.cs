using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class PageEditorsQueryHandler : IRequestHandler<PageEditorsQuery, PagedList<PageEditorsQueryResult>>
    {
        UnitOfWork unitOfWork;
        AppSettings appSettings;

        public PageEditorsQueryHandler(IUnitOfWork unitOfWork, AppSettings appSettings)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.appSettings = appSettings;
        }

        public async Task<PagedList<PageEditorsQueryResult>> Handle(PageEditorsQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            req.OrderBy = Regex.IsMatch(req.OrderBy, @"^[\d\w]+$") ? req.OrderBy : throw new ArgumentException("not orderby field");
            req.OrderBy_sc = (req.OrderBy_sc ?? "desc").ToLower();
            req.OrderBy_sc = req.OrderBy_sc.In("asc", "desc") ? req.OrderBy_sc : throw new ArgumentException("not asc|desc");

            var sql = $@"
select *,row_number()over(order by {req.OrderBy} {req.OrderBy_sc}) as _i
from (
 select t.*,u.Account,u.Username
 from TotalDateUserEditSchool t 
 inner join Total_User u on u.id=t.UserId and u.RoleId='{appSettings.GidEditor}' --and u.Date='{DateTime.Now.ToString("yyyy-MM-dd")}'
 where t.Date=(select max(Date) from TotalDateUserEditSchool)
) T
";
            sql = $@"

select count(1) from ({sql}) _t ;

SELECT * FROM ({sql}) _t where _i between (@PageIndex-1)*@PageSize+1 and @PageIndex*@PageSize ;
";

            var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, req);
            var pg = new PagedList<PageEditorsQueryResult>();
            pg.PageSize = req.PageSize;
            pg.CurrentPageIndex = req.PageIndex;
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();

            var items = (await gr.ReadAsync<PageEditorsQueryResult>()).AsList();

            pg.CurrentPageItems = items;
            return pg;   
        }
    }
}
