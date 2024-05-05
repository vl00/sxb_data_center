using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a KeyValue.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("KeyValue")]
    public class KeyValue 
    {
		public  KeyValue()
		{
		    this.ModifyDateTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
		}
	        public int Id { get; set; }
        public string Name { get; set; }
        public int Parentid { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public Guid? Createor { get; set; }
        public DateTime? ModifyDateTime { get; set; }
        public Guid? Modifier { get; set; }
        public bool IsValid { get; set; }
        //通用数据。好物分类的图片
        public string Attach { get; set; }
    }
}      
