using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using MediatR;

namespace iSchool.Application.Service
{
    public class GetSchoolExtCourseQuery : IRequest<SchoolExtCourseDto>
    {
        public GetSchoolExtCourseQuery(Guid sid, Guid extId, bool isAll, Guid userId)
        {
            Sid = sid;
            ExtId = extId;
            IsAll = isAll;
            UserId = userId;
        }

        public Guid Sid { get; set; }

        public Guid ExtId { get; set; }

        public bool IsAll { get; set; }
        public Guid UserId { get; set; }
    }
}
