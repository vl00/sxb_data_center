using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a school_v3.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("school_v3")]
    public class school_v3 
    {
		public  school_v3()
		{
		    //this.ModifyDateTime = DateTime.Now;
      //      this.CreateTime = DateTime.Now;
		}
	        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Name_e { get; set; }
        public string Website { get; set; }
        public string Intro { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Logo { get; set; }
        public bool? Show { get; set; }
    }
}      
