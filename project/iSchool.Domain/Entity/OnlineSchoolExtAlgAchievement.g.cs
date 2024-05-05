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
	[Table("OnlineSchoolExtAlgAchievement")]
	public partial class OnlineSchoolExtAlgAchievement
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
		/// 升学考试平均分(中考,高考)
		/// </summary> 
		public double? ExtamAvgscore { get; set; } 

		/// <summary>
		/// 状元人数
		/// </summary> 
		public int? No1Count { get; set; } 

		/// <summary>
		/// 保送人数
		/// </summary> 
		public int? CmstuCount { get; set; } 

		/// <summary>
		/// 自主招生
		/// </summary> 
		public int? RecruitCount { get; set; } 

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
