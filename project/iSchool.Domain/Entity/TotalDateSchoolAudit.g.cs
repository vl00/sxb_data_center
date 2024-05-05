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
	[Table("TotalDateSchoolAudit")]
	public partial class TotalDateSchoolAudit
	{

		/// <summary>
		/// 
		/// </summary> 
		[ExplicitKey] 
		public DateTime Date { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int AuditCount { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public long AllAuditCount { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int? AuditExtCount { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public long? AllAuditExtCount { get; set; } 

	}
}
