using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class AddSchoolExtChargeCommand : IRequest<HttpResponse<object>>
    {
        public Guid CurrentUserId { get; set; }

        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
        //public YearKeyValueDto<double>[] Otherfee { get; set; }  //....
        public float Completion { get; set; }

        /// <summary>
        /// 年份字段 change
        /// </summary>
        public YearChange[] Yearslist { get; set; }
    }
}
