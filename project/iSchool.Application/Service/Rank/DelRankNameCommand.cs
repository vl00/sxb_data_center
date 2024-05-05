using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class DelRankNameCommand : IRequest
    {
        public Guid RankId { get; set; }

        public DelRankNameCommand(Guid rankId)
        {
            RankId = rankId;
        }
    }
}
