using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a SchoolYearFieldContent.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("OnlineSchoolYearFieldContent")]
    public class OnlineSchoolYearFieldContent : Entity
    {
        public OnlineSchoolYearFieldContent()
        {
           
        }

        public Guid Eid { get; set; }
        public int Year { get; set; }

        public string Field { get; set; }
      
        public string Content { get; set; }

        public bool IsValid { get; set; } = true;
      
    }
}
