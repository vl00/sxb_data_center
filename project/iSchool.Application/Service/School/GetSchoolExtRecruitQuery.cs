using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using MediatR;
namespace iSchool.Application.Service
{
    public class GetSchoolExtRecruitQuery : IRequest<SchoolExtRecruitDto>
    {


        public GetSchoolExtRecruitQuery()
        {
        }

        public GetSchoolExtRecruitQuery(Guid extId, Guid sid, bool isAll, Guid userId)
        {
            ExtId = extId;
            Sid = sid;
            IsAll = isAll;
            UserId = userId;
        }

        public Guid ExtId { get; set; }

        public Guid Sid { get; set; }
        public bool IsAll { get; set; } = false;
        public Guid UserId { get; set; }
    }
}
