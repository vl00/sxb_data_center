using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using MediatR;

namespace iSchool.Application.Service
{
    public class CitydataQuery : IRequest<CitydataResult[]>
    {
    }

    public class CitydataResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
    }
}
