using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class SchoolExtCourseDto
    {
        public Guid Eid { get; set; }
        public List<KeyValueDto<string>> Courses { get; set; }
        public string Characteristic { get; set; }
        public List<KeyValueDto<string>> Authentication { get; set; }
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

        public string SchFtype { get; set; }
    }
}
