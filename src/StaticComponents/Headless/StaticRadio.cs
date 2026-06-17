using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// A headless component abstract class to implement your own radio elements. Primary use is for developing UI kits and libraries.
/// </summary>
public abstract class StaticRadio : StaticInputBase
{
    /// <summary>
    /// The value of the radio input element.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; } = default!;

    /// inheritdoc
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);
    }
}
