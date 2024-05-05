using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using MediatR;

namespace iSchool.Application.Service
{
    public class Alg2Command : IRequest
    {
        public Guid UserId { get; set; }
        public Alg2QyRstDto Dto { get; set; }
    }
}
