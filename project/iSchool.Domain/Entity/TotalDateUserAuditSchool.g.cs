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
	[Table("TotalDateUserAuditSchool")]
	public partial class TotalDateUserAuditSchool
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
		/// 审核数
		/// </summary> 
		//dbDefaultValue// select ((0)) 
		public int AuditCount { get; set; } = 0; 

		/// <summary>
		/// 审核学部数
		/// </summary> 
		public int? AuditExtCount { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select ((1)) 
		public bool IsValid { get; set; } = true; 

	}
}
