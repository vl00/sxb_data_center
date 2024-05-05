using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a RankingList.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("RankingList")]
    public class RankingList : Entity
    {
		public  RankingList()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}
	   
        public Guid RankNameId { get; set; }
        public Guid SchoolId { get; set; }
        public Int16 Type { get; set; }
        public Int16 Placing { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
    }
}      
