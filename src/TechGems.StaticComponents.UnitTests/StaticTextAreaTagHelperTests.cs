using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using TechGems.StaticComponents.Headless.TagHelpers;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticTextAreaTagHelperTests
{
    // StaticTextAreaTagHelper:
    //   - only accepts string as the inner model type (everything else throws)
    //   - sets name/id via StaticHeadlessUtils.SetGeneralInputAttributes (so name preserves
    //     the dotted path and id sanitizes it, plus the data-val-* attributes are emitted)
    //   - sets the value attribute via StaticHeadlessUtils.SetValueAttribute (the value
    //     attribute on a <textarea> isn't standard HTML — this just mirrors what the helper
    //     currently does so we can lock in the contract)

    private class InnerString { public string Body { get; set; } = string.Empty; }
    private class InnerStringWithRequired { [Required] public string Body { get; set; } = string.Empty; }
    private class InnerStringWithLength
    {
        [StringLength(maximumLength: 500, MinimumLength = 10)]
        public string Body { get; set; } = string.Empty;
    }
    private class InnerInt { public int Count { get; set; } }
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
            tagName: "textarea",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private static TagHelperOutput CreateTagHelperOutput(TagHelperAttributeList? initialAttributes = null)
    {
        return new TagHelperOutput(
            "textarea",
            initialAttributes ?? new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }

    private static string? GetAttr(TagHelperOutput output, string name)
    {
        output.Attributes.TryGetAttribute(name, out var attr);
        return attr?.Value?.ToString();
    }

    // ============================================================
    //  name / id / value
    // ============================================================

    [Test]
    public void Process_SetsNameAndIdFromInnerExpression()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Body),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "name"), Is.EqualTo("Body"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("Body"));
    }

    [Test]
    public void Process_PreservesDotsInName_SanitizesDotsInId()
    {
        var provider = CreateMetadataProvider();
        var containerExplorer = provider.GetModelExplorerForType(typeof(InnerString), new InnerString());
        var innerExplorer = containerExplorer.GetExplorerForProperty(nameof(InnerString.Body));
        var inner = new ModelExpression("Post.Author.Bio", innerExplorer);
        var outerContainer = new OuterPlain { Field = inner };
        var outer = BuildExpression(provider, outerContainer, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "name"), Is.EqualTo("Post.Author.Bio"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("Post_Author_Bio"));
    }

    [Test]
    public void Process_WithModelValue_SetsValueAttribute()
    {
        // The helper sets a value attribute (via StaticHeadlessUtils.SetValueAttribute) even
        // though that isn't a standard textarea attribute. We assert the documented contract.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString { Body = "hello there" }, nameof(InnerString.Body),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "value"), Is.EqualTo("hello there"));
    }

    [Test]
    public void Process_WithExplicitValue_PreservesExplicitValue()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString { Body = "model-body" }, nameof(InnerString.Body),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput(new TagHelperAttributeList { { "value", "explicit-body" } });

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "value"), Is.EqualTo("explicit-body"));
    }

    // ============================================================
    //  Type validation
    // ============================================================

    [Test]
    public void Process_WithStringInnerModel_DoesNotThrow()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Body),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output), Throws.Nothing);
    }

    [Test]
    public void Process_WithIntInnerModel_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerInt(), nameof(InnerInt.Count),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Process_WithBoolInnerModel_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBool(), nameof(InnerBool.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
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

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
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
            new InnerStringWithRequired(), nameof(InnerStringWithRequired.Body),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val"), Is.EqualTo("true"));
        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("The Body field is required."));
    }

    [Test]
    public void Process_WithStringLengthAttribute_SetsDataValLengthAttributes()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithLength(), nameof(InnerStringWithLength.Body),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticTextAreaTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val-length-min"), Is.EqualTo("10"));
        Assert.That(GetAttr(output, "data-val-length-max"), Is.EqualTo("500"));
        Assert.That(GetAttr(output, "maxlength"), Is.EqualTo("500"));
        Assert.That(GetAttr(output, "data-val-length"),
            Is.EqualTo("The field Body must be between 10 and 500 characters long."));
    }
}
