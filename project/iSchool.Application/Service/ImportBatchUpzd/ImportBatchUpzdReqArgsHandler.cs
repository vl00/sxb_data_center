using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Linq;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure.Dapper;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.Service.ImportBatchUpzd
{
    public class ImportBatchUpzdReqArgsHandler : IRequestHandler<ImportBatchUpzdReqArgs, ImportBatchUpzdResResult>
    {
        UnitOfWork _unitOfWork;
        IMediator _mediator;
        IServiceProvider services;

        public ImportBatchUpzdReqArgsHandler(IUnitOfWork unitOfWork, IMediator mediator, IServiceProvider sp)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.services = sp;
        }

        public async Task<ImportBatchUpzdResResult> Handle(ImportBatchUpzdReqArgs req, CancellationToken cancellationToken)
        {
            var res = new ImportBatchUpzdResResult();
            switch (1)
            {
                case 1 when req.Search != null:
                    await Handle_Search(res, req, cancellationToken);
                    break;
            }
            
            return res;
        }

        internal async Task Handle_Search(ImportBatchUpzdResResult res, ImportBatchUpzdReqArgs req, CancellationToken cancellationToken)
        {
            var sql = $@"
select count(1) from ImportBatchUpzdFlow where CreateTime>'2022-02-02' ;

select * from ImportBatchUpzdFlow where CreateTime>'2022-02-02'
order by CreateTime desc
OFFSET (@pageIndex-1)*@pageSize ROWS FETCH NEXT @pageSize ROWS ONLY
";
            var gr = await _unitOfWork.DbConnection.QueryMultipleAsync(sql, new { req.Search.PageIndex, req.Search.PageSize });
            res.SearchResult = new PagedList<ImportBatchUpzdResResult.SearchItem>();
            res.SearchResult.CurrentPageIndex = req.Search.PageIndex;
            res.SearchResult.PageSize = req.Search.PageSize;
            res.SearchResult.TotalItemCount = await gr.ReadFirstAsync<int>();
            res.SearchResult.CurrentPageItems = (await gr.ReadAsync<ImportBatchUpzdResResult.SearchItem>()).Select(x => 
            {
                x.CreateTime = Tryv(() => DateTime.Parse(x.CreateTime).ToString("yyyy-MM-dd HH:mm:ss"));
                x.UpdateTime = Tryv(() => DateTime.Parse(x.UpdateTime).ToString("yyyy-MM-dd HH:mm:ss"));
                x.User = string.IsNullOrEmpty(x.User) ? "" : x.User;
                return x;
            }).AsList();
        }
    }
}
