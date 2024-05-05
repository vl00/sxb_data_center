using System;

namespace iSchool.Domain
{
    /// <summary>
    /// Represents a SchoolExtContent.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("SchoolExtContent")]
    public class SchoolExtContent : Entity
    {
        public SchoolExtContent()
        {
            this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
        }

        public Guid Eid { get; set; }
        public int? Province { get; set; }
        public int? City { get; set; }
        public int? Area { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public iSchool.LngLatLocation LatLong { get; set; }
        public string Tel { get; set; }
        /// <summary>
        /// 是否寄宿
        /// </summary>
        public bool? Lodging { get; set; }

        /// <summary>
        /// 是否走读
        /// </summary>
        public bool? SdExtern { get; set; }
        public int? Studentcount { get; set; }
        public int? Teachercount { get; set; }
        public float? TsPercent { get; set; }
        public string Authentication { get; set; }
        public string Abroad { get; set; }
        public bool? Canteen { get; set; }
        public string Meal { get; set; }
        public string Characteristic { get; set; }
        public string Project { get; set; }
        public int? SeniorTea { get; set; }
        public int? CharacteristicTea { get; set; }
        public int? ForeignTeaCount { get; set; }
        public float? ForeignTea { get; set; }
        public DateTimeOffset? Creationdate { get; set; }
        public double? Acreage { get; set; }
        public string Openhours { get; set; }
        public string Calendar { get; set; }
        public string Range { get; set; }
        public string Counterpart { get; set; }
        public string Afterclass { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid Creator { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public Guid Modifier { get; set; }
        public bool IsValid { get; set; } = true;
        public float Completion { get; set; }
        /// <summary>
        /// 是否有校车
        /// </summary> 
        public bool? HasSchoolBus { get; set; }
        /// <summary>
        /// 派位学校
        /// <para>
        /// [{"key":"SchoolName ExtName", "value":"eid"}]
        /// </para>
        /// </summary>
        public string Allocation { get; set; }
    }
}
