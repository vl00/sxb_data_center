using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class GetSchoolDetailsQuery : IRequest<SchoolDto>
    {
        public GetSchoolDetailsQuery(Guid sid, bool isAll, Guid userId)
        {
            Sid = sid;
            IsAll = isAll;
            UserId = userId;
        }

        public Guid Sid { get; set; }

        public bool IsAll { get; set; } = false;

        public Guid UserId { get; set; }
    }
}
