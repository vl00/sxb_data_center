using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class SchoolExtQuery : IRequest<SchoolExtQyResult>
    {
        public Guid ExtId { get; set; }
        public string Type { get; set; }
    }

    public class SchoolExtQyResult { }
}

namespace iSchool.Application.Service.Audit
{
    /// <summary>
    /// 基本信息
    /// </summary>
    public class SchoolExtQyResult_Ext : SchoolExtQyResult
    {
        public SchoolExtension Data { get; set; }

        public string[] TagNames { get; set; } = new string[0];
    }

    /// <summary>
    /// 学部概况
    /// </summary>
    public class SchoolExtQyResult_Content : SchoolExtQyResult
    {
        public SchoolExtContent Data { get; set; }
        public string ProvinceName { get; set; }
        public string CityName { get; set; }
        public string AreaName { get; set; }
        public TagItem[] AuthTags { get; set; } = new TagItem[0];
        public TagItem[] AbroadTags { get; set; } = new TagItem[0];
        public TagItem[] CharacteristicTags { get; set; } = new TagItem[0];
        //public SchoolVideo[] Videos { get; set; } = new SchoolVideo[0];

        /// <summary>
        /// 学校视频(学校简介、线上体验课、学校专访)
        /// </summary>
        public IEnumerable<VideosInfo> Videos { get; set; }

        /// <summary>
        /// 学校品牌图片
        /// </summary>
        public IEnumerable<UploadImgDto> SchoolImages { get; set; }
        public List<SchoolYearFieldContentDto> YearTagList { get; set; }

        /// <summary>
        /// 寄宿走读（0：未收录；1：走读；2：寄宿；3：可走读、寄宿）
        /// </summary>
        public int Lodging { get; set; }

    }

    /// <summary>
    /// 招生
    /// </summary>
    public class SchoolExtQyResult_Recruit : SchoolExtQyResult
    {
        public SchoolExtRecruit Data { get; set; }
        public TagItem[] TargetTags { get; set; } = new TagItem[0];
        public TagItem[] SubjectsTags { get; set; } = new TagItem[0];
        public List<SchoolYearFieldContentDto> YearTagList { get; set; }
    }

    /// <summary>
    /// 课程
    /// </summary>
    public class SchoolExtQyResult_Course : SchoolExtQyResult
    {
        public SchoolExtCourse Data { get; set; }
        public TagItem[] CoursesTags { get; set; } = new TagItem[0];
        public TagItem[] AuthTags { get; set; } = new TagItem[0];
    }

    /// <summary>
    /// 收费标准
    /// </summary>
    public class SchoolExtQyResult_Charge : SchoolExtQyResult
    {
        public SchoolExtCharge Data { get; set; }
        public List<SchoolYearFieldContentDto> YearTagList { get; set; }
    }

    /// <summary>
    /// 师资力量及教学质量
    /// </summary>
    public class SchoolExtQyResult_Quality : SchoolExtQyResult
    {
        public SchoolExtQuality Data { get; set; }

        /// <summary>
        /// 校长风采视频
        /// </summary>
        public IEnumerable<VideosInfo> SchoolVideos { get; set; }

        /// <summary>
        /// 校长风采、教师风采、学校荣誉、学生荣誉--图片
        /// </summary>
        public IEnumerable<UploadImgDto> SchoolImages { get; set; }
    }

    /// <summary>
    /// 生活
    /// </summary>
    public class SchoolExtQyResult_Life : SchoolExtQyResult
    { 
        public SchoolExtLife Data { get; set; }

        /// <summary>
        /// 硬件设施及校园生活
        /// </summary>
        public IEnumerable<UploadImgDto> SchoolImages { get; set; }
    }

    public class SchoolExtQyResult_Alg1 : SchoolExtQyResult
    {
        public Alg1QyRstDto Data { get; set; }
    }

    public class SchoolExtQyResult_Alg2 : SchoolExtQyResult
    {
        public Alg2QyRstDto Data { get; set; }
    }

    public class SchoolExtQyResult_Alg3 : SchoolExtQyResult
    {
        public Alg3QyRstDto Data { get; set; }
    }
}
