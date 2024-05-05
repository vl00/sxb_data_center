using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Searchs
{
    public class ESearchLikeQuery : IRequest<FnResult<PagedList<Guid>>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Name { get; set; }
        public Guid? Sid { get; set; }
        public byte? Grade { get; set; }
        public byte? Type { get; set; }
        public int? Province { get; set; }
        public int? City { get; set; }
        public int? Area { get; set; }
        public int[] AuditStatus { get; set; }
        /// <summary>
        /// 提交时间1 school表createtime字段
        /// </summary>
        public DateTime? CreateTime1 { get; set; }
        /// <summary>
        /// 提交时间2 school表createtime字段
        /// </summary>
        public DateTime? CreateTime2 { get; set; }
        /// <summary>
        /// 编辑人IDs
        /// </summary>
        public Guid[] EidtorIds { get; set; }
        /// <summary>
        /// 审核人IDs
        /// </summary>
        public Guid[] AuditorIds { get; set; }
    }

    public class ESearchLikeApiResult
    {
        public long Total { get; set; }
        public Guid[] Sid { get; set; }
    }
}
