using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class SchoolYearFieldContentDto
    {
        public Guid Id { get; set; }
        public List<KeyValueDto<string>> Target { get; set; }
        public Guid Eid { get; set; }
        public int Year { get; set; }

        public string Field { get; set; }

        public string Content { get; set; }

        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 字段对应所有年份
        /// </summary>
        public List<string> Years { get; set; }

        /// <summary>
        /// 添加其他年份--下拉框允许的所有年份
        /// </summary>
        public List<string> NewOtherYears { get; set; }
    }
}
