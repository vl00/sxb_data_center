using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class UploadMoreInput
    {
        //先用小数接收 接口只支持整数
        public double x { get; set; }
        public double y { get; set; }
        public double height { get; set; }
        public double width { get; set; }

        public int rotate { get; set; }
    }
}
