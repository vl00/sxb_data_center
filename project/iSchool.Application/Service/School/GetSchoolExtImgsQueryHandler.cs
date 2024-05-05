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
    public class GetSchoolExtImgsQueryHandler : IRequestHandler<GetSchoolExtImgsQuery, Dictionary<byte, UploadImgDto>>
    {
        private readonly IRepository<SchoolImage> _schoolImageRepository;
        private readonly IMapper _mapper;
        private readonly UnitOfWork UnitOfWork;

        public GetSchoolExtImgsQueryHandler(IRepository<SchoolImage> schoolImageRepository,
            IMapper mapper, IUnitOfWork unitOfWork)
        {
            _schoolImageRepository = schoolImageRepository;
            _mapper = mapper;
            UnitOfWork = unitOfWork as UnitOfWork;
        }

        public async Task<Dictionary<byte, UploadImgDto>> Handle(GetSchoolExtImgsQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            if (req.Types?.Any() != true) return new Dictionary<byte, UploadImgDto>();

            var sql = $@"
select * from SchoolImage where IsValid=1 and eid=@Eid and type in ({string.Join(',', req.Types)})
order by type,sort
";
            var imgs = await UnitOfWork.DbConnection.QueryAsync<SchoolImage>(sql, new { req.Eid });
            var dict = imgs.GroupBy(_ => _.Type).ToDictionary(_ => _.Key, img => new UploadImgDto
            {
                Type = img.Key,
                Items = img.OrderBy(_ => _.Sort).Select(_ => new UploadImgItemDto 
                {
                    Id = _.Id,
                    Url = _.Url,
                    Url_s = _.SUrl,
                    Desc = _.ImageDesc,
                }),
            });
            return dict;
        }

    }
}
