using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class CheckSchoolNameQuery : IRequest<HttpResponse<string>>
    {
        /// <summary>
        /// 学校名称
        /// </summary>
        public string Name { get; set; }

        public Guid Sid { get; set; }
    }
}
