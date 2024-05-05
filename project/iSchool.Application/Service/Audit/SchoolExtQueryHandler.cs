using Dapper;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Domain.Enum;
using iSchool.Application.ViewModels;
using static iSchool.Infrastructure.ObjectHelper;
using AutoMapper;

namespace iSchool.Application.Service.Audit
{
    public class SchoolExtQueryHandler : IRequestHandler<SchoolExtQuery, SchoolExtQyResult>
    {
        UnitOfWork unitOfWork;
        IMediator mediator;
        IRepository<Tag> tagRepository;
        IRepository<SchoolExtension> schoolExtensionRepository;
        IRepository<SchoolExtContent> schoolContentRepository;
        IRepository<SchoolVideo> schoolVideoRepository;
        IRepository<SchoolImage> schoolImageRepository;
        IRepository<SchoolExtRecruit> schoolRecruitRepository;
        IRepository<SchoolExtCourse> schoolCourseRepository;
        IRepository<SchoolExtCharge> schoolChargeRepository;
        IRepository<SchoolExtQuality> schoolQualityRepository;
        IRepository<SchoolExtLife> schoolExtLifeRepository;
        IRepository<SchoolYearFieldContent> _schoolYearFieldContentRepository;
        private readonly IMapper _mapper;

        public SchoolExtQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator,
            IRepository<Tag> tagRepository,
            IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<SchoolExtContent> schoolContentRepository,
            IRepository<SchoolVideo> schoolVideoRepository,
            IRepository<SchoolImage> schoolImageRepository,
            IRepository<SchoolExtRecruit> schoolRecruitRepository,
            IRepository<SchoolExtCourse> schoolCourseRepository,
            IRepository<SchoolExtCharge> schoolChargeRepository,
            IRepository<SchoolExtQuality> schoolQualityRepository,
            IRepository<SchoolExtLife> schoolExtLifeRepository, IRepository<SchoolYearFieldContent> schoolYearFieldContentRepository)
        {
            this._mapper = mapper;
            this._schoolYearFieldContentRepository = schoolYearFieldContentRepository;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.mediator = mediator;
            this.tagRepository = tagRepository;
            this.schoolExtensionRepository = schoolExtensionRepository;
            this.schoolContentRepository = schoolContentRepository;
            this.schoolVideoRepository = schoolVideoRepository;
            this.schoolImageRepository = schoolImageRepository;
            this.schoolRecruitRepository = schoolRecruitRepository;
            this.schoolCourseRepository = schoolCourseRepository;
            this.schoolChargeRepository = schoolChargeRepository;
            this.schoolQualityRepository = schoolQualityRepository;
            this.schoolExtLifeRepository = schoolExtLifeRepository;
        }

        Task<SchoolExtQyResult> IRequestHandler<SchoolExtQuery, SchoolExtQyResult>.Handle(SchoolExtQuery req, CancellationToken cancellationToken)
        {
            var schext = schoolExtensionRepository.GetIsValid(_ => _.Id == req.ExtId);
            if (schext == null) throw new Exception("学部无效");

            SchoolExtQyResult res = null;
            switch (req.Type?.ToLower())
            {
                case "ext": //基本信息
                    res = Handle_ext(req);
                    break;
                case "content": //学部概况 
                    res = Handle_content(req);
                    break;
                case "recruit": //招生
                    res = Handle_Recruit(req);
                    break;
                case "course": //课程
                    res = Handle_Course(req);
                    break;
                case "charge": //收费
                    res = Handle_Charge(req);
                    break;
                case "quality": //师资力量及教学质量
                    res = Handle_Quality(req);
                    break;
                case "life": //硬件设施及学生生活
                    res = Handle_Life(req);
                    break;

                case "alg1": //算法-社会
                    res = Handle_alg1(req, schext.Sid);
                    break;
                case "alg2": //算法-经济
                    res = Handle_alg2(req, schext.Sid);
                    break;
                case "alg3": //算法-个人
                    res = Handle_alg3(req, schext.Sid);
                    break;
            }
           
            return Task.FromResult(res);
        }

        SchoolExtQyResult_Ext Handle_ext(SchoolExtQuery req)
        {
            var res = new SchoolExtQyResult_Ext();

            res.Data = schoolExtensionRepository.Get(req.ExtId);

            {
                var sql = @"
                            select t.name from dbo.GeneralTag t 
                            inner join GeneralTagBind b on t.id=b.tagID
                            where b.dataID=@eid and b.dataType=2
                            ";

                res.TagNames = unitOfWork.DbConnection.Query<string>(sql, new { eid = req.ExtId }).ToArray();
            }

            return res;
        }

