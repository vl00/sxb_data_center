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
	[Table("Total_User")]
	public partial class Total_User
	{

		/// <summary>
		/// 
		/// </summary> 
		public Guid Id { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Account { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public string Username { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid? RoleId { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid? QxId { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime Date { get; set; } 

	}
}
