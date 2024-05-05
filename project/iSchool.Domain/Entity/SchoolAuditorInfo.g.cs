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
	[Table("SchoolAuditorInfo")]
	public partial class SchoolAuditorInfo
	{

		/// <summary>
		/// 学校sid
		/// </summary> 
		public Guid Sid { get; set; } 

		/// <summary>
		/// 审核人id （学校首次审核后的负责人）
		/// </summary> 
		public Guid AuditorUid { get; set; } 

		/// <summary>
		/// 最近所属审核人的审核单id
		/// </summary> 
		public Guid Aid { get; set; } 

		/// <summary>
		/// 审核状态（成功/失败）
		/// </summary> 
		public byte Status { get; set; } 

		/// <summary>
		/// 审核时间
		/// </summary> 
		public DateTime AuditTime { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select ((1)) 
		public bool IsValid { get; set; } = true; 

	}
}
