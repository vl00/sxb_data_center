using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public abstract class DpwdRecorrQueryBase
    {
        /// <summary>
        /// 1：学校id，2：学部id
        /// </summary>
        public int? Sty { get; set; }
        public string Txt { get; set; }
        public DateTime? DelTime1 { get; set; }
        public DateTime? DelTime2 { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DpwdRecorrQuery : DpwdRecorrQueryBase, IRequest<PagedList<DpwdRecorrQueryResult>>
    {      
    }

    public class ExportDeletedSchextDpwdCommand : DpwdRecorrQueryBase, IRequest<byte[]>
    {
        public ExportDeletedSchextDpwdCommand() 
        {
            this.PageIndex = -1;
            this.PageSize = -1;
        }
    }

    public class DpwdRecorrQueryResult
    {
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
        public string Sname { get; set; }
        public string Ename { get; set; }
        public Guid Dwid { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// 1：点评，2：问答(含对比)
        /// </summary>
        public int Dtype { get; set; } 
        public DateTime DelTime { get; set; }
    }
}
