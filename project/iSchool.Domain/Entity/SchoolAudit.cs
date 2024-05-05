using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a SchoolAudit.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("SchoolAudit")]
    public class SchoolAudit : Entity
    {
		public  SchoolAudit()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}

        public Guid Sid { get; set; }
        public string AuditMessage { get; set; }
        public byte Status { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
    }
}      
