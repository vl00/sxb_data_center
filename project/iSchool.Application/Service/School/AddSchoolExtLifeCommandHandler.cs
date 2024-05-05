using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;

namespace iSchool.Application.Service
{
    public class AddSchoolExtLifeCommandHandler : IRequestHandler<AddSchoolExtLifeCommand, HttpResponse<object>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<School> _schoolRepository;
        private readonly IRepository<SchoolExtension> _schoolextensionRepository;
        private readonly IRepository<SchoolExtLife> _schoolExtLifeRepository;
        private readonly IMediator _mediator;

        public AddSchoolExtLifeCommandHandler(IMapper mapper, IUnitOfWork unitOfWork,
            IRepository<School> schoolRepository, IMediator mediator,
            IRepository<SchoolExtLife> schoolExtLifeRepository,
            IRepository<SchoolExtension> schoolextensionRepository)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _schoolRepository = schoolRepository;
            _schoolextensionRepository = schoolextensionRepository;
            _schoolExtLifeRepository = schoolExtLifeRepository;
            _mediator = mediator;
        }

        public async Task<HttpResponse<object>> Handle(AddSchoolExtLifeCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var sg = _schoolRepository.GetIsValid(p => p.Id == request.Dto.Sid);
            if (sg == null)
                return new HttpResponse<object> { State = 400, Message = "学校不存在" };

            var ext = _schoolextensionRepository.GetIsValid(p => p.Id == request.Dto.Eid && p.Sid == request.Dto.Sid);
            if (ext == null)
                return new HttpResponse<object> { State = 400, Message = "学部不存在" };

            var quality = _schoolExtLifeRepository.GetIsValid(_ => _.Eid == request.Dto.Eid);
            if (quality == null)
            {
                quality = _mapper.Map<SchoolExtLife>(request.Dto);
                quality.CreateTime = DateTime.Now;
                quality.Creator = request.CurrentUserId;
                quality.Completion = request.Dto.Completion;
            }
            else
            {
                quality.Completion = request.Dto.Completion;
                //_mapper.Map(request.Dto, quality);
            }
            quality.IsValid = true;
            quality.ModifyDateTime = DateTime.Now;
            quality.Modifier = request.CurrentUserId;

            if (quality.Id == Guid.Empty)
            {
                quality.Id = Guid.NewGuid();
                _schoolExtLifeRepository.Insert(quality);
            }
            else _schoolExtLifeRepository.Update(quality);
            
            // 处理图片的数据
            await _mediator.Send(new UploadImgsCommand
            {
                Eid = request.Dto.Eid,
                UserId = request.CurrentUserId,
                Imgs = request.Dto.Imgs,
            });
            
            await _mediator.Publish(new SchoolUpdatedEvent { Sid = request.Dto.Sid, Eid = request.Dto.Eid });
            return new HttpResponse<object> { State = 200 };
        }
    }
}
