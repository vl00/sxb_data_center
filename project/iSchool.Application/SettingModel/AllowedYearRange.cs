using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Application.SettingModel
{
    /// <summary>
    /// 允许年份范围实体类
    /// </summary>
    public class AllowedYearRange
    {
        /// <summary>
        /// 允许的最小年份
        /// </summary>
        public string MinYear { get; set; }

        /// <summary>
        /// 允许的最大年份
        /// </summary>
        public string MaxYear { get; set; }
    }
}
