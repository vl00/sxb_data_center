using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using MediatR;
namespace iSchool.Application.Service
{
    public class GetSchoolExtensionQuery : IRequest<object>
    {
        public GetSchoolExtensionQuery(Guid sid, Guid extId, SchoolExtensionType type, bool isAll, Guid userId)
        {
            Sid = sid;
            ExtId = extId;
            Type = type;
            IsAll = isAll;
            UserId = userId;
        }

        public Guid Sid { get; set; }
        public Guid ExtId { get; set; }
        public SchoolExtensionType Type { get; set; }
        public bool IsAll { get; set; } = false;
        public Guid UserId { get; set; }
    }
}
