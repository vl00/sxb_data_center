using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 学校视频
    /// </summary>
    public class AddSchoolVideoCommand:IRequest<HttpResponse<bool>>
    {
        /// <summary>
        /// 学部Id
        /// </summary>
        public Guid Eid { get; set; }

        /// <summary>
        /// 用于删除历史数据
        /// </summary>
        public string CurrentVideoTypes { get; set; }

        /// <summary>
        /// 视频
        /// </summary> 
        public string[] Videos { get; set; }

        /// <summary>
        /// 视频封面
        /// </summary> 
        public string[] Covers { get; set; }

        /// <summary>
        /// 视频描述
        /// </summary>
        public string[] VideoDescs { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// 视频类型
        /// </summary>
        public string[] Types { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        public Guid CurrentUserId { get; set; }

    }
}
