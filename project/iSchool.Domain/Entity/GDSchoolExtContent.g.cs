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
	[Table("GDSchoolExtContent")]
	public partial class GDSchoolExtContent
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
		/// 
		/// </summary> 
		public int? Province { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int? City { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int? Area { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Address { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public double? Latitude { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public double? Longitude { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime? CreateTime { get; set; } 

	}
}
