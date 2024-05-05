using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.UI.Models
{
    /// <summary>
    /// 同步类型文本框类型
    /// </summary>
    public enum TextFormat
    {
        /// <summary>
        /// 省市区下拉框
        /// </summary>
        [Description("reginclickon")]
        ReginClickOn = 1,
        /// <summary>
        /// 经纬度控件
        /// </summary>
        [Description("latlongdigital")]
        LatLongDigital = 2,

        /// <summary>
        /// 数字输入框
        /// </summary>
        [Description("digital")]
        Digital = 3,

        /// <summary>
        ///下拉框
        /// </summary>
        [Description("clickon")]
        ClickOn = 4,

        /// <summary>
        /// 富文本编辑器
        /// </summary>
        [Description("editor")]
        Editor = 5,


        /// <summary>
        ///视频控件
        /// </summary>
        [Description("video")]
        Video = 6,
        /// <summary>
        /// 标签选择器
        /// </summary>
        [Description("tagcheckon")]
        TagCheckOn = 7,
        /// <summary>
        /// 文本框
        /// </summary>
        [Description("text")]
        Text = 8,

        /// <summary>
        /// 学校特色课程或项目组合控件
        /// </summary>
        [Description("characteristiceditor")]
        CharacteristicEditor = 9,

        /// <summary>
        /// 校长风采（富文本加视频上传）
        /// </summary>
        [Description("editorvideo")]
        EditorVideo = 10,

        /// <summary>
        /// 时间控件
        /// </summary>
        [Description("datetime")]
        DateTime = 11,

        /// <summary>
        /// 开放日及行事历
        /// </summary>
        [Description("textdatetime")]
        TextDateTime = 12,
        /// <summary>
        /// 图片上传
        /// </summary>
        [Description("imageupload")]
        ImageUpload = 13
    }



}
