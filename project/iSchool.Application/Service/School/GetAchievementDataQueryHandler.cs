using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using System.Linq;
using MediatR;

namespace iSchool.Application.Service
{
    public class GetAchievementDataQueryHandler : IRequestHandler<GetAchievementDataQuery, HttpResponse<object>>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<SchoolAchievement> _schoolAchievementRepository;
        private readonly IRepository<MiddleSchoolAchievement> _middleschoolAchievementRepository;
        private readonly IRepository<PrimarySchoolAchievement> _primaryschoolAchievementRepository;
        private readonly IRepository<HighSchoolAchievement> _highschoolAchievementRepository;
        private readonly IRepository<KindergartenAchievement> _kindergartenAchievementRepository;

        public GetAchievementDataQueryHandler(
            IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<SchoolAchievement> schoolAchievementRepository,
            IRepository<MiddleSchoolAchievement> middleschoolAchievementRepository,
            IRepository<PrimarySchoolAchievement> primaryschoolAchievementRepository,
            IRepository<HighSchoolAchievement> highschoolAchievementRepository,
            IRepository<KindergartenAchievement> kindergartenAchievementRepository)
        {
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolAchievementRepository = schoolAchievementRepository;
            _middleschoolAchievementRepository = middleschoolAchievementRepository;
            _primaryschoolAchievementRepository = primaryschoolAchievementRepository;
            _highschoolAchievementRepository = highschoolAchievementRepository;
            _kindergartenAchievementRepository = kindergartenAchievementRepository;
        }

        public Task<HttpResponse<object>> Handle(GetAchievementDataQuery request, CancellationToken cancellationToken)
        {
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            if (ext == null)
            {
                return Task.FromResult(new HttpResponse<object> { State = 404, Message = "不存在该分部的升学成绩！" });
            }
            else
            {
                var grade = ext.Grade;
                var type = ext.Type;
                var schftype = new SchFType0(ext.SchFtype);

                //普通高中
                if (grade == (byte)iSchool.Domain.Enum.SchoolGrade.SeniorMiddleSchool
                   && SchUtils.Canshow2("hsa.keyundergraduate", schftype))
                {
                    var ach = _highschoolAchievementRepository.GetIsValid(p => p.ExtId == request.ExtId && p.Year == request.Year);
                    if (ach == null) ach = new HighSchoolAchievement();
                    float Completion = 0;
                    //高中
                    var schoolach = _schoolAchievementRepository.GetIsValid(p => p.ExtId == request.ExtId && p.Year == request.Year);
                    if (schoolach != null) { Completion = schoolach.Completion; }
                    var result = new HttpResponse<object> { State = 200, Result = new { Data = ach, completion = Completion } };
                    return Task.FromResult(result);
                }
                //初中
                else if (grade == (byte)iSchool.Domain.Enum.SchoolGrade.JuniorMiddleSchool)
                {
                    var ach = _middleschoolAchievementRepository.GetIsValid(p => p.ExtId == request.ExtId && p.Year == request.Year);
                    if (ach == null) ach = new MiddleSchoolAchievement();
                    //初中
                    float Completion = 0;
                    var schoolach = _schoolAchievementRepository.GetIsValid(p => p.ExtId == request.ExtId && p.Year == request.Year);
                    if (schoolach != null) { Completion = schoolach.Completion; }
                    var result = new HttpResponse<object> { State = 200, Result = new { Data = ach, completion = Completion } };
                    return Task.FromResult(result);
                }
                //小学
                else if (grade == (byte)iSchool.Domain.Enum.SchoolGrade.PrimarySchool)
                {
                    var ach = _primaryschoolAchievementRepository.GetAll(p => p.IsValid == true && p.Year == request.Year && p.ExtId == request.ExtId).OrderBy(p => p.CreateTime);
                    var result = new HttpResponse<object> { State = 200, Result = new { Data = ach } };
                    return Task.FromResult(result);
                }
                //幼儿园
                else if (grade == (byte)iSchool.Domain.Enum.SchoolGrade.Kindergarten)
                {
                    var ach = _kindergartenAchievementRepository.GetAll(p => p.IsValid == true && p.Year == request.Year && p.ExtId == request.ExtId).OrderBy(p => p.CreateTime);
                    var result = new HttpResponse<object> { State = 200, Result = new { Data = ach } };
                    return Task.FromResult(result);
                }

                return Task.FromResult(new HttpResponse<object> { State = 404, Message = "不存在该分部的升学成绩！" });
            }
        }
    }
}
