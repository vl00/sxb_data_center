using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class SchoolImageDto
    {
        public string Url { get; set; }
        public string SUrl { get; set; }
        public string ImageDesc { get; set; }
        public byte Type { get; set; }
        public byte Sort { get; set; }
    }
}
