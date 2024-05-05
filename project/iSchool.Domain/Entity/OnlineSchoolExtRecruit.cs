using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a OnlineSchoolExtRecruit.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("OnlineSchoolExtRecruit")]
    public class OnlineSchoolExtRecruit
    {
        public OnlineSchoolExtRecruit()
        {
            this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
        }
        public Guid Id { get; set; }
        public Guid Eid { get; set; }
        public float? Age { get; set; }
        public float? MaxAge { get; set; }
        public int? Count { get; set; }
        public string Target { get; set; }
        public float? Proportion { get; set; }
        public string Date { get; set; }
        public string Point { get; set; }
        public string Data { get; set; }
        public string Contact { get; set; }
        public string Subjects { get; set; }
        public string Pastexam { get; set; }
        public string Scholarship { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
        public float Completion { get; set; }
    }
}
