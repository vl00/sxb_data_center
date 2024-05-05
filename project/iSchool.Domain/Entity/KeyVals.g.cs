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
	[Table("KeyVals")]
	public partial class KeyVals
	{

		/// <summary>
		/// 
		/// </summary> 
		public string Id { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Name { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Parentid { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int Type { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Description { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime? CreateTime { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid? Createor { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime? ModifyDateTime { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid? Modifier { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select ((1)) 
		public bool IsValid { get; set; } = true; 

	}
}
