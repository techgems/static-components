using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

namespace TechGems.StaticComponents.Headless.TagHelpers;

/// <summary>
/// Applies a <see cref="StaticAttributeCollection"/> to the element it is placed on, letting the
/// attributes a consumer wrote on a headless input component pass through to the real markup in the
/// component's Razor template.
/// </summary>
/// <remarks>
/// <para>
/// <c>class</c> and <c>style</c> are merged with the values already authored on the element;
/// every other attribute overwrites the authored value, so a template's attributes act as defaults
/// the consumer can override. A consumer can discard the authored classes entirely by using
/// <c>class-replace</c> instead of <c>class</c>.
/// </para>
/// <para>
/// It runs before the <c>static-*</c> tag helpers so the <c>name</c>, <c>id</c> and <c>data-val-*</c>
/// attributes they derive from the model expression always win over anything passed through.
/// </para>
/// </remarks>
[HtmlTargetElement(Attributes = StaticAttributesAttributeName)]
public class StaticAttributesTagHelper : TagHelper
{
    private const string StaticAttributesAttributeName = "static-attributes";
    private const string ClassAttributeName = "class";
    private const string ClassReplaceAttributeName = "class-replace";
    private const string StyleAttributeName = "style";

    /// <summary>
    /// Runs ahead of the <c>static-*</c> tag helpers, which default to <c>0</c>, so the attributes
    /// they own overwrite anything passed through rather than the other way around.
    /// </summary>
    public override int Order => -1000;

    /// <summary>
    /// The attributes to apply, normally <c>@Model.AdditionalAttributes</c> from a component
    /// inheriting <see cref="StaticInputBase"/>.
    /// </summary>
    [HtmlAttributeName(StaticAttributesAttributeName)]
    public StaticAttributeCollection? Attributes { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Attributes is null || Attributes.Count == 0)
        {
            return;
        }

        if (Attributes.Contains(ClassAttributeName) && Attributes.Contains(ClassReplaceAttributeName))
        {
            throw new ArgumentException($@"An element cannot receive both ""{ClassAttributeName}"" and ""{ClassReplaceAttributeName}"". Use ""{ClassAttributeName}"" to add to the component's classes, or ""{ClassReplaceAttributeName}"" to replace them.");
        }

        foreach (var attribute in Attributes)
        {
            if (string.Equals(attribute.Name, ClassAttributeName, StringComparison.OrdinalIgnoreCase))
            {
                MergeClass(output, attribute);
            }
            else if (string.Equals(attribute.Name, ClassReplaceAttributeName, StringComparison.OrdinalIgnoreCase))
            {
                output.Attributes.SetAttribute(ClassAttributeName, attribute.Value);
            }
            else if (string.Equals(attribute.Name, StyleAttributeName, StringComparison.OrdinalIgnoreCase))
            {
                MergeStyle(output, attribute);
            }
            else
            {
                output.Attributes.SetAttribute(attribute);
            }
        }
    }

    /// <summary>
    /// Appends the passed through classes to the ones authored on the element, dropping duplicates
    /// and keeping the authored classes first.
    /// </summary>
    private static void MergeClass(TagHelperOutput output, TagHelperAttribute attribute)
    {
        var authored = Flatten(output.Attributes[ClassAttributeName]?.Value);
        var passedThrough = Flatten(attribute.Value);

        var classes = new List<string>();

        foreach (var name in Tokenize(authored).Concat(Tokenize(passedThrough)))
        {
            if (!classes.Contains(name, StringComparer.Ordinal))
            {
                classes.Add(name);
            }
        }

        SetMerged(output, ClassAttributeName, string.Join(" ", classes));
    }

    /// <summary>
    /// Appends the passed through declarations after the ones authored on the element. Inline styles
    /// resolve last declaration wins, so the consumer's values take effect.
    /// </summary>
    private static void MergeStyle(TagHelperOutput output, TagHelperAttribute attribute)
    {
        var authored = Flatten(output.Attributes[StyleAttributeName]?.Value);
        var passedThrough = Flatten(attribute.Value);

        var declarations = new[] { authored, passedThrough }
            .Select(x => x.Trim().TrimEnd(';').Trim())
            .Where(x => !string.IsNullOrEmpty(x));

        SetMerged(output, StyleAttributeName, string.Join("; ", declarations));
    }

    /// <summary>
    /// Writes an already encoded value back without letting it be encoded a second time.
    /// </summary>
    private static void SetMerged(TagHelperOutput output, string name, string encodedValue)
    {
        if (string.IsNullOrEmpty(encodedValue))
        {
            output.Attributes.RemoveAll(name);
            return;
        }

        output.Attributes.SetAttribute(name, new HtmlString(encodedValue));
    }

    /// <summary>
    /// Reduces an attribute value to its encoded HTML text. Attribute values reach a tag helper as
    /// <see cref="IHtmlContent"/> holding already encoded markup, but a value assigned in C# can be a
    /// plain string, which still needs encoding.
    /// </summary>
    private static string Flatten(object? value)
    {
        switch (value)
        {
            case null:
                return string.Empty;
            case string text:
                return HtmlEncoder.Default.Encode(text);
            case IHtmlContent content:
                using (var writer = new StringWriter())
                {
                    content.WriteTo(writer, HtmlEncoder.Default);
                    return writer.ToString();
                }
            default:
                return HtmlEncoder.Default.Encode(value.ToString() ?? string.Empty);
        }
    }

    private static IEnumerable<string> Tokenize(string value)
    {
        return value.Split(new[] { ' ', '\t', '\r', '\n', '\f' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