        #region 学校概况
        /// <summary>
        /// 学校概况
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
         SchoolExtQyResult_Content Handle_content(SchoolExtQuery req)
        {
            
            var res = new SchoolExtQyResult_Content();

            res.Data =  schoolContentRepository.GetAll(_ => _.IsValid && _.Eid == req.ExtId).FirstOrDefault();
            if (res.Data == null) return res;

            //省市区
            {
                //var citydata = mediator.Send(new CitydataQuery()).Result;

                //res.ProvinceName = citydata.FirstOrDefault(_ => _.Id == (res.Data.Province ?? 0))?.Name;
                //res.CityName = citydata.FirstOrDefault(_ => _.Id == (res.Data.City ?? 0))?.Name;
                //res.AreaName = citydata.FirstOrDefault(_ => _.Id == (res.Data.Area ?? 0))?.Name;

                var sql = @"
select name from dbo.KeyValue where id=@Province and type=1 and IsValid=1;
select name from dbo.KeyValue where id=@City and type=1 and IsValid=1;
select name from dbo.KeyValue where id=@Area and type=1 and IsValid=1;
";
                var g = unitOfWork.DbConnection.QueryMultiple(sql, res.Data);
                res.ProvinceName = g.ReadFirstOrDefault<string>();
                res.CityName = g.ReadFirstOrDefault<string>();
                res.AreaName = g.ReadFirstOrDefault<string>();
            }
            //学校认证
            {
                res.AuthTags = Tryv0(() => res.Data.Authentication.ToObject<KeyValueDto<Guid>[]>(true), new KeyValueDto<Guid>[0])
                    .Select(_ => new TagItem { Id = _.Value, Name = _.Key })
                    .ToArray();

                //var authIds = Tryv0(() => res.Data.Authentication.ToObject<Guid[]>(), new Guid[0]);
                //if (authIds.Any())
                //{
                //    res.AuthTags = tagRepository.GetAll(_ => _.IsValid == true)
                //        .Where(_ => authIds.Contains(_.Id))
                //        .ToArray()
                //        .Select(_ => new TagItem
                //        {
                //            Id = _.Id,
                //            Name = _.Name,
                //            SpellCode = _.SpellCode,
                //            CreateTime = _.CreateTime,
                //        })
                //        .ToArray();
                //}
            }
            //出国方向
            {
                res.AbroadTags = Tryv0(() => res.Data.Abroad.ToObject<KeyValueDto<Guid>[]>(true), new KeyValueDto<Guid>[0])
                    .Select(_ => new TagItem { Id = _.Value, Name = _.Key })
                    .ToArray();

                //var abroadIds = Tryv0(() => res.Data.Abroad.ToObject<Guid[]>(), new Guid[0]);
                //if (abroadIds.Any())
                //{
                //    res.AbroadTags = tagRepository.GetAll(_ => _.IsValid == true)
                //        .Where(_ => abroadIds.Contains(_.Id))
                //        .ToArray()
                //        .Select(_ => new TagItem
                //        {
                //            Id = _.Id,
                //            Name = _.Name,
                //            SpellCode = _.SpellCode,
                //            CreateTime = _.CreateTime,
                //        })
                //        .ToArray();
                //}
            }
            //特色项目或课程图文
            {
                res.CharacteristicTags = Tryv0(() => res.Data.Characteristic.ToObject<KeyValueDto<Guid>[]>(true), new KeyValueDto<Guid>[0])
                    .Select(_ => new TagItem { Id = _.Value, Name = _.Key })
                    .ToArray();

                //var characteristicIds = Tryv0(() => res.Data.Characteristic.ToObject<Guid[]>(), new Guid[0]);
                //if (characteristicIds.Any())
                //{
                //    res.CharacteristicTags = tagRepository.GetAll(_ => _.IsValid == true)
                //        .Where(_ => characteristicIds.Contains(_.Id))
                //        .ToArray()
                //        .Select(_ => new TagItem
                //        {
                //            Id = _.Id,
                //            Name = _.Name,
                //            SpellCode = _.SpellCode,
                //            CreateTime = _.CreateTime,
                //        })
                //        .ToArray();
                //}
            }
            //学校视频(学校简介、线上体验课、学校专访)
            {
                var types = new byte[] 
                {
                     (byte)VideoType.Profile
                    ,(byte)VideoType.Experience
                    ,(byte)VideoType.Interview
                };
                res.Videos = schoolVideoRepository.GetAll(_ => _.IsValid && _.Eid == req.ExtId && _.Type != (byte)VideoType.Principal)
                    .OrderBy(_ => _.Type).ThenBy(_ => _.Sort).Select(p => new VideosInfo()
                    {
                        VideoUrl = p.VideoUrl,
                        VideoDesc = p.VideoDesc,
                        Type = p.Type,
                        Cover = p.Cover
                    });

            }
            //学校品牌图片
            {
                var dict =  mediator.Send(new GetSchoolExtImgsQuery
                {
                    Eid = req.ExtId,
                    Types = new[]
                  {
                        
                        (byte)SchoolImageEnum.SchoolBrand
                    }
                }).Result;
                res.SchoolImages = dict.Values;
            }

            #region 部分字段不同年份数据（年份标签）
            //Dictionary<string, string> quryparam = new Dictionary<string, string>();
            //quryparam.Add("Eid", req.ExtId.ToString());
            //quryparam.Add("IsValid", "1");
            //var lisYear = _schoolYearFieldContentRepository.GetListByFileds(quryparam);
            //var listYearDto = new List<SchoolYearFieldContentDto>();
            //foreach (var year in lisYear)
            //{

            //    var ydto = _mapper.Map<SchoolYearFieldContentDto>(year);
            //    listYearDto.Add(ydto);


            //}
            //res.YearTagList = listYearDto;

            #endregion

            #region 部分字段最新年份数据（年份标签）
            res.YearTagList = mediator.Send(new LatestYearFieldDataQuery { EId = req.ExtId })?.Result;
            #endregion

            //寄宿走读
            {
                res.Lodging = BusinessHelper.SetLodgingSdExternSelectValue(res.Data.Lodging, res.Data.SdExtern);
            }
            return res;
        } 
        #endregion

