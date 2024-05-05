using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Audit
{
    public class SearchSchool4TypeaheadQuery : IRequest<SearchSchool4TypeaheadQueryResult[]>
    {
        public string Keyword { get; set; }
        public int Top { get; set; }
        public bool IsOnline { get; set; }
    }

    public class SearchSchool4TypeaheadQueryResult
    {
        public Guid Sid { get; set; }
        public string Sgname { get; set; }
    }
}
