using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a RankName.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("RankName")]
    public class RankName : Entity
    {
		public  RankName()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}

        public string Name { get; set; }
        public int Year { get; set; }
        public bool IsCollege { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
    }
}      
