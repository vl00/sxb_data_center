using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a OnlineSchoolExtension.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("OnlineSchoolExtension")]
    public class OnlineSchoolExtension
    {
        public OnlineSchoolExtension()
        {
            this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
        }
        public Guid Id { get; set; }
        public Guid Sid { get; set; }
        public byte Grade { get; set; }
        public byte Type { get; set; }
        public bool? Discount { get; set; }
        public bool? Diglossia { get; set; }
        public bool? Chinese { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string SchFtype { get; set; } = "";
        public string Source { get; set; }
        public string Weixin { get; set; }
        public bool? AllowEdit { get; set; }
        public DateTime? CreateTime { get; set; }
        public Guid? Creator { get; set; }
        public DateTime? ModifyDateTime { get; set; }
        public Guid? Modifier { get; set; }
        public bool IsValid { get; set; } = true;
        public Guid? ClaimedAmapEid { get; set; }
    }
}
