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

namespace iSchool.Application.Service
{
    public class FindGeneralTagsByIdsQueryHandler : IRequestHandler<FindGeneralTagsByIdsQuery, GeneralTag[]>
    {
        UnitOfWork unitOfWork;
        IRepository<GeneralTag> generalTagRepository;

        public FindGeneralTagsByIdsQueryHandler(IRepository<GeneralTag> generalTagRepository, IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.generalTagRepository = generalTagRepository;
        }

        public async Task<GeneralTag[]> Handle(FindGeneralTagsByIdsQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = @"select * from GeneralTag where Id in @ids";
            var gtags = unitOfWork.DbConnection.Query<GeneralTag>(sql, new { ids = req.Ids }).ToArray();
            return gtags;
        }
    }
}
