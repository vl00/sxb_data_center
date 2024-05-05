using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.UI.Modules
{
    /// <summary>
    /// 菜单结果
    /// </summary>
    public class MenuRespDto
    {
        public Guid MenuId { get; set; }
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// 菜单编码
        /// </summary>
        public string MenuCode { get; set; }
        ///// <summary>
        ///// 子菜单
        ///// </summary>
        //public List<MenuRespDto> Children { get; set; }

        /// <summary>
        /// 含有的功能
        /// </summary>
        public List<MenuPointDto> Points { get; set; }

    }

    /// <summary>
    /// 菜单功能
    /// </summary>
    public class MenuPointDto
    {
        public Guid Id { get; set; }
        public Guid MenuId { get; set; }
        /// <summary>
        /// 功能名称
        /// </summary>
        public string PointName { get; set; }
        /// <summary>
        /// 功能编码 = Selector
        /// </summary>
        public string PointCode { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public string Selector { get; set; }
    }

}
