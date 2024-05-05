using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a OnlineSchoolExtQuality.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("OnlineSchoolExtQuality")]
    public class OnlineSchoolExtQuality 
    {
		public  OnlineSchoolExtQuality()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}
	        public Guid Id { get; set; }
        public Guid Eid { get; set; }
        public string Principal { get; set; }
        public string Videos { get; set; }
        public string Teacher { get; set; }
        public string Schoolhonor { get; set; }
        public string Studenthonor { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
        public float Completion { get; set; }
    }
}      
