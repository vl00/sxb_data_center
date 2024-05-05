using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic; 
using System.Data;
using System.Text;

namespace iSchool.Domain
{ 
	/// <summary>
	/// 政策文件
	/// </summary>
	[Table("DgAySchPcyFile")]
	public partial class DgAySchPcyFile
	{

		/// <summary>
		/// 
		/// </summary> 
		[ExplicitKey] 
		public Guid Id { get; set; } 

		/// <summary>
		/// 年份
		/// </summary> 
		public int Year { get; set; } 

		/// <summary>
		/// 政策文件类型
		/// 1=对口入学 2=统筹 3=积分入学 4=对口直升 5=电脑派位
		/// </summary> 
		public int Type { get; set; } 

		/// <summary>
		/// 市
		/// </summary> 
		public int City { get; set; } 

		/// <summary>
		/// 区
		/// </summary> 
		public int Area { get; set; } 

		/// <summary>
		/// 政策文件名
		/// </summary> 
		public string Title { get; set; } 

		/// <summary>
		/// 政策文件url
		/// </summary> 
		public string Url { get; set; } 

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
		public Guid? Modifier { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool IsValid { get; set; } 

	}
}
