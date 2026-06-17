using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents;

namespace StaticComponents.Web.Views.InputStatic;

public class InputStaticSelectTest : StaticComponent
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? InputExpression { get; set; }

    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    [HtmlAttributeName("show-label")]
    public bool ShowLabel { get; set; } = true;

    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);
    }
}
