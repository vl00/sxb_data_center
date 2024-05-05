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
	[Table("TagType")]
	public partial class TagType
	{

		/// <summary>
		/// 
		/// </summary> 
		[Key] 
		//db column is AutoIncrement 
		public byte Id { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Name { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public byte Parentid { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int? Sort { get; set; }

		/// <summary>
		/// 
		/// </summary> 
		public DateTime CreateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// 
		/// </summary> 
		public Guid? Creator { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime ModifyDateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// 
		/// </summary> 
		public Guid? Modifier { get; set; }

		/// <summary>
		/// 1=有效，0=已删除
		/// </summary> 
		public bool IsValid { get; set; } = true;

	}
}
