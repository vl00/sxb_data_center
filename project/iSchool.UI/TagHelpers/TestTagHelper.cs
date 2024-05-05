using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.UI.TagHelpers
{

    [HtmlTargetElement("TestDiv")]
    public class TestTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var html = new StringBuilder();
            foreach (var item in new string[] { "1", "2" })
            {
                html.Append($"<a class=\"nav-item nav-link\" >{item}</ a>");
            }

            Console.WriteLine(html.ToString());

            output.Content.AppendHtml(html.ToString());
        }
    }
}
