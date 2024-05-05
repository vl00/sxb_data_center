using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using MediatR;

namespace iSchool.Application.Service
{
    public class KVQuery : IRequest<KeyValue[]>
    {
        public int ParentId { get; set; }

        public int Type { get; set; } = 1;
    }
}
