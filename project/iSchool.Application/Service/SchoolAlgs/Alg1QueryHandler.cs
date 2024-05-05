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
    public class Alg1QueryHandler : IRequestHandler<Alg1Query, Alg1QyRstDto>
    {
        UnitOfWork unitOfWork;
        IRepository<School> repo_school;
        IRepository<SchoolExtension> repo_schext;
        IRepository<SchoolExtAlgTeQuality> repo_alg1;
        IRepository<SchoolExtAlgCourseKind> repo_alg2;

        public Alg1QueryHandler(
            IRepository<School> repo_school,
            IRepository<SchoolExtension> repo_schext,
            IRepository<SchoolExtAlgTeQuality> repo_alg1,
            IRepository<SchoolExtAlgCourseKind> repo_alg2,
            IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.repo_school = repo_school;
            this.repo_schext = repo_schext;
            this.repo_alg1 = repo_alg1;
            this.repo_alg2 = repo_alg2;
        }

        public Task<Alg1QyRstDto> Handle(Alg1Query req, CancellationToken cancellationToken)
        {
            return Task.FromResult(Handle(req));
        }

        Alg1QyRstDto Handle(Alg1Query req)
        {
            var sch = repo_school.GetIsValid(_ => _.Id == req.Sid);
            if (sch == null) throw new Exception("无效的学校");
            //if (!sch.Status.In((byte)SchoolStatus.Initial, (byte)SchoolStatus.Edit, (byte)SchoolStatus.Failed))
            //    throw new Exception("学校现在处于不可修改状态");

            var schext = repo_schext.GetIsValid(_ => _.Id == req.Eid && _.Sid == req.Sid);
            if (schext == null) throw new Exception("无效的学部");

            var dto = new Alg1QyRstDto();
            dto.Eid = req.Eid;
            dto.Ext.Sid = req.Sid;
            dto.Ext.Eid = req.Eid;
            dto.Ext.Grade = schext.Grade;
            dto.Ext.Type = schext.Type;
            dto.Ext.Discount = schext.Discount;
            dto.Ext.Diglossia = schext.Diglossia;
            dto.Ext.Chinese = schext.Chinese;

            var alg1 = repo_alg1.GetIsValid(_ => _.Eid == req.Eid);
            dto.TeacherCount = alg1?.TeacherCount;
            dto.FgnTeacherCount = alg1?.FgnTeacherCount;
            dto.UndergduateOverCount = alg1?.UndergduateOverCount;
            dto.GduateOverCount = alg1?.GduateOverCount;
            dto.TeacherHonor = alg1?.TeacherHonor?.ToObject<KeyValueDto<int>[]>() ?? new KeyValueDto<int>[0];

            var alg2 = repo_alg2.GetIsValid(_ => _.Eid == req.Eid);
            dto.SubjsKvs = alg2?.SubjsJson.ToObject<KeyValueDto<string>[]>() ?? new KeyValueDto<string>[0];
            dto.ArtsKvs = alg2?.ArtsJson.ToObject<KeyValueDto<string>[]>() ?? new KeyValueDto<string>[0];
            dto.SportsKvs = alg2?.SportsJson.ToObject<KeyValueDto<string>[]>() ?? new KeyValueDto<string>[0];
            dto.ScienceKvs = alg2?.ScienceJson.ToObject<KeyValueDto<string>[]>() ?? new KeyValueDto<string>[0];

            return dto;
        }
    }
}
