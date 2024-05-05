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
using System.Text.RegularExpressions;

namespace iSchool.Application.Service.Totals
{
    public class PageAuditorsQueryHandler : IRequestHandler<PageAuditorsQuery, PagedList<PageAuditorsQueryResult>>
    {
        UnitOfWork unitOfWork;
        AppSettings appSettings;

        public PageAuditorsQueryHandler(IUnitOfWork unitOfWork, AppSettings appSettings)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.appSettings = appSettings;
        }

        public async Task<PagedList<PageAuditorsQueryResult>> Handle(PageAuditorsQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            req.OrderBy = Regex.IsMatch(req.OrderBy, @"^[\d\w]+$") ? req.OrderBy : throw new ArgumentException("not orderby field");
            req.OrderBy_sc = (req.OrderBy_sc ?? "desc").ToLower();
            req.OrderBy_sc = req.OrderBy_sc.In("asc", "desc") ? req.OrderBy_sc : throw new ArgumentException("not asc|desc");

            var sql = $@"
select *,row_number()over(order by {req.OrderBy} {req.OrderBy_sc}) as _i
from (
 select t.*,u.Account,u.Username
 from TotalDateUserAuditSchool t 
 inner join Total_User u on u.id=t.UserId and u.QxId='{appSettings.GidQxAudit}' --and u.Date='{DateTime.Now.ToString("yyyy-MM-dd")}'
 where t.Date=(select max(Date) from TotalDateUserAuditSchool)
)T
";
            sql = $@"
select count(1) from ({sql}) _t ;

SELECT * FROM ({sql}) _t where _i between (@PageIndex-1)*@PageSize+1 and @PageIndex*@PageSize ;
";

            var gr = await unitOfWork.DbConnection.QueryMultipleAsync(sql, req);
            var pg = new PagedList<PageAuditorsQueryResult>();
            pg.PageSize = req.PageSize;
            pg.CurrentPageIndex = req.PageIndex;
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();

            var items = (await gr.ReadAsync<PageAuditorsQueryResult>()).AsList();
            //var ns = AdminInfoUtil.GetUsers(items.Select(p => p.UserId));
            //items.ForEach(p => 
            //{
            //    p.Username = ns[p.UserId].Displayname;
            //    p.Account = ns[p.UserId].Name;
            //});

            pg.CurrentPageItems = items;
            return pg;   
        }
    }
}
