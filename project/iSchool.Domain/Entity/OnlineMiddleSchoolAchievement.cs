
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace iSchool.Domain
{
    /// <summary>
    /// 
    /// </summary>
    [Table("OnlineMiddleSchoolAchievement")]
    public partial class OnlineMiddleSchoolAchievement : Entity
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
        /// ÷ÿµ„¬ 
        /// </summary> 
        public double? Keyrate { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public double? Average { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public double? Highest { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public double? Ratio { get; set; }

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
