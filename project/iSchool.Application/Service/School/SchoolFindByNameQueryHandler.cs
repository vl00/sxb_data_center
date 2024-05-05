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
    public class SchoolFindByNameQueryHandler : IRequestHandler<SchoolFindByNameQuery, SchoolFindByNameQryResult>
    {
        UnitOfWork _unitOfWork;
        IRepository<School> _schoolRepository;

        public SchoolFindByNameQueryHandler(IRepository<School> schoolRepository, IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
            this._schoolRepository = schoolRepository;
        }

        public async Task<SchoolFindByNameQryResult> Handle(SchoolFindByNameQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var res = new SchoolFindByNameQryResult();

            if (!req.Name.IsNullOrEmpty())
            {
                var schs = _schoolRepository.GetAll(_ => _.IsValid && _.Name == req.Name);
                res.Ls = schs.AsArray();
            }

            return res;
        }
    }
}
