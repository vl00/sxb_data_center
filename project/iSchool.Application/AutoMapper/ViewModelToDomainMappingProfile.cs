using AutoMapper;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Infrastructure;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.Service;

namespace iSchool.Application.AutoMapper
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile()
        {
            CreateMap<SchoolExtensionDto, SchoolExtension>()
                .ForMember(target => target.Source,
                option => option.MapFrom(source => JsonSerializationHelper.Serialize(source.Source)));

            CreateMap<SchoolExtQualityDto, SchoolExtQuality>()
                .ForMember(t => t.Videos, option => option.MapFrom((s, t) => s.Videos.ToJsonString()));

            CreateMap<SchoolExtLifeDto, SchoolExtLife>();

            CreateMap<AddSchoolExtRecruitCommand, SchoolExtRecruit>();

            CreateMap<AddSchoolExtCourseCommand, SchoolExtCourse>();

            CreateMap<Alg2QyRstDto, SchoolExtAlgHwf>()
                .ForMember(t => t.LodgingFacilities, option => option.MapFrom((s, t) => s.LodgingFacilities.ToJsonString()))
                .AfterMap((s, t) =>
                {
                    if (t.HasSwimpool != true)
                    {
                        t.SwimpoolWhere = null;
                        t.SwimpoolTemperature = null;
                    }
                    if (t.HasLodging != true)
                    {
                        t.LodgingAreaperp = null;
                        t.LodgingFacilities = null;
                        t.LodgingPersionNum = null;
                    }
                    if (t.HasPgd != true)
                    {
                        t.PgdLength = null;
                    }
                    if (t.HasCanteen != true)
                    {
                        t.CanteenAreaperp = null;
                        t.CanteenHealthRate = null;
                        t.CanteenNum = null;
                    }
                });

            CreateMap<AddSchoolExtChargeCommand, SchoolExtCharge>();
        }
    }
}
