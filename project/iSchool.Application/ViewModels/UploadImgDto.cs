using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class UploadImgDto
    {
        public byte Type { get; set; }
        public IEnumerable<UploadImgItemDto> Items { get; set; }
    }

    public class UploadImgItemDto
    {
        public Guid? Id { get; set; }
        public string Desc { get; set; }
        public string Url_s { get; set; }
        public string Url { get; set; }
    }

}
