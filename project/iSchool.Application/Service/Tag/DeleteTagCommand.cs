using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class DeleteTagCommand : IRequest
    {
        public Guid[] Ids { get; set; }
    }
}
