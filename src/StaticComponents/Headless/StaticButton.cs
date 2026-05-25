using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// A headless static component class for creating your own &lt;button&gt; based component. It adds a type attribute, which only accepts the following values: "button", "submit" and "reset", matching the HTML specification. 
/// It also adds a disabled attribute, which can be set to true or false.
/// </summary>
public class StaticButton : StaticComponent
{
    /// <summary>
    /// The HTML Attribute "type" of the button. Accepts the following values: "button", "submit" and "reset". If another value is provided, an exception will be thrown.
    /// </summary>
    [HtmlAttributeName("type")]
    public string Type { get; set; } = "button";

    /// <summary>
    /// The HTML Attribute "disabled" of the button. You can use this value to implement your own markup.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// An override for ProcessAsync. Checks if the "type" attribute has an acceptable value, and throws an exception if it doesn't.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Type) || (Type != "button" && Type != "submit" && Type != "reset"))
        {
            throw new ArgumentException(@"The ""type"" attribute of the PinesButton component only accepts the following values: ""button"", ""submit"" and ""reset"".");
        }

        await base.ProcessAsync(context, output);
    }
}
