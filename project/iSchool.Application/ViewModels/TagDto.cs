using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{

    [Serializable]
    public class TagDto
    {
        public string SpellCode { get; set; }
        public int Type { get; set; }
        public List<TagItem> Tags { get; set; }
    }

    [Serializable]
    public class TagItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string SpellCode { get; set; }

        public DateTime CreateTime { get; set; }

        public int? Subdivision { get; set; }
    }
}
