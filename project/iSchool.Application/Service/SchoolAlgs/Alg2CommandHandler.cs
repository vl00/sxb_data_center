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
using AutoMapper;

namespace iSchool.Application.Service
{
    public class Alg2CommandHandler : IRequestHandler<Alg2Command>
    {
        UnitOfWork unitOfWork;
        IMapper mapper;
        IRepository<School> repo_school;
        IRepository<SchoolExtension> repo_schext;
        IRepository<SchoolExtAlgHwf> repo_algHwf;

        public Alg2CommandHandler(IMapper mapper,
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

        public Task<Unit> Handle(Alg2Command cmd, CancellationToken cancellationToken)
        {
            var sch = repo_school.GetIsValid(_ => _.Id == cmd.Dto.Sid);
            if (sch == null) throw new Exception("无效的学校");
            if (!sch.Status.In((byte)SchoolStatus.Initial, (byte)SchoolStatus.Edit, (byte)SchoolStatus.Failed))
                throw new Exception("学校现在处于不可修改状态");

            var schext = repo_schext.GetIsValid(_ => _.Id == cmd.Dto.Eid && _.Sid == cmd.Dto.Sid);
            if (schext == null) throw new Exception("无效的学部");

            var algHwf = repo_algHwf.GetIsValid(_ => _.Eid == cmd.Dto.Eid);
            var isNew = algHwf == null;
            if (isNew)
            {
                algHwf = new SchoolExtAlgHwf();
                algHwf.Id = Guid.NewGuid();
                algHwf.Eid = schext.Id;
                algHwf.Creator = cmd.UserId;
                algHwf.CreateTime = DateTime.Now;
            }
            mapper.Map(cmd.Dto, algHwf);
            algHwf.Modifier = cmd.UserId;
            algHwf.ModifyDateTime = DateTime.Now;
            algHwf.IsValid = true;

            if (isNew) repo_algHwf.Insert(algHwf);
            else repo_algHwf.Update(algHwf);

            return Task.FromResult(Unit.Value);
        }
    }
}
