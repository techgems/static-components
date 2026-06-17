using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechGems.StaticComponents.Headless.TagHelpers;

/// <summary>
/// 
/// </summary>
[HtmlTargetElement("select", Attributes = StaticForAttributeName)]
public class StaticSelectTagHelper : TagHelper
{
    private const string StaticForAttributeName = "static-for";
    private const string StaticItemsAttributeName = "static-items";

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

    /// <summary>
    /// The <see cref="ModelExpression"/> bound via <c>asp-for</c>. Drives the inference of the
    /// <c>name</c>, <c>id</c>, <c>required</c>, <c>type</c> and <c>value</c> attributes.
    /// </summary>
    [HtmlAttributeName(StaticForAttributeName)]
    public ModelExpression InputExpression { get; set; } = default!;

    /// <summary>
    /// Due to the layering of ModelExpressions, this property becomes necessary to actually retrieve the Property values necessary.
    /// </summary>
    private ModelExpression InputExpressionOneLayerDeep => (ModelExpression)InputExpression.ModelExplorer.Model;

    /// <summary>
    /// The <see cref="IEnumerable{SelectListItem}"/> bound via <c>asp-items</c>. Drives the inference of the
    /// <c>options</c> for the <c>select</c> element.
    /// </summary>
    [HtmlAttributeName(StaticItemsAttributeName)]
    public List<SelectListItem> Items { get; set; } = new List<SelectListItem>();

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(InputExpression);
        ArgumentNullException.ThrowIfNull(InputExpressionOneLayerDeep);

        var modelType = InputExpressionOneLayerDeep.Metadata.ModelType;

        ValidateTypeMatching(modelType, output);

        StaticHeadlessUtils.SetGeneralInputAttributes(output, InputExpressionOneLayerDeep.Name, InputExpressionOneLayerDeep.Metadata);

        // Determine which option (if any) should be marked selected.
        //
        // Selection priority:
        //   1. If the bound model value matches one of the item Values, that item wins and
        //      every other item's SelectListItem.Selected flag is ignored (so a stale
        //      "Selected = true" on the items list cannot override the actual model value).
        //   2. If the model value is null OR does not match any item, fall back to the
        //      SelectListItem.Selected flags as authored.
        //
        // We format the model value through StaticHeadlessUtils.FormatModelValue so that
        // numeric, Guid and DateTime model values are compared against item Values using
        // the same invariant string representation the rest of the helpers emit.
        var modelValue = InputExpressionOneLayerDeep.Model;
        string? modelValueString = modelValue is null ? null : StaticHeadlessUtils.FormatModelValue(modelValue);
        var modelMatchesAnOption = modelValueString is not null
            && (Items?.Any(x => string.Equals(x.Value, modelValueString, StringComparison.Ordinal)) ?? false);

        bool IsItemSelected(SelectListItem item) => modelMatchesAnOption
            ? string.Equals(item.Value, modelValueString, StringComparison.Ordinal)
            : item.Selected;

        var optionsBuilder = new StringBuilder();
        if (Items != null && Items.Any())
        {
            var renderedGroups = new HashSet<SelectListGroup>();

            foreach (var item in Items)
            {
                if (item.Group is null)
                {
                    optionsBuilder.AppendLine(RenderOption(item, IsItemSelected(item)));
                    continue;
                }

                if (!renderedGroups.Add(item.Group))
                {
                    continue;
                }

                var disabledAttr = item.Group.Disabled ? " disabled" : string.Empty;
                optionsBuilder.AppendLine($"<optgroup label=\"{item.Group.Name}\"{disabledAttr}>");

                foreach (var groupItem in Items.Where(x => ReferenceEquals(x.Group, item.Group)))
                {
                    optionsBuilder.AppendLine(RenderOption(groupItem, IsItemSelected(groupItem)));
                }

                optionsBuilder.AppendLine("</optgroup>");
            }
        }

        var childContent = (await output.GetChildContentAsync()).GetContent();
        output.Content.SetHtmlContent(childContent + optionsBuilder.ToString());
    }


    private static string RenderOption(SelectListItem item, bool isSelected)
    {
        return $"<option value=\"{item.Value}\" {(isSelected ? "selected" : "")} {(item.Disabled ? "disabled" : "")} >{item.Text}</option>";
    }
}
