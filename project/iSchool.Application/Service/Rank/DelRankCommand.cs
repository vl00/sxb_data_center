using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class DelRankSchoolCommand : IRequest<int>
    {
        public DelRankSchoolCommand(Guid sid, Guid rankId)
        {
            Sid = sid;
            RankId = rankId;
        }

        public Guid Sid { get; set; }
        public Guid RankId { get; set; }

    }
}
