using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless.TagHelpers;

/// <summary>
/// A TagHelper for rendering a static checkbox input. It uses the "static-checkbox-for" attribute to bind to a model property, and it infers the "name", "id", "required", "type" and "value" attributes based on the model expression. 
/// It also adds a hidden input with the same name and a value of "false" to ensure that a value is sent when the checkbox is unchecked. 
/// Bool is the only supported type for the model property.
/// </summary>
[HtmlTargetElement("input", Attributes = StaticCheckboxForAttributeName)]
public class StaticCheckboxTagHelper : TagHelper
{
    private const string StaticCheckboxForAttributeName = "static-checkbox-for";

    private static readonly HashSet<Type> SupportedModelTypes = new()
    {
        typeof(bool)
    };

    /// <summary>
    /// The <see cref="ModelExpression"/> bound via <c>asp-for</c>. Drives the inference of the
    /// <c>name</c>, <c>id</c>, <c>required</c>, <c>type</c> and <c>value</c> attributes.
    /// </summary>
    [HtmlAttributeName(StaticCheckboxForAttributeName)]
    public ModelExpression InputExpression { get; set; } = default!;

    /// <summary>
    /// Due to the layering of ModelExpressions, this property provides the actual property information.
    /// </summary>
    private ModelExpression InputExpressionOneLayerDeep => (ModelExpression)InputExpression.ModelExplorer.Model;

    private void ValidateTypeMatching(Type modelType, TagHelperOutput output)
    {
        if (!SupportedModelTypes.Contains(modelType))
        {
            throw new ArgumentException(@"Only bool is supported by ""static-checkbox-for""  as a model type.");
        }
    }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(InputExpression);
        ArgumentNullException.ThrowIfNull(InputExpressionOneLayerDeep);

        var modelType = InputExpressionOneLayerDeep.Metadata.ModelType;

        ValidateTypeMatching(modelType, output);

        StaticHeadlessUtils.SetGeneralInputAttributes(output, InputExpressionOneLayerDeep.Name, InputExpressionOneLayerDeep.Metadata);

        output.Attributes.SetAttribute("type", "checkbox");

        output.Attributes.SetAttribute("value", "true");

        if (!output.Attributes.ContainsName("checked") && (bool)InputExpressionOneLayerDeep.Model)
        {
            output.Attributes.SetAttribute("checked", "checked");
        }

        output.PostElement.AppendHtml($@"<input type=""hidden"" name=""{InputExpressionOneLayerDeep.Name}"" value=""false"" />");

    }

}
