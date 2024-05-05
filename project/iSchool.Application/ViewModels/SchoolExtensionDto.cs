using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    [Serializable]
    public class SchoolExtensionDto
    {
        public Guid Sid { get; set; }
        public Guid? ExtId { get; set; }
        public string Name { get; set; }
        //别称
        public string NickName { get; set; }
        //总类型
        public string SchFtype { get; set; }
        public byte Grade { get; set; }
        public string Tags { get; set; }
        public byte Type { get; set; }
        //是否普惠学校
        public bool Discount { get; set; }
        //是否中国国籍学校
        public bool Chinese { get; set; }
        //是否双语
        public bool Diglossia { get; set; }
        public string[] Source { get; set; }
        public string Weixin { get; set; }
        //完成率
        public double Completion { get; set; }
        public Guid? UserId { get; set; }
        public bool? AllowEdit { get; set; }
        public Guid? ClaimedAmapEid { get; set; }
        /// <summary>
        /// 当前审核意见
        /// </summary>
        public string CurrAuditMessage { get; set; }
        //学校来源附件
        public List<SoureAttach> ListSourceAttchments { get; set; }
        public string SourceAttachments { get; set; }
              
        /// <summary>
        /// 学部简介
        /// </summary>
        public string ExtIntro { get; set; }
    }
    public class SoureAttach {
        //1图片，2其他
        public int type { get; set; }
        public string uri { get; set; }

        public string  filename { get; set; }
        public string  icon { get; set; }
    }
}
