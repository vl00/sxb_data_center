using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System.Linq;
using iSchool.Infrastructure;
using Dapper;
using iSchool.Domain.Enum;

namespace iSchool.Application.Service
{
    public class SchoolImageLogicHandler : IRequestHandler<SchoolImageLogicCommand, bool>
    {
        private readonly IRepository<SchoolImage> _schoolImageRepository;
        private readonly IMapper _mapper;
        private readonly UnitOfWork UnitOfWork;

        public SchoolImageLogicHandler(IRepository<SchoolImage> schoolImageRepository,
            IMapper mapper,
             IUnitOfWork unitOfWork)
        {

            _schoolImageRepository = schoolImageRepository;
            _mapper = mapper;
            UnitOfWork = unitOfWork as UnitOfWork;
        }

        public async Task<bool> Handle(SchoolImageLogicCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            UnitOfWork.BeginTransaction();
            try
            {
                List<SchoolImage> schoolImages = new List<SchoolImage>();
                var types = new List<byte>();
                for (int i = 0; i < request.UploadImgArry.Count(); i++)
                {
                    for (int j = 0; j < request.UploadImgArry[i].Data.Count(); j++)
                    {
                        if (Enum.TryParse<SchoolImageEnum>(request.UploadImgArry[i].Name, out var school))
                        {
                            types.Add((byte)Convert.ToInt16(school));
                            schoolImages.Add(new SchoolImage()
                            {
                                SUrl = request.UploadImgArry[i].Data[j].Url.CompressUrl,
                                Creator = request.UserId,
                                Eid = request.Eid,
                                Sort = (byte)(99 - j),
                                Modifier = request.UserId,
                                Type = (byte)school,
                                Url = request.UploadImgArry[i].Data[j].Url.Url,
                                ImageDesc = request.UploadImgArry[i].Data[j].Title,
                                Id = Guid.NewGuid(),
                                IsValid = true
                            });
                        }
                    }
                }

                string sql = $"UPDATE dbo.SchoolImage SET IsValid=0,ModifyDateTime=GETDATE() WHERE eid=@eid AND IsValid=1 AND type in ({string.Join(",", types)})";
                UnitOfWork.DbConnection.Execute(sql, new { eid = request.Eid }, UnitOfWork.DbTransaction);

                _schoolImageRepository.BatchInsert(schoolImages);
                UnitOfWork.CommitChanges();
                return true;
            }
            catch
            {
                UnitOfWork.Rollback();
                return false;
            }
        }
    }
}
