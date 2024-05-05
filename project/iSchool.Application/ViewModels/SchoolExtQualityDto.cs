using iSchool.Application.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    [Serializable]
    public class SchoolExtQualityDto
    {
        public Guid Sid { get; set; }
        public Guid Eid { get; set; }

        /// <summary>
        /// 用于删除历史数据
        /// </summary>
        public string CurrentVideoTypes { get; set; }

        /// <summary>
        /// 视频
        /// </summary> 
        public string[] Videos { get; set; }

        /// <summary>
        /// 视频封面
        /// </summary> 
        public string[] Covers { get; set; }

        /// <summary>
        /// 视频类型
        /// </summary>
        public string[] Types { get; set; }

        /// <summary>
        /// 视频描述
        /// </summary>
        public string[] VideoDescs { get; set; }

        /// <summary>
        /// 完成率
        /// </summary> 
        public float Completion { get; set; }

        public bool? Chinese { get; set; }
        public bool? Discount { get; set; }
        public bool? Diglossia { get; set; }
        public byte? Grade { get; set; }
        public byte? Type { get; set; }

        public IEnumerable<UploadImgDto> Imgs { get; set; }
    }
}
