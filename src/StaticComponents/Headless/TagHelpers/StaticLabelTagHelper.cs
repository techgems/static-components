using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless.TagHelpers;

/// <summary>
/// 
/// </summary>
[HtmlTargetElement("label", Attributes = StaticForAttributeName)]
public class StaticLabelTagHelper : TagHelper
{
    private const string StaticForAttributeName = "static-for";

    /// <summary>
    /// 
    /// </summary>
    [HtmlAttributeName(StaticForAttributeName)]
    public ModelExpression StaticFor { get; set; } = default!;

    private ModelExpression NestedModelExpression => (ModelExpression)StaticFor.ModelExplorer.Model;


    /// <summary>
    /// The text rendered inside the &lt;label&gt;. Uses the <see cref="System.ComponentModel.DisplayNameAttribute"/>
    /// applied to the property when present, otherwise falls back to the property name (i.e. <c>nameof</c>).
    /// </summary>
    [HtmlAttributeNotBound]
    public string LabelContent => NestedModelExpression?.Metadata.DisplayName
        ?? NestedModelExpression?.Metadata.PropertyName
        ?? string.Empty;


    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("for", NestedModelExpression.Name);
        output.Content.SetContent(LabelContent);

        base.Process(context, output);
    }
}
