using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using iSchool.Infrastructure;
using Dapper;

namespace iSchool.Application.Service
{
    public class AddSchoolAchListCommandHandler : IRequestHandler<AddSchoolAchListCommand, HttpResponse<string>>
    {
        private readonly IRepository<SchoolExtension> _schoolextensionRepository;
        private readonly IRepository<SchoolAchievement> _schoolAchievementRepository;
        private UnitOfWork UnitOfWork { get; set; }
        private ILog _log;


        public AddSchoolAchListCommandHandler(IRepository<SchoolExtension> schoolextensionRepository, IRepository<SchoolAchievement> schoolAchievementRepository, ILog log, IUnitOfWork unitOfWork)
        {
            _schoolextensionRepository = schoolextensionRepository;
            _schoolAchievementRepository = schoolAchievementRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
            this._log = log;
        }

        public Task<HttpResponse<string>> Handle(AddSchoolAchListCommand request, CancellationToken cancellationToken)
        {
            //去重
            var data = request.Data.GroupBy(p => p.Id).Select(p => p.First());

            try
            {
                UnitOfWork.BeginTransaction();
                var delSql = @"DELETE FROM dbo.SchoolAchievement WHERE extId=@Id AND year=@Year";
                UnitOfWork.DbConnection.Execute(delSql, new { Id = request.ExtId, Year = request.Year }, UnitOfWork.DbTransaction);
                foreach (var item in data)
                {
                    SchoolAchievement achievement = new SchoolAchievement()
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = item.Id,
                        CreateTime = DateTime.Now,
                        Creator = request.UserId,
                        Modifier = request.UserId,
                        ModifyDateTime = DateTime.Now,
                        Count = item.Count,
                        ExtId = request.ExtId,
                        IsValid = true,
                        Year = request.Year,
                        Completion = request.Completion
                    };
                    _schoolAchievementRepository.Insert(achievement);
                }
                UnitOfWork.CommitChanges();
                return Task.FromResult(new HttpResponse<string> { State = 200 });
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                UnitOfWork.Rollback();
                return Task.FromResult(new HttpResponse<string> { State = 200, Result = ex.Message, Message = "保存失败" });
            }
        }
    }
}
