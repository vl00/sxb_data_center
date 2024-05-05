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

namespace iSchool.Application.Service
{
    public class Alg1CommandHandler : IRequestHandler<Alg1Command>
    {
        UnitOfWork unitOfWork;
        IRepository<School> repo_school;
        IRepository<SchoolExtension> repo_schext;
        IRepository<SchoolExtAlgTeQuality> repo_alg1;
        IRepository<SchoolExtAlgCourseKind> repo_alg2;

        public Alg1CommandHandler(
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

        public Task<Unit> Handle(Alg1Command cmd, CancellationToken cancellationToken)
        {
            var sch = repo_school.GetIsValid(_ => _.Id == cmd.Sid);
            if (sch == null) throw new Exception("无效的学校");
            if (!sch.Status.In((byte)SchoolStatus.Initial, (byte)SchoolStatus.Edit, (byte)SchoolStatus.Failed))
                throw new Exception("学校现在处于不可修改状态");

            var schext = repo_schext.GetIsValid(_ => _.Id == cmd.Dto.Eid && _.Sid == cmd.Sid);
            if (schext == null) throw new Exception("无效的学部");

            var alg1 = repo_alg1.GetIsValid(_ => _.Eid == cmd.Dto.Eid);
            var isNew1 = alg1 == null;
            if (isNew1)
            {
                alg1 = new SchoolExtAlgTeQuality();
                alg1.Id = Guid.NewGuid();
                alg1.Eid = schext.Id;
                alg1.Creator = cmd.UserId;
                //alg1.CreateTime = DateTime.Now;
            }
            alg1.Modifier = cmd.UserId;
            alg1.ModifyDateTime = DateTime.Now;
            alg1.TeacherCount = cmd.Dto.TeacherCount;
            alg1.FgnTeacherCount = cmd.Dto.FgnTeacherCount;
            alg1.UndergduateOverCount = cmd.Dto.UndergduateOverCount;
            alg1.GduateOverCount = cmd.Dto.GduateOverCount;
            alg1.TeacherHonor = cmd.Dto.TeacherHonor.ToJsonString();

            var alg2 = repo_alg2.GetIsValid(_ => _.Eid == cmd.Dto.Eid);
            var isNew2 = alg2 == null;
            if (isNew2)
            {
                alg2 = new SchoolExtAlgCourseKind();
                alg2.Id = Guid.NewGuid();
                alg2.Eid = cmd.Dto.Eid;
                alg2.Creator = cmd.UserId;
            }
            alg2.Modifier = cmd.UserId;
            alg2.ModifyDateTime = DateTime.Now;
            alg2.SubjsJson = cmd.Dto.SubjsKvs.ToJsonString();
            alg2.ArtsJson = cmd.Dto.ArtsKvs.ToJsonString();
            alg2.SportsJson = cmd.Dto.SportsKvs.ToJsonString();
            alg2.ScienceJson = cmd.Dto.ScienceKvs.ToJsonString();

            try
            {
                unitOfWork.BeginTransaction();

                if (isNew1) repo_alg1.Insert(alg1);
                else repo_alg1.Update(alg1);

                if (isNew2) repo_alg2.Insert(alg2);
                else repo_alg2.Update(alg2);

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw ex;
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
