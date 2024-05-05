using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic; 
using System.Data;
using System.Text;

namespace iSchool.Domain
{ 
	/// <summary>
	/// 
	/// </summary>
	[Table("OnlineSchoolExtAlgTeQuality")]
	public partial class OnlineSchoolExtAlgTeQuality
	{

		/// <summary>
		/// 
		/// </summary> 
		[ExplicitKey] 
		public Guid Id { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid Eid { get; set; } 

		/// <summary>
		/// 教师总数
		/// </summary> 
		public int? TeacherCount { get; set; } 

		/// <summary>
		/// 外教人数
		/// </summary> 
		public int? FgnTeacherCount { get; set; } 

		/// <summary>
		/// 本科及研究生以上
		/// </summary> 
		public int? UndergduateOverCount { get; set; } 

		/// <summary>
		/// 研究生以上人数
		/// </summary> 
		public int? GduateOverCount { get; set; } 

		/// <summary>
		/// 教师荣誉（json）
		/// </summary> 
		public string TeacherHonor { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid Creator { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select (getdate()) 
		public DateTime CreateTime { get; set; } = DateTime.Now; 

		/// <summary>
		/// 
		/// </summary> 
		public Guid Modifier { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select (getdate()) 
		public DateTime ModifyDateTime { get; set; } = DateTime.Now; 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select ((1)) 
		public bool IsValid { get; set; } = true; 

	}
}
