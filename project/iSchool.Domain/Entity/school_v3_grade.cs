using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a school_v3_grade.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("school_v3_grade")]
    public class school_v3_grade 
    {
		public  school_v3_grade()
		{
		    //this.ModifyDateTime = DateTime.Now;
      //      this.CreateTime = DateTime.Now;
		}
	        public Guid? Ext_id { get; set; }
        public byte? Grade { get; set; }
    }
}      
