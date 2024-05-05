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
	[Table("TotalDateSchoolEdit")]
	public partial class TotalDateSchoolEdit
	{

		/// <summary>
		/// 
		/// </summary> 
		[ExplicitKey] 
		public DateTime Date { get; set; } 

		/// <summary>
		/// 学校录入数
		/// </summary> 
		public int SchoolEntryCount { get; set; } 

		/// <summary>
		/// 累计到date的总录入学校数
		/// </summary> 
		public long AllSchoolEntryCount { get; set; } 

		/// <summary>
		/// 学部录入数
		/// </summary> 
		public int? SchoolExtEntryCount { get; set; } 

		/// <summary>
		/// 累计到date的总录入学部数
		/// </summary> 
		public long? AllSchoolExtEntryCount { get; set; } 

	}
}
