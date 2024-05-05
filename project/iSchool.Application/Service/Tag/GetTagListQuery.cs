using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class GetTagListQuery : IRequest<List<TagDto>>
    {
        public bool IsCache { get; set; }

        public GetTagListQuery(bool isCache)
        {
            IsCache = isCache;
        }

        public int? TagType { get; set; }
    }
}
