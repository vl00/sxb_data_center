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
    public class GetSchoolExtLifeQueryHandler : IRequestHandler<GetSchoolExtLifeQuery, GetSchoolExtLifeResult>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<SchoolExtLife> _schoolExtLifeRepository;
        private readonly IRepository<School> _schoolRepository;
        private readonly IMediator _mediator;

        private UnitOfWork UnitOfWork { get; set; }

        public GetSchoolExtLifeQueryHandler(IMapper mapper,
            IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<SchoolExtLife> schoolExtLifeRepository,
            IRepository<School> schoolRepository,
            IUnitOfWork unitOfWork, IMediator mediator)
        {
            _mapper = mapper;
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolExtLifeRepository = schoolExtLifeRepository;
            _schoolRepository = schoolRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
            _mediator = mediator;

        }

        public async Task<GetSchoolExtLifeResult> Handle(GetSchoolExtLifeQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            SchoolExtLifeDto dto = null;

            var ext = _schoolExtensionRepository.GetIsValid(p => p.Sid == request.Sid && p.Id == request.Eid);
            if (ext == null) return null;

            if (!request.IsAll)
            {
                var school = _schoolRepository.GetIsValid(p => p.Id == request.Sid);
                if (school?.Creator != request.UserId)
                    return null;
            }

            do
            {
                var data = _schoolExtLifeRepository.GetIsValid(p => p.Eid == request.Eid);
                if (data == null)
                    break;

                dto = _mapper.Map<SchoolExtLifeDto>(data);
                dto.Sid = request.Sid;
            }
            while (false);

            var res = new GetSchoolExtLifeResult();
            res.Dto = dto ?? new SchoolExtLifeDto { Sid = request.Sid, Eid = request.Eid };
            res.Ext = ext;
            res.Dto.Chinese = ext.Chinese;
            res.Dto.Diglossia = ext.Diglossia;
            res.Dto.Discount = ext.Discount;
            res.Dto.Grade = ext.Grade;
            res.Dto.Type = ext.Type;

            var dict = await _mediator.Send(new GetSchoolExtImgsQuery 
            {
                Eid = ext.Id,
                Types = new [] 
                { 
                    (byte)SchoolImageEnum.Hardware,
                    (byte)SchoolImageEnum.CommunityActivities,
                    (byte)SchoolImageEnum.GradeSchedule,
                    (byte)SchoolImageEnum.Schedule,
                    (byte)SchoolImageEnum.Diagram,
                }
            });
            res.Dto.Imgs = dict.Values;

            return res;
        }
    }
}
