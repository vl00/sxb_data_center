using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Application.ViewModels;

namespace iSchool.Application.Service
{
    public class SaveExtYearFieldChangesCommand : IRequest
    {
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }

        /// <summary>
        /// 年份字段 change
        /// </summary>
        public YearChange[] Yearslist { get; set; }
    }
}
