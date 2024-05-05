using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a SchoolExtLife.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("SchoolExtLife")]
    public class SchoolExtLife : Entity
    {
		public  SchoolExtLife()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}

        public Guid Eid { get; set; }
        public string Hardware { get; set; }
        public string Community { get; set; }
        public string Timetables { get; set; }
        public string Schedule { get; set; }
        public string Diagram { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
        public float Completion { get; set; }
    }
}      
