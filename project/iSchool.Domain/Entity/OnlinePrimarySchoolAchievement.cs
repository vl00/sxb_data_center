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
    [Table("OnlinePrimarySchoolAchievement")]
    public partial class OnlinePrimarySchoolAchievement
    {

        /// <summary>
        /// 
        /// </summary> 
        //[ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public Guid ExtId { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public int Year { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public string Link { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 
        /// </summary> 
        public Guid Createor { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public DateTime ModifyDateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 
        /// </summary> 
        public Guid Modifier { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public bool IsValid { get; set; } = true;

    }
}
