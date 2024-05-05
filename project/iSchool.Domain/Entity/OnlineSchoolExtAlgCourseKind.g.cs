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
	[Table("OnlineSchoolExtAlgCourseKind")]
	public partial class OnlineSchoolExtAlgCourseKind
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
		/// 学科类
		/// </summary> 
		public string SubjsJson { get; set; } 

		/// <summary>
		/// 艺术类
		/// </summary> 
		public string ArtsJson { get; set; } 

		/// <summary>
		/// 体育类
		/// </summary> 
		public string SportsJson { get; set; } 

		/// <summary>
		/// 科学类
		/// </summary> 
		public string ScienceJson { get; set; } 

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
