using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service.Audit
{
    public class GetAuditMessageQuery : IRequest<string>
    {
        public GetAuditMessageQuery(Guid sid)
        {
            Sid = sid;
        }

        public Guid Sid { get; set; }
    }
}
