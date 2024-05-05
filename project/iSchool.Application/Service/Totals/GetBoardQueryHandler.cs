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
    public class GetBoardQueryHandler : IRequestHandler<GetBoardQuery, TotalDataboard>
    {
        UnitOfWork unitOfWork;

        public GetBoardQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
        }

        public async Task<TotalDataboard> Handle(GetBoardQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var sql = $@"
select * from dbo.{nameof(TotalDataboard)}
";
            var bs = await unitOfWork.DbConnection.QueryFirstOrDefaultAsync<TotalDataboard>(sql);

            return bs ?? new TotalDataboard();   
        }
    }
}
