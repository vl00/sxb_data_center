using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class AddSchoolExtRecruitCommand : IRequest<HttpResponse<string>>
    {
        public Guid Eid { get; set; }
        public Guid Sid { get; set; }
        public float Completion { get; set; } = 0;

        public Guid UserId { get; set; }
        public string Operation { get; set; }

        /// <summary>
        ///招录比例 
        /// </summary>
        public float? Proportion { get; set; }
        /// <summary>
        /// 录取分数线
        /// </summary>
        public KeyValuePair<string, float>[] Point { get; set; }
        /// <summary>
        /// 招生日期
        /// </summary>
        public KeyValuePair<string, string>[] Date { get; set; }

        /// <summary>
        /// 年份字段 change
        /// </summary>
        public YearChange[] Yearslist { get; set; }
    }
}
