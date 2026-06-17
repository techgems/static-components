using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// A headless component abstract class to implement your own label elements. Primary use is for developingUI kits and libraries.
/// </summary>
public abstract class StaticLabel : StaticComponent
{
    /// <summary>
    /// The model expression for a label element. This is used to bind the label to a model property when using forms.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = default!;

    /// inheritdoc
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);
    }
}
