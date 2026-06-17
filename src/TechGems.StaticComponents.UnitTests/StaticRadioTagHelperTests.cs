using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechGems.StaticComponents.Headless.TagHelpers;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticRadioTagHelperTests
{
    // StaticRadioTagHelper requires an explicit Value (the option this radio represents)
    // and emits:
    //   - type="radio"
    //   - value from the Value property (formatted via FormatModelValue)
    //   - checked="checked" when the model value matches Value (compared as strings)
    //   - name/id from the inner expression
    //   - data-val-* attributes from the validation attributes on the inner property
    //
    // Like the other static-* helpers it uses a dual-layer ModelExpression: the outer
    // expression wraps an inner ModelExpression bound to the actual scalar property.

    private class InnerString { public string Choice { get; set; } = string.Empty; }
    private class InnerStringWithValue { public string Choice { get; set; } = "option1"; }
    private class InnerStringWithRequired { [Required] public string Choice { get; set; } = string.Empty; }
    private class InnerStringWithDisplay
    {
        [Required, DisplayName("Plan")]
        public string Selection { get; set; } = string.Empty;
    }
    private class InnerInt { public int Number { get; set; } = 2; }
    private class InnerDate { public DateTime When { get; set; } }
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
            tagName: "input",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private static TagHelperOutput CreateTagHelperOutput(TagHelperAttributeList? initialAttributes = null)
    {
        return new TagHelperOutput(
            "input",
            initialAttributes ?? new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }

    private static string? GetAttr(TagHelperOutput output, string name)
    {
        output.Attributes.TryGetAttribute(name, out var attr);
        return attr?.Value?.ToString();
    }

    // ============================================================
    //  Core behavior: type / value
    // ============================================================

    [Test]
    public void Process_AlwaysSetsTypeRadio()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "anything" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("radio"));
    }

    [Test]
    public void Process_SetsValueAttributeFromValueProperty()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "option-a" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "value"), Is.EqualTo("option-a"));
    }

    // ============================================================
    //  checked logic
    // ============================================================

    [Test]
    public void Process_WhenValueMatchesModel_SetsCheckedAttribute()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithValue(), nameof(InnerStringWithValue.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "option1" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "checked"), Is.EqualTo("checked"));
    }

    [Test]
    public void Process_WhenValueDoesNotMatchModel_DoesNotSetCheckedAttribute()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithValue(), nameof(InnerStringWithValue.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "option2" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(output.Attributes.ContainsName("checked"), Is.False);
    }

    [Test]
    public void Process_WhenIntValueMatchesModel_SetsCheckedAttribute()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerInt(), nameof(InnerInt.Number),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "2" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "checked"), Is.EqualTo("checked"));
    }

    [Test]
    public void Process_WithExplicitChecked_PreservesExplicitChecked()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithValue(), nameof(InnerStringWithValue.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        // Value does not match model — but explicit checked should still survive.
        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "option2" };
        var output = CreateTagHelperOutput(new TagHelperAttributeList { { "checked", "checked" } });

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "checked"), Is.EqualTo("checked"));
    }

    // ============================================================
    //  name / id
    // ============================================================

    [Test]
    public void Process_SetsNameAndIdFromInnerExpression()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "option1" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "name"), Is.EqualTo("Choice"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("Choice"));
    }

    // ============================================================
    //  Exception cases
    // ============================================================

    [Test]
    public void Process_WithoutValue_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
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

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "x" };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Process_WithDateTimeInnerModel_ThrowsArgumentException()
    {
        // DateTime is supported by StaticInputTagHelper but not by StaticRadioTagHelper —
        // radios only support the discrete scalar types listed in SupportedModelTypes.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerDate(), nameof(InnerDate.When),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "2024-01-01" };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    // ============================================================
    //  data-val attribute generation (shared purpose with asp-for)
    // ============================================================

    [Test]
    public void Process_WithRequiredAttribute_SetsDataValRequired()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithRequired(), nameof(InnerStringWithRequired.Choice),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "option1" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val"), Is.EqualTo("true"));
        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("The Choice field is required."));
    }

    [Test]
    public void Process_WithDisplayName_UsesDisplayNameInDefaultRequiredMessage()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithDisplay(), nameof(InnerStringWithDisplay.Selection),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticRadioTagHelper { InputExpression = outer, Value = "basic" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("The Plan field is required."));
    }
}
