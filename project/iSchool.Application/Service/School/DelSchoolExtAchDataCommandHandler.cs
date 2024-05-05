using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Domain.Enum;

namespace iSchool.Application.Service
{
    public class DelSchoolExtAchDataCommandHandler : IRequestHandler<DelSchoolExtAchDataCommand, HttpResponse<string>>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private ILog _log;
        private UnitOfWork UnitOfWork { get; set; }

        public DelSchoolExtAchDataCommandHandler(IRepository<SchoolExtension> schoolExtensionRepository, ILog log, IUnitOfWork unitOfWork)
        {
            _schoolExtensionRepository = schoolExtensionRepository;
            _log = log;
            UnitOfWork = (UnitOfWork)unitOfWork;
        }

        public Task<HttpResponse<string>> Handle(DelSchoolExtAchDataCommand request, CancellationToken cancellationToken)
        {
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            try
            {
                var sql = "	UPDATE {0} SET IsValid=0,ModifyDateTime=GETDATE(),Modifier=@userId WHERE  extId=@id AND year=@year ";
                UnitOfWork.BeginTransaction();
                if (ext.Grade == (byte)Domain.Enum.SchoolGrade.SeniorMiddleSchool && ext.Type == (byte)SchoolType.International)
                {
                    //国际高中
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "SchoolAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);
                }
                else if (ext.Grade == (byte)Domain.Enum.SchoolGrade.SeniorMiddleSchool && ext.Type != (byte)SchoolType.International)
                {
                    //普通高中
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "HighSchoolAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "SchoolAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);
                }
                else if (ext.Grade == (byte)Domain.Enum.SchoolGrade.JuniorMiddleSchool)
                {
                    //普通初中
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "MiddleSchoolAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "SchoolAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);

                }
                else if (ext.Grade == (byte)Domain.Enum.SchoolGrade.PrimarySchool)
                {
                    //小学
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "PrimarySchoolAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "SchoolAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);

                }
                else
                {
                    //幼儿园
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "KindergartenAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);
                    UnitOfWork.DbConnection.Execute(string.Format(sql, "SchoolAchievement"), new { id = request.ExtId, year = request.Year, userId = request.UserId }, UnitOfWork.DbTransaction);

                }

                UnitOfWork.CommitChanges();
                return Task.FromResult(new HttpResponse<string> { State = 200 });
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                UnitOfWork.Rollback();
                return Task.FromResult(new HttpResponse<string> { State = 500 });
            }
        }
    }
}
