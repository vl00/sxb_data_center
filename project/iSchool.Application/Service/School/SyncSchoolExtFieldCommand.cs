using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class SyncSchoolExtFieldCommand : IRequest<HttpResponse<string>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="eid"></param>
        /// <param name="fields"></param>
        /// <param name="table"></param>
        /// <param name="isVideo"></param>
        /// <param name="fieldItem">字段信息</param>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        public SyncSchoolExtFieldCommand(Guid sid,
            Guid eid,
            List<KeyValueDto<string, object, int>> fields,
            string table, bool isVideo,
            fieldItem fieldItem,
            Guid userId,
            IDictionary<string, object> data)
        {
            Sid = sid;
            Eid = eid;
            Fields = fields;
            Table = table;
            IsVideo = isVideo;
            this.fieldItem = fieldItem;
            UserId = userId;
            Data = data;
        }

        public Guid Sid { get; set; }

        public Guid Eid { get; set; }

        /// <summary>
        /// 字段对应的名字与值
        /// </summary>
        public List<KeyValueDto<string,object,int>> Fields { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// 是否视频
        /// </summary>
        public bool IsVideo { get; set; }

        /// <summary>
        /// 字段详情信息
        /// </summary>
        public fieldItem fieldItem { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }

        //同步分部的信息
        public IDictionary<string, object> Data { get; set; }

    }
}
