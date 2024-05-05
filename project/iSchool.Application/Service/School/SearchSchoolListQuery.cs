using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using MediatR;
namespace iSchool.Application.Service
{
    public class SearchSchoolListQuery : IRequest<SearchSchoolListDto>
    {
        public string Search { get; set; }

        public byte? Grade { get; set; } = 0;

        public int Province { get; set; } = 0;

        public int City { get; set; } = 0;

        public int Area { get; set; } = 0;

        public byte? Type { get; set; } = 0;

        public byte? Status { get; set; } = 0;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsAll { get; set; }

        public int Index { get; set; }

        public Guid UserId { get; set; }
        public int PageSize { get; set; } = 10;

        public Guid? Editors { get; set; }
    }    
}
