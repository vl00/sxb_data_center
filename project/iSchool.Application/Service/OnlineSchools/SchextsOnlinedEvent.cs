using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.OnlineSchools
{
    /// <summary>
    /// SchoolOnlinedEvent -> SchextsOnlinedEvent
    /// </summary>
    public class SchextsOnlinedEvent : INotification
    {
        public (Guid Sid, Guid Eid, bool IsValid)[] Exts { get; set; }
        public DateTime T { get; set; }
    }
}
