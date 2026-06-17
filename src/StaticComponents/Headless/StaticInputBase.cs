using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// A headless component abstract class to implement your own input elements. Primary use is for developing UI kits and libraries.
/// </summary>
public abstract class StaticInputBase : StaticComponent
{
    /// <summary>
    /// The model expression for an input or label element. This is used to bind the input to a model property when using forms.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? InputExpression { get; set; }

    /// <summary>
    /// Set to true when implementing your own input components to allow the label to be included with the input element.
    /// </summary>
    [HtmlAttributeName("show-label")]
    public bool ShowLabel { get; set; } = true;

    /// <summary>
    /// Set to true to disable the input element.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    /// inheritdoc
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);
    }
}
