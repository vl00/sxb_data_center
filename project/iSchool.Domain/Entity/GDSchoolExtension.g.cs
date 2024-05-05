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
	[Table("GDSchoolExtension")]
	public partial class GDSchoolExtension
	{

		/// <summary>
		/// 
		/// </summary> 
		[ExplicitKey] 
		public Guid Id { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid Sid { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Name { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int? Grade { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int? Type { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime? CreateTime { get; set; } 

	}
}
