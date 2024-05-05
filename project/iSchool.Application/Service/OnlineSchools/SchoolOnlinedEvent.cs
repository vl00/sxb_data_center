using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.OnlineSchools
{
    public class DelOnlineSchoolEvent : INotification
    {
        public Guid Sid { get; set; }
        public DateTime T { get; set; } = DateTime.Now;
    }

    public class SchoolOfflineEvent : INotification
    {
        public Guid Sid { get; set; }
        public DateTime T { get; set; } = DateTime.Now;
    }

    public class SchoolOnlinedEvent : INotification
    {
        public Guid Aid { get; set; }
        public Guid Sid { get; set; }
        public DateTime T { get; set; } = DateTime.Now;
    }
}
