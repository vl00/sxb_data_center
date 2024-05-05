using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class UpSchoolExtCompletionCommand : IRequest
    {
        public Guid Eid { get; set; }

        //public Guid UserId { get; set; }
    }

    public class SchoolExtCompletionUpdatedEvent : INotification
    {
        public Guid UserId { get; set; }
        public Guid Eid { get; set; }
        public DateTime Time { get; set; }
    }
}
