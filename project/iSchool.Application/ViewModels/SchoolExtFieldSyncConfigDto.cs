using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    [Serializable]
    public class SchoolExtFieldSyncConfigDto
    {
        public List<tabItem> Tab { get; set; }
        public List<fieldItem> Fields { get; set; }
    }
    [Serializable]
    public class tabItem
    {
        /// <summary>
        /// tab页名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// tab子页
        /// </summary>
        public int Index { get; set; }
        public string Id { get; set; }
    }
    [Serializable]
    public class fieldItem
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string TextFormat { get; set; }
        public string DBtable { get; set; }
        public List<string> DBfield { get; set; }
        public int TabIndex { get; set; }
        public int Sort { get; set; }
        public List<KeyValueDto<string>> Data { get; set; }
        public int DataType { get; set; }
        /// <summary>
        /// 标签类型
        /// </summary>
        public int? TagType { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string Unit { get; set; }

        public int? VideoType { get; set; }
    }
}
