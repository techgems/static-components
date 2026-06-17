using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TechGems.StaticComponents.Headless.TagHelpers;

/// <summary>
/// A tag helper that targets the &lt;input&gt; element and acts as a replacement for the built-in
/// <c>asp-for</c> tag helper. It uses <see cref="InputExpression"/> to infer the <c>name</c>, <c>id</c>,
/// <c>required</c>, <c>type</c>, <c>value</c> attributes for the rendered input.
/// Any additional attributes set on the element are preserved on the output. Explicit attributes take
/// precedence over inferred values, with the exception of <c>name</c> and <c>id</c>, which are always
/// derived from the <see cref="InputExpression"/>.
/// </summary>
[HtmlTargetElement("input", Attributes = StaticForAttributeName)]
public class StaticInputTagHelper : TagHelper
{
    private const string StaticForAttributeName = "static-for";

    private static readonly HashSet<Type> SupportedModelTypes = new()
    {
        typeof(Guid), typeof(Guid?),
        typeof(string),
        typeof(int), typeof(int?),
        typeof(long), typeof(long?),
        typeof(short), typeof(short?),
        typeof(byte), typeof(byte?),
        typeof(float), typeof(float?),
        typeof(double), typeof(double?),
        typeof(decimal), typeof(decimal?),
        typeof(DateTime), typeof(DateTime?)
    };

    private static readonly HashSet<string> ValidHtmlInputTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text", "number", "date", "email", "password", "tel", "url", "search",
        "color", "datetime-local", "month", "time", "week", "hidden"
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
    /// An optional explicit override for the input's <c>value</c> attribute. If not set, the value will be inferred from the <see cref="InputExpression"/>.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; } = null;

    /// <summary>
    /// Due to the layering of ModelExpressions, this property becomes necessary to actually retrieve the Property values necessary.
    /// </summary>
    private ModelExpression InputExpressionOneLayerDeep => (ModelExpression)InputExpression.ModelExplorer.Model;

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

        var resolvedType = ResolveInputType(output, modelType);

        StaticHeadlessUtils.SetGeneralInputAttributes(output, InputExpressionOneLayerDeep.Name, InputExpressionOneLayerDeep.Metadata);
        StaticHeadlessUtils.SetValueAttribute(output, InputExpressionOneLayerDeep);
    }

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

        if (!string.IsNullOrEmpty(Type) && !ValidHtmlInputTypes.Contains(Type!))
        {
            throw new ArgumentException($@"The ""type"" attribute value ""{Type}"" is not supported by the StaticInput tag helper. Supported values: {string.Join(", ", ValidHtmlInputTypes)}.");
        }
    }

    private string ResolveInputType(TagHelperOutput output, Type modelType)
    {
        if (!string.IsNullOrEmpty(Type))
        {
            output.Attributes.SetAttribute("type", Type);
            return Type;
        }

        var inferredType = InferInputType(modelType);
        output.Attributes.SetAttribute("type", inferredType);
        return inferredType;
    }

    private string InferInputType(Type modelType)
    {
        if (string.Equals(InputExpressionOneLayerDeep.Metadata.TemplateHint, "HiddenInput", StringComparison.Ordinal))
        {
            return "hidden";
        }

        var dataTypeName = InputExpressionOneLayerDeep.Metadata.DataTypeName;

        if (string.Equals(dataTypeName, nameof(DataType.Password), StringComparison.Ordinal))
        {
            return "password";
        }

        if (string.Equals(dataTypeName, nameof(DataType.EmailAddress), StringComparison.Ordinal))
        {
            return "email";
        }

        if (string.Equals(dataTypeName, nameof(DataType.Url), StringComparison.Ordinal))
        {
            return "url";
        }

        if (string.Equals(dataTypeName, nameof(DataType.PhoneNumber), StringComparison.Ordinal))
        {
            return "tel";
        }

        var underlying = Nullable.GetUnderlyingType(modelType) ?? modelType;

        if (underlying == typeof(string) || underlying == typeof(Guid))
        {
            return "text";
        }

        if (underlying == typeof(int) || underlying == typeof(long)
            || underlying == typeof(short) || underlying == typeof(byte)
            || underlying == typeof(float) || underlying == typeof(double)
            || underlying == typeof(decimal))
        {
            return "number";
        }

        if (underlying == typeof(DateTime))
        {
            return "date";
        }

        return "text";
    }
}
