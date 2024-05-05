using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Colleges
{
    public class CollegeQuery : IRequest<College[]>
    {
        public Guid[] Ids { get; set; }
    }
}
