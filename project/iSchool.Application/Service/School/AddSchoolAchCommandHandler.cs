using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Dapper;
using iSchool.Application.ViewModels;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 添加高中的升学成绩
    /// </summary>
    public class AddHighSchoolAchCommandHandler : IRequestHandler<AddHighSchoolAchCommand, HttpResponse<string>>
    {
        private readonly IRepository<HighSchoolAchievement> _highSchoolAchievementRepository;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;

        public AddHighSchoolAchCommandHandler(IRepository<HighSchoolAchievement> highSchoolAchievementRepository, IRepository<SchoolExtension> schoolExtensionRepository)
        {
            _highSchoolAchievementRepository = highSchoolAchievementRepository;
            _schoolExtensionRepository = schoolExtensionRepository;
        }

        public Task<HttpResponse<string>> Handle(AddHighSchoolAchCommand request, CancellationToken cancellationToken)
        {
            //判断分部是否存存在
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            if (ext == null)
            {
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "没有找到当前分部" });
            }
            if (ext.Grade != (byte)iSchool.Domain.Enum.SchoolGrade.SeniorMiddleSchool)
            {
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "没有找到当前分部" });
            }
            else if (ext.Type == (byte)iSchool.Domain.Enum.SchoolType.International)
            {
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "没有找到当前分部" });
            }
            var ach = _highSchoolAchievementRepository.GetIsValid(p => p.ExtId == request.ExtId && p.Year == request.Year);

            var keyValue = new List<KeyValueDto<string>>();
            if (request.Name == null || request.Point == null)
            {
            }
            else
            {
                var nameCount = request.Name.Count();
                var pointCount = request.Point.Count();
                var count = nameCount > pointCount ? pointCount : nameCount;
                for (int i = 0; i < count; i++)
                {
                    var name = request.Name[i];
                    var point = request.Point[i];
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(point))
                        keyValue.Add(new KeyValueDto<string> { Key = name, Value = point });
                }
            }

            if (ach == null)
            {
                HighSchoolAchievement achievement = new HighSchoolAchievement()
                {
                    Id = Guid.NewGuid(),
                    Count = request.Count,
                    Completion = request.Completion,
                    ExtId = request.ExtId,
                    Fractionaline = JsonSerializationHelper.Serialize(keyValue.Select(p => new { Name = p.Key, Point = p.Value })),
                    IsValid = true,
                    Keyundergraduate = request.Keyundergraduate,
                    Undergraduate = request.Undergraduate,
                    Year = request.Year,
                    Creator = Guid.Empty,
                    Modifier = Guid.Empty,
                    CreateTime = DateTime.Now,
                    ModifyDateTime = DateTime.Now
                };
                _highSchoolAchievementRepository.Insert(achievement);
            }
            else
            {
                ach.Count = request.Count;
                ach.Completion = request.Completion;
                ach.Fractionaline = JsonSerializationHelper.Serialize(keyValue.Select(p => new { Name = p.Key, Point = p.Value }));
                ach.Keyundergraduate = request.Keyundergraduate;
                ach.Undergraduate = request.Undergraduate;
                ach.Year = request.Year;
                ach.Modifier = Guid.Empty;
                ach.ModifyDateTime = DateTime.Now;
                _highSchoolAchievementRepository.Update(ach);
            }
            return Task.FromResult(new HttpResponse<string> { State = 200 });
        }
    }
    /// <summary>
    /// 添加初中的升学成绩
    /// </summary>
    public class AddMiddleSchoolAchCommandHandler : IRequestHandler<AddMiddleSchoolAchCommand, HttpResponse<string>>

    {
        private readonly IRepository<MiddleSchoolAchievement> _middleSchoolAchievementRepository;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;



        public AddMiddleSchoolAchCommandHandler(IRepository<MiddleSchoolAchievement> middleSchoolAchievementRepository, IRepository<SchoolExtension> schoolExtensionRepository)
        {
            _middleSchoolAchievementRepository = middleSchoolAchievementRepository;
            _schoolExtensionRepository = schoolExtensionRepository;
        }


        public Task<HttpResponse<string>> Handle(AddMiddleSchoolAchCommand request, CancellationToken cancellationToken)
        {
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            if (ext == null || ext.Grade != (byte)iSchool.Domain.Enum.SchoolGrade.JuniorMiddleSchool)
            {
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "没有找到当前分部" });
            }
            var ach = _middleSchoolAchievementRepository.GetIsValid(p => p.ExtId == request.ExtId && p.Year == request.Year);
            if (ach == null)
            {
                var data = new MiddleSchoolAchievement
                {
                    Id = Guid.NewGuid(),
                    Ratio = request.Ratio,
                    Average = request.Average,
                    ExtId = request.ExtId,
                    Year = request.Year,
                    IsValid = true,
                    Creator = request.UserId,
                    Modifier = request.UserId,
                    Keyrate = request.Keyrate,
                    Highest = request.Highest,
                    CreateTime = DateTime.Now,
                    ModifyDateTime = DateTime.Now,
                    Completion = request.Completion
                };
                _middleSchoolAchievementRepository.Insert(data);
            }
            else
            {
                ach.Keyrate = request.Keyrate;
                ach.Highest = request.Highest;
                ach.Average = request.Average;
                ach.Ratio = request.Ratio;
                ach.Completion = request.Completion;
                ach.ModifyDateTime = DateTime.Now;
                ach.Modifier = request.UserId;
                _middleSchoolAchievementRepository.Update(ach);
            }
            return Task.FromResult(new HttpResponse<string> { State = 200 });
        }
    }
    /// <summary>
    /// 添加小学的升学成绩
    /// </summary>
    public class AddPrimarySchoolAchCommandHandler : IRequestHandler<AddPrimarySchoolAchCommand, HttpResponse<string>>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<PrimarySchoolAchievement> _primarySchoolAchievementRepository;
        private UnitOfWork UnitOfWork { get; set; }
        private ILog _log;

        public AddPrimarySchoolAchCommandHandler(
            IRepository<PrimarySchoolAchievement> primarySchoolAchievementRepository,
             IRepository<SchoolExtension> schoolExtensionRepository,
            IUnitOfWork unitOfWork, ILog log)
        {
            _primarySchoolAchievementRepository = primarySchoolAchievementRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
            _schoolExtensionRepository = schoolExtensionRepository;
            _log = log;
        }

        public Task<HttpResponse<string>> Handle(AddPrimarySchoolAchCommand request, CancellationToken cancellationToken)
        {

            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            if (ext == null || ext.Grade != (byte)iSchool.Domain.Enum.SchoolGrade.PrimarySchool)
            {
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "没有找到当前分部" });
            }
            request.Link = request.Link.Where(p => !string.IsNullOrEmpty(p)).ToList();
            var achs = _primarySchoolAchievementRepository.GetAll(p => p.Year == request.Year && p.ExtId == request.ExtId && p.IsValid == true);
            try
            {
                UnitOfWork.BeginTransaction();
                //删除旧数据
                if (achs != null && achs.Count() > 0)
                {
                    var sql = @"UPDATE  dbo.PrimarySchoolAchievement SET IsValid=0,ModifyDateTime=GETDATE() WHERE extId=@extId AND year = @year";
                    UnitOfWork.DbConnection.Execute(sql, new { extId = request.ExtId, year = request.Year }, UnitOfWork.DbTransaction);
                }
                foreach (var item in request.Link)
                {
                    PrimarySchoolAchievement achievement = new PrimarySchoolAchievement()
                    {
                        Id = Guid.NewGuid(),
                        Link = item,
                        Year = request.Year,
                        Createor = request.UserId ?? Guid.Empty,
                        CreateTime = DateTime.Now,
                        Modifier = request.UserId ?? Guid.Empty,
                        ModifyDateTime = DateTime.Now,
                        ExtId = request.ExtId,
                        IsValid = true,
                        Completion = 1
                    };
                    _primarySchoolAchievementRepository.Insert(achievement);
                }
                UnitOfWork.CommitChanges();
                return Task.FromResult(new HttpResponse<string> { State = 200 });
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                _log.Error(ex);
                return Task.FromResult(new HttpResponse<string> { State = 500, Result = ex.Message, Message = "操作失败！" });

            }
        }
    }
    /// <summary>
    /// 添加幼儿园的升学成绩
    /// </summary>
    public class AddKindergartenSchoolAchCommandHandler : IRequestHandler<AddKindergartenSchoolAchCommand, HttpResponse<string>>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<KindergartenAchievement> _kindergartenSchoolAchievementRepository;
        private UnitOfWork UnitOfWork { get; set; }
        private ILog _log;

        public AddKindergartenSchoolAchCommandHandler(
            IRepository<KindergartenAchievement> kindergartenSchoolAchievementRepository,
            IRepository<SchoolExtension> schoolExtensionRepository,
            IUnitOfWork unitOfWork, ILog log)
        {
            _kindergartenSchoolAchievementRepository = kindergartenSchoolAchievementRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
            _schoolExtensionRepository = schoolExtensionRepository;
            _log = log;
        }

        public Task<HttpResponse<string>> Handle(AddKindergartenSchoolAchCommand request, CancellationToken cancellationToken)
        {
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            if (ext == null || ext.Grade != (byte)iSchool.Domain.Enum.SchoolGrade.Kindergarten)
            {
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "没有找到当前分部" });
            }

            request.Link = request.Link.Where(p => !string.IsNullOrEmpty(p)).ToList();
            var achs = _kindergartenSchoolAchievementRepository.GetAll(p => p.Year == request.Year && p.ExtId == request.ExtId && p.IsValid == true);
            try
            {
                UnitOfWork.BeginTransaction();
                //删除旧数据
                if (achs != null && achs.Count() > 0)
                {
                    var sql = @"UPDATE dbo.KindergartenAchievement SET IsValid=0,ModifyDateTime=GETDATE() WHERE extId=@extId AND year=@year";
                    UnitOfWork.DbConnection.Execute(sql, new { extId = request.ExtId, year = request.Year }, UnitOfWork.DbTransaction);
                }
                foreach (var item in request.Link)
                {
                    var achievement = new KindergartenAchievement()
                    {
                        Id = Guid.NewGuid(),
                        Link = item,
                        Year = request.Year,
                        Creator = request.UserId ?? Guid.Empty,
                        CreateTime = DateTime.Now,
                        Modifier = request.UserId ?? Guid.Empty,
                        ModifyDateTime = DateTime.Now,
                        ExtId = request.ExtId,
                        IsValid = true,
                        Completion = 1
                    };
                    _kindergartenSchoolAchievementRepository.Insert(achievement);
                }
                UnitOfWork.CommitChanges();
                return Task.FromResult(new HttpResponse<string> { State = 200 });
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                _log.Error(ex);
                return Task.FromResult(new HttpResponse<string> { State = 500, Result = ex.Message, Message = "操作失败！" });
            }
        }
    }

}

