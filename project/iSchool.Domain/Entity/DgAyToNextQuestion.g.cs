using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic; 
using System.Data;
using System.Text;

namespace iSchool.Domain
{ 
	/// <summary>
	/// 跳转下一题
	/// </summary>
	[Table("DgAyToNextQuestion")]
	public partial class DgAyToNextQuestion
	{

		/// <summary>
		/// 
		/// </summary> 
		[ExplicitKey] 
		public Guid Id { get; set; } 

		/// <summary>
		/// 选项 or 问题
		/// </summary> 
		public long Scid { get; set; } 

		/// <summary>
		/// 1=scid为选项id, 2=scid为问题id
		/// </summary> 
		public byte Sctype { get; set; } 

		/// <summary>
		/// 下一问题
		/// </summary> 
		public long? NextQid { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime? CreateTime { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid? Creator { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime? ModifyDateTime { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool IsValid { get; set; } 

	}
}
