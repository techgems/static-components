using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents;

[HtmlTargetElement("render-static-scripts", TagStructure = TagStructure.WithoutEndTag)]
public class StaticScriptRenderer : TagHelper
{
    /// <summary>
    /// The View Context necessary to get the Html Helper that renders the partial views.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    /// <summary>
    /// Renders the stored scripts.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="output"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext is null)
        {
            throw new ArgumentNullException(nameof(ViewContext));
        }

        output.TagName = null;        

        var sb = new StringBuilder();

        sb.AppendLine("<!-- Static Scripts -->");

        var scriptList = (List<string>)ViewContext.HttpContext.Items[StaticComponentsConstants.StaticScriptKey]!;

        if (scriptList == null) 
        { 
            output.SuppressOutput();
            return;
        }

        foreach (var script in scriptList)
        {
            sb.AppendLine(script);
        }

        sb.AppendLine("<!-- Static Scripts End -->");

        output.Content.SetHtmlContent(sb.ToString());
    }
}
