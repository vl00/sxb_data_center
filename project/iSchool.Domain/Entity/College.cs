using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a College.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("College")]
    public class College : Entity
    {
		public  College()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
        }

        public string Name { get; set; }
        public string Name_e { get; set; }
        public byte Type { get; set; }
        public int? Weight { get; set; }
        public string Nation { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Createor { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; } = true;
    }
}      
