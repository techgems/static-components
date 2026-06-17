using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// A headless component abstract class to implement your own select elements. Primary use is for developing UI kits and libraries.
/// </summary>
public abstract class StaticSelect : StaticInputBase
{
    /// inheritdoc
    [HtmlAttributeName("asp-items")]
    public List<SelectListItem> Items { get; set; } = new List<SelectListItem>();

    /// inheritdoc
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);
    }
}
