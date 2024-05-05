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
    public class GetSchoolExtRecruitQueryHandler : IRequestHandler<GetSchoolExtRecruitQuery, SchoolExtRecruitDto>
    {
        private readonly IRepository<SchoolYearFieldContent> _schoolYearFieldContentRepository;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<SchoolExtRecruit> _schoolExtRecruitRepository;
        private readonly IRepository<School> _schoolRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private UnitOfWork UnitOfWork { get; set; }

        public GetSchoolExtRecruitQueryHandler(IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<SchoolExtRecruit> schoolExtRecruitRepository,
             IRepository<School> schoolRepository,
            IMapper mapper, IUnitOfWork unitOfWork, IRepository<SchoolYearFieldContent> schoolYearFieldContentRepository
            , IMediator mediator)
        {
            _schoolYearFieldContentRepository = schoolYearFieldContentRepository;
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolExtRecruitRepository = schoolExtRecruitRepository;
            _schoolRepository = schoolRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
        }

        public Task<SchoolExtRecruitDto> Handle(GetSchoolExtRecruitQuery request, CancellationToken cancellationToken)
        {
            SchoolExtRecruitDto dto = null;
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.ExtId);
            if (ext == null)
            {
                return Task.FromResult(dto);
            }
            else
            {
                if (!request.IsAll)
                {
                    var school = _schoolRepository.GetIsValid(p => p.Id == request.Sid);
                    if (school?.Creator != request.UserId)
                        return null;
                }

                var data = _schoolExtRecruitRepository.GetIsValid(p => p.Eid == request.ExtId);
                dto = _mapper.Map<SchoolExtRecruitDto>(data);
                if (dto == null) dto = new SchoolExtRecruitDto();
                dto.SchFtype = ext.SchFtype;
                dto.Grade = ext.Grade;
                dto.Type = ext.Type;
                dto.Diglossia = ext.Diglossia;
                dto.Discount = ext.Discount;
                dto.Chinese = ext.Chinese;
                #region 原有业务暂不使用 部分字段不同年份数据（年份标签）
                //Dictionary< string ,string> quryparam = new Dictionary<string, string>();
                //quryparam.Add("Eid", ext.Id.ToString());
                //quryparam.Add("IsValid", "1");
                //var lisYear = _schoolYearFieldContentRepository.GetListByFileds(quryparam);
                //var listYearDto = new List<SchoolYearFieldContentDto>();
                //foreach (var year in lisYear)
                //{

                //        var ydto = _mapper.Map<SchoolYearFieldContentDto>(year);
                //        listYearDto.Add(ydto);


                //}
                //dto.YearTagList = listYearDto;

                #endregion

                #region 部分字段最新年份数据（年份标签）
                dto.YearTagList = _mediator.Send(new LatestYearFieldDataQuery { EId = ext.Id }).Result;                
                #endregion

                return Task.FromResult(dto);
            }
        }

        


    }
}
