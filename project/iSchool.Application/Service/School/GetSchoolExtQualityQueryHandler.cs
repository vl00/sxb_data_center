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
using iSchool.Domain.Enum;
using iSchool.Infrastructure;
using Dapper;

namespace iSchool.Application.Service
{
    public class GetSchoolExtQualityQueryHandler : IRequestHandler<GetSchoolExtQualityQuery, GetSchoolExtQualityQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<SchoolExtQuality> _schoolExtQualityRepository;
        private readonly IRepository<SchoolImage> _schoolImageRepository;
        private readonly IRepository<SchoolVideo> _schoolVideoRepository;
        private readonly IMediator _mediator;
        private readonly IRepository<School> _schoolRepository;

        private UnitOfWork UnitOfWork { get; set; }

        public GetSchoolExtQualityQueryHandler(IMapper mapper, IMediator mediator,
            IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<SchoolExtQuality> schoolExtQualityRepository,
            IRepository<SchoolImage> schoolImageRepository,
            IRepository<School> schoolRepository,
            IRepository<SchoolVideo> schoolVideoRepository,IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _mediator = mediator;
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolExtQualityRepository = schoolExtQualityRepository;
            _schoolRepository = schoolRepository;
            _schoolImageRepository = schoolImageRepository;
            _schoolVideoRepository = schoolVideoRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<GetSchoolExtQualityQueryResult> Handle(GetSchoolExtQualityQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            SchoolExtQualityDto dto = null;

            var ext = _schoolExtensionRepository.GetIsValid(p => p.Sid == request.Sid && p.Id == request.Eid);
            if (ext == null)
                return null;
            if (!request.IsAll)
            {
                var school = _schoolRepository.GetIsValid(p => p.Id == request.Sid);
                if (school?.Creator != request.UserId)
                    return null;
            }

            var data = _schoolExtQualityRepository.GetIsValid(p => p.Eid == request.Eid);
            if (data != null)
            {
                dto = new SchoolExtQualityDto(); //{ Videos = JsonSerializationHelper.JSONToObject<string[]>(data.Videos) };
                if (data.Videos != null && data.Videos.Length > 0)
                {
                    dto.Videos = JsonSerializationHelper.JSONToObject<string[]>(data.Videos);
                }

                // 图片
                var dict = await _mediator.Send(new GetSchoolExtImgsQuery
                {
                    Eid = ext.Id,
                    Types = new[]
                    {
                        (byte)SchoolImageEnum.SchoolHonor,
                        (byte)SchoolImageEnum.StudentHonor,
                        (byte)SchoolImageEnum.Principal,
                        (byte)SchoolImageEnum.Teacher,
                    }
                });
                dto.Imgs = dict.Values;

                dto.Eid = request.Eid;
                dto.Sid = request.Sid;
            }

            //校长视频信息
            var vData = _schoolVideoRepository.GetListByFileds(new Dictionary<string, string>(){ { "eid",request.Eid.ToString()},{"type", ((byte)VideoType.Principal).ToString() }, { "IsValid", "true" } }).ToList();
            if (vData != null && vData.Count > 0)
            {
                dto.VideoDescs = vData.OrderBy(p => p.Sort).Select(p => p.VideoDesc).ToArray();
                dto.Videos = vData.OrderBy(p => p.Sort).Select(p => p.VideoUrl).ToArray();
                dto.Covers = vData.OrderBy(p => p.Sort).Select(p => p.Cover).ToArray();
            }

            var res = new GetSchoolExtQualityQueryResult();
            res.Dto = dto ?? new SchoolExtQualityDto { Sid = request.Sid, Eid = request.Eid };
            res.Ext = ext;
            res.Dto.Chinese = ext.Chinese;
            res.Dto.Diglossia = ext.Diglossia;
            res.Dto.Discount = ext.Discount;
            res.Dto.Grade = ext.Grade;
            res.Dto.Type = ext.Type;

            return res;
        }
    }
}
