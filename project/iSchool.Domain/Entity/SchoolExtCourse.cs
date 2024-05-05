using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a SchoolExtCourse.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("SchoolExtCourse")]
    public class SchoolExtCourse : Entity
    {
		public  SchoolExtCourse()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}

        public Guid Eid { get; set; }
        public string Courses { get; set; }
        public string Characteristic { get; set; }
        public string Authentication { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
        public float Completion { get; set; }
    }
}      
