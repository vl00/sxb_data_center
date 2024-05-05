using iSchool.Domain;
using System;
using System.Collections.Generic;

namespace iSchool.Application.ViewModels
{
    public class SchoolExtContentDto
    {
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
        public int? Province { get; set; }
        public int? City { get; set; }
        public int? Area { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public LngLatLocation LatLong { get; set; }
        public string Tel { get; set; }

        /// <summary>
        /// 寄宿走读（0：未收录；1：走读；2：寄宿；3：可走读、寄宿）
        /// </summary>
        public int Lodging { get; set; }

        public int? Studentcount { get; set; }
        /// <summary>
        /// 师生比例
        /// </summary>
        public float? TsPercent { get; set; }

        public int? Teachercount { get; set; }
        //学校认证
        public List<KeyValueDto<string>> Authentication { get; set; }
        //出国方向
        public List<KeyValueDto<string>> Abroad { get; set; }
        /// <summary>有无校车</summary>
        public bool? HasSchoolBus { get; set; }
        //有无饭堂
        public bool? Canteen { get; set; }
        //伙食情况
        public string Meal { get; set; }
        //学校特色课程或项目
        public List<KeyValueDto<string>> Characteristic { get; set; }
        public string Project { get; set; }
        //高级教师
        public int? SeniorTea { get; set; }
        //特级教师
        public int? CharacteristicTea { get; set; }
        //外教人数
        public int? ForeignTeaCount { get; set; }
        //外教占比
        public float? ForeignTea { get; set; }
        //建校时间
        public DateTimeOffset? Creationdate { get; set; }
        //建筑面积
        public double? Acreage { get; set; }
        //学费
        public double? Tuition { get; set; }
        /// <summary>
        /// 学校简介
        /// </summary>
        public List<VideosInfo> ProfileVideos { get; set; }
        /// <summary>
        /// 学校专访
        /// </summary>
        public List<VideosInfo> InterviewVideos { get; set; }
        /// <summary>
        /// 体验课程
        /// </summary>
        public List<VideosInfo> ExperienceVideos { get; set; }
        //public List<KeyValueDto<string>> ExperienceVideos { get; set; }
        /// <summary>
        /// 开放日
        /// </summary>
        public KeyValueDto<string>[] Openhours { get; set; }
        /// <summary>
        /// 行事历
        /// </summary>
        public KeyValueDto<string>[] Calendar { get; set; }
        /// <summary>
        /// 升学成绩
        /// </summary>
        public List<KeyValueDto<Guid>> Achievement { get; set; }
        /// <summary>
        /// 划片范围
        /// </summary>
        public string Range { get; set; }
        /// <summary>
        /// 对口学校
        /// </summary>
        public List<KeyValueDto<Guid>> CounterPart { get; set; }
        /// <summary>
        /// 课后
        /// </summary>
        public string AfterClass { get; set; }
        /// <summary>
        /// 是否中国人学校
        /// </summary>
        public bool? Chinese { get; set; }
        /// <summary>
        /// 是否普惠
        /// </summary>
        public bool? Discount { get; set; }
        /// <summary>
        /// 是否双语
        /// </summary>
        public bool? Diglossia { get; set; }

        public string SchFtype { get; set; }

        public byte? Grade { get; set; }

        public byte? Type { get; set; }
        /// <summary>
        /// 当前审核意见
        /// </summary>
        public string CurrAuditMessage { get; set; }
        /// <summary>
        /// 部分字段年份拓展标签
        /// </summary>
        public List<SchoolYearFieldContentDto> YearTagList { get; set; }
        
        // 图片
        public IEnumerable<UploadImgDto> Imgs { get; set; }
    }

    [Serializable]
    public class KeyValueDto<T>
    {
        public KeyValueDto()
        {
        }
        public string Year { get; set; }
        public string Key { get; set; }
        public T Value { get; set; }
        public string Message { get; set; }
    }


    [Serializable]
    public class KeyValueDto<K, V, M>
    {
        public KeyValueDto()
        {
        }

        public K Key { get; set; }
        public V Value { get; set; }
        public M Message { get; set; }
    }

    [Serializable]
    public class KeyValueDto<K, V, M, D>
    {
        public KeyValueDto()
        {
        }
        public K Key { get; set; }
        public V Value { get; set; }
        public M Message { get; set; }
        public D Desc { get; set; }
    }

}
