using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using MediatR;

namespace iSchool.Application.Service
{
    public class FindGeneralTagsByIdsQuery : IRequest<GeneralTag[]>
    {
        public Guid[] Ids { get; set; }
    }
}
