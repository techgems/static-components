using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents;


namespace StaticComponents.Web.Views;

[HtmlTargetElement("simple-parameters")]
public class SimpleParametersComponent : StaticComponent
{
    public SimpleParametersComponent()
    {
    }

    [HtmlAttributeName("sample")]
    public string Sample { get; set; } = string.Empty;

}
