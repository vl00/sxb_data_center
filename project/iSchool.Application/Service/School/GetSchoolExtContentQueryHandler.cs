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
    public class GetSchoolExtContentQueryHandler : IRequestHandler<GetSchoolExtContentQuery, SchoolExtContentDto>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SchoolYearFieldContent> _schoolYearFieldContentRepository;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<SchoolExtContent> _schoolExtContentRepository;
        private readonly IRepository<SchoolVideo> _schoolVideoRepository;
        private readonly IRepository<School> _schoolRepository;
        private readonly IMediator _mediator;
        private UnitOfWork UnitOfWork { get; set; }

        public GetSchoolExtContentQueryHandler(IMapper mapper,
            IRepository<SchoolExtension> schoolExtensionRepository, IRepository<SchoolYearFieldContent> schoolYearFieldContentRepository,
            IRepository<SchoolExtContent> schoolExtContentRepository,
            IRepository<SchoolVideo> schoolVideoRepository,
            IRepository<School> schoolRepository,
            IUnitOfWork unitOfWork
            , IMediator mediator)
        {
            _mapper = mapper;
            _schoolYearFieldContentRepository = schoolYearFieldContentRepository;
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolExtContentRepository = schoolExtContentRepository;
            _schoolVideoRepository = schoolVideoRepository;
            _schoolRepository = schoolRepository;
            _mediator = mediator;
            UnitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<SchoolExtContentDto> Handle(GetSchoolExtContentQuery request, CancellationToken cancellationToken)
        {
            SchoolExtContentDto dto = null;
            await default(ValueTask);

            if (!request.IsAll)
            {
                var school = _schoolRepository.GetIsValid(p => p.Id == request.Sid);
                if (school?.Creator != request.UserId)
                    return dto;
            }
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Sid == request.Sid && p.Id == request.Eid);

            if (ext != null)
            {
                dto = new SchoolExtContentDto();
                //查询学校简介
                var data = _schoolExtContentRepository.GetIsValid(p => p.Eid == request.Eid);
                if (data != null)
                {
                    dto = _mapper.Map<SchoolExtContentDto>(data);
                    //学校视频
                    var videos = _schoolVideoRepository.GetAll(p => p.Eid == request.Eid && p.IsValid == true);
                    //体验课程
                     dto.ExperienceVideos = videos.OrderBy(p => p.Sort).Where(p => p.Type == (int)VideoType.Experience).Select(p => new VideosInfo() { Type=(byte)VideoType.Experience, VideoUrl = p.VideoUrl.ToString(), VideoDesc=p.VideoDesc, Cover=p.Cover  }).ToList();
                    //学校专访
                    dto.InterviewVideos = videos.OrderBy(p => p.Sort).Where(p => p.Type == (int)VideoType.Interview).Select(p => new VideosInfo() {Type=(byte)VideoType.Interview , VideoUrl = p.VideoUrl.ToString(), VideoDesc = p.VideoDesc, Cover = p.Cover }).ToList();
                    //学校简介
                    dto.ProfileVideos = videos.OrderBy(p => p.Sort).Where(p => p.Type == (int)VideoType.Profile).Select(p => new VideosInfo() { Type = (byte)VideoType.Profile, VideoUrl = p.VideoUrl.ToString(), VideoDesc = p.VideoDesc, Cover = p.Cover }).ToList();
                    //寄宿走读
                    dto.Lodging = BusinessHelper.SetLodgingSdExternSelectValue(data.Lodging, data.SdExtern);
                }
                //学部信息
                dto.SchFtype = ext.SchFtype;
                dto.Grade = ext.Grade;
                dto.Type = ext.Type;
                dto.Chinese = ext.Chinese;
                dto.Discount = ext.Discount;
                dto.Diglossia = ext.Diglossia;
                
              
                var schftype = new SchFType0(ext.SchFtype);
                #region 升学成绩
                #region old code
                /* var sql = "SELECT  DISTINCT year AS [key], @extId AS [value], Completion AS [message] FROM {0} WHERE extId=@extId and IsValid=1 ORDER BY YEAR DESC";
                if (ext.Grade == (byte)Domain.Enum.SchoolGrade.SeniorMiddleSchool && !SchUtils.Canshow2("hsa.keyundergraduate", schftype))
                {
                    //国际高中
                    dto.Achievement = UnitOfWork.DbConnection.Query<int, Guid, float, KeyValueDto<Guid>>(string.Format(sql, "SchoolAchievement"), (key, value, message) =>
                    {
                        return new KeyValueDto<Guid> { Key = key.ToString(), Value = value, Message = message.ToString() };
                    }, new { extId = request.Eid }, UnitOfWork.DbTransaction, true, "value,message").ToList();
                }
                else if (ext.Grade == (byte)Domain.Enum.SchoolGrade.SeniorMiddleSchool)
                {
                    //普通高中
                    dto.Achievement = UnitOfWork.DbConnection.Query<int, Guid, float, KeyValueDto<Guid>>(string.Format(sql, "HighSchoolAchievement"), (key, value, message) =>
                    {
                        return new KeyValueDto<Guid> { Key = key.ToString(), Value = value, Message = message.ToString() };
                    }, new { extId = request.Eid }, UnitOfWork.DbTransaction, true, "value,message").ToList();
                }
                else if (ext.Grade == (byte)Domain.Enum.SchoolGrade.JuniorMiddleSchool)
                {
                    //普通初中
                    dto.Achievement = UnitOfWork.DbConnection.Query<int, Guid, float, KeyValueDto<Guid>>(string.Format(sql, "MiddleSchoolAchievement"), (key, value, message) =>
                    {
                        return new KeyValueDto<Guid> { Key = key.ToString(), Value = value, Message = message.ToString() };
                    },
                    new { extId = request.Eid }, UnitOfWork.DbTransaction, true, "value,message").ToList();
                }
                else if (ext.Grade == (byte)Domain.Enum.SchoolGrade.PrimarySchool)
                {
                    //小学
                    dto.Achievement = UnitOfWork.DbConnection.Query<int, Guid, float, KeyValueDto<Guid>>(string.Format(sql, "PrimarySchoolAchievement"), (key, value, message) =>
                    {
                        return new KeyValueDto<Guid> { Key = key.ToString(), Value = value, Message = message.ToString() };
                    }, new { extId = request.Eid }, UnitOfWork.DbTransaction, true, "value,message").ToList();
                }
                else if (ext.Grade == (byte)Domain.Enum.SchoolGrade.Kindergarten)
                {
                    //幼儿园
                    dto.Achievement = UnitOfWork.DbConnection.Query<int, Guid, float, KeyValueDto<Guid>>(string.Format(sql, "KindergartenAchievement"), (key, value, message) =>
                    {
                        return new KeyValueDto<Guid> { Key = key.ToString(), Value = value, Message = message.ToString() };
                    }, new { extId = request.Eid }, UnitOfWork.DbTransaction, true, "value,message").ToList();
                }
                else
                {
                    //other?
                    dto.Achievement = new List<KeyValueDto<Guid>>();
                } //*/
                #endregion

                var sql = $@"
select [key],[value],max([message])as [message] from(
{(ext.Grade == (byte)Domain.Enum.SchoolGrade.SeniorMiddleSchool && !SchUtils.Canshow2("hsa.keyundergraduate", schftype) ? //国际|外籍 高中
"SELECT year AS [key], extId AS [value], convert(real,(case when [count]>0 then 1 else 0 end)) AS [message] FROM SchoolAchievement WHERE extId=@extId and IsValid=1 "
: // 普通高中 + 初中,小学,幼儿园
$@"SELECT year AS [key], extId AS [value], Completion AS [message] 
FROM {(ext.Grade == (byte)SchoolGrade.SeniorMiddleSchool ? "HighSchoolAchievement" :
ext.Grade == (byte)SchoolGrade.JuniorMiddleSchool ? "MiddleSchoolAchievement" :
ext.Grade == (byte)SchoolGrade.PrimarySchool ? "PrimarySchoolAchievement" :
ext.Grade == (byte)SchoolGrade.Kindergarten ? "KindergartenAchievement" : "")}
WHERE extId=@extId and IsValid=1 "
)}
{(ext.Grade.In((byte)SchoolGrade.PrimarySchool,(byte)SchoolGrade.Kindergarten) ? "" //非幼儿园,小学
: @"union 
SELECT year AS [key], extId AS [value], convert(real,(case when [count]>0 then 1 else 0 end)) AS [message] FROM SchoolAchievement WHERE extId=@extId and IsValid=1 "
)}
)T group by [key],[value]
order by [key] desc
";
                dto.Achievement = UnitOfWork.DbConnection.Query<int, Guid, float, KeyValueDto<Guid>>(sql, (key, value, message) =>
                {
                    return new KeyValueDto<Guid> { Key = key.ToString(), Value = value, Message = message.ToString() };
                }, new { extId = request.Eid }, UnitOfWork.DbTransaction, true, "value,message").ToList();

                #endregion

                var QueryAuditSql = @"select top 1 * from dbo.SchoolAudit a where a.Sid=@Sid and a.IsValid=1 order by CreateTime desc";
                var a = UnitOfWork.DbConnection.QueryFirstOrDefault<SchoolAudit>(QueryAuditSql, new { Sid = request.Sid });
                dto.CurrAuditMessage = a?.AuditMessage;

                #region 部分字段不同年份数据（年份标签）
                Dictionary<string, string> quryparam = new Dictionary<string, string>();
                quryparam.Add("Eid", ext.Id.ToString());
                quryparam.Add("IsValid", "1");
                //dto.YearTagList = _schoolYearFieldContentRepository.GetListByFileds(quryparam).ToList();
                dto.YearTagList =_mediator.Send(new LatestYearFieldDataQuery { EId = ext.Id }).Result;
                #endregion

                // 图片
                var dict = await _mediator.Send(new GetSchoolExtImgsQuery
                {
                    Eid = ext.Id,
                    Types = new[]
                    {
                        (byte)SchoolImageEnum.SchoolBrand,
                    }
                });
                dto.Imgs = dict.Values;
            }
            return dto;
        }
    }
}
