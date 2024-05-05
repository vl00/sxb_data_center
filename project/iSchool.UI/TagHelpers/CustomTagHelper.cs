using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.UI.TagHelpers
{


    /// <summary>
    /// 自定义下拉框
    /// </summary>
    [HtmlTargetElement("CustomSelect")]
    public class CustomSelectTagHelper : TagHelper
    {
        /// <summary>
        /// id和name的属性
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 下拉框对应的值
        /// </summary>
        public object SetValue { get; set; }
        /// <summary>
        /// 下拉框填充内容
        /// </summary>
        public List<KeyValueDto<string>> Data { get; set; }

        /// <summary>
        /// 数据的类型
        /// </summary>
        public int Type { get; set; }



        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var html = new StringBuilder();
            html.Append($"<select id=\"{Name}\" name=\"{Name}\" class=\"form-control\">");
            foreach (var item in Data)
            {
                if (item != null && item.Value != null)
                {
                    //如果是布尔类型
                    if (Type == (int)FielDataType.Bool)
                    {
                        bool? itemValue = item.Value.Trim().ToLower() == "true" ? true : (item.Value.Trim().ToLower() == "false" ? (bool?)false : null);
                        html.Append($"<option value=\"{item.Value}\" "
                            + ((bool?)SetValue == itemValue ? "selected=\"selected\"" : "")
                            + $">{ item.Key}</ option > ");
                    }
                    else
                    {
                        html.Append($"<option value=\"{item.Value}\" "
                        + (item.Value.ToString() == SetValue?.ToString() ? "selected=\"selected\"" : "")
                        + $">{ item.Key}</ option > ");
                    }
                }
            }
            html.Append("</select>");
            output.Content.AppendHtml(html.ToString());
        }
    }


    [HtmlTargetElement("CustomDiv")]
    public class CustomDivTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "card-body text-secondary");
            output.Attributes.SetAttribute("data-TagId", "");
        }
    }

    [HtmlTargetElement("text")]
    public class inputtextTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "input";
            output.TagMode = TagMode.StartTagAndEndTag;

        }
    }

    #region 年份字段内容专用下拉框
    /// <summary>
    /// 年份下拉框专用
    /// </summary>
    [HtmlTargetElement("CustomYears")]
    public class CustomSelectYearHelper : TagHelper
    {
        /// <summary>
        /// name的属性(字段名称)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 下拉框对应的值
        /// </summary>
        public string SetValue { get; set; }

        /// <summary>
        /// 年份对应的字段，多个字段用|隔开
        /// </summary>
        public string Fields { get; set; }

        /// <summary>
        /// 下拉框填充内容
        /// </summary>
        public List<string> Data { get; set; }

        #region 点选标签才需要给这两个字段赋值
        /// <summary>
        /// 展示标签div的id后缀
        /// </summary>
        public string TageShowIdPostfix { get; set; }

        /// <summary>
        /// 标签类型
        /// </summary>
        public string TagType { get; set; }
        #endregion

        #region ue编辑器专用属性
        /// <summary>
        /// ue编辑器Id
        /// </summary>
        public string UEId { get; set; }

        /// <summary>
        /// 预存ue编辑器要显示内容的标签Id
        /// </summary>
        public string PreId { get; set; }
        #endregion

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            output.Content.Clear();
            output.Attributes.SetAttribute("name", Name);
            output.Attributes.SetAttribute("class", "form-control");
            output.Attributes.SetAttribute("data-input", Fields);
            output.Attributes.SetAttribute("data-selval", SetValue);
            output.Attributes.SetAttribute("tage_show_id_postfix", TageShowIdPostfix);
            output.Attributes.SetAttribute("data-tag", TagType);
            output.Attributes.SetAttribute("ueid", UEId);
            output.Attributes.SetAttribute("preid", PreId);
            output.Attributes.SetAttribute("disabled", "disabled");
            output.Attributes.SetAttribute("hidden", "hidden");
            


            foreach (var item in Data)
            {
                var selected = "";
                if (SetValue != null && SetValue == item)
                {
                    selected = "selected=\"selected\"";
                }
                var listItem = $"<option value=\"{item}\" {selected}>{item}</option>";
                output.Content.AppendHtml(listItem);
            }
        }
    }


    /// <summary>
    /// 添加其他年份--下拉框专用
    /// </summary>
    [HtmlTargetElement("CustomNewOtherYears")]
    public class CustomNewOtherYearsHelper : TagHelper
    {
        public string Class { get; set; }

        /// <summary>
        /// name的属性(字段名称)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 当前年份
        /// </summary>
        public string SetValue { get; set; }

        /// <summary>
        /// 下拉框填充内容
        /// </summary>
        public List<string> Data { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            output.Content.Clear();
            output.Attributes.SetAttribute("name", Name??"");
            output.Attributes.SetAttribute("class", "form-control " + Class);
            output.Attributes.SetAttribute("data-selval", SetValue);

            foreach (var item in Data)
            {
                var selected = "";
                if (SetValue != null && SetValue == item)
                {
                    selected = "selected=\"selected\"";
                }
                var listItem = $"<option value=\"{item}\" {selected}>{item}</option>";
                output.Content.AppendHtml(listItem);
            }
        }
    }

    #endregion


    #region 年份按钮列表
    /// <summary>
    /// 年份按钮列表
    /// </summary>
    [HtmlTargetElement("CustomYearsBtnShow")]
    public class CustomDivYearsBtnHelper : TagHelper
    {
        /// <summary>
        /// 年份列表
        /// </summary>
        public List<string> Years { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 最新年份
        /// </summary>
        public string LatestYear { get; set; }

        /// <summary>
        /// 多输入项--必须传值
        /// </summary>
        public string PageCacheInputId { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Content.Clear();
            output.Attributes.SetAttribute("class", "row");
            output.Attributes.SetAttribute("style", "margin-left:-1px;");
            int count = 1;
            if (Years == null || Years.Count == 0) return;
            foreach (var year in Years)
            {
                #region 默认选中最新年份的数据，暂时不需要该功能
                //最新年份class=btnyearclick btn btn-red；否则是btnyearclick btn btnCommon
                //var classValue = LatestYear == year ? "btnyearclick btn btn-red" : "btnyearclick btn btnCommon";
                #endregion
                var classValue = "btnyearclick btn btnCommon";

                var listItem = $"<div class=\"form-inline\"><button type = \"button\" id = \"" + Field+"_"+year+"\" style = \"position:relative;\" class=\""+classValue+ "\" data-input=\"2\"  page-cache-input-id=\"" + PageCacheInputId + "\"   data-extid=\"" + Field+"_"+year+"\">"+ year + "</button></div>";
                output.Content.AppendHtml(listItem);
                if (count % 8 == 0)//每满8个btn，则换行
                {
                    var item = $"<div class=\"form-inline col-md-4\"></div></div><div class=\"row\" style=\"margin-left:-1px;\" >";
                    output.Content.AppendHtml(item);
                }
                ++count;
            }
        }
    }

    #endregion


    #region 预览--年份按钮列表
    /// <summary>
    /// 预览--年份按钮列表
    /// </summary>
    [HtmlTargetElement("PreviewCustomYearsBtnShow")]
    public class CustomPreviewDivYearsBtnHelper : TagHelper
    {
        /// <summary>
        /// 年份列表
        /// </summary>
        public List<string> Years { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 最新年份
        /// </summary>
        public string LatestYear { get; set; }

        /// <summary>
        /// 多输入项--必须传值
        /// </summary>
        public string PageCacheInputId { get; set; }

        public string StepName { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Content.Clear();
            output.Attributes.SetAttribute("class", "row");
            output.Attributes.SetAttribute("style", "margin-left:-1px;");
            int count = 1;
            if (Years == null || Years.Count == 0) return;
            foreach (var year in Years)
            {
                #region 默认选中最新年份的数据，暂时不需要该功能
                //最新年份class=btnyearclick btn btn-red；否则是btnyearclick btn btnCommon
                //var classValue = LatestYear == year ? "btnyearclick btn btn-red" : "btnyearclick btn btnCommon";
                #endregion
                var classValue = StepName+"btnyearclick btn btnCommon";

                var listItem = $"<div class=\"form-inline\"><button type = \"button\" id = \"" + Field + "_" + year + "\" style = \"position:relative;\" class=\"" + classValue + "\" data-input=\"2\"  page-cache-inputId=\"" + PageCacheInputId + "\"   data-extid=\"" + Field + "_" + year + "\">" + year + "</button></div>";
                output.Content.AppendHtml(listItem);
                if (count % 8 == 0)//每满8个btn，则换行
                {
                    var item = $"<div class=\"form-inline col-md-4\"></div></div><div class=\"row\" style=\"margin-left:-1px;\" >";
                    output.Content.AppendHtml(item);
                }
                ++count;
            }
        }
    }

    #endregion

}
