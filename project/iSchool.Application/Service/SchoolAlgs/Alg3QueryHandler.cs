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

namespace iSchool.Application.Service
{
    public class Alg3QueryHandler : IRequestHandler<Alg3Query, Alg3QyRstDto>
    {
        UnitOfWork unitOfWork;
        IRepository<School> repo_school;
        IRepository<SchoolExtension> repo_schext;
        IRepository<SchoolExtAlgAchievement> repo_algAchi;

        public Alg3QueryHandler(
            IRepository<School> repo_school,
            IRepository<SchoolExtension> repo_schext,
            IRepository<SchoolExtAlgAchievement> repo_algAchi,
            IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.repo_school = repo_school;
            this.repo_schext = repo_schext;
            this.repo_algAchi = repo_algAchi;
        }

        public Task<Alg3QyRstDto> Handle(Alg3Query req, CancellationToken cancellationToken)
        {
            return Task.FromResult(Handle(req));
        }

        Alg3QyRstDto Handle(Alg3Query req)
        {
            var sch = repo_school.GetIsValid(_ => _.Id == req.Sid);
            if (sch == null) throw new Exception("无效的学校");
            //if (!sch.Status.In((byte)SchoolStatus.Initial, (byte)SchoolStatus.Edit, (byte)SchoolStatus.Failed))
            //    throw new Exception("学校现在处于不可修改状态");

            var schext = repo_schext.GetIsValid(_ => _.Id == req.Eid && _.Sid == req.Sid);
            if (schext == null) throw new Exception("无效的学部");

            var dto = new Alg3QyRstDto();
            dto.Ext.Sid = req.Sid;
            dto.Ext.Eid = req.Eid;
            dto.Ext.Grade = schext.Grade;
            dto.Ext.Type = schext.Type;
            dto.Ext.Discount = schext.Discount;
            dto.Ext.Diglossia = schext.Diglossia;
            dto.Ext.Chinese = schext.Chinese;

            var algAchi = repo_algAchi.GetIsValid(_ => _.Eid == req.Eid);
            dto.ExtamAvgscore = algAchi?.ExtamAvgscore;
            dto.No1Count = algAchi?.No1Count;
            dto.CmstuCount = algAchi?.CmstuCount;
            dto.RecruitCount = algAchi?.RecruitCount;

            return dto;
        }
    }
}
