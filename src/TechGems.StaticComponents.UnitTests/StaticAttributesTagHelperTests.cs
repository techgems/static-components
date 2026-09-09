using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using TechGems.StaticComponents.Headless;
using TechGems.StaticComponents.Headless.TagHelpers;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticAttributesTagHelperTests
{
    // ============================================================
    //  Helpers
    //
    //  A StaticAttributeCollection is only ever built by capturing a tag helper's output
    //  attributes, so the tests build one the same way the runtime does.
    //
    //  Attribute values arrive at a tag helper as HtmlString holding already encoded markup,
    //  so the fakes mirror that rather than using raw strings, except where a test is
    //  specifically covering the plain-string path.
    // ============================================================

    private static StaticAttributeCollection Capture(params TagHelperAttribute[] attributes)
    {
        return StaticAttributeCollection.Capture(new TagHelperAttributeList(attributes));
    }

    private static TagHelperAttribute Attr(string name, string encodedValue)
    {
        return new TagHelperAttribute(name, new HtmlString(encodedValue));
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
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()))
        {
            TagMode = TagMode.SelfClosing
        };
    }

    private static string? GetAttr(TagHelperOutput output, string name)
    {
        output.Attributes.TryGetAttribute(name, out var attr);
        return attr?.Value?.ToString();
    }

    private static TagHelperOutput Run(StaticAttributeCollection? attributes, TagHelperAttributeList? authored = null)
    {
        var output = CreateTagHelperOutput(authored);
        new StaticAttributesTagHelper { Attributes = attributes }.Process(CreateTagHelperContext(), output);
        return output;
    }

    private static string Render(TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    // ============================================================
    //  Capture
    // ============================================================

    [Test]
    public void Capture_KeepsUnboundAttributesInSourceOrder()
    {
        var collection = Capture(
            Attr("placeholder", "Email"),
            Attr("data-testid", "email"),
            Attr("autocomplete", "off"));

        Assert.That(collection.Count, Is.EqualTo(3));
        Assert.That(collection.Select(x => x.Name), Is.EqualTo(new[] { "placeholder", "data-testid", "autocomplete" }));
    }

    [Test]
    public void Capture_DropsTypeBecauseStaticForOwnsIt()
    {
        var collection = Capture(Attr("type", "password"), Attr("placeholder", "Secret"));

        Assert.That(collection.Contains("type"), Is.False);
        Assert.That(collection.Count, Is.EqualTo(1));
    }

    [Test]
    public void Capture_DropsTypeRegardlessOfCasing()
    {
        var collection = Capture(Attr("TYPE", "password"));

        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void Capture_PreservesValueStyleForMinimizedAttributes()
    {
        var collection = Capture(new TagHelperAttribute("required", new HtmlString(""), HtmlAttributeValueStyle.Minimized));

        Assert.That(collection["required"]!.ValueStyle, Is.EqualTo(HtmlAttributeValueStyle.Minimized));
    }

    [Test]
    public void Capture_WithNoAttributesReturnsEmpty()
    {
        Assert.That(Capture(), Is.SameAs(StaticAttributeCollection.Empty));
    }

    // ============================================================
    //  Collection access and filtering
    // ============================================================

    [Test]
    public void NameIndexerAndContains_AreCaseInsensitive()
    {
        var collection = Capture(Attr("data-testid", "email"));

        Assert.That(collection["DATA-TESTID"], Is.Not.Null);
        Assert.That(collection.Contains("Data-TestId"), Is.True);
        Assert.That(collection["missing"], Is.Null);
        Assert.That(collection.Contains("missing"), Is.False);
    }

    [Test]
    public void Only_KeepsNamedAttributes()
    {
        var collection = Capture(Attr("class", "mt-4"), Attr("placeholder", "Email"), Attr("style", "color: red"));

        var filtered = collection.Only("class", "style");

        Assert.That(filtered.Select(x => x.Name), Is.EqualTo(new[] { "class", "style" }));
    }

    [Test]
    public void Except_DropsNamedAttributes()
    {
        var collection = Capture(Attr("class", "mt-4"), Attr("placeholder", "Email"));

        var filtered = collection.Except("class");

        Assert.That(filtered.Select(x => x.Name), Is.EqualTo(new[] { "placeholder" }));
    }

    [Test]
    public void WithPrefix_KeepsMatchingAttributes()
    {
        var collection = Capture(Attr("x-model", "email"), Attr("hx-get", "/search"), Attr("placeholder", "Email"));

        var filtered = collection.WithPrefix("x-", "hx-");

        Assert.That(filtered.Select(x => x.Name), Is.EqualTo(new[] { "x-model", "hx-get" }));
    }

    [Test]
    public void WithoutPrefix_DropsMatchingAttributes()
    {
        var collection = Capture(Attr("x-model", "email"), Attr("hx-get", "/search"), Attr("placeholder", "Email"));

        var filtered = collection.WithoutPrefix("x-", "hx-");

        Assert.That(filtered.Select(x => x.Name), Is.EqualTo(new[] { "placeholder" }));
    }

    [Test]
    public void Filters_DoNotMutateTheSourceCollection()
    {
        var collection = Capture(Attr("class", "mt-4"), Attr("placeholder", "Email"));

        collection.Except("class");

        Assert.That(collection.Count, Is.EqualTo(2));
    }

    [Test]
    public void Filters_CanBeChained()
    {
        var collection = Capture(Attr("x-model", "email"), Attr("class", "mt-4"), Attr("placeholder", "Email"));

        var filtered = collection.WithoutPrefix("x-").Except("class");

        Assert.That(filtered.Select(x => x.Name), Is.EqualTo(new[] { "placeholder" }));
    }

    [Test]
    public void Only_WithNoNamesReturnsEmpty()
    {
        var collection = Capture(Attr("placeholder", "Email"));

        Assert.That(collection.Only().Count, Is.EqualTo(0));
    }

    [Test]
    public void Except_WithNoNamesKeepsEverything()
    {
        var collection = Capture(Attr("placeholder", "Email"));

        Assert.That(collection.Except().Count, Is.EqualTo(1));
    }

    // ============================================================
    //  Applying attributes
    // ============================================================

    [Test]
    public void Process_AppliesArbitraryAttributes()
    {
        var output = Run(Capture(Attr("data-testid", "email"), Attr("autocomplete", "off")));

        Assert.That(GetAttr(output, "data-testid"), Is.EqualTo("email"));
        Assert.That(GetAttr(output, "autocomplete"), Is.EqualTo("off"));
    }

    [Test]
    public void Process_ConsumerAttributeOverridesTemplateAuthoredValue()
    {
        var authored = new TagHelperAttributeList { Attr("placeholder", "template default") };

        var output = Run(Capture(Attr("placeholder", "consumer wins")), authored);

        Assert.That(GetAttr(output, "placeholder"), Is.EqualTo("consumer wins"));
        Assert.That(output.Attributes.Count(x => x.Name == "placeholder"), Is.EqualTo(1));
    }

    [Test]
    public void Process_PreservesMinimizedValueStyle()
    {
        var attributes = Capture(new TagHelperAttribute("required", new HtmlString(""), HtmlAttributeValueStyle.Minimized));

        var output = Run(attributes);

        Assert.That(Render(output), Does.Contain(" required"));
        Assert.That(Render(output), Does.Not.Contain(@"required="""));
    }

    [Test]
    public void Process_WithNullCollectionIsANoOp()
    {
        var authored = new TagHelperAttributeList { Attr("class", "border") };

        var output = Run(null, authored);

        Assert.That(GetAttr(output, "class"), Is.EqualTo("border"));
        Assert.That(output.Attributes.Count, Is.EqualTo(1));
    }

    [Test]
    public void Process_WithEmptyCollectionIsANoOp()
    {
        var authored = new TagHelperAttributeList { Attr("class", "border") };

        var output = Run(StaticAttributeCollection.Empty, authored);

        Assert.That(GetAttr(output, "class"), Is.EqualTo("border"));
        Assert.That(output.Attributes.Count, Is.EqualTo(1));
    }

    [Test]
    public void Order_RunsBeforeTheStaticForTagHelpers()
    {
        //static-for tag helpers use the default order of 0, so the attributes they own must win.
        Assert.That(new StaticAttributesTagHelper().Order, Is.LessThan(0));
    }

    // ============================================================
    //  class merging
    // ============================================================

    [Test]
    public void Process_AppendsClassesAfterTheAuthoredOnes()
    {
        var authored = new TagHelperAttributeList { Attr("class", "border rounded px-3") };

        var output = Run(Capture(Attr("class", "mt-4")), authored);

        Assert.That(GetAttr(output, "class"), Is.EqualTo("border rounded px-3 mt-4"));
    }

    [Test]
    public void Process_DropsDuplicateClasses()
    {
        var authored = new TagHelperAttributeList { Attr("class", "border rounded") };

        var output = Run(Capture(Attr("class", "mt-4 border")), authored);

        Assert.That(GetAttr(output, "class"), Is.EqualTo("border rounded mt-4"));
    }

    [Test]
    public void Process_ClassDeduplicationIsCaseSensitive()
    {
        //CSS class names are case sensitive, so "Border" and "border" are different classes.
        var authored = new TagHelperAttributeList { Attr("class", "border") };

        var output = Run(Capture(Attr("class", "Border")), authored);

        Assert.That(GetAttr(output, "class"), Is.EqualTo("border Border"));
    }

    [Test]
    public void Process_SetsClassWhenTheElementHasNone()
    {
        var output = Run(Capture(Attr("class", "mt-4")));

        Assert.That(GetAttr(output, "class"), Is.EqualTo("mt-4"));
    }

    [Test]
    public void Process_CollapsesExtraWhitespaceBetweenClasses()
    {
        var authored = new TagHelperAttributeList { Attr("class", "  border   rounded ") };

        var output = Run(Capture(Attr("class", "mt-4")), authored);

        Assert.That(GetAttr(output, "class"), Is.EqualTo("border rounded mt-4"));
    }

    // ============================================================
    //  class-replace
    // ============================================================

    [Test]
    public void Process_ClassReplaceDiscardsTheAuthoredClasses()
    {
        var authored = new TagHelperAttributeList { Attr("class", "border rounded px-3") };

        var output = Run(Capture(Attr("class-replace", "w-32")), authored);

        Assert.That(GetAttr(output, "class"), Is.EqualTo("w-32"));
    }

    [Test]
    public void Process_ClassReplaceDoesNotLeakItsOwnAttribute()
    {
        var output = Run(Capture(Attr("class-replace", "w-32")));

        Assert.That(output.Attributes.ContainsName("class-replace"), Is.False);
    }

    [Test]
    public void Process_ClassAndClassReplaceTogetherThrows()
    {
        var attributes = Capture(Attr("class", "mt-4"), Attr("class-replace", "w-32"));

        var exception = Assert.Throws<ArgumentException>(() => Run(attributes));

        Assert.That(exception!.Message, Does.Contain("class-replace"));
    }

    // ============================================================
    //  style merging
    // ============================================================

    [Test]
    public void Process_AppendsStyleAfterTheAuthoredDeclarations()
    {
        var authored = new TagHelperAttributeList { Attr("style", "color: black") };

        var output = Run(Capture(Attr("style", "color: red")), authored);

        //Inline styles resolve last declaration wins, so the consumer's colour takes effect.
        Assert.That(GetAttr(output, "style"), Is.EqualTo("color: black; color: red"));
    }

    [Test]
    public void Process_NormalisesTrailingSemicolonsWhenMergingStyles()
    {
        var authored = new TagHelperAttributeList { Attr("style", "color: black;") };

        var output = Run(Capture(Attr("style", "margin: 0;")), authored);

        Assert.That(GetAttr(output, "style"), Is.EqualTo("color: black; margin: 0"));
    }

    [Test]
    public void Process_SetsStyleWhenTheElementHasNone()
    {
        var output = Run(Capture(Attr("style", "color: red")));

        Assert.That(GetAttr(output, "style"), Is.EqualTo("color: red"));
    }

    // ============================================================
    //  Encoding
    // ============================================================

    [Test]
    public void Process_DoesNotDoubleEncodeMergedValues()
    {
        //Attribute values reach a tag helper already encoded, so re-encoding would corrupt them.
        var authored = new TagHelperAttributeList { Attr("style", "color: black") };

        var output = Run(Capture(Attr("style", "background: url(/img?w=1&amp;h=2)")), authored);

        Assert.That(Render(output), Does.Contain("background: url(/img?w=1&amp;h=2)"));
        Assert.That(Render(output), Does.Not.Contain("&amp;amp;"));
    }

    [Test]
    public void Process_EncodesPlainStringValuesWhenMerging()
    {
        //A value assigned from C# rather than markup arrives unencoded and still needs encoding.
        var attributes = Capture(new TagHelperAttribute("style", "background: url(/img?w=1&h=2)"));

        var output = Run(attributes);

        Assert.That(Render(output), Does.Contain("background: url(/img?w=1&amp;h=2)"));
    }

    [Test]
    public void Process_DoesNotDoubleEncodeNonMergedValues()
    {
        var output = Run(Capture(Attr("data-query", "a &amp; b")));

        Assert.That(Render(output), Does.Contain(@"data-query=""a &amp; b"""));
        Assert.That(Render(output), Does.Not.Contain("&amp;amp;"));
    }
}
