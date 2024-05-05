using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.ViewModels
{
    /// <summary>
    /// 视频信息
    /// </summary>
    public class VideosInfo
    {
        /// <summary>
        /// 视频类型
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// 视频url
        /// </summary>
        public string VideoUrl { get; set; }

        /// <summary>
        /// 视频描述
        /// </summary>
        public string VideoDesc { get; set; }

        /// <summary>
        /// 封面图url
        /// </summary>
        public string Cover { get; set; }
    }


    /// <summary>
    /// Step2-视频信息
    /// </summary>
    public class VideosInfo2
    {
        /// <summary>
        /// 视频guid
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 视频url
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 视频描述
        /// </summary>
        public string VideoDesc { get; set; }

        /// <summary>
        /// 封面图url
        /// </summary>
        public string Cover { get; set; }
    }
}
