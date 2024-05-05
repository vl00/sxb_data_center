using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service
{
    public class Alg1Command : IRequest
    {
        public Guid UserId { get; set; }
        public Guid Sid { get; set; }
        public Alg1QyRstDto Dto { get; set; }
    }
}
