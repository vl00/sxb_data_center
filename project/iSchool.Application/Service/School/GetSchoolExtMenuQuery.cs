using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class GetSchoolExtMenuQuery : IRequest<List<ExtMenuItem>>
    {
        public GetSchoolExtMenuQuery(Guid extId)
        {
            ExtId = extId;
        }

        public GetSchoolExtMenuQuery()
        {
        }

        public Guid ExtId { get; set; }        

        public bool Reset { get; set; } = false;
    }
}
