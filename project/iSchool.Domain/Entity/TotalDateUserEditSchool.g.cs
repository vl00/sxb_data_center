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
	[Table("TotalDateUserEditSchool")]
	public partial class TotalDateUserEditSchool
	{

		/// <summary>
		/// 
		/// </summary> 
		public DateTime Date { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid UserId { get; set; } 

		/// <summary>
		/// 学校录入数
		/// </summary> 
		//dbDefaultValue// select ((0)) 
		public int SchoolEntryCount { get; set; } = 0; 

		/// <summary>
		/// 学部录入数
		/// </summary> 
		public int? SchoolExtEntryCount { get; set; } 

		/// <summary>
		/// 已发布数
		/// </summary> 
		//dbDefaultValue// select ((0)) 
		public int AuditSuccessCount { get; set; } = 0; 

		/// <summary>
		/// 待审核数
		/// </summary> 
		//dbDefaultValue// select ((0)) 
		public int UnAuditCount { get; set; } = 0; 

		/// <summary>
		/// 需修改数
		/// </summary> 
		//dbDefaultValue// select ((0)) 
		public int AuditFailCount { get; set; } = 0; 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select ((1)) 
		public bool? IsValid { get; set; } = true; 

	}
}
