using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Jobs
{
    public class ExecSqlFileCommand : IRequest
    {
        public string Fname { get; set; }
        public bool UseTran { get; set; }
        public string[] Args { get; set; }
    }
}
