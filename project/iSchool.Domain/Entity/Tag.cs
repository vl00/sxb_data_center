using Dapper.Contrib.Extensions;
using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a Tag.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("Tag")]
    public class Tag : Entity
    {
        public Tag()
        {
            this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
            this.IsValid = true;
            this.Id = Guid.NewGuid();
        }

        public string Name { get; set; }
        public string SpellCode { get; set; }
        public byte Type { get; set; }
        /// <summary>
		/// 二级分类(null=其它)
		/// </summary> 
		public byte? Subdivision { get; set; }
        public byte Sort { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
    }
}
