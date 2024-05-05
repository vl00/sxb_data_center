using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a school_v3_extension.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("school_v3_extension")]
    public class school_v3_extension 
    {
		public  school_v3_extension()
		{
		    //this.ModifyDateTime = DateTime.Now;
      //      this.CreateTime = DateTime.Now;
		}
	        public Guid Id { get; set; }
        public Guid? Sid { get; set; }
        public byte? Type { get; set; }
        public string Name { get; set; }
        public int? Province { get; set; }
        public int? City { get; set; }
        public int? Area { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Tel { get; set; }
        public decimal? Fee { get; set; }
        public string Intro { get; set; }
        public string Course { get; set; }
        public string Teacher { get; set; }
        public string Admissions { get; set; }
        public string Feedesc { get; set; }
        public string Student { get; set; }
        public string Hardware { get; set; }
        public string Quality { get; set; }
        public byte? Sort { get; set; }
        public bool? Show { get; set; }
        public int? Uv { get; set; }
        public bool? Import { get; set; }
        public byte? Nature { get; set; }
        public string Video { get; set; }
        public string VideoDesc { get; set; }
    }
}      
