using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.ApiService.Schools
{
    public class OESchoolByFTypeQuery : IRequest<OESchoolByFTypeQueryResult[]>
    {
        public string SchoolName { get; set; }
        public OESchoolFinalType[] SchoolType { get; set; } = new OESchoolFinalType[0];
        public int[] CityCodes { get; set; } = new int[0];
        public int Top { get; set; } = 10;
    }

    /// <summary>
    /// 学校类型（最终）
    /// </summary>
    public class OESchoolFinalType
    {
        /// <summary>
        /// 年级
        /// </summary>
        public int Grade { get; set; }
        /// <summary>
        /// 办学类型
        /// </summary>
        public int RunType { get; set; }
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
    }

    public class OESchoolByFTypeQueryResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