        SchoolExtQyResult_Recruit Handle_Recruit(SchoolExtQuery req)
        {
            var res = new SchoolExtQyResult_Recruit();

            res.Data = schoolRecruitRepository.GetAll(_ => _.IsValid && _.Eid == req.ExtId).FirstOrDefault();

            res.TargetTags = Tryv0(() => res.Data?.Target.ToObject<KeyValueDto<Guid>[]>(true), new KeyValueDto<Guid>[0])
                .Select(_ => new TagItem { Id = _.Value, Name = _.Key })
                .ToArray();

            res.SubjectsTags = Tryv0(() => res.Data?.Subjects.ToObject<KeyValueDto<Guid>[]>(true), new KeyValueDto<Guid>[0])
                .Select(_ => new TagItem { Id = _.Value, Name = _.Key })
                .ToArray();
            #region 部分字段不同年份数据（年份标签）
            //Dictionary<string, string> quryparam = new Dictionary<string, string>();
            //quryparam.Add("Eid", req.ExtId.ToString());
            //quryparam.Add("IsValid", "1");
            //var lisYear = _schoolYearFieldContentRepository.GetListByFileds(quryparam);
            //var listYearDto = new List<SchoolYearFieldContentDto>();
            //foreach (var year in lisYear)
            //{

            //    var ydto = _mapper.Map<SchoolYearFieldContentDto>(year);
            //    listYearDto.Add(ydto);


            //}
            //res.YearTagList = listYearDto;

            #endregion

            #region 部分字段最新年份数据（年份标签）
            res.YearTagList = mediator.Send(new LatestYearFieldDataQuery { EId= req.ExtId })?.Result;
            #endregion
            return res;
        }

        SchoolExtQyResult_Course Handle_Course(SchoolExtQuery req)
        {
            var res = new SchoolExtQyResult_Course();

            res.Data = schoolCourseRepository.GetAll(_ => _.IsValid && _.Eid == req.ExtId).FirstOrDefault();
            if (res.Data == null) return res;

            res.CoursesTags = Tryv0(() => res.Data.Courses.ToObject<KeyValueDto<Guid>[]>(true), new KeyValueDto<Guid>[0])
                .Select(_ => new TagItem { Id = _.Value, Name = _.Key })
                .ToArray();

            res.AuthTags = Tryv0(() => res.Data.Authentication.ToObject<KeyValueDto<Guid>[]>(true), new KeyValueDto<Guid>[0])
                .Select(_ => new TagItem { Id = _.Value, Name = _.Key })
                .ToArray();

            return res;
        }

