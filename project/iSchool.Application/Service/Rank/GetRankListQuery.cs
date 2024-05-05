using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 查询排行榜名称列表
    /// </summary>
    public class GetRankListQuery : IRequest<List<RankNameDto>>
    {
        public GetRankListQuery(bool isCache)
        {
            IsCache = isCache;
        }

        public bool IsCache { get; set; }
    }
}
