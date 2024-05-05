using Dapper.Contrib.Extensions;
using System;
namespace iSchool.Domain
{
    /// <summary>
    /// Represents a GeneralTag.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
	[Table("GeneralTag")]
    public class GeneralTag
    {
        public GeneralTag()
        {
            //this.ModifyDateTime = DateTime.Now;
            //      this.CreateTime = DateTime.Now;
        }
        [ExplicitKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
