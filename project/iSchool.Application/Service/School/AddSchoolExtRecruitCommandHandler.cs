using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;

namespace iSchool.Application.Service
{
    public class ModifySchoolExtRecruitCommandHandler : IRequestHandler<AddSchoolExtRecruitCommand, HttpResponse<string>>
    {
        private readonly IMapper _mapper;        
        private readonly IRepository<SchoolExtension> _schoolextensionRepository;
        private readonly IRepository<SchoolExtRecruit> _schoolextRecruitRepository;
        private readonly UnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public ModifySchoolExtRecruitCommandHandler(IMapper mapper,
            IRepository<SchoolExtension> schoolextensionRepository,
            IRepository<SchoolExtRecruit> schoolextRecruitRepository,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
            _schoolextensionRepository = schoolextensionRepository;
            _schoolextRecruitRepository = schoolextRecruitRepository;
            _unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<HttpResponse<string>> Handle(AddSchoolExtRecruitCommand request, CancellationToken cancellation)
        {        
            var ext = _schoolextensionRepository.GetIsValid(p => p.Id == request.Eid);
            if (ext == null)            
                return new HttpResponse<string> { State = 404, Message = "未找到学部" };

            var recruit = _schoolextRecruitRepository.GetIsValid(p => p.Eid == request.Eid);            
            if (recruit == null)
            {
                recruit = new SchoolExtRecruit();
                recruit.CreateTime = DateTime.Now;
                recruit.Creator = request.UserId;
                recruit.Eid = request.Eid;                
            }
            recruit.Proportion = request.Proportion;
            //recruit.Date = request.Date.ToJsonString();
            //recruit.Point = request.Point.ToJsonString();
            recruit.Modifier = request.UserId;
            recruit.ModifyDateTime = DateTime.Now;
            recruit.IsValid = true;
            recruit.Completion = request.Completion;
            if (recruit.Id == Guid.Empty)
            {
                recruit.Id = Guid.NewGuid();
                _schoolextRecruitRepository.Insert(recruit);
            }
            else
            {
                _schoolextRecruitRepository.Update(recruit);
            }

            // 年份字段
            await _mediator.Send(new SaveExtYearFieldChangesCommand { Sid = request.Sid, Eid = request.Eid, Yearslist = request.Yearslist });

            // updated
            await _mediator.Publish(new SchoolUpdatedEvent { Sid = request.Sid, Eid = request.Eid });
            return new HttpResponse<string> { State = 200 };
        }        
    }
}
