using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.OnlineSchool
{
    public class AfterSchoolOffDelEvent : INotification
    {
        public Guid Sid { get; set; }
        public bool IsOff { get; set; }
    }
}
