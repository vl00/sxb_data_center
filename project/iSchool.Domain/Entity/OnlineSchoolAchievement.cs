using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace iSchool.Domain
{
    /// <summary>
    /// 
    /// </summary>
    [Table("OnlineSchoolAchievement")]
    public class OnlineSchoolAchievement : Entity
    {

        /// <summary>
        /// 
        /// </summary> 
        public Guid ExtId { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public int Year { get; set; }

        /// <summary>
        /// ³É¼¨
        /// </summary> 
        public Guid SchoolId { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public double Count { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 
        /// </summary> 
        public Guid Creator { get; set; }

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
