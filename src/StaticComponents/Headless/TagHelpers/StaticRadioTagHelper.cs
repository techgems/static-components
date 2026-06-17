using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless.TagHelpers;

/// <summary>
/// A TagHelper for rendering a static radio input. It uses the "static-radio-for" attribute to bind to a model property, and it infers the "name", "id", "required", "type" and "value" attributes based on the model expression.
/// </summary>
[HtmlTargetElement("input", Attributes = StaticRadioForAttributeName)]
public class StaticRadioTagHelper : TagHelper
{
    private const string StaticRadioForAttributeName = "static-radio-for";

    private static readonly HashSet<Type> SupportedModelTypes = new()
    {
        typeof(string),
        typeof(int), typeof(int?), 
        typeof(short), typeof(short?),
        typeof(long), typeof(long?),
        typeof(byte), typeof(byte?),
        typeof(bool), typeof(bool?),
        typeof(Enum),
    };

    /// <summary>
    /// An optional explicit override for the input's <c>value</c> attribute. If not set, the value will be inferred from the <see cref="InputExpression"/>.
    /// Mandatory for radio inputs.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; } = null;

    /// <summary>
    /// The <see cref="ModelExpression"/> bound via <c>asp-for</c>. Drives the inference of the
    /// <c>name</c>, <c>id</c>, <c>required</c>, <c>type</c> and <c>value</c> attributes.
    /// </summary>
    [HtmlAttributeName(StaticRadioForAttributeName)]
    public ModelExpression InputExpression { get; set; } = default!;

    /// <summary>
    /// Due to the layering of ModelExpressions, this property becomes necessary to actually retrieve the Property values necessary.
    /// </summary>
    private ModelExpression InputExpressionOneLayerDeep => (ModelExpression)InputExpression.ModelExplorer.Model;

    private void ValidateTypeMatching(Type modelType, TagHelperOutput output)
    {
        if (!SupportedModelTypes.Contains(modelType))
        {
            throw new ArgumentException(@"Only the following types are supported by ""static-radio-for"" as a model type: string, int, short, long, byte, bool, Enum (and it's nullable counterparts).");
        }

        if (Value is null)
        {
            throw new ArgumentException($@"The ""value"" attribute is required for radio inputs.");
        }
    }


    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(InputExpression);
        ArgumentNullException.ThrowIfNull(InputExpressionOneLayerDeep);

        //normally this works with asp-for and without two layers of ModelExpression,
        //but in our case with the StaticComponent architecture we have an extra layer of ModelExpression which causes the model expression to bury its underlying type one layer deep.
        //Due to this we need to get the ModelType from the inner ModelExpression.
        var modelType = InputExpressionOneLayerDeep.Metadata.ModelType;

        ValidateTypeMatching(modelType, output);

        output.Attributes.SetAttribute("type", "radio");

        StaticHeadlessUtils.SetGeneralInputAttributes(output, InputExpressionOneLayerDeep.Name, InputExpressionOneLayerDeep.Metadata);

        var modelValueString = StaticHeadlessUtils.FormatModelValue(Value!);
        output.Attributes.SetAttribute("value", modelValueString);

        if (!output.Attributes.ContainsName("checked"))
        {
            var explicitValue = Value!.ToString() ?? string.Empty;

            if (Value == InputExpressionOneLayerDeep.Model?.ToString())
            {
                output.Attributes.SetAttribute("checked", "checked");
            }
        }
    }
}
