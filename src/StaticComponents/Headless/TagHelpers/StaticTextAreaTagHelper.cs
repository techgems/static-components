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
/// 
/// </summary>
[HtmlTargetElement("textarea", Attributes = StaticForAttributeName)]
public class StaticTextAreaTagHelper : TagHelper
{
    private const string StaticForAttributeName = "static-for";

    private static readonly HashSet<Type> SupportedModelTypes = new()
    {
        typeof(string)
    };

    /// <summary>
    /// The <see cref="ModelExpression"/> bound via <c>asp-for</c>. Drives the inference of the
    /// <c>name</c>, <c>id</c>, <c>required</c>, <c>type</c> and <c>value</c> attributes.
    /// </summary>
    [HtmlAttributeName(StaticForAttributeName)]
    public ModelExpression InputExpression { get; set; } = default!;

    /// <summary>
    /// An optional explicit override for the input's <c>type</c> attribute. If not set, the type will be inferred from the <see cref="InputExpression"/>.
    /// </summary>
    [HtmlAttributeName("type")]
    public string? Type { get; set; } = null;


    /// <summary>
    /// Due to the layering of ModelExpressions, this property becomes necessary to actually retrieve the Property values necessary.
    /// </summary>
    private ModelExpression InputExpressionOneLayerDeep => (ModelExpression)InputExpression.ModelExplorer.Model;

    /// <summary>
    /// Validates that the correct type logic is used and returns an explicit type if set.
    /// </summary>
    /// <param name="modelType"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private void ValidateTypeMatching(Type modelType, TagHelperOutput output)
    {
        if (!SupportedModelTypes.Contains(modelType))
        {
            throw new ArgumentException(@"The model type used in ""asp-for"" is not supported by the StaticInput tag helper. The supported types are: Guid, string, int, long, short, byte, float, double, decimal, DateTime and bool (and their nullable counterparts).");
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
        StaticHeadlessUtils.SetValueAttribute(output, InputExpressionOneLayerDeep);
    }
}
