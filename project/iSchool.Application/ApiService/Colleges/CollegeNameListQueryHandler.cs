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
    public class CollegeNameListQueryHandler : IRequestHandler<CollegeNameListQuery, College[]>
    {
        UnitOfWork unitOfWork;

        public CollegeNameListQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<College[]> Handle(CollegeNameListQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = $@"select * from College where [name] in @Names and IsValid=1";

            var sgs = unitOfWork.DbConnection.Query<College>(sql, req);

            return sgs.ToArray();
        }
    }
}
