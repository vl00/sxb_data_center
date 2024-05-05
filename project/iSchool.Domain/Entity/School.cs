using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a School.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
    [Serializable]
    [Table("School")]
    public class School : Entity
    {
        public School()
        {
            this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
            this.Id = Guid.NewGuid();
        }
        public string Name { get; set; }
        public string Name_e { get; set; }
        public string Website { get; set; }
        public string Intro { get; set; }
        public bool? Show { get; set; }
        public byte? Status { get; set; }
        public string Logo { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; }
        public float Completion { get; set; }

		/// <summary>	
		/// Ñ§ÖÆ
		/// </summary> 
		public byte? EduSysType { get; set; } 

	}
}
