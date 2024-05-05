using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using MediatR;
namespace iSchool.Application.Service
{
    [Serializable]
    public class AddSchoolCommand : IRequest<Guid?>
    {
        public AddSchoolCommand()
        {
        }
        public AddSchoolCommand(string logo, string name, string eName, string webSite, Guid sid, string intro, string tags)
        {
            Logo = logo;
            Name = name;
            EName = eName;
            WebSite = webSite;
            Sid = sid;
            Intro = intro;
            Tags = tags;
        }

        public string Logo { get; set; }
        public string Name { get; set; }
        public string EName { get; set; }
        public string WebSite { get; set; }
        public Guid Sid { get; set; }
        public string Intro { get; set; }
        public string Tags { get; set; }
        /// <summary>
        ///(1 新增 2 修改 3 查看)
        /// </summary>
        public int Status { get; set; }
        public Guid UserId { get; set; }
        /// <summary>学制</summary>
        public byte? EduSysType { get; set; }
    }


}
