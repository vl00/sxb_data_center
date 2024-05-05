using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Dapper;
using System.Linq;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service
{
    public class SearchSchoolQueryHandler : IRequestHandler<SearchSchoolQuery, List<KeyValueDto<Guid>>>
    {
        private IMediator mediator;
        private UnitOfWork UnitOfWork { get; set; }

        public SearchSchoolQueryHandler(IMediator mediator, IUnitOfWork unitOfWork)
        {
            UnitOfWork = (UnitOfWork)unitOfWork;
            this.mediator = mediator;
        }

        #region old-code
        //public Task<List<KeyValueDto<Guid>>> Handle(SearchSchoolQuery request, CancellationToken cancellationToken)
        //{

        //    var sql = new StringBuilder();
        //    if (request.IsCollage)
        //        sql.Append($"select top {request.Top} name AS [key],id AS [value] FROM dbo.College where IsValid=1 and name like CONCAT('%',@name,'%');");
        //    else
        //    {
        //        if (request.ContainExt)
        //        {
        //            sql.Append($@"select top {request.Top} sch.name+'_'+ext.name AS [key], ext.id AS[value] FROM   {(request.IsOnline ? "dbo.OnlineSchoolExtension" : "SchoolExtension")} AS ext
        //             LEFT JOIN {(request.IsOnline ? "dbo.OnlineSchool" : "dbo.School")}  AS  sch ON ext.sid=sch.id 
        //             WHERE {(request.Grade == 0 ? string.Empty : " ext.grade=@grade AND") }  ext.IsValid=1 and sch.IsValid=1 AND sch.name like CONCAT('%',@name,'%');");
        //        }
        //        else
        //        {
        //            sql.Append($@"select top {request.Top} sch.name AS [key],sch.id as [value] from
        //                        {(request.IsOnline ? "dbo.OnlineSchool" : "dbo.School")} AS sch  WHERE {(request.Grade == 0 ? string.Empty : " ext.grade=@grade AND") }  sch.IsValid=1 AND sch.name like CONCAT('%',@name,'%');");
        //        }
        //    }
        //    var data = UnitOfWork.DbConnection.Query<KeyValueDto<Guid>>(sql.ToString(), new { name = request.Name, grade = request.Grade },
        //        UnitOfWork.DbTransaction).ToList();
        //    return Task.FromResult(data);
        //}
        #endregion

        public async Task<List<KeyValueDto<Guid>>> Handle(SearchSchoolQuery request, CancellationToken cancellationToken)
        {            
            if (request.IsCollage)
            {
                var sql = $"select top {request.Top} name AS [key],id AS [value] FROM dbo.College where IsValid=1 and name like CONCAT('%',@name,'%');";
                var data = await UnitOfWork.DbConnection.QueryAsync<KeyValueDto<Guid>>(sql, new { name = request.Name, grade = request.Grade });
                return data.AsList();
            }
            else
            {
                var fr = await mediator.Send(new Searchs.ESearchLikeQuery
                {
                    Name = request.Name,
                    AuditStatus = request.IsOnline ? new[] { (int)SchoolAuditStatus.Success } : null,
                    PageIndex = 1,
                    PageSize = request.Top,
                    Grade = request.Grade == 0 ? (byte?)null : request.Grade,
                });
                if (!fr.IsOk) throw new FnResultException(fr.Code, fr.Msg);

                var sids = fr.Data.CurrentPageItems.AsArray();

                var sql = $"select name as [key],id as [value] from school where id in @sids";

                var ls = await UnitOfWork.DbConnection.QueryAsync<KeyValueDto<Guid>>(sql, new { sids });

                return ls.OrderBy(_ => Array.IndexOf(sids, _.Value)).ToList();
            }
            
        }
    }
}
