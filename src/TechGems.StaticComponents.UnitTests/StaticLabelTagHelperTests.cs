using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using TechGems.StaticComponents.Headless.TagHelpers;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticLabelTagHelperTests
{
    // StaticLabelTagHelper uses the same dual-layer ModelExpression pattern as the other
    // static-* tag helpers: the outer "static-for" ModelExpression's .ModelExplorer.Model
    // is itself a ModelExpression bound to the actual model property. The helper reads
    // DisplayName / PropertyName from the *inner* (nested) expression's metadata.
    //
    // So in these tests we always build the nested ModelExpression via a real container
    // property (GetExplorerForProperty) — never via GetModelExplorerForType — so that the
    // nested explorer carries the property's metadata (DisplayName, PropertyName, validation
    // attributes, etc.) just like it would at runtime.

    // ---- Inner containers (the actual model properties the label targets) ----

    private class InnerEmailDisplayName
    {
        [DisplayName("Custom Label Text")]
        public string Email { get; set; } = string.Empty;
    }

    private class InnerFirstName
    {
        public string FirstName { get; set; } = string.Empty;
    }

    private class InnerEmailField
    {
        public string EmailField { get; set; } = string.Empty;
    }

    private class InnerPlainName
    {
        public string Name { get; set; } = string.Empty;
    }

    // ---- Outer container exposing a ModelExpression-typed property ----

    private class OuterPlain
    {
        public ModelExpression Field { get; set; } = default!;
    }

    // ---- Infrastructure ----

    private static IModelMetadataProvider CreateMetadataProvider()
    {
        // The concrete IMetadataDetailsProvider implementations needed to honor [DisplayName]
        // are internal to ASP.NET Core. Wiring them through AddMvcCore().AddDataAnnotations()
        // is the supported public path to get a fully configured DefaultModelMetadataProvider.
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

    /// <summary>
    /// Builds the dual-layer ModelExpression the tag helper expects:
    /// an outer expression whose .Model is the nested ModelExpression bound to the real
    /// inner property (so the nested explorer carries DisplayName / PropertyName metadata).
    /// </summary>
    private static ModelExpression BuildOuterExpression<TInner, TOuter>(
        IModelMetadataProvider provider,
        TInner innerContainer,
        string innerPropertyName,
        Func<ModelExpression, TOuter> outerFactory,
        string outerPropertyName)
    {
        var nested = BuildExpression(provider, innerContainer, innerPropertyName);
        var outerContainer = outerFactory(nested);
        return BuildExpression(provider, outerContainer, outerPropertyName);
    }

    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            tagName: "label",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "label",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }

    // ============================================================
    //  Label content (DisplayName / PropertyName)
    // ============================================================

    [Test]
    public void Process_WhenDisplayNameAttributeIsApplied_UsesDisplayNameAsLabelContent()
    {
        // The nested expression points at InnerEmailDisplayName.Email, which carries
        // [DisplayName("Custom Label Text")]. NestedModelExpression.Metadata.DisplayName
        // resolves to "Custom Label Text".
        var provider = CreateMetadataProvider();
        var staticFor = BuildOuterExpression(
            provider,
            new InnerEmailDisplayName(), nameof(InnerEmailDisplayName.Email),
            nested => new OuterPlain { Field = nested }, nameof(OuterPlain.Field));

        var tagHelper = new StaticLabelTagHelper { StaticFor = staticFor };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(output.Content.GetContent(), Is.EqualTo("Custom Label Text"));
    }

    [Test]
    public void Process_WhenNoDisplayNameAttribute_FallsBackToPropertyNameAsLabelContent()
    {
        // Without [DisplayName], the helper falls back to the nested property's name.
        var provider = CreateMetadataProvider();
        var staticFor = BuildOuterExpression(
            provider,
            new InnerFirstName(), nameof(InnerFirstName.FirstName),
            nested => new OuterPlain { Field = nested }, nameof(OuterPlain.Field));

        var tagHelper = new StaticLabelTagHelper { StaticFor = staticFor };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(output.Content.GetContent(), Is.EqualTo("FirstName"));
    }

    // ============================================================
    //  "for" attribute
    // ============================================================

    [Test]
    public void Process_ForAttribute_UsesNestedModelExpressionName()
    {
        var provider = CreateMetadataProvider();
        var staticFor = BuildOuterExpression(
            provider,
            new InnerEmailField(), nameof(InnerEmailField.EmailField),
            nested => new OuterPlain { Field = nested }, nameof(OuterPlain.Field));

        var tagHelper = new StaticLabelTagHelper { StaticFor = staticFor };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(output.Attributes.TryGetAttribute("for", out var forAttribute), Is.True);
        Assert.That(forAttribute!.Value?.ToString(), Is.EqualTo("EmailField"));
    }

    [Test]
    public void Process_ForAttribute_UsesDottedNestedExpressionNameAsIs()
    {
        // The helper currently writes NestedModelExpression.Name to the "for" attribute
        // verbatim — it does NOT run the same dot/bracket sanitization that the input,
        // checkbox and select helpers apply to their id attribute. asp-net's built-in
        // label tag helper does sanitize; this test locks in the current divergent
        // behavior so a future fix that adds sanitization will surface here.
        //
        // We build the nested expression via a real container property so metadata is
        // populated, then override its Name with a dotted property path to exercise the
        // "for" attribute code path.
        var provider = CreateMetadataProvider();
        var containerExplorer = provider.GetModelExplorerForType(typeof(InnerPlainName), new InnerPlainName());
        var innerExplorer = containerExplorer.GetExplorerForProperty(nameof(InnerPlainName.Name));
        var nested = new ModelExpression("User.Address.Street", innerExplorer);
        var outerContainer = new OuterPlain { Field = nested };
        var staticFor = BuildExpression(provider, outerContainer, nameof(OuterPlain.Field));

        var tagHelper = new StaticLabelTagHelper { StaticFor = staticFor };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(output.Attributes.TryGetAttribute("for", out var forAttribute), Is.True);
        Assert.That(forAttribute!.Value?.ToString(), Is.EqualTo("User.Address.Street"));
    }
}
