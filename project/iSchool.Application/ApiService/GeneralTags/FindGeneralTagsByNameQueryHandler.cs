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
    public class FindGeneralTagsByNameQueryHandler : IRequestHandler<FindGeneralTagsByNameQuery, GeneralTag[]>
    {
        UnitOfWork unitOfWork;
        IRepository<GeneralTag> generalTagRepository;

        public FindGeneralTagsByNameQueryHandler(IRepository<GeneralTag> generalTagRepository, IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.generalTagRepository = generalTagRepository;
        }

        public async Task<GeneralTag[]> Handle(FindGeneralTagsByNameQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = req.Like ? @"select * from GeneralTag where Name like @name"
                    : "select * from GeneralTag where Name=@name";

            var name = req.Like ? ("%" + req.Name + "%") : req.Name;

            var gtags = unitOfWork.DbConnection.Query<GeneralTag>(sql, new { name }).ToArray();
            return gtags;
        }
    }
}
