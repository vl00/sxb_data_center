using System;
using System.Collections.Generic;
using System.Text;
using MediatR;


namespace iSchool.Application.Service
{
    public class SortRankCommand : IRequest<int>
    {
        public SortRankCommand(Guid sid, Guid rankId, double placing, bool isJux)
        {
            Sid = sid;
            RankId = rankId;
            Placing = placing;
            IsJux = isJux;
        }

        public Guid Sid { get; set; }
        public Guid RankId { get; set; }
        public double Placing { get; set; }
        /// <summary>
        /// 是否并列
        /// </summary>
        public bool IsJux { get; set; }
    }
}
