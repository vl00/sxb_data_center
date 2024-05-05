using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class GetRankDetailQuery : IRequest<List<RankDetailDto>>
    {
        public GetRankDetailQuery(Guid id, bool isCache)
        {
            Id = id;
            IsCache = isCache;
        }

        public Guid Id { get; set; }

        public bool IsCache { get; set; } = true;
    }
}
