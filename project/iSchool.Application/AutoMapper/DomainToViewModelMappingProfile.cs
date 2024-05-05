using AutoMapper;
using iSchool.Application.Service;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            CreateMap<School, SchoolDto>()
                .ForMember(target => target.EName, option => option.MapFrom(source => source.Name_e))
                .ForMember(target => target.Sid, option => option.MapFrom(source => source.Id))
                .ForMember(target => target.ExtList, option => option.MapFrom(source => new List<Domain.Modles.ExtItem>()))
                .ForMember(target => target.Completion, option => option.MapFrom(source => 0));

            CreateMap<SchoolExtension, SchoolExtensionDto>()
               .ForMember(target => target.Source, option => option.MapFrom(source => (JsonSerializationHelper.Deserialize(source.Source))))
               .ForMember(target => target.ExtId, option => option.MapFrom(source => source.Id))
               .ForMember(target => target.Completion, option => option.MapFrom(source => 0));

            CreateMap<SchoolExtContent, SchoolExtContentDto>()
                .ForMember(target => target.Authentication, option => option.MapFrom(source => string.IsNullOrEmpty(source.Authentication) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Authentication)))
                .ForMember(target => target.Abroad, option => option.MapFrom(source => string.IsNullOrEmpty(source.Abroad) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Abroad)))
                .ForMember(target => target.Characteristic, option => option.MapFrom(source => string.IsNullOrEmpty(source.Characteristic) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Characteristic)))
                .ForMember(target => target.ProfileVideos, option => option.MapFrom(source => new List<KeyValueDto<string>>()))
                .ForMember(target => target.InterviewVideos, option => option.MapFrom(source => new List<KeyValueDto<string>>()))
                .ForMember(target => target.ExperienceVideos, option => option.MapFrom(source => new List<KeyValueDto<string>>()))
                .ForMember(target => target.Openhours, option => option.MapFrom(source => string.IsNullOrEmpty(source.Openhours) ? new KeyValueDto<string>[0] : JsonSerializationHelper.JSONToObject<KeyValueDto<string>[]>(source.Openhours)))
                .ForMember(target => target.Calendar, option => option.MapFrom(source => string.IsNullOrEmpty(source.Calendar) ? new KeyValueDto<string>[0] : JsonSerializationHelper.JSONToObject<KeyValueDto<string>[]>(source.Calendar)))
                .ForMember(target => target.CounterPart, option => option.MapFrom(source => string.IsNullOrEmpty(source.Counterpart) ? new List<KeyValueDto<Guid>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<Guid>>>(source.Counterpart)));

            CreateMap<SchoolExtCharge, SchoolExtChargeDto>()
                .ForMember(t => t.Otherfee, option => option.MapFrom((s, t) => s.Otherfee == null ? new KeyValueDto<double>[0] : s.Otherfee.ToObject<KeyValueDto<double>[]>(true)));

            CreateMap<SchoolExtQuality, SchoolExtQualityDto>()
                .ForMember(t => t.Videos, option => option.MapFrom((s, t) => s.Videos == null ? new string[0] : s.Videos.ToObject<string[]>(true)));

            CreateMap<SchoolExtRecruit, SchoolExtRecruitDto>()
                .ForMember(t => t.Point, option => option.MapFrom(source => string.IsNullOrEmpty(source.Point) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Point)))
                .ForMember(t => t.Date, option => option.MapFrom(source => string.IsNullOrEmpty(source.Date) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Date)))
                .ForMember(t => t.Subjects, option => option.MapFrom(source => string.IsNullOrEmpty(source.Subjects) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Subjects)))
                .ForMember(t => t.Target, option => option.MapFrom(source => string.IsNullOrEmpty(source.Target) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Target)));


            CreateMap<SchoolExtLife, SchoolExtLifeDto>();

            CreateMap<SchoolExtCourse, SchoolExtCourseDto>()
                .ForMember(t => t.Courses, option => option.MapFrom(source => string.IsNullOrEmpty(source.Courses) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Courses)))
                .ForMember(t => t.Authentication, option => option.MapFrom(source => string.IsNullOrEmpty(source.Authentication) ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Authentication)));

            CreateMap<SchoolExtAlgHwf, Service.Alg2QyRstDto>()
                .ForMember(t => t.LodgingFacilities, option => option.MapFrom((s, t) => s.LodgingFacilities.ToObject<KeyValueDto<int>[]>() ?? new KeyValueDto<int>[0]));

            CreateMap<Service.Audit.ExportDeletedSchextDpwdCommand, Service.Audit.DpwdRecorrQuery>();

            CreateMap<SchoolYearFieldContent, SchoolYearFieldContentDto>()
                .ForMember(t => t.Target, option => option.MapFrom(source => string.IsNullOrEmpty(source.Content) || source.Field != "Target" && source.Field != "Subjects" ? new List<KeyValueDto<string>>() : JsonSerializationHelper.JSONToObject<List<KeyValueDto<string>>>(source.Content)));

            CreateMap<SchoolImage, SchoolImageDto>();

            //学校图片附件的映射
            CreateMap<SchoolImageDto, VueUploadImgArryItem>()
                .ForMember(x => x.Title, o => o.MapFrom(s => s.ImageDesc))
                .ForMember(x => x.ShowDel, o => o.MapFrom(s => false))
                .ForMember(x => x.Url, o => o.MapFrom(s => new VueUploadImgArryItemUrl
                {
                   CompressUrl=s.SUrl,
                   Url=s.Url
                }));
        }
    }
}
