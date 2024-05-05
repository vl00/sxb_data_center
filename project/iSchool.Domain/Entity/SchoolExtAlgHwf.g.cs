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
	[Table("SchoolExtAlgHwf")]
	public partial class SchoolExtAlgHwf
	{

		/// <summary>
		/// 
		/// </summary> 
		[ExplicitKey] 
		public Guid Id { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid Eid { get; set; } 

		/// <summary>
		/// 占地面积
		/// </summary> 
		public double? Acreage { get; set; } 

		/// <summary>
		/// 占地面积单位
		/// </summary> 
		public string AcreageUnit { get; set; } 

		/// <summary>
		/// 投入金额
		/// </summary> 
		public double? Inputamount { get; set; } 

		/// <summary>
		/// 金额增幅
		/// </summary> 
		public double? MoneyDiff { get; set; } 

		/// <summary>
		/// 校车
		/// </summary> 
		public int? SchbusCount { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? HasSwimpool { get; set; } 

		/// <summary>
		/// 泳池位置
		/// </summary> 
		public byte? SwimpoolWhere { get; set; } 

		/// <summary>
		/// 泳池温度
		/// </summary> 
		public byte? SwimpoolTemperature { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? HasLodging { get; set; } 

		/// <summary>
		/// 住宿设施
		/// </summary> 
		public string LodgingFacilities { get; set; } 

		/// <summary>
		/// 住宿人均面加
		/// </summary> 
		public double? LodgingAreaperp { get; set; } 

		/// <summary>
		/// 住宿人数
		/// </summary> 
		public int? LodgingPersionNum { get; set; } 

		/// <summary>
		/// 图书馆藏书量
		/// </summary> 
		public int? LibyBookNum { get; set; } 

		/// <summary>
		/// 图书馆人均面积
		/// </summary> 
		public double? LibyAreaperp { get; set; } 

		/// <summary>
		/// 图书馆人均藏书比例
		/// </summary> 
		public double? LibyBookper { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? HasPgd { get; set; } 

		/// <summary>
		/// 操场跑道长度
		/// </summary> 
		public int? PgdLength { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public bool? HasCanteen { get; set; } 

		/// <summary>
		/// 饭堂餐数
		/// </summary> 
		public int? CanteenNum { get; set; } 

		/// <summary>
		/// 饭堂人均面积
		/// </summary> 
		public double? CanteenAreaperp { get; set; } 

		/// <summary>
		/// 饭堂卫生评级
		/// </summary> 
		public string CanteenHealthRate { get; set; } 

		/// <summary>
		/// 附件-卫生室
		/// </summary> 
		public int? HealthRoom { get; set; } 

		/// <summary>
		/// 附加-steam科室
		/// </summary> 
		public int? SteamRoom { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		public Guid Creator { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select (getdate()) 
		public DateTime CreateTime { get; set; } = DateTime.Now; 

		/// <summary>
		/// 
		/// </summary> 
		public Guid Modifier { get; set; } 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select (getdate()) 
		public DateTime ModifyDateTime { get; set; } = DateTime.Now; 

		/// <summary>
		/// 
		/// </summary> 
		//dbDefaultValue// select ((1)) 
		public bool IsValid { get; set; } = true; 

	}
}
