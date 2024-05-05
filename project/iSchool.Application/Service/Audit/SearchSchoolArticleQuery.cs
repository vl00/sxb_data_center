using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class SearchSchoolArticleQuery : IRequest<PagedList<SearchSchoolArticleResult>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public Guid Eid { get; set; }
    }

    public class SearchSchoolArticleResult
    {
        //public Guid Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        //public DateTime Time { get; set; }
    }
}
