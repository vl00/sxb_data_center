using Dapper.Contrib.Extensions;
using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a SchoolExtension.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("SchoolExtension")]
    public class SchoolExtension
    {
        public SchoolExtension()
        {
            this.Id = Guid.NewGuid();
            this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
            this.IsValid = true;
        }
        [ExplicitKey]
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
        public bool IsValid { get; set; }
        public float Completion { get; set; }
        public Guid? ClaimedAmapEid { get; set; }
        public string SourceAttachments { get; set; }
        
        /// <summary>
        /// Ñ§²¿¼ò½é
        /// </summary>
        public string ExtIntro { get; set; }

    }
}
