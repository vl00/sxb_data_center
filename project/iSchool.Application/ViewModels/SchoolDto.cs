using iSchool.Domain.Modles;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    public class SchoolDto
    {
        public Guid Sid { get; set; }
        public string Logo { get; set; }

        public string Name { get; set; }

        public string EName { get; set; }
        /// <summary>
        /// 网站
        /// </summary>
        public string WebSite { get; set; }

        public string Tags { get; set; }
        //学校的状态
        public byte? Status { get; set; }

        public string Intro { get; set; }
        public string AuditMessage { get; set; }

        public double Completion { get; set; }
        //分部简单信息
        public List<ExtItem> ExtList { get; set; }

        public Guid Creator { get; set; }
        /// <summary>学制</summary>
        public byte? EduSysType { get; set; }
    }
}


