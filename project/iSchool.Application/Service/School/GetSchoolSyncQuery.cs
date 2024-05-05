using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class GetSchoolSyncQuery : IRequest<IDictionary<string, object>>
    {
        public GetSchoolSyncQuery()
        {
        }

        public GetSchoolSyncQuery(Guid sid, Guid eid, SchoolExtFieldSyncConfigDto schoolExtFieldSyncConfigDto)
        {
            Sid = sid;
            Eid = eid;
            this.schoolExtFieldSyncConfigDto = schoolExtFieldSyncConfigDto;
        }

        public Guid Sid { get; set; }
        public Guid Eid { get; set; }
        public SchoolExtFieldSyncConfigDto schoolExtFieldSyncConfigDto { get; set; }
    }
}
