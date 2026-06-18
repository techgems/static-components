using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// A headless component abstract class to implement your own checkbox elements. Primary use is for developing UI kits and libraries.
/// </summary>
public abstract class StaticCheckbox : StaticInputBase
{
    /// <summary>
    /// The value of the checkbox when it is checked. This is used to bind the checkbox to a model property when using forms.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; } = default!;

    /// <summary>
    /// Checked let's you mark a value as checked regardless of the model value.
    /// </summary>
    [HtmlAttributeName("checked")]
    public bool Checked { get; set; } = false;

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);
    }
}
