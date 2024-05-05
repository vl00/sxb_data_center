using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic; 
using System.Data;
using System.Text;

namespace iSchool.Domain
{ 
	/// <summary>
	/// 选项
	/// </summary>
	[Table("DgAyQuestionOption")]
	public partial class DgAyQuestionOption
	{

		/// <summary>
		/// 选项编号
		/// </summary> 
		[ExplicitKey] 
		public long Id { get; set; } 

		/// <summary>
		/// 选项名
		/// </summary> 
		public string Title { get; set; } 

		/// <summary>
		/// 问题
		/// </summary> 
		public long? Qid { get; set; } 

		/// <summary>
		/// 分数,当问题题型为计分类时使用
		/// </summary> 
		public double? Point { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int Sort { get; set; } 

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
		/// 查民办小学字段
		/// </summary> 
		public string FindField { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string FindFieldFw { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string FindFieldFwJx { get; set; } 

	}
}
