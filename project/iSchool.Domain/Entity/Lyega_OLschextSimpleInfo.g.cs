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
	[Table("Lyega_OLschextSimpleInfo")]
	public partial class Lyega_OLschextSimpleInfo
	{

		/// <summary>
		/// 
		/// </summary> 
		[ExplicitKey] 
		public Guid Eid { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid Sid { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Schname { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Extname { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public byte Grade { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public byte Type { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? Discount { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? Diglossia { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? Chinese { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string SchFType { get; set; } 

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
		public iSchool.LngLatLocation LatLong { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? Lodging { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? Sdextern { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public double? TotalScore { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? IsAuthedByOpen { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid? AuditId { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime ModifyTime { get; set; } 

	}
}
