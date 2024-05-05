using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class YearChange
    {
        public int Year { get; set; }
        public string Field { get; set; }
        public string Content { get; set; }
        public int Act { get; set; }
        
        /// <summary>
        /// 没变化
        /// </summary>
        public const int Act_none = 2;
        /// <summary>
        /// 新添加
        /// </summary>
        public const int Act_add = 3;
        /// <summary>
        /// 修改
        /// </summary>
        public const int Act_update = 1;
        /// <summary>
        /// 删除
        /// </summary>
        public const int Act_remove = 0;

        /// <summary>
        /// 合并年份字段change记录
        /// </summary>
        public static IEnumerable<YearChange> CombinChange(IEnumerable<YearChange> changes)
        {
            var dict = new Dictionary<(int Year, string Field), YearChange>();
            foreach (var change in changes)
            {
                if (change.Act < 0 || change.Year <= 0) continue;                
                //同一年相同字段进行合并
                if (dict.TryGetValue((change.Year, change.Field), out var v))
                {                    
                    v.Content = change.Content;
                    v.Act = change.Act == YearChange.Act_none ? YearChange.Act_update : change.Act;
                }
                else
                {
                    dict[(change.Year, change.Field)] = v = change;
                    v.Act = change.Act == YearChange.Act_none ? YearChange.Act_add : change.Act;
                }
            }
            return dict.Values;
        }
    }
}
