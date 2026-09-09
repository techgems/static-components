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

    /// <summary>
    /// The HTML attributes the consumer placed on the component that were not bound to one of its
    /// C# properties. Apply them to an element in the component's Razor template with the
    /// <c>static-attributes</c> tag helper so end users can pass through arbitrary markup:
    /// <code>&lt;input static-for="@Model.InputExpression" static-attributes="@Model.AdditionalAttributes" /&gt;</code>
    /// </summary>
    [HtmlAttributeNotBound]
    public StaticAttributeCollection AdditionalAttributes { get; private set; } = StaticAttributeCollection.Empty;

    /// inheritdoc
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        //The output attributes at this point hold exactly the attributes that weren't bound to a property,
        //so they have to be captured before the razor view renders and consumes them.
        AdditionalAttributes = StaticAttributeCollection.Capture(output.Attributes);

        await base.ProcessAsync(context, output);
    }
}
