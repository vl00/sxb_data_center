using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a GeneralTagBind.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("GeneralTagBind")]
    public class GeneralTagBind 
    {
		public  GeneralTagBind()
		{
		    //this.ModifyDateTime = DateTime.Now;
      //      this.CreateTime = DateTime.Now;
		}
	        public Guid? TagId { get; set; }
        public Guid? DataId { get; set; }
        public byte? DataType { get; set; }
        public byte? DataType_s { get; set; }
        public bool? Ms { get; set; }
    }
}      
