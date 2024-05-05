using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Colleges
{
    public class CollegeNameQuery : IRequest<College[]>
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
