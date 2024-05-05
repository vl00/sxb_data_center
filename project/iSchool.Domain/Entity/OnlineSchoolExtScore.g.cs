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
	[Table("OnlineSchoolExtScore")]
	public partial class OnlineSchoolExtScore
	{

        /// <summary>
        /// 分部eid，这个表推送过来的
        /// </summary> 
        [ExplicitKey]
        public Guid SchoolSectionId { get; set; }

        /// <summary>
        /// sid，这个表推送过来的
        /// </summary> 
        public Guid SchoolId { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public double AggScore { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public double TeachScore { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public double HardScore { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public double EnvirScore { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public double ManageScore { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public double LifeScore { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int CommentCount { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public int AttendCommentCount { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public DateTime LastCommentTime { get; set; } 

	}
}
