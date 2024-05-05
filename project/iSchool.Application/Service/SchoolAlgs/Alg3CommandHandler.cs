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
    public class Alg3CommandHandler : IRequestHandler<Alg3Command>
    {
        UnitOfWork unitOfWork;
        IRepository<School> repo_school;
        IRepository<SchoolExtension> repo_schext;
        IRepository<SchoolExtAlgAchievement> repo_algAchi;

        public Alg3CommandHandler(
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

        public Task<Unit> Handle(Alg3Command cmd, CancellationToken cancellationToken)
        {
            var sch = repo_school.GetIsValid(_ => _.Id == cmd.Sid);
            if (sch == null) throw new Exception("无效的学校");
            if (!sch.Status.In((byte)SchoolStatus.Initial, (byte)SchoolStatus.Edit, (byte)SchoolStatus.Failed))
                throw new Exception("学校现在处于不可修改状态");

            var schext = repo_schext.GetIsValid(_ => _.Id == cmd.Eid && _.Sid == cmd.Sid);
            if (schext == null) throw new Exception("无效的学部");

            var algAchi = repo_algAchi.GetIsValid(_ => _.Eid == cmd.Eid);
            var isNew = algAchi == null;
            if (isNew)
            {
                algAchi = new SchoolExtAlgAchievement();
                algAchi.Id = Guid.NewGuid();
                algAchi.Eid = schext.Id;
                algAchi.Creator = cmd.UserId;
                algAchi.CreateTime = DateTime.Now;
            }
            algAchi.Modifier = cmd.UserId;
            algAchi.ModifyDateTime = DateTime.Now;
            algAchi.ExtamAvgscore = cmd.ExtamAvgscore;
            algAchi.No1Count = cmd.No1Count;
            algAchi.CmstuCount = cmd.CmstuCount;
            algAchi.RecruitCount = cmd.RecruitCount;
            algAchi.IsValid = true;

            try
            {
                unitOfWork.BeginTransaction();

                if (isNew) repo_algAchi.Insert(algAchi);
                else repo_algAchi.Update(algAchi);

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
