using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a OnlineSchoolVideo.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("OnlineSchoolVideo")]
    public class OnlineSchoolVideo 
    {
		public  OnlineSchoolVideo()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}
	        public Guid Id { get; set; }
        public Guid Eid { get; set; }
        public string VideoUrl { get; set; }
        public string VideoDesc { get; set; }
        public bool IsOutSide { get; set; }
        public byte Type { get; set; }
        public byte Sort { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
        public float Completion { get; set; }
    }
}      
