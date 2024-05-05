using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Enum;
using MediatR;

namespace iSchool.Application.Service
{
    public class KvsQuery : IRequest<KeyValueDto<string>[]>
    {
        public string ParentId { get; set; }

        public int Type { get; set; }
    }
}
