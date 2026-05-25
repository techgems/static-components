using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// A headless static component class for creating your own &lt;input&gt; based component. 
/// It adds an "asp-for" attribute, which accepts a ModelExpression, and a "type" attribute, which can be set to any valid HTML input type. 
/// If the "type" attribute is not set, it will be inferred from the model type of the "asp-for" attribute. 
/// It also adds a "show-label" attribute, which can be set to true or false, and a "disabled" attribute, which can also be set to true or false.
/// </summary>
public class StaticInput : StaticNode
{
    /// <summary>
    /// asp-for is a ModelExpression that represents the model property that the input is bound to. It is used to infer the input type if the "type" attribute is not set, and to get the display name for the label if "show-label" is set to true.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? InputExpression { get; set; }

    /// <summary>
    /// Type is a value that represents the HTML input type. This property is optional as the HTML input type can be inferred from the model type of the "asp-for" attribute.
    /// It should only be set if you are not using model binding with the asp-for attribute.
    /// </summary>
    [HtmlAttributeName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// ShowLabel is a boolean value that indicates whether to show a label for the input. If set to true, the label content will be inferred from the display name of the model property specified in the "asp-for" attribute.
    /// This is a tool to use in your own markup to conditionally render a label along with the input.
    /// </summary>
    [HtmlAttributeName("show-label")]
    public bool ShowLabel { get; set; } = true;

    /// <summary>
    /// Disabled is a boolean value that indicates whether the input should be disabled. You can use this value to implement your own markup.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// The inferred content for the label, based on the display name of the model property specified in the "asp-for" attribute. 
    /// It uses the DisplayName attribute of the model in question to get the display name.
    /// </summary>
    [HtmlAttributeNotBound]
    public string LabelContent => InputExpression?.Metadata.DisplayName ?? string.Empty;

    /// <summary>
    /// Retrieves the HTML input type based on the "type" attribute or the model type of the "asp-for" attribute. If the "type" attribute is set, it returns that value. If not, it infers the input type from the model type of the "asp-for" attribute. 
    /// If neither is available, it defaults to "text". The inference is based on common mappings between C# types and HTML input types.
    /// </summary>
    /// <returns>The HTML input type as a string.</returns>
    public string GetHtmlInputType()
    {
        if (!string.IsNullOrEmpty(Type))
        {
            return Type;
        }

        if (InputExpression == null)
        {
            return "text";
        }

        var modelType = InputExpression.Metadata.ModelType;

        switch (modelType)
        {
            case Type t when t == typeof(string) || t == typeof(Guid) || t == typeof(Guid?):
                return "text";
            case Type t when t == typeof(int) || t == typeof(int?) || t == typeof(long) || t == typeof(long?) || t == typeof(short) || t == typeof(short?) || t == typeof(byte) || t == typeof(byte?):
                return "number";
            case Type t when t == typeof(float) || t == typeof(float?) || t == typeof(double) || t == typeof(double?) || t == typeof(decimal) || t == typeof(decimal?):
                return "number";
            case Type t when t == typeof(DateTime) || t == typeof(DateTime?):
                return "date";
            default:
                return "text";
        }
    }

    private bool IsInputModelValid(Type type)
    {
        var validTypes = new List<Type> {
            typeof(Guid),
            typeof(Guid?),
            typeof(string),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
            typeof(short),
            typeof(short?),
            typeof(byte),
            typeof(byte?),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(decimal),
            typeof(decimal?),
            typeof(DateTime),
            typeof(DateTime?)
        };


        return validTypes.Contains(type);
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (InputExpression is not null)
        {
            var modelType = InputExpression.Metadata?.ModelType!;

            if (!IsInputModelValid(modelType))
            {
                throw new ArgumentException(@"The model type used in ""asp-for"" is not supported by this component. The supported types are: string, int, long, short, byte, float, double, decimal and DateTime.");
            }
        }

        await base.ProcessAsync(context, output);
    }
}
