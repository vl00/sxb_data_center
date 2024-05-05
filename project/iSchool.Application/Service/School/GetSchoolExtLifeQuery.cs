using iSchool.Application.ViewModels;
using iSchool.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class GetSchoolExtLifeQuery : IRequest<GetSchoolExtLifeResult>
    {
        public GetSchoolExtLifeQuery() { }

        public GetSchoolExtLifeQuery(Guid sid, Guid eid, bool isAll, Guid userId)
        {
            Sid = sid;
            Eid = eid;
            IsAll = isAll;
            UserId = userId;
        }

        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
        public bool IsAll { get; set; } = false;
        public Guid UserId { get; set; }
    }

    public class GetSchoolExtLifeResult
    {
        public SchoolExtLifeDto Dto { get; set; }
        public SchoolExtension Ext { get; set; }
    }
}
