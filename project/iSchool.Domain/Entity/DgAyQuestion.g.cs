using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic; 
using System.Data;
using System.Text;

namespace iSchool.Domain
{ 
	/// <summary>
	/// 问题
	/// </summary>
	[Table("DgAyQuestion")]
	public partial class DgAyQuestion
	{

		/// <summary>
		/// 问题编号
		/// </summary> 
		[ExplicitKey] 
		public long Id { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Title { get; set; } 

		/// <summary>
		/// 题型
		/// </summary> 
		public byte Type { get; set; } 

		/// <summary>
		/// 是否为第一题
		/// </summary> 
		public bool Is1st { get; set; } 

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
		//dbDefaultValue// select ((1)) 
		public bool IsValid { get; set; } = true; 

		/// <summary>
		/// 题型=多选 时最多可选多少选项
		/// </summary> 
		public int? MaxSelected { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string FindField { get; set; } 

	}
}
