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
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
namespace iSchool.Application.Service
{
    public class GetSchoolExtensionQueryHandler : IRequestHandler<GetSchoolExtensionQuery, object>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<School> _schoolRepository;

        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        UnitOfWork unitOfWork;

        public GetSchoolExtensionQueryHandler(
            IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<School> schoolRepository,
            IMapper mapper,
            ITagRepository tagRepository, IUnitOfWork unitOfWork)
        {
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolRepository = schoolRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public Task<object> Handle(GetSchoolExtensionQuery request, CancellationToken cancellationToken)
        {
            switch (request.Type)
            {
                case Domain.Enum.SchoolExtensionType.Extension:
                    object dto = GetSchoolExtensionData(request);
                    return Task.FromResult(dto);
                case Domain.Enum.SchoolExtensionType.ExtContent:
                    return null;
                case Domain.Enum.SchoolExtensionType.ExtCharge:
                    return null;
                case Domain.Enum.SchoolExtensionType.ExtLife:
                    return null;
                case Domain.Enum.SchoolExtensionType.ExtQuality:
                    return null;
                case Domain.Enum.SchoolExtensionType.ExtRecruit:
                    return null;
            }
            return null;

        }

        /// <summary>
        /// 获取学部的信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SchoolExtensionDto GetSchoolExtensionData(GetSchoolExtensionQuery request)
        {
            var data = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId && p.Sid == request.Sid);
            if (data != null)
            {
                if (!request.IsAll)
                {
                    var school = _schoolRepository.GetIsValid(p => p.Id == request.Sid);
                    if (school?.Creator != request.UserId)
                        return null;
                }
                var dto = _mapper.Map<SchoolExtensionDto>(data);
                dto.Tags = dto.Tags = string.Join(",",
                _tagRepository.GetSchoolTagItems(request.ExtId, 2)
                .Select(p => p.Name));

                var sql = @"select top 1 * from dbo.SchoolAudit a where a.Sid=@Sid and a.IsValid=1 
                           order by CreateTime desc";
                var a = unitOfWork.DbConnection.QueryFirstOrDefault<SchoolAudit>(sql, new { Sid = request.Sid });
                dto.CurrAuditMessage = a?.AuditMessage;

                return dto;
            }
            return null;
        }
    }
}