        SchoolExtQyResult_Charge Handle_Charge(SchoolExtQuery req)
        {
            var res = new SchoolExtQyResult_Charge();

            res.Data = schoolChargeRepository.GetAll(_ => _.IsValid && _.Eid == req.ExtId).FirstOrDefault();
            #region 部分字段不同年份数据（年份标签）
            //Dictionary<string, string> quryparam = new Dictionary<string, string>();
            //quryparam.Add("Eid", req.ExtId.ToString());
            //quryparam.Add("IsValid", "1");
            //var lisYear = _schoolYearFieldContentRepository.GetListByFileds(quryparam);
            //var listYearDto = new List<SchoolYearFieldContentDto>();
            //foreach (var year in lisYear)
            //{

            //    var ydto = _mapper.Map<SchoolYearFieldContentDto>(year);
            //    listYearDto.Add(ydto);


            //}
            //res.YearTagList = listYearDto;

            #endregion

            #region 部分字段最新年份数据（年份标签）
            res.YearTagList = mediator.Send(new LatestYearFieldDataQuery { EId = req.ExtId }).Result;
            #endregion
            return res;
        }

        #region 师资力量及教学质量
        SchoolExtQyResult_Quality Handle_Quality(SchoolExtQuery req)
        {
            var res = new SchoolExtQyResult_Quality();

            res.Data = schoolQualityRepository.GetAll(_ => _.IsValid && _.Eid == req.ExtId).FirstOrDefault();
            //校长风采视频res.SchoolVideos
            {
                res.SchoolVideos = schoolVideoRepository.GetAll(_ => _.IsValid && _.Eid == req.ExtId && _.Type == (byte)VideoType.Principal)
                   .OrderBy(_ => _.Type).ThenBy(_ => _.Sort).Select(p => new VideosInfo()
                   {
                       Cover = p.Cover,
                       Type = p.Type,
                       VideoDesc = p.VideoDesc,
                       VideoUrl = p.VideoUrl
                   });


            }
            //校长风采、教师风采、学校荣誉、学生荣誉--图片
            {
                var dict = mediator.Send(new GetSchoolExtImgsQuery
                {
                    Eid = req.ExtId,
                    Types = new[]
                  {
                        (byte)SchoolImageEnum.Principal,
                        (byte)SchoolImageEnum.Teacher,
                        (byte)SchoolImageEnum.SchoolHonor,
                        (byte)SchoolImageEnum.StudentHonor,
                        
                    }
                }).Result;
                res.SchoolImages = dict.Values;              
            }
            return res;
        }
        #endregion

        #region 硬件设施及学生生活
        SchoolExtQyResult_Life Handle_Life(SchoolExtQuery req)
        {
            var res = new SchoolExtQyResult_Life();

            res.Data = schoolExtLifeRepository.GetAll(_ => _.IsValid && _.Eid == req.ExtId).FirstOrDefault();
            //硬件设施、社团活动、各年级课程表、作息时间表、校车路线--图片
            {
                var dict = mediator.Send(new GetSchoolExtImgsQuery
                {
                    Eid = req.ExtId,
                    Types = new[]
                  {
                        (byte)SchoolImageEnum.Hardware,
                        (byte)SchoolImageEnum.CommunityActivities,
                        (byte)SchoolImageEnum.GradeSchedule,
                        (byte)SchoolImageEnum.Schedule,
                        (byte)SchoolImageEnum.Diagram,
                    }
                }).Result;
                res.SchoolImages = dict.Values;
            }
            return res;
        } 
        #endregion

        SchoolExtQyResult_Alg1 Handle_alg1(SchoolExtQuery req, Guid sid)
        {
            var res = new SchoolExtQyResult_Alg1();

            res.Data = mediator.Send(new Alg1Query { Sid = sid, Eid = req.ExtId }).Result;

            return res;
        }

        SchoolExtQyResult_Alg2 Handle_alg2(SchoolExtQuery req, Guid sid)
        {
            var res = new SchoolExtQyResult_Alg2();

            res.Data = mediator.Send(new Alg2Query { Sid = sid, Eid = req.ExtId })?.Result;

            return res;
        }

        SchoolExtQyResult_Alg3 Handle_alg3(SchoolExtQuery req, Guid sid)
        {
            var res = new SchoolExtQyResult_Alg3();

            res.Data = mediator.Send(new Alg3Query { Sid = sid, Eid = req.ExtId })?.Result;

            return res;
        }
    }
}
