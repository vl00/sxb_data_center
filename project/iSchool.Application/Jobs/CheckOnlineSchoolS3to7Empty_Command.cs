using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Jobs
{
    public class CheckOnlineSchoolS3to7Empty_Command : IRequest
    {
        public Guid? Sid { get; set; }
        public DateTime? Now { get; set; }
        public bool IsBckgd { get; set; }
    }
}
