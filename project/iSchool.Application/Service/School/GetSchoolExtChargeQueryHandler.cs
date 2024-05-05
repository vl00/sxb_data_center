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
    public class GetSchoolExtChargeQueryHandler : IRequestHandler<GetSchoolExtChargeQuery, GetSchoolExtChargeQueryResult>
    {
        private readonly IRepository<SchoolYearFieldContent> _schoolYearFieldContentRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<SchoolExtCharge> _schoolExtChargeRepository;
        private readonly IRepository<School> _schoolRepository;
        private readonly IMediator _mediator;

        private UnitOfWork UnitOfWork { get; set; }

        public GetSchoolExtChargeQueryHandler(IMapper mapper, IRepository<SchoolYearFieldContent> schoolYearFieldContentRepository,
            IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<SchoolExtCharge> schoolExtChargeRepository,
            IRepository<School> schoolRepository,
            IMediator mediator,
        IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _schoolYearFieldContentRepository = schoolYearFieldContentRepository;
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolExtChargeRepository = schoolExtChargeRepository;
            _schoolRepository = schoolRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
            this._mediator = mediator;
        }

        public async Task<GetSchoolExtChargeQueryResult> Handle(GetSchoolExtChargeQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            SchoolExtChargeDto dto = null;


            var ext = _schoolExtensionRepository.GetIsValid(p => p.Sid == request.Sid && p.Id == request.Eid);
            do
            {
                if (ext == null)
                    return null;

                if (!request.IsAll)
                {
                    var school = _schoolRepository.GetIsValid(p => p.Id == ext.Sid);
                    if (school?.Creator != request.UserId)
                        return null;
                }

                var data = _schoolExtChargeRepository.GetIsValid(p => p.Eid == request.Eid);
                if (data == null)
                    break;

                dto = _mapper.Map<SchoolExtChargeDto>(data);
                dto.Sid = request.Sid;
            }
            while (false);

            var res = new GetSchoolExtChargeQueryResult();
            res.Dto = dto ?? new SchoolExtChargeDto { Sid = request.Sid, Eid = request.Eid };
            #region 部分字段不同年份数据（年份标签）
            //Dictionary<string, string> quryparam = new Dictionary<string, string>();
            //quryparam.Add("Eid", ext.Id.ToString());
            //quryparam.Add("IsValid", "1");
            //res.Dto.YearTagList = _schoolYearFieldContentRepository.GetListByFileds(quryparam).ToList();
            #endregion
            #region 部分字段最新年份数据
            //res.Dto.YearTagList = BusinessHelper.GetNewYearFieldData(ext.Id.ToString(), _mapper, UnitOfWork);
            res.Dto.YearTagList = _mediator.Send(new LatestYearFieldDataQuery { EId = ext.Id }).Result;
            #endregion
            res.Ext = ext;
            res.Ext.Chinese = ext.Chinese;
            res.Ext.Diglossia = ext.Diglossia;
            res.Ext.Discount = ext.Discount;
            res.Ext.Grade = ext.Grade;
            res.Ext.Type = ext.Type;            
         
            return res;
        }
    }
}
