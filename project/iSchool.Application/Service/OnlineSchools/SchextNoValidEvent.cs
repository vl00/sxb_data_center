using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.OnlineSchools
{
    /// <summary>
    /// AfterSchoolOffDelEvent -> SchextNoValidEvent
    /// </summary>
    public class SchextNoValidEvent : INotification
    {
        public (Guid Sid, Guid Eid)[] Exts { get; set; }
    }
}
