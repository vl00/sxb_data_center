using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Totals
{
    public class PageSchoolEditQuery : IRequest<PagedList<TotalDateSchoolEdit>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? Date { get; set; }
    }
}
