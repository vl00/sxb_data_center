using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class AddSchoolExtContentCommand : IRequest<HttpResponse<object>>
    {
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
        public int? Province { get; set; }
        public int? City { get; set; }
        public int? Area { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Tel { get; set; }
       /// <summary>
       /// 是否寄宿
       /// </summary>
        public string Lodging { get; set; }        
        /// <summary>
        /// 走读寄宿需要保存的值
        /// </summary>
        public Dictionary<string,bool?> LodgingExtern { get; set; }

        /// <summary>
        /// 学生人数
        /// </summary>
        public int? Studentcount { get; set; }

        /// <summary>
        /// 教师人数
        /// </summary>
        public int? Teachercount { get; set; }

        /// <summary>
        /// 师生比例
        /// </summary>
        public float? TsPercent { get; set; }

        public string Authentication { get; set; }
        public string Abroad { get; set; }
        /// <summary>有无校车</summary>
        public bool? HasSchoolBus { get; set; }
        //有无饭堂
        public bool? Canteen { get; set; }
        //伙食情况
        public string Meal { get; set; }
        //学校特色课程或项目
        public string Characteristic { get; set; }
        public string Project { get; set; }
        //高级教师
        public int? SeniorTea { get; set; }
        //特级教师
        public int? CharacteristicTea { get; set; }
        /// <summary>
        /// 外教人数
        /// </summary>
        public int? ForeignTeaCount { get; set; }
        /// <summary>
        /// 外教占比
        /// </summary>
        public float? ForeignTea { get; set; }
        //建校时间
        public DateTimeOffset? Creationdate { get; set; }
        //建筑面积
        public double? Acreage { get; set; }
        ///// <summary>
        ///// 学校简介
        ///// </summary>
        //public string ProfileVideos { get; set; }
        ///// <summary>
        ///// 学校专访
        ///// </summary>
        //public string InterviewVideos { get; set; }

        ///// <summary>
        ///// 体验课程
        ///// </summary>
        //public string ExperienceVideos { get; set; }

        /// <summary>
        /// 用于删除历史数据
        /// </summary>
        public string CurrentVideoTypes { get; set; }

        /// <summary>
        /// 视频类型
        /// </summary>
        public string[] Types { get; set; }

        /// <summary>
        /// 视频url
        /// </summary>
        public string[] Videos { get; set; }

        /// <summary>
        /// 视频封面图url
        /// </summary>
        public string[] Covers { get; set; }

        /// <summary>
        /// 视频描写
        /// </summary>
        public string[] VideoDescs { get; set; }

        /// <summary>
        /// 开放日的名字
        /// </summary>
        public string[] OpenHourName { get; set; }

        /// <summary>
        /// 开放日的时间
        /// </summary>
        public string[] OpenHourTime { get; set; }

        /// <summary>
        /// 行事历的名称
        /// </summary>
        public string[] CalendarName { get; set; }

        /// <summary>
        /// 行事历的时间
        /// </summary>
        public string[] CalendarTime { get; set; }
        /// <summary>
        /// 划片范围
        /// </summary>
        public string Range { get; set; }
        public string[] CounterPart { get; set; }
        public string[] CounterPartText { get; set; }
        public string[] CounterPartYears { get; set; }
        /// <summary>
        /// 课后
        /// </summary>
        public string AfterClass { get; set; }

        public float? Completion { get; set; }

        public Guid UserId { get; set; }
        
        public YearChange[] Yearslist { get; set; }
        
        // 图片信息
        public IEnumerable<UploadImgDto> Imgs { get; set; }
    }
}
