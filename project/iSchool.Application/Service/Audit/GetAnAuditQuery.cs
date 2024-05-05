using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class GetAnAuditQuery : IRequest<AnAuditQueryResult>
    {
        public Guid Id { get; set; }
        public bool IsRead { get; set; }
        public Guid AuditorId { get; set; }
        public bool IsPreGet { get; set; }
        public bool CanDoAll { get; set; } //能否处理其他人的学校
    }

    public class AnAuditQueryResult
    {
        public bool IsPreGet { get; set; }
        public Guid Id { get; set; }
        public Guid SchoolId { get; set; }
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
        public string[] Tags { get; set; }
        /// <summary>
        /// 分部s
        /// </summary>
        public AuditSchoolExt[] Exts { get; set; }

        /// <summary>
        /// 当前审核状态
        /// </summary>
        public SchoolAuditStatus CurrAuditStatus { get; set; }
        /// <summary>
        /// 当前审核意见
        /// </summary>
        public string CurrAuditMessage { get; set; }
        /// <summary>
        /// 当前审核人id
        /// </summary>
        public Guid CurrAuditorId { get; set; }
        /// <summary>学制</summary>
        public byte? EduSysType { get; set; }
    }

    public class AuditSchoolExt
    {
        public Guid ExtId { get; set; }
        public string ExtName { get; set; }
        public double Completion { get; set; }
    }
}
