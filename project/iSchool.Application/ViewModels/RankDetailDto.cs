using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class RankDetailDto
    {
        public Guid Id { get; set; }

        public Guid SchoolId { get; set; }

        public string SchoolName { get; set; }

        public int Placing { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
