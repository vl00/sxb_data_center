using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class SchoolFindByNameQuery : IRequest<SchoolFindByNameQryResult>
    {
        /// <summary>
        /// 学校名称
        /// </summary>
        public string Name { get; set; }
    }

    public class SchoolFindByNameQryResult
    {
        public IEnumerable<Domain.School> Ls { get; set; }
    }
}
