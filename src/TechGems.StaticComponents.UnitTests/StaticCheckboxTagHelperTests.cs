using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechGems.StaticComponents.Headless.TagHelpers;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticCheckboxTagHelperTests
{
    // StaticCheckboxTagHelper expects a bool inner property and emits:
    //   - type="checkbox", value="true"
    //   - checked="checked" when the model value is true
    //   - a hidden post-element with the same name and value="false" so unchecked posts a value
    //   - name/id from the inner expression
    //   - data-val-* attributes from the validation attributes on the inner property
    //
    // It uses the same dual-layer ModelExpression pattern as StaticInputTagHelper:
    // the outer expression is a ModelExpression-typed property whose .Model is itself
    // a ModelExpression bound to the actual bool property.

    private class InnerBool { public bool Flag { get; set; } }
    private class InnerBoolTrueDefault { public bool Flag { get; set; } = true; }
    private class InnerBoolWithRequired { [Required] public bool Flag { get; set; } }
    private class InnerBoolWithRequiredAndDisplayName
    {
        [Required(ErrorMessage = "You must agree."), DisplayName("Terms")]
        public bool Accepted { get; set; }
    }
    private class InnerInt { public int Count { get; set; } }
    private class InnerString { public string Name { get; set; } = string.Empty; }

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
    //  Core behavior: type / value / checked / hidden post-element
    // ============================================================

    [Test]
    public void Process_AlwaysSetsTypeCheckboxAndValueTrue()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBool(), nameof(InnerBool.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("checkbox"));
        Assert.That(GetAttr(output, "value"), Is.EqualTo("true"));
    }

    [Test]
    public void Process_WithModelTrue_SetsCheckedAttribute()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBoolTrueDefault(), nameof(InnerBoolTrueDefault.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "checked"), Is.EqualTo("checked"));
    }

    [Test]
    public void Process_WithModelFalse_DoesNotSetCheckedAttribute()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBool(), nameof(InnerBool.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(output.Attributes.ContainsName("checked"), Is.False);
    }

    [Test]
    public void Process_WithExplicitChecked_PreservesExplicitChecked()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBool(), nameof(InnerBool.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        // even though the model is false, an explicit checked attribute should win
        var output = CreateTagHelperOutput(new TagHelperAttributeList { { "checked", "checked" } });

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "checked"), Is.EqualTo("checked"));
    }

    [Test]
    public void Process_AlwaysAppendsHiddenFalsePostElementWithMatchingName()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBool(), nameof(InnerBool.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        var postContent = output.PostElement.GetContent();
        Assert.That(postContent, Does.Contain(@"type=""hidden"""));
        Assert.That(postContent, Does.Contain(@"name=""Flag"""));
        Assert.That(postContent, Does.Contain(@"value=""false"""));
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
            new InnerBool(), nameof(InnerBool.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "name"), Is.EqualTo("Flag"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("Flag"));
    }

    [Test]
    public void Process_SanitizesDotsInExpressionNameForId()
    {
        // Use a container-derived ModelExplorer (so PropertyAttributes is non-null inside
        // SetValidationAttributes), then construct a ModelExpression with that explorer's
        // metadata but a dotted Name to exercise sanitization.
        var provider = CreateMetadataProvider();
        var containerExplorer = provider.GetModelExplorerForType(typeof(InnerBool), new InnerBool());
        var innerExplorer = containerExplorer.GetExplorerForProperty(nameof(InnerBool.Flag));
        var inner = new ModelExpression("User.Preferences.OptIn", innerExplorer);
        var outerContainer = new OuterPlain { Field = inner };
        var outer = BuildExpression(provider, outerContainer, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        // name preserves the dotted path; id sanitizes dots — matching asp-for.
        Assert.That(GetAttr(output, "name"), Is.EqualTo("User.Preferences.OptIn"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("User_Preferences_OptIn"));
    }

    // ============================================================
    //  Unsupported model types
    // ============================================================

    [Test]
    public void Process_WithIntInnerModel_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerInt(), nameof(InnerInt.Count),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Process_WithStringInnerModel_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
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
            new InnerBoolWithRequired(), nameof(InnerBoolWithRequired.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val"), Is.EqualTo("true"));
        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("The Flag field is required."));
    }

    [Test]
    public void Process_WithRequiredCustomMessageAndDisplayName_UsesCustomMessage()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBoolWithRequiredAndDisplayName(),
            nameof(InnerBoolWithRequiredAndDisplayName.Accepted),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticCheckboxTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val"), Is.EqualTo("true"));
        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("You must agree."));
    }
}
