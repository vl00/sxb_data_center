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

namespace iSchool.Application.Service.Colleges
{
    public class CollegeQueryHandler : IRequestHandler<CollegeQuery, College[]>
    {
        UnitOfWork unitOfWork;

        public CollegeQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<College[]> Handle(CollegeQuery req, CancellationToken cancellationToken)
        {
            var ls = new List<College>(req.Ids.Length);
            foreach (var arr in getIds(req.Ids, 1000))
            {
                var sql = @"select * from dbo.College where IsValid=1 and Id in @Ids";
                var sgs = await unitOfWork.DbConnection.QueryAsync<College>(sql, new { Ids = arr });
                ls.AddRange(sgs);
            }
            return ls.ToArray();
        }

        static IEnumerable<Guid[]> getIds(Guid[] _ids, int idx = 1000)
        {
            var ids = _ids.AsMemory();
            for (var i = 0; i < ids.Length;)
            {
                var i0 = i;
                i = Math.Min(i + idx, ids.Length);
                yield return ids[i0..i].ToArray();
            }
        }
    }
}
