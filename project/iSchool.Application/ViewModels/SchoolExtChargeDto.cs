using iSchool.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    [Serializable]
    public class SchoolExtChargeDto
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid Sid { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public Guid Eid { get; set; }

        /// <summary>
        /// 申请费用
        /// </summary> 
        public double? Applicationfee { get; set; }

        /// <summary>
        /// 学费
        /// </summary> 
        public double? Tuition { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public KeyValueDto<double>[] Otherfee { get; set; }  //....

        /// <summary>
        /// 
        /// </summary> 
        public float Completion { get; set; }

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

        public byte? Grade { get; set; }

        public byte? Type { get; set; }
        ///// <summary>
        ///// 部分字段年份拓展标签
        ///// </summary>
        //public List<SchoolYearFieldContent> YearTagList { get; set; }
        public string FieldYearTagList { get; set; }

        /// <summary>
        /// 部分字段年份拓展标签(最新记录)
        /// </summary>
        public List<SchoolYearFieldContentDto> YearTagList { get; set; }
    }
}
