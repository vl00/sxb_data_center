using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Jobs
{
    public class ComputeUserCommand : IRequest
    {
        public DateTime Date { get; set; }
    }
}
