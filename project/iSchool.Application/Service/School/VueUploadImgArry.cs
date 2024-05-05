using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    [Obsolete]
    public class VueUploadImgArry
    {
        public string Name { get; set; }
        public List<VueUploadImgArryItem> Data { get; set; }
    }

    [Obsolete]
    public class VueUploadImgArryItem
    {
        public string Title { get; set; }

        public bool ShowDel { get; set; }

        public VueUploadImgArryItemUrl Url { get; set; }
    }

    [Obsolete]
    public class VueUploadImgArryItemUrl
    {
        public string CompressUrl { get; set; }

        public string Url { get; set; }
    }
}
