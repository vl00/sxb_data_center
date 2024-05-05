using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a Ranking.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("Ranking")]
    public class Ranking    
    {
		public  Ranking()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}

        public Guid ExtensionId { get; set; }
        public Guid CollegeId { get; set; }
        public int Count { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Createor { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
    }
}      
