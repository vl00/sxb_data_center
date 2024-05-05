using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System.Linq;
using AutoMapper;
using iSchool.Domain.Enum;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 学校主页
    /// </summary>
    public class GetSchoolDetailsQueryHandler : IRequestHandler<GetSchoolDetailsQuery, SchoolDto>
    {
        private readonly IRepository<School> _schoolRepository;
        private readonly IRepository<SchoolAudit> _schoolAuditRepository;
        private readonly ISchoolExtReposiory _schoolExtReposiory;
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;


        public GetSchoolDetailsQueryHandler(IRepository<School> schoolRepository,
            IRepository<SchoolAudit> schoolAuditRepository,
            ISchoolExtReposiory schoolExtReposiory,
            ITagRepository tagRepository,
            IMapper mapper)
        {
            _schoolRepository = schoolRepository;
            _schoolAuditRepository = schoolAuditRepository;
            _schoolExtReposiory = schoolExtReposiory;
            _tagRepository = tagRepository;
            _mapper = mapper;

        }


        public Task<SchoolDto> Handle(GetSchoolDetailsQuery request, CancellationToken cancellationToken)
        {
            SchoolDto dto = null;
            //获取学校数据
            var school = _schoolRepository
                .GetIsValid(p => p.Id == request.Sid);

            if (school == null || !new byte?[] { (byte)SchoolStatus.Edit, (byte)SchoolStatus.Initial, (byte)SchoolStatus.Failed }.Contains(school.Status))
            {
                return Task.FromResult(dto);
            }

            if (school != null && !request.IsAll && school.Status != (byte)SchoolStatus.Initial)
            {
                if (school.Creator != request.UserId)
                    return Task.FromResult(dto);
            }


            dto = new SchoolDto();

            //审核数据
            var audit = _schoolAuditRepository
                .GetAll(p => p.Sid == request.Sid && p.IsValid == true && p.Status == (byte)SchoolAuditStatus.Failed)
                .OrderByDescending(p => p.CreateTime).FirstOrDefault();
            //分部信息
            dto = _mapper.Map<SchoolDto>(school);
            dto.AuditMessage = audit?.AuditMessage;
            //分部信息
            dto.ExtList = _schoolExtReposiory.GetSimpleExt(request.Sid);
            //学校标签
            dto.Tags = string.Join(",",
                _tagRepository.GetSchoolTagItems(request.Sid, 2)
                .Select(p => p.Name));

            //学校进度
            if (dto.ExtList != null && dto.ExtList.Count() > 0)
            {
                dto.Completion = Convert.ToInt64(dto.ExtList
                    .Sum(p => p.Completion) / dto.ExtList.Count() * 100);
            }
            else
            {
                dto.Completion = 0;
            }
            return Task.FromResult(dto);
        }
    }
}
