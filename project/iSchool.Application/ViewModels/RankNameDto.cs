using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class RankNameDto
    {
        public int Year { get; set; }

        public List<RankNameItem> RankNames { get; set; }

    }

    public class RankNameItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
