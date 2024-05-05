using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using MediatR;

namespace iSchool.Application.Service
{
    public class FindGeneralTagsByNameQuery : IRequest<GeneralTag[]>
    {
        public string Name { get; set; }
        public bool Like { get; set; }
    }
}
