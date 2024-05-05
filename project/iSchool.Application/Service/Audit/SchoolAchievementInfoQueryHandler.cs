using Dapper;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Domain.Enum;
using iSchool.Application.ViewModels;

namespace iSchool.Application.Service.Audit
{
    public class SchoolAchievementInfoQueryHandler : IRequestHandler<SchoolAchievementInfoQuery, SchoolAchievementInfoQueryResult>
    {
        UnitOfWork unitOfWork;
        IRepository<SchoolExtension> schoolExtensionRepository;
        IRepository<HighSchoolAchievement> repy_HighSchoolAchievement;
        IRepository<MiddleSchoolAchievement> repy_MiddleSchoolAchievement;
        IRepository<PrimarySchoolAchievement> repy_PrimarySchoolAchievement;
        IRepository<KindergartenAchievement> repy_KindergartenAchievement;

        public SchoolAchievementInfoQueryHandler(IUnitOfWork unitOfWork,
            IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<HighSchoolAchievement> repy_HighSchoolAchievement,
            IRepository<MiddleSchoolAchievement> repy_MiddleSchoolAchievement,
            IRepository<PrimarySchoolAchievement> repy_PrimarySchoolAchievement,
            IRepository<KindergartenAchievement> repy_KindergartenAchievement)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.schoolExtensionRepository = schoolExtensionRepository;
            this.repy_HighSchoolAchievement = repy_HighSchoolAchievement;
            this.repy_MiddleSchoolAchievement = repy_MiddleSchoolAchievement;
            this.repy_PrimarySchoolAchievement = repy_PrimarySchoolAchievement;
            this.repy_KindergartenAchievement = repy_KindergartenAchievement;
        }

        public Task<SchoolAchievementInfoQueryResult> Handle(SchoolAchievementInfoQuery req, CancellationToken cancellationToken)
        {
            var res = new SchoolAchievementInfoQueryResult();

            var sgext = schoolExtensionRepository.GetAll(_ => _.IsValid == true && _.Id == req.Eid).FirstOrDefault();
            if (sgext == null) throw new Exception($"不存在的学校eid={sgext.Id}");

            var schoolGrade = (Domain.Enum.SchoolGrade)sgext.Grade;
            var schoolType = (SchoolType)sgext.Type;

            switch (schoolGrade)
            {
                //国际|外籍 高中
                case Domain.Enum.SchoolGrade.SeniorMiddleSchool when !SchUtils.Canshow2("hsa.keyundergraduate", new SchFType0(sgext.SchFtype)):
                    res = new SchoolAchievementInfoQueryResult();
                    break;

                //高中
                case Domain.Enum.SchoolGrade.SeniorMiddleSchool:
                    res = Handle_highSchool(req, sgext, schoolGrade, schoolType);
                    break;

                //初中
                case Domain.Enum.SchoolGrade.JuniorMiddleSchool:
                    res = Handle_middleSchool(req, sgext, schoolGrade, schoolType);
                    break;

                //小学
                case Domain.Enum.SchoolGrade.PrimarySchool:
                    res = Handle_primarySchool(req, sgext, schoolGrade, schoolType);
                    break;

                //幼儿园
                case Domain.Enum.SchoolGrade.Kindergarten:
                    res = Handle_kindergartenSchool(req, sgext, schoolGrade, schoolType);
                    break;
            }
            res.Eid = req.Eid;
            res.Year = req.Year;
            res.SchFtype = sgext.SchFtype;
            res.SchoolGrade = schoolGrade;
            res.SchoolType = schoolType;

            return Task.FromResult(res);
        }

        HighSchoolAchievementInfoQueryResult Handle_highSchool(SchoolAchievementInfoQuery req, SchoolExtension sg, Domain.Enum.SchoolGrade schoolGrade, SchoolType schoolType)
        {
            var res = new HighSchoolAchievementInfoQueryResult();

            res.Data = repy_HighSchoolAchievement.GetAll(_ => _.IsValid == true && _.ExtId == req.Eid && _.Year == req.Year).FirstOrDefault();

            return res;
        }

        MiddleSchoolAchievementInfoQueryResult Handle_middleSchool(SchoolAchievementInfoQuery req, SchoolExtension sg, Domain.Enum.SchoolGrade schoolGrade, SchoolType schoolType)
        {
            var res = new MiddleSchoolAchievementInfoQueryResult();

            res.Data = repy_MiddleSchoolAchievement.GetAll(_ => _.IsValid == true && _.ExtId == req.Eid && _.Year == req.Year).FirstOrDefault();

            return res;
        }

        PrimarySchoolAchievementInfoQueryResult Handle_primarySchool(SchoolAchievementInfoQuery req, SchoolExtension sg, Domain.Enum.SchoolGrade schoolGrade, SchoolType schoolType)
        {
            var res = new PrimarySchoolAchievementInfoQueryResult();

            res.Data = repy_PrimarySchoolAchievement.GetAll(_ => _.IsValid == true && _.ExtId == req.Eid && _.Year == req.Year).ToArray();

            return res;
        }

        KindergartenAchievementInfoQueryResult Handle_kindergartenSchool(SchoolAchievementInfoQuery req, SchoolExtension sg, Domain.Enum.SchoolGrade schoolGrade, SchoolType schoolType)
        {
            var res = new KindergartenAchievementInfoQueryResult();

            res.Data = repy_KindergartenAchievement.GetAll(_ => _.IsValid == true && _.ExtId == req.Eid && _.Year == req.Year).ToArray();

            return res;
        }
    }
}
