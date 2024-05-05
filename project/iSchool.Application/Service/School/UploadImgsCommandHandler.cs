using AutoMapper;
using Dapper;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class UploadImgsCommandHandler : IRequestHandler<UploadImgsCommand, bool>
    {
        private readonly IRepository<SchoolImage> _schoolImageRepository;
        private readonly IMapper _mapper;
        private readonly UnitOfWork UnitOfWork;

        public UploadImgsCommandHandler(IRepository<SchoolImage> schoolImageRepository,
            IMapper mapper, IUnitOfWork unitOfWork)
        {
            _schoolImageRepository = schoolImageRepository;
            _mapper = mapper;
            UnitOfWork = unitOfWork as UnitOfWork;
        }

        public async Task<bool> Handle(UploadImgsCommand cmd, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            if (cmd.Imgs?.Any() != true) return true;
            try
            {
                UnitOfWork.BeginTransaction();

                var types = cmd.Imgs.Select(_ => _.Type).Distinct();
                var sql = "update dbo.SchoolImage set IsValid=0,ModifyDateTime=GETDATE(),Modifier=@UserId where eid=@Eid and type in @types";
                UnitOfWork.DbConnection.Execute(sql, new { cmd.Eid, cmd.UserId, types }, UnitOfWork.DbTransaction);

                foreach (var ls in GetSchoolImages(cmd))
                {
                    foreach (var item in ls)
                    {
                        if (item.Id == default)
                        {
                            item.Id = Guid.NewGuid();
                            _schoolImageRepository.Insert(item);
                        }
                        else
                        {
                            _schoolImageRepository.Update(item);
                        }
                    }
                }

                UnitOfWork.CommitChanges();
                return true;
            }
            catch (Exception ex)
            {
                try { UnitOfWork.Rollback(); } catch { }
                throw ex;
            }
        }

        static IEnumerable<List<SchoolImage>> GetSchoolImages(UploadImgsCommand cmd)
        {
            var ls = new List<SchoolImage>(20);
            foreach (var img in cmd.Imgs)
            {
                if (img.Items == null) continue;
                var i = 0;
                foreach (var item in img.Items)
                {
                    ls.Add(new SchoolImage
                    {
                        Id = item.Id ?? default,
                        Eid = cmd.Eid,
                        Type = img.Type,
                        Url = item.Url,
                        SUrl = item.Url_s,
                        ImageDesc = item.Desc,
                        Sort = (byte)i,
                        Creator = cmd.UserId,
                        Modifier = cmd.UserId,
                        CreateTime = DateTime.Now,
                        ModifyDateTime = DateTime.Now,
                        IsValid = true,
                    });
                    if (ls.Count == 20) 
                    {
                        yield return ls;
                        ls.Clear();
                    }
                    i++;
                }
            }
            if (ls.Any())
            {
                yield return ls;
            }
        }

    }
}
