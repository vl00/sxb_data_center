using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class AddSchoolExtContentCommandHandler : IRequestHandler<AddSchoolExtContentCommand, HttpResponse<object>>
    {
        private readonly IRepository<SchoolYearFieldContent> _schoolYearFieldContentRepository;
        private readonly IRepository<SchoolExtension> _schoolextensionRepository;
        private readonly IRepository<SchoolExtContent> _schoolextcontentRepository;
        //private readonly IRepository<SchoolVideo> _schoolVideoRepository;
        private readonly IRepository<SchoolImage> _schoolImageRepository;
        private ILog _log;
        private readonly IMediator _mediator;

        private UnitOfWork UnitOfWork { get; set; }

        public AddSchoolExtContentCommandHandler(
            IRepository<SchoolExtension> schoolextensionRepository, IRepository<SchoolYearFieldContent> schoolYearFieldContentRepository,
            IRepository<SchoolExtContent> schoolextcontentRepository,
            //IRepository<SchoolVideo> schoolVideoRepository,
            IRepository<SchoolImage> schoolImageRepository,
            IUnitOfWork unitOfWork
            , IMediator mediator,
            ILog log)
        {
            _schoolYearFieldContentRepository = schoolYearFieldContentRepository;
            _schoolextensionRepository = schoolextensionRepository;
            _schoolextcontentRepository = schoolextcontentRepository;
            //_schoolVideoRepository = schoolVideoRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
            _log = log;
            _mediator = mediator;
            _schoolImageRepository = schoolImageRepository;
        }

        public async Task<HttpResponse<object>> Handle(AddSchoolExtContentCommand request, CancellationToken cancellationToken)
        {
            if (request.Eid == Guid.Empty)
                return new HttpResponse<object>() { State = 404, Message = "没有找到学部！" };
            var ext = _schoolextensionRepository.GetIsValid(p => p.Id == request.Eid);
            if (ext == null)
                return new HttpResponse<object>() { State = 404, Message = "没有找到学部！" };


            //开放日
            var openhourKeyValue = new List<KeyValueDto<string>>();
            if (request.OpenHourName != null && request.OpenHourTime != null)
            {
                var openhourCount = request.OpenHourName.Count() > request.OpenHourTime.Count() ? request.OpenHourTime.Count() : request.OpenHourName.Count();
                for (int i = 0; i < openhourCount; i++)
                {
                    if (!string.IsNullOrEmpty(request.OpenHourName[i]) && !string.IsNullOrEmpty(request.OpenHourTime[i]))
                    {
                        openhourKeyValue.Add(new KeyValueDto<string> { Key = request.OpenHourName[i], Value = request.OpenHourTime[i] });
                    }
                }
            }
            //行事日历
            var calendarKeyValue = new List<KeyValueDto<string>>();
            if (request.CalendarName != null && request.CalendarTime != null)
            {
                var calendarCount = request.CalendarName.Count() > request.CalendarTime.Count() ? request.CalendarTime.Count() : request.CalendarName.Count();
                for (int i = 0; i < calendarCount; i++)
                {
                    if (!string.IsNullOrEmpty(request.CalendarName[i]) && !string.IsNullOrEmpty(request.CalendarTime[i]))
                    {
                        calendarKeyValue.Add(new KeyValueDto<string> { Key = request.CalendarName[i], Value = request.CalendarTime[i] });
                    }
                }
            }

            //对口学校
            var counterPartKeyValue = new List<KeyValueDto<string>>();
            if (request.CounterPart != null && request.CounterPartText != null)
            {
                var counterPartCount = request.CounterPart.Count() > request.CounterPartText.Count() ? request.CounterPartText.Count() : request.CounterPart.Count();

                if (request.CounterPartYears != null)
                {
                    for (int i = 0; i < counterPartCount; i++)
                    {
                        if (!string.IsNullOrEmpty(request.CounterPart[i]) && !string.IsNullOrEmpty(request.CounterPartText[i]))
                        {
                            counterPartKeyValue.Add(new KeyValueDto<string> { Year = request.CounterPartYears[i], Key = request.CounterPartText[i], Value = request.CounterPart[i] });
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < counterPartCount; i++)
                    {
                        if (!string.IsNullOrEmpty(request.CounterPart[i]) && !string.IsNullOrEmpty(request.CounterPartText[i]))
                        {
                            counterPartKeyValue.Add(new KeyValueDto<string> { Key = request.CounterPartText[i], Value = request.CounterPart[i] });
                        }
                    }
                }

            }


            var extContent = _schoolextcontentRepository.GetIsValid(p => p.Eid == request.Eid);

            #region 师生比例、外教占比
            //float? tsPercent = BusinessHelper.GetFloatByRounding(request.Teachercount, request.Studentcount) ?? request.TsPercent,
            //foreignTea = BusinessHelper.GetFloatByRounding(request.ForeignTeaCount,request.Teachercount) ?? request.ForeignTea;

            float? tsPercent = BusinessHelper.GetFloatByRounding(request.Teachercount, request.Studentcount) ?? request.TsPercent,
            foreignTea = BusinessHelper.GetPercentage(request.ForeignTeaCount, request.Teachercount) ?? request.ForeignTea;
            #endregion

            if (extContent == null)
            {
                //添加
                try
                {
                    UnitOfWork.BeginTransaction();
                    extContent = new SchoolExtContent
                    {
                        Id = Guid.NewGuid(),
                        Abroad = request.Abroad,
                        Acreage = request.Acreage,
                        Address = request.Address,
                        Afterclass = request.AfterClass,
                        Area = request.Area,
                        Authentication = request.Authentication,
                        Calendar = JsonSerializationHelper.Serialize(calendarKeyValue),
                        HasSchoolBus = request.HasSchoolBus,
                        Canteen = request.Canteen,
                        City = request.City,
                        Characteristic = request.Characteristic,
                        CharacteristicTea = request.CharacteristicTea,
                        //Counterpart = JsonSerializationHelper.Serialize(counterPartKeyValue),
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        LatLong = request.Longitude != null && request.Latitude != null ? new LngLatLocation(request.Longitude.Value, request.Latitude.Value) : null,
                        Lodging = request.LodgingExtern["lodging"],
                        SdExtern = request.LodgingExtern["sdextern"],
                        ForeignTeaCount = request.ForeignTeaCount,
                        //外教占比
                        ForeignTea = foreignTea,
                        Eid = request.Eid,
                        Project = request.Project,
                        Meal = request.Meal,
                        //Range = request.Range,
                        SeniorTea = request.SeniorTea,
                        //Completion = request.Completion != null ? (float)Math.Round(request.Completion.Value, 2) : 0,
                        Completion = request.Completion ?? 0,
                        Openhours = JsonSerializationHelper.Serialize(openhourKeyValue),
                        Studentcount = request.Studentcount,
                        Teachercount = request.Teachercount,
                        Creationdate = request.Creationdate,
                        Tel = request.Tel,
                        Province = request.Province,
                        //师生比例
                        TsPercent = tsPercent,
                        //Tuition = request.Tuition,
                        CreateTime = DateTime.Now,
                        Creator = request.UserId,
                        Modifier = request.UserId,
                        ModifyDateTime = DateTime.Now,
                        IsValid = true
                    };
                    _schoolextcontentRepository.Insert(extContent);

                    UnitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    var message = ex.InnerException;
                    UnitOfWork.Rollback();
                    _log.Error(ex);
                    return new HttpResponse<object> { State = 500, Message = "操作失败！" };
                }
            }
            else //修改
            {                
                try
                {
                    UnitOfWork.BeginTransaction();
                    //修改
                    extContent.Abroad = request.Abroad;
                    extContent.Acreage = request.Acreage;
                    extContent.Address = request.Address;
                    extContent.Afterclass = request.AfterClass;
                    extContent.Area = request.Area;
                    extContent.Authentication = request.Authentication;
                    extContent.Calendar = JsonSerializationHelper.Serialize(calendarKeyValue);
                    extContent.HasSchoolBus = request.HasSchoolBus;
                    extContent.Canteen = request.Canteen;
                    extContent.City = request.City;
                    extContent.Characteristic = request.Characteristic;
                    extContent.CharacteristicTea = request.CharacteristicTea;
                    //extContent.Counterpart = JsonSerializationHelper.Serialize(counterPartKeyValue);
                    extContent.Latitude = request.Latitude;
                    extContent.Longitude = request.Longitude;
                    extContent.LatLong = request.Longitude != null && request.Latitude != null ? new LngLatLocation(request.Longitude.Value, request.Latitude.Value) : null;
                    extContent.Lodging = request.LodgingExtern["lodging"];
                    extContent.SdExtern = request.LodgingExtern["sdextern"];
                    extContent.ForeignTeaCount = request.ForeignTeaCount;
                    //外教占比
                    extContent.ForeignTea = foreignTea;
                    extContent.Eid = request.Eid;
                    extContent.Project = request.Project;
                    extContent.Meal = request.Meal;
                    //extContent.Range = request.Range;
                    extContent.SeniorTea = request.SeniorTea;
                    extContent.Completion = request.Completion ?? 0;
                    extContent.Openhours = JsonSerializationHelper.Serialize(openhourKeyValue);
                    extContent.Studentcount = request.Studentcount;
                    extContent.Teachercount = request.Teachercount;
                    extContent.Creationdate = request.Creationdate;
                    extContent.Tel = request.Tel;
                    extContent.Province = request.Province;
                    //师生比例
                    extContent.TsPercent = tsPercent;                    
                    extContent.ModifyDateTime = DateTime.Now;
                    extContent.Modifier = request.UserId;
                    _schoolextcontentRepository.Update(extContent);

                    UnitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    UnitOfWork.Rollback();
                    _log.Error(ex);
                    return new HttpResponse<object> { State = 500, Message = "操作失败！" };

                }
            }

            //视频
            {
                await _mediator.Send(new AddSchoolVideoCommand()
                {
                    Videos = request.Videos,
                    Covers = request.Covers,
                    Eid = request.Eid,
                    OperatorId = request.UserId,
                    VideoDescs = request.VideoDescs,
                    Types = request.Types,
                    CurrentVideoTypes = request.CurrentVideoTypes
                });
            }

            // 处理图片的数据
            await _mediator.Send(new UploadImgsCommand
            {
                Eid = request.Eid,
                UserId = request.UserId,
                Imgs = request.Imgs,
            });

            // 年份字段
            await _mediator.Send(new SaveExtYearFieldChangesCommand { Sid = request.Sid, Eid = extContent.Eid, Yearslist = request.Yearslist });

            await _mediator.Publish(new SchoolUpdatedEvent { Sid = request.Sid, Eid = request.Eid });
            return new HttpResponse<object> { State = 200 };
        }
    }
}
