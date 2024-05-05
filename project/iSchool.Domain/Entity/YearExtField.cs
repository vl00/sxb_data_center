using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain
{
    /// <summary>
    /// 字段与年份对应表
    /// </summary>
    [Table("YearExtField")]
    public class YearExtField
    {

        /// <summary>
        /// 学部Id
        /// </summary>
        public Guid Eid { get; set; }

        /// <summary>
        /// 字段名
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 字段所有年份，用英文,分隔
        /// </summary>
        public string Years  { get; set; }

        /// <summary>
        /// 最新年份
        /// </summary>
        public int? Latest { get; set; }
    }
}
