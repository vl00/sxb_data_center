using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a OnlineSchool.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("OnlineSchool")]
    public class OnlineSchool : Entity
    {
		public  OnlineSchool()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
            IsValid = true;
		}

        public string Name { get; set; }
        public string Name_e { get; set; }
        public string Website { get; set; }
        public string Intro { get; set; }
        public bool? Show { get; set; }
        public byte? Status { get; set; }
        public string Logo { get; set; }
        public DateTime? CreateTime { get; set; }
        public Guid? Creator { get; set; }
        public DateTime? ModifyDateTime { get; set; }
        public Guid? Modifier { get; set; }
        public bool IsValid { get; set; }

        /// <summary>	
		/// Ñ§ÖÆ
		/// </summary> 
		public byte? EduSysType { get; set; }
    }
}      
