using iSchool.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class SchoolExtRecruitDto
    {
        public Guid Eid { get; set; }
        public float? Age { get; set; }
        public float? MaxAge { get; set; }
        public int? Count { get; set; }
        /// <summary>
        /// 招生对象
        /// </summary>
        public List<KeyValueDto<string>> Target { get; set; }
        /// <summary>
        /// 招生比例
        /// </summary>
        public float? Proportion { get; set; }
        /// <summary>
        /// 招生日期
        /// </summary>
        public List<KeyValueDto<string>> Date { get; set; }
        /// <summary>
        /// 录取分数线
        /// </summary>
        public List<KeyValueDto<string>> Point { get; set; }
        /// <summary>
        /// 报名方式
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// 考试资料
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 考试科目
        /// </summary>
        public List<KeyValueDto<string>> Subjects { get; set; }
        /// <summary>
        /// 奖学金计划
        /// </summary>
        public string Scholarship { get; set; }
        /// <summary>
        /// 以往考试内容
        /// </summary>
        public string Pastexam { get; set; }

        public string SchFtype { get; set; }

        public byte? Grade { get; set; }

        public byte? Type { get; set; }
        /// <summary>
        /// 是否普惠
        /// </summary>
        public bool? Discount { get; set; }
        /// <summary>
        /// 是否双语
        /// </summary>
        public bool? Diglossia { get; set; }
        /// <summary>
        /// 是否中国人学校
        /// </summary>
        public bool? Chinese { get; set; }

        /// <summary>
        /// 当前审核意见
        /// </summary>
        public string CurrAuditMessage { get; set; }
        /// <summary>
        /// 部分字段年份拓展标签(最新记录)
        /// </summary>
        public List<SchoolYearFieldContentDto> YearTagList { get; set; }

      
    }
}
