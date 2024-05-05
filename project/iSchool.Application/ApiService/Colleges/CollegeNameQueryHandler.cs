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
    public class CollegeNameQueryHandler : IRequestHandler<CollegeNameQuery, College[]>
    {
        UnitOfWork unitOfWork;

        public CollegeNameQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<College[]> Handle(CollegeNameQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = $@"select top {req.Count} * from College where [name] like @Name and IsValid=1 ";

            var sgs = unitOfWork.DbConnection.Query<College>(sql, new { Name = $"%{req.Name}%" });

            return sgs.ToArray();
        }
    }
}
