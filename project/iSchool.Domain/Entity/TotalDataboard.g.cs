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
	[Table("TotalDataboard")]
	public partial class TotalDataboard
	{

		/// <summary>
		/// 编辑人数
		/// </summary> 
		public int EditorCount { get; set; } 

		/// <summary>
		/// 审核人数
		/// </summary> 
		public int AuditorCount { get; set; } 

		/// <summary>
		/// 总待审核数
		/// </summary> 
		public long UnAuditCount { get; set; } 

		/// <summary>
		/// 总需修改数
		/// </summary> 
		public long AuditFailedCount { get; set; } 

		/// <summary>
		/// 总已发布学校数
		/// </summary> 
		public long AuditSuccessCount { get; set; } 

		/// <summary>
		/// 总录入学校数
		/// </summary> 
		public long SchoolCount { get; set; } 

		/// <summary>
		/// 总录入学部数
		/// </summary> 
		public long SchoolExtCount { get; set; } 

		/// <summary>
		/// 总已发布学部数
		/// </summary> 
		public long SchoolExtAuditSuccessCount { get; set; } 

	}
}
