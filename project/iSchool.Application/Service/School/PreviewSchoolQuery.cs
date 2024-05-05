using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service
{
    public class PreviewSchoolQuery : IRequest<PreviewSchoolQueryResult>
    {
        public Guid Sid { get; set; }
        public Guid? Eid { get; set; }
    }

    public class PreviewSchoolQueryResult
    {
        public Guid Sid { get; set; }
        public string SchoolName { get; set; }
        public string SchoolName_e { get; set; }
        public string Logo { get; set; }
        public string WebSite { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// 标签s
        /// </summary>
        public string[] Tags { get; set; } = new string[0];
        /// <summary>
        /// 分部s
        /// </summary>
        public PreviewSchoolExt[] Exts { get; set; } = new PreviewSchoolExt[0];

        /// <summary>
        /// 当前审核意见
        /// </summary>
        public string CurrAuditMessage { get; set; }
        /// <summary>
        /// 当前审核状态
        /// </summary>
        public SchoolAuditStatus? CurrAuditStatus { get; set; }
        /// <summary>学制</summary>
        public byte? EduSysType { get; set; }
    }

    public class PreviewSchoolExt
    {
        public Guid ExtId { get; set; }
        public string ExtName { get; set; }
        public double Completion { get; set; }
    }
}
