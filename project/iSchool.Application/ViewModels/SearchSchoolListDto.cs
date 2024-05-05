using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class SearchSchoolListDto
    {
        public List<SearchSchoolItem> list { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }

    public class SearchSchoolItem
    {
        public Guid Sid { get; set; }

        public string Name { get; set; }
        public Guid Creator { get; set; }

        public Guid? AuditUserId { get; set; }

        public float Completion { get; set; } = 0;

        public DateTime CreateTime { get; set; }
        public byte? SchoolStatus { get; set; }

        public DateTime ModifyDateTime { get; set; }
        public byte? Status { get; set; }
    }
}
