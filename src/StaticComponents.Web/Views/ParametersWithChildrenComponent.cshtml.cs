using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents;

namespace StaticComponents.Web.Views;

[HtmlTargetElement("parameters-children")]
public class ParametersWithChildrenComponent : StaticComponent
{
    public ParametersWithChildrenComponent()
    {
    }

    [HtmlAttributeName("sample")]
    public string Sample { get; set; } = string.Empty;
}
