using iSchool.Application.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    [Serializable]
    public class SchoolExtLifeDto
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid Sid { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public Guid Eid { get; set; }

        #region old code
        ///// <summary>
        ///// 硬件设施
        ///// </summary> 
        //public string Hardware { get; set; }

        ///// <summary>
        ///// 社团活动
        ///// </summary> 
        //public string Community { get; set; }

        ///// <summary>
        ///// 各个年级课程表
        ///// </summary> 
        //public string Timetables { get; set; }

        ///// <summary>
        ///// 
        ///// </summary> 
        //public string Schedule { get; set; }

        ///// <summary>
        ///// 
        ///// </summary> 
        //public string Diagram { get; set; }
        #endregion old code

        /// <summary>
        /// 
        /// </summary> 
        public float Completion { get; set; }

        /// <summary>
        /// 是否中国人学校
        /// </summary>
        public bool? Chinese { get; set; }
        /// <summary>
        /// 是否普惠
        /// </summary>
        public bool? Discount { get; set; }
        /// <summary>
        /// 是否双语
        /// </summary>
        public bool? Diglossia { get; set; }

        public byte? Grade { get; set; }

        public byte? Type { get; set; }

        public IEnumerable<UploadImgDto> Imgs { get; set; }
    }
        
}
