using System;
using System.Collections.Generic;
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
    public class AddSchoolExtChargeCommandHandler : IRequestHandler<AddSchoolExtChargeCommand, HttpResponse<object>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<School> _schoolRepository;
        private readonly IRepository<SchoolExtension> _schoolextensionRepository;
        private readonly IRepository<SchoolExtCharge> _schoolExtChargeRepository;
        private readonly IMediator _mediator;

        public AddSchoolExtChargeCommandHandler(IMapper mapper, IUnitOfWork unitOfWork,
            IRepository<School> schoolRepository,
            IRepository<SchoolExtCharge> schoolExtChargeRepository,
            IRepository<SchoolExtension> schoolextensionRepository, 
            IMediator mediator)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _schoolRepository = schoolRepository;
            _schoolextensionRepository = schoolextensionRepository;
            _schoolExtChargeRepository = schoolExtChargeRepository;
            _mediator = mediator;

        }

        public async Task<HttpResponse<object>> Handle(AddSchoolExtChargeCommand cmd, CancellationToken cancellationToken)
        {
            var sg = _schoolRepository.GetIsValid(p => p.Id == cmd.Sid);
            if (sg == null)
                return new HttpResponse<object> { State = 400, Message = "学校不存在" };

            var ext = _schoolextensionRepository.GetIsValid(p => p.Id == cmd.Eid && p.Sid == cmd.Sid);
            if (ext == null)
                return new HttpResponse<object> { State = 400, Message = "学部不存在" };

            var charge = _schoolExtChargeRepository.GetIsValid(_ => _.Eid == cmd.Eid);
            if (charge == null)
            {
                charge = _mapper.Map<SchoolExtCharge>(cmd);
                charge.CreateTime = DateTime.Now;
                charge.Creator = cmd.CurrentUserId;
            }
            else
            {
                var x = _mapper.Map(cmd, charge);
                //_ = x == charge;
            }
            charge.IsValid = true;
            charge.ModifyDateTime = DateTime.Now;
            charge.Modifier = cmd.CurrentUserId;
            if (charge.Id == Guid.Empty)
            {
                charge.Id = Guid.NewGuid();
                _schoolExtChargeRepository.Insert(charge);
            }
            else
            { 
                _schoolExtChargeRepository.Update(charge);
            }

            // 年份字段
            await _mediator.Send(new SaveExtYearFieldChangesCommand { Sid = cmd.Sid, Eid = cmd.Eid, Yearslist = cmd.Yearslist });

            await _mediator.Publish(new SchoolUpdatedEvent { Sid = cmd.Sid, Eid = cmd.Eid });
            return new HttpResponse<object> { State = 200 };
        }
    }
}
