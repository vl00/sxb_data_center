using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain
{

    [Table("SchoolImage")]
    public class SchoolImage : Entity
    {
        public Guid Eid { get; set; }

        public string Url { get; set; }

        public string SUrl { get; set; }

        public string ImageDesc { get; set; }

        public byte Type { get; set; }

        public byte Sort { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }

    }
}
