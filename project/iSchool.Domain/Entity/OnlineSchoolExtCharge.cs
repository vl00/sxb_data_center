using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a OnlineSchoolExtCharge.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("OnlineSchoolExtCharge")]
    public class OnlineSchoolExtCharge 
    {
		public  OnlineSchoolExtCharge()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}
	        public Guid Id { get; set; }
        public Guid Eid { get; set; }
        public double? Applicationfee { get; set; }
        public double? Tuition { get; set; }
        public string Otherfee { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
        public float Completion { get; set; }
    }
}      
