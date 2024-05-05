using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Application.ViewModels;
using AutoMapper;

namespace iSchool.Application.Service
{
    public class Alg2QueryHandler : IRequestHandler<Alg2Query, Alg2QyRstDto>
    {
        UnitOfWork unitOfWork;
        IMapper mapper;
        IRepository<School> repo_school;
        IRepository<SchoolExtension> repo_schext;
        IRepository<SchoolExtAlgHwf> repo_algHwf;

        public Alg2QueryHandler(IMapper mapper,
            IRepository<School> repo_school,
            IRepository<SchoolExtension> repo_schext,
            IRepository<SchoolExtAlgHwf> repo_algHwf,
            IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.mapper = mapper;
            this.repo_school = repo_school;
            this.repo_schext = repo_schext;
            this.repo_algHwf = repo_algHwf;
        }

        public Task<Alg2QyRstDto> Handle(Alg2Query req, CancellationToken cancellationToken)
        {
            return Task.FromResult(Handle(req));
        }

        Alg2QyRstDto Handle(Alg2Query req)
        {
            var sch = repo_school.GetIsValid(_ => _.Id == req.Sid);
            if (sch == null) throw new Exception("无效的学校");
            //if (!sch.Status.In((byte)SchoolStatus.Initial, (byte)SchoolStatus.Edit, (byte)SchoolStatus.Failed))
            //    throw new Exception("学校现在处于不可修改状态");

            var schext = repo_schext.GetIsValid(_ => _.Id == req.Eid && _.Sid == req.Sid);
            if (schext == null) throw new Exception("无效的学部");

            var dto = new Alg2QyRstDto();
            dto.Sid = req.Sid;
            dto.Eid = req.Eid;

            var algHwf = repo_algHwf.GetIsValid(_ => _.Eid == req.Eid);
            if (algHwf != null)
            {
                mapper.Map(algHwf, dto);
            }

            return dto;
        }
    }
}
