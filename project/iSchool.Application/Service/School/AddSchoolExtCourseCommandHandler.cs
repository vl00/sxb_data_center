using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using MediatR;

namespace iSchool.Application.Service
{
    public class AddSchoolExtCourseCommandHandler : IRequestHandler<AddSchoolExtCourseCommand, HttpResponse<string>>
    {
        private readonly IRepository<SchoolExtCourse> _schoolExtCourseRepository;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;


        public AddSchoolExtCourseCommandHandler(IRepository<SchoolExtCourse> schoolExtCourseRepository,
            IRepository<SchoolExtension> schoolExtensionRepository,
            IMediator mediator, IMapper mapper)
        {
            _schoolExtCourseRepository = schoolExtCourseRepository;
            _schoolExtensionRepository = schoolExtensionRepository;
            _mapper = mapper;
            _mediator = mediator;

        }

        public Task<HttpResponse<string>> Handle(AddSchoolExtCourseCommand request, CancellationToken cancellationToken)
        {
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.Eid);
            if (ext == null)
            {
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "没找到该学校得学部" });
            }
            var extCourse = _schoolExtCourseRepository.GetIsValid(p => p.Eid == request.Eid);
            if (extCourse == null)
            {
                //新增
                var data = _mapper.Map<SchoolExtCourse>(request);
                data.Id = Guid.NewGuid();
                data.IsValid = true;
                data.Modifier = request.UserId;
                data.ModifyDateTime = DateTime.Now;
                data.CreateTime = DateTime.Now;
                data.Creator = request.UserId;
                data.Completion = request.Completion;
                _schoolExtCourseRepository.Insert(data);
            }
            else
            {
                //修改
                extCourse.Authentication = request.Authentication;
                extCourse.Characteristic = request.Characteristic;
                extCourse.Completion = request.Completion;
                extCourse.Courses = request.Courses;
                extCourse.Completion = request.Completion;
                extCourse.ModifyDateTime = DateTime.Now;
                extCourse.Modifier = request.UserId;
                _schoolExtCourseRepository.Update(extCourse);
            }
            _mediator.Publish(new SchoolUpdatedEvent { Sid = request.Sid, Eid = request.Eid }).Wait();
            return Task.FromResult(new HttpResponse<string> { State = 200 });
        }
    }
}
