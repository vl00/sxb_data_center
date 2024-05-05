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
namespace iSchool.Application.Service
{
    public class GetSchoolExtCourseQueryHandler : IRequestHandler<GetSchoolExtCourseQuery, SchoolExtCourseDto>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<SchoolExtCourse> _schoolExtCourseRepository;
        private readonly IRepository<School> _schoolRepository;
        private readonly IMapper _mapper;

        public GetSchoolExtCourseQueryHandler(IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<SchoolExtCourse> schoolExtCourseRepository,
            IRepository<School> schoolRepository,
            IMapper mapper)
        {
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolExtCourseRepository = schoolExtCourseRepository;
            _schoolRepository = schoolRepository;
            _mapper = mapper;
        }

        public Task<SchoolExtCourseDto> Handle(GetSchoolExtCourseQuery request, CancellationToken cancellationToken)
        {
            SchoolExtCourseDto dto = null;
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            if (ext == null)
            {
                return Task.FromResult(dto);
            }
            else
            {
                if (!request.IsAll)
                {
                    var school = _schoolRepository.GetIsValid(p => p.Id == ext.Sid);
                    if (school?.Creator != request.UserId)
                        return null;
                }

                var extCourse = _schoolExtCourseRepository.GetIsValid(p => p.Eid == request.ExtId);
                dto = _mapper.Map<SchoolExtCourseDto>(extCourse);
                if (dto == null) dto = new SchoolExtCourseDto();
                dto.Eid = ext.Id;
                dto.SchFtype = ext.SchFtype;
                dto.Chinese = ext.Chinese;
                dto.Diglossia = ext.Diglossia;
                dto.Discount = ext.Discount;
                dto.Grade = ext.Grade;
                dto.Type = ext.Type;
                return Task.FromResult(dto);
            }
        }
    }
}
