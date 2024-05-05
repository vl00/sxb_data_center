using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using Dapper;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 学校视频
    /// </summary>
    public class AddSchoolVideoCommandHandler : IRequestHandler<AddSchoolVideoCommand, HttpResponse<bool>>
    {
        private readonly IRepository<SchoolVideo> _schoolVideoRepository;
        private UnitOfWork _unitOfWork;
        public AddSchoolVideoCommandHandler(IRepository<SchoolVideo> schoolVideoRepository, IUnitOfWork unitOfWork)
        {
            _schoolVideoRepository = schoolVideoRepository;
            _unitOfWork = (UnitOfWork)unitOfWork;
        }

        public Task<HttpResponse<bool>> Handle(AddSchoolVideoCommand request, CancellationToken cancellationToken)
        {
            var listData = new List<SchoolVideo>();
            
            for (int i = 0; i < request.Videos?.Length; i++)
            {
                var type = Convert.ToInt32(request.Types[i]);               
                listData.Add(new SchoolVideo()
                {
                    Id = Guid.NewGuid(),
                    Eid = request.Eid,
                    IsOutSide = false,
                    IsValid = true,
                    Sort = (byte)i,
                    Type =(byte)type,
                    Completion = 1,
                    Creator = request.OperatorId,
                    Modifier = request.OperatorId,
                    VideoDesc = request.VideoDescs[i],
                    VideoUrl = request.Videos[i],
                    Cover = request.Covers[i]

                });
            }
            try
            {
                _unitOfWork.BeginTransaction();
                //1、先删除原有视频记录
                string updateSql = $"UPDATE SchoolVideo SET IsValid=0 WHERE eid=@eid {request.CurrentVideoTypes}";                
                _unitOfWork.DbConnection.Execute(updateSql,new DynamicParameters().Set("eid", request.Eid),_unitOfWork.DbTransaction); 

                //2、插入视频
                _schoolVideoRepository.BatchInsert(listData);
                _unitOfWork.CommitChanges();

                return Task.FromResult(new HttpResponse<bool> { State = 200 });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return Task.FromResult(new HttpResponse<bool> { State = 500 });
            }
            
        }
       
    }
}
