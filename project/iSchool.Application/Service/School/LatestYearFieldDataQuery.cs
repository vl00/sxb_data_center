using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 获取年份标签字段最新记录
    /// </summary>
    public class LatestYearFieldDataQuery:IRequest<List<SchoolYearFieldContentDto>>
    {
        /// <summary>
        /// 学部ID
        /// </summary>
        public Guid EId  { get; set; }
    }
}
