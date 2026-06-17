using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using TechGems.StaticComponents.Headless.TagHelpers;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticSelectTagHelperTests
{
    // StaticSelectTagHelper responsibilities:
    //   - validate the inner model type (string/Guid/numerics/DateTime + nullables; not bool/collections)
    //   - emit name/id and data-val-* attributes via StaticHeadlessUtils
    //   - render SelectListItems into <option> tags, grouping items that share a
    //     SelectListGroup reference into a single <optgroup> placed at the position of
    //     the group's first occurrence
    //   - selection priority:
    //       1. if the bound model value matches an item Value, that item is "selected"
    //          and SelectListItem.Selected flags on all items are ignored
    //       2. otherwise (null model or no match) fall back to SelectListItem.Selected
    //   - preserve any child content authored inside the <select>

    private class InnerString { public string Choice { get; set; } = string.Empty; }
    private class InnerStringWithValue { public string Choice { get; set; } = "3"; }
    private class InnerNullableString { public string? Choice { get; set; } }
    private class InnerStringWithRequired { [Required] public string Choice { get; set; } = string.Empty; }
    private class InnerIntWithValue { public int Choice { get; set; } = 7; }
    private class InnerNullableInt { public int? Choice { get; set; } }
    private class InnerGuid { public Guid Choice { get; set; } = Guid.Empty; }
    private class InnerDate { public DateTime Choice { get; set; } = new DateTime(2024, 5, 4); }
    private class InnerBool { public bool Flag { get; set; } }
    private class InnerUnsupported { public List<int> Items { get; set; } = new(); }

    private class OuterPlain
    {
        public ModelExpression Field { get; set; } = default!;
    }

    private static IModelMetadataProvider CreateMetadataProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore().AddDataAnnotations();
        return services.BuildServiceProvider().GetRequiredService<IModelMetadataProvider>();
    }

    private static ModelExpression BuildExpression<TContainer>(
        IModelMetadataProvider provider,
        TContainer container,
        string propertyName)
    {
        var containerExplorer = provider.GetModelExplorerForType(typeof(TContainer), container);
        var propertyExplorer = containerExplorer.GetExplorerForProperty(propertyName);
        return new ModelExpression(propertyName, propertyExplorer);
    }

    private static ModelExpression BuildOuterExpression<TInner, TOuter>(
        IModelMetadataProvider provider,
        TInner innerContainer,
        string innerPropertyName,
        Func<ModelExpression, TOuter> outerFactory,
        string outerPropertyName)
    {
        var inner = BuildExpression(provider, innerContainer, innerPropertyName);
        var outerContainer = outerFactory(inner);
        return BuildExpression(provider, outerContainer, outerPropertyName);
    }

    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            tagName: "select",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a TagHelperOutput whose GetChildContentAsync returns the supplied html string
    /// so we can verify the helper preserves caller-authored child content.
    /// </summary>
    private static TagHelperOutput CreateTagHelperOutput(string childHtml = "")
    {
        return new TagHelperOutput(
            "select",
            new TagHelperAttributeList(),
            (_, _) =>
            {
                var content = new DefaultTagHelperContent();
                if (!string.IsNullOrEmpty(childHtml))
                {
                    content.SetHtmlContent(childHtml);
                }
                return Task.FromResult<TagHelperContent>(content);
            });
    }

    private static string? GetAttr(TagHelperOutput output, string name)
    {
        output.Attributes.TryGetAttribute(name, out var attr);
        return attr?.Value?.ToString();
    }

    private static async Task<string> ProcessAndGetContentAsync(StaticSelectTagHelper tagHelper, TagHelperOutput output)
    {
        await tagHelper.ProcessAsync(CreateTagHelperContext(), output);
        return output.Content.GetContent();
    }

    // ============================================================
    //  name / id
    // ============================================================

    [Test]
    public async Task Process_SetsNameAndIdFromInnerExpression()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        await tagHelper.ProcessAsync(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "name"), Is.EqualTo("Choice"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("Choice"));
    }

    [Test]
    public async Task Process_PreservesDotsInName_SanitizesDotsInId()
    {
        var provider = CreateMetadataProvider();
        var containerExplorer = provider.GetModelExplorerForType(typeof(InnerString), new InnerString());
        var innerExplorer = containerExplorer.GetExplorerForProperty(nameof(InnerString.Choice));
        var inner = new ModelExpression("User.Profile.Department", innerExplorer);
        var outerContainer = new OuterPlain { Field = inner };
        var outer = BuildExpression(provider, outerContainer, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        await tagHelper.ProcessAsync(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "name"), Is.EqualTo("User.Profile.Department"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("User_Profile_Department"));
    }

    // ============================================================
    //  Type validation
    // ============================================================

    [Test]
    public void Process_WithBoolInnerModel_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBool(), nameof(InnerBool.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        Assert.That(async () => await tagHelper.ProcessAsync(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Process_WithUnsupportedInnerModelType_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerUnsupported(), nameof(InnerUnsupported.Items),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        Assert.That(async () => await tagHelper.ProcessAsync(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    // ============================================================
    //  Ungrouped option rendering
    // ============================================================

    [Test]
    public async Task Process_WithUngroupedItems_RendersOptionTagsInOrder()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "1", Text = "Sales" },
                new() { Value = "2", Text = "Marketing" }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<option value=\"1\""));
        Assert.That(content, Does.Contain(">Sales</option>"));
        Assert.That(content, Does.Contain("<option value=\"2\""));
        Assert.That(content, Does.Contain(">Marketing</option>"));
        // No optgroup wrapping for ungrouped items.
        Assert.That(content, Does.Not.Contain("<optgroup"));
        // Order is preserved: Sales appears before Marketing.
        Assert.That(content.IndexOf(">Sales<"), Is.LessThan(content.IndexOf(">Marketing<")));
    }

    [Test]
    public async Task Process_WithDisabledItem_RendersDisabledAttribute()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "1", Text = "Sales", Disabled = true }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("disabled"));
    }

    [Test]
    public async Task Process_WithNoItemsAndNoChildContent_EmitsNoOptions()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Not.Contain("<option"));
        Assert.That(content, Does.Not.Contain("<optgroup"));
    }

    [Test]
    public async Task Process_PreservesAuthoredChildContent()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "1", Text = "Sales" }
            }
        };
        // simulate <select static-for="..." static-items="..."><option>-- pick one --</option></select>
        var output = CreateTagHelperOutput(childHtml: "<option value=\"\">-- pick one --</option>");

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("-- pick one --"));
        Assert.That(content, Does.Contain(">Sales</option>"));
    }

    // ============================================================
    //  Grouped option rendering
    // ============================================================

    [Test]
    public async Task Process_WithGroupedItems_RendersOptgroupWithItems()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var creative = new SelectListGroup { Name = "Creative" };
        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "3", Text = "UI/UX Designer", Group = creative },
                new() { Value = "4", Text = "Graphic Designer", Group = creative }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<optgroup label=\"Creative\""));
        Assert.That(content, Does.Contain("</optgroup>"));
        Assert.That(content, Does.Contain(">UI/UX Designer</option>"));
        Assert.That(content, Does.Contain(">Graphic Designer</option>"));

        // Items must appear inside the optgroup (i.e. between <optgroup ...> and </optgroup>).
        var openIdx = content.IndexOf("<optgroup");
        var closeIdx = content.IndexOf("</optgroup>");
        Assert.That(openIdx, Is.GreaterThanOrEqualTo(0));
        Assert.That(closeIdx, Is.GreaterThan(openIdx));
        var inside = content.Substring(openIdx, closeIdx - openIdx);
        Assert.That(inside, Does.Contain(">UI/UX Designer</option>"));
        Assert.That(inside, Does.Contain(">Graphic Designer</option>"));
    }

    [Test]
    public async Task Process_WithSingleGroupReferenceUsedTwice_EmitsSingleOptgroup()
    {
        // Even if the same group reference appears later in the list (interleaved with
        // ungrouped items), the helper should not emit a duplicate <optgroup>.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var creative = new SelectListGroup { Name = "Creative" };
        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "3", Text = "UI/UX Designer", Group = creative },
                new() { Value = "1", Text = "Sales" },
                new() { Value = "4", Text = "Graphic Designer", Group = creative }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        // Only one optgroup open + close pair.
        Assert.That(CountOccurrences(content, "<optgroup"), Is.EqualTo(1));
        Assert.That(CountOccurrences(content, "</optgroup>"), Is.EqualTo(1));
        // Both grouped items end up inside the single optgroup block.
        var openIdx = content.IndexOf("<optgroup");
        var closeIdx = content.IndexOf("</optgroup>");
        var inside = content.Substring(openIdx, closeIdx - openIdx);
        Assert.That(inside, Does.Contain(">UI/UX Designer</option>"));
        Assert.That(inside, Does.Contain(">Graphic Designer</option>"));
    }

    [Test]
    public async Task Process_WithMultipleDistinctGroups_RendersOneOptgroupPerGroup()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var creative = new SelectListGroup { Name = "Creative" };
        var engineering = new SelectListGroup { Name = "Engineering" };
        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "3", Text = "UI/UX Designer", Group = creative },
                new() { Value = "5", Text = "Backend Engineer", Group = engineering },
                new() { Value = "4", Text = "Graphic Designer", Group = creative },
                new() { Value = "6", Text = "Frontend Engineer", Group = engineering }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<optgroup label=\"Creative\""));
        Assert.That(content, Does.Contain("<optgroup label=\"Engineering\""));
        Assert.That(CountOccurrences(content, "<optgroup"), Is.EqualTo(2));
        // Creative came first in the list, so its optgroup appears before Engineering's.
        Assert.That(content.IndexOf("<optgroup label=\"Creative\""),
            Is.LessThan(content.IndexOf("<optgroup label=\"Engineering\"")));
    }

    [Test]
    public async Task Process_WithTwoGroupsSameNameDifferentReferences_RendersTwoOptgroups()
    {
        // Reference equality is used to identify groups (matching MVC SelectTagHelper).
        // Two SelectListGroup instances with the same Name are NOT collapsed.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var groupA = new SelectListGroup { Name = "Creative" };
        var groupB = new SelectListGroup { Name = "Creative" };
        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "3", Text = "UI/UX Designer", Group = groupA },
                new() { Value = "4", Text = "Graphic Designer", Group = groupB }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(CountOccurrences(content, "<optgroup label=\"Creative\""), Is.EqualTo(2));
    }

    [Test]
    public async Task Process_WithDisabledGroup_RendersDisabledOnOptgroup()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var creative = new SelectListGroup { Name = "Creative", Disabled = true };
        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "3", Text = "UI/UX Designer", Group = creative }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<optgroup label=\"Creative\" disabled"));
    }

    [Test]
    public async Task Process_WithMixedGroupedAndUngrouped_RendersBoth()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var creative = new SelectListGroup { Name = "Creative" };
        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "1", Text = "Sales" },
                new() { Value = "3", Text = "UI/UX Designer", Group = creative },
                new() { Value = "2", Text = "Marketing" }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain(">Sales</option>"));
        Assert.That(content, Does.Contain(">Marketing</option>"));
        Assert.That(content, Does.Contain("<optgroup label=\"Creative\""));
        Assert.That(content, Does.Contain(">UI/UX Designer</option>"));
        // The Sales <option> precedes the Creative optgroup (its position in the list).
        Assert.That(content.IndexOf(">Sales<"),
            Is.LessThan(content.IndexOf("<optgroup label=\"Creative\"")));
    }

    // ============================================================
    //  Selection priority: model value > SelectListItem.Selected
    // ============================================================

    [Test]
    public async Task Process_WhenModelValueMatchesAnItem_MarksThatItemSelectedAndIgnoresItemFlags()
    {
        // Model.Choice == "3"; item 4 has Selected=true. The model value must win and
        // the Selected=true on item 4 must be ignored.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithValue(), nameof(InnerStringWithValue.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "3", Text = "UI/UX Designer" },
                new() { Value = "4", Text = "Graphic Designer", Selected = true }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<option value=\"3\" selected"));
        // Item 4 must not be selected, even though SelectListItem.Selected was true.
        var optionFour = content.Substring(content.IndexOf("<option value=\"4\""),
            content.IndexOf("</option>", content.IndexOf("<option value=\"4\"")) - content.IndexOf("<option value=\"4\""));
        Assert.That(optionFour, Does.Not.Contain("selected"));
    }

    [Test]
    public async Task Process_WhenModelValueIsNull_FallsBackToSelectListItemSelected()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "3", Text = "UI/UX Designer" },
                new() { Value = "4", Text = "Graphic Designer", Selected = true }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<option value=\"4\" selected"));
        var optionThree = content.Substring(content.IndexOf("<option value=\"3\""),
            content.IndexOf("</option>", content.IndexOf("<option value=\"3\"")) - content.IndexOf("<option value=\"3\""));
        Assert.That(optionThree, Does.Not.Contain("selected"));
    }

    [Test]
    public async Task Process_WhenModelValueMatchesNoItem_FallsBackToSelectListItemSelected()
    {
        // Model.Choice == "3" but no item has Value="3"; fall back to the Selected flag.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithValue(), nameof(InnerStringWithValue.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "1", Text = "Sales" },
                new() { Value = "2", Text = "Marketing", Selected = true }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<option value=\"2\" selected"));
    }

    [Test]
    public async Task Process_WhenIntModelValueMatchesAnItem_MarksThatItemSelected()
    {
        // The int model value is formatted via FormatModelValue → invariant "7"; matches Value="7".
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerIntWithValue(), nameof(InnerIntWithValue.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "5", Text = "Five", Selected = true },
                new() { Value = "7", Text = "Seven" }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<option value=\"7\" selected"));
        var optionFive = content.Substring(content.IndexOf("<option value=\"5\""),
            content.IndexOf("</option>", content.IndexOf("<option value=\"5\"")) - content.IndexOf("<option value=\"5\""));
        Assert.That(optionFive, Does.Not.Contain("selected"));
    }

    [Test]
    public async Task Process_WhenNullableIntModelIsNull_FallsBackToSelectListItemSelected()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableInt(), nameof(InnerNullableInt.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "5", Text = "Five", Selected = true }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<option value=\"5\" selected"));
    }

    [Test]
    public async Task Process_ModelValueSelectionWorksAcrossOptgroups()
    {
        // Selection by model value also applies inside <optgroup>s.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithValue(), nameof(InnerStringWithValue.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var creative = new SelectListGroup { Name = "Creative" };
        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "3", Text = "UI/UX Designer", Group = creative },
                new() { Value = "4", Text = "Graphic Designer", Group = creative, Selected = true }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<option value=\"3\" selected"));
        var optionFour = content.Substring(content.IndexOf("<option value=\"4\""),
            content.IndexOf("</option>", content.IndexOf("<option value=\"4\"")) - content.IndexOf("<option value=\"4\""));
        Assert.That(optionFour, Does.Not.Contain("selected"));
    }

    [Test]
    public async Task Process_WhenDateTimeModelMatchesItemValue_MarksSelected()
    {
        // FormatModelValue formats DateTime as yyyy-MM-dd, so a SelectListItem with that
        // value should be selected.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerDate(), nameof(InnerDate.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper
        {
            InputExpression = outer,
            Items = new List<SelectListItem>
            {
                new() { Value = "2024-05-04", Text = "Star Wars Day" },
                new() { Value = "2024-12-25", Text = "Christmas" }
            }
        };
        var output = CreateTagHelperOutput();

        var content = await ProcessAndGetContentAsync(tagHelper, output);

        Assert.That(content, Does.Contain("<option value=\"2024-05-04\" selected"));
    }

    // ============================================================
    //  data-val attribute generation
    // ============================================================

    [Test]
    public async Task Process_WithRequiredAttribute_SetsDataValRequired()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithRequired(), nameof(InnerStringWithRequired.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticSelectTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        await tagHelper.ProcessAsync(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val"), Is.EqualTo("true"));
        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("The Choice field is required."));
    }

    // ============================================================
    //  Helpers
    // ============================================================

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var idx = 0;
        while ((idx = haystack.IndexOf(needle, idx, StringComparison.Ordinal)) != -1)
        {
            count++;
            idx += needle.Length;
        }
        return count;
    }
}
