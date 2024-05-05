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
using iSchool.Domain.Modles;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using MediatR;
using iSchool.Infrastructure.Dapper;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.Service.Audit
{
    public class SearchSchool4TypeaheadQueryHandler : IRequestHandler<SearchSchool4TypeaheadQuery, SearchSchool4TypeaheadQueryResult[]>
    {
        UnitOfWork unitOfWork;
        IMediator mediator;

        public SearchSchool4TypeaheadQueryHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.mediator = mediator;
        }

        #region old code
        //        public async Task<SearchSchool4TypeaheadQueryResult[]> Handle(SearchSchool4TypeaheadQuery req, CancellationToken cancellationToken)
        //        {
        //            await Task.CompletedTask;
        //            var sql = $@"
        //select top {req.Top} s.id as sid,s.name as sgname from dbo.{(req.IsOnline ? nameof(OnlineSchool) : nameof(School))} s 
        //where s.IsValid=1 and s.name like CONCAT('%',@Keyword,'%') ;
        //";

        //            var res = unitOfWork.DbConnection.Query<SearchSchool4TypeaheadQueryResult>(sql, req);
        //            return res.ToArray();
        //        }
        #endregion

        public async Task<SearchSchool4TypeaheadQueryResult[]> Handle(SearchSchool4TypeaheadQuery req, CancellationToken cancellationToken)
        {
            var fr = await mediator.Send(new Searchs.ESearchLikeQuery
            {
                Name = req.Keyword,
                AuditStatus = req.IsOnline ? new[] { (int)SchoolAuditStatus.Success } : null,
                PageIndex = 1,
                PageSize = req.Top,                
            });
            if (!fr.IsOk) throw new FnResultException(fr.Code, fr.Msg);

            var sids = fr.Data.CurrentPageItems.AsArray();

            var sql = $"select name as Sgname,id as sid from school where id in @sids";

            var ls = await unitOfWork.DbConnection.QueryAsync<SearchSchool4TypeaheadQueryResult>(sql, new { sids });

            return ls.OrderBy(_ => Array.IndexOf(sids, _.Sid)).ToArray();
        }
    }
}
