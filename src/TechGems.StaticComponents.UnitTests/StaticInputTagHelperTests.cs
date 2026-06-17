using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechGems.StaticComponents.Headless.TagHelpers;
using DataAnnotationsRangeAttribute = System.ComponentModel.DataAnnotations.RangeAttribute;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticInputTagHelperTests
{
    // ============================================================
    //  Test model containers
    //
    //  The tag helper uses two nested ModelExpressions:
    //    - "Outer" exposes a ModelExpression-typed property whose metadata is read for
    //      [DataType(...)] and [HiddenInput] inference (Metadata.DataTypeName / TemplateHint).
    //    - "Inner" exposes the actual typed property — string, int, etc. — whose
    //      metadata supplies ModelType (for type inference + supported-type validation),
    //      Name, the current Model value, and the validation attributes that drive the
    //      data-val-* HTML attributes.
    //
    //  Note: bool is no longer a supported model type for StaticInputTagHelper; checkbox
    //  inputs are now handled by StaticCheckboxTagHelper, radio inputs by StaticRadioTagHelper.
    // ============================================================

    private class InnerString { public string Name { get; set; } = string.Empty; }
    private class InnerNullableString { public string? Name { get; set; } }
    private class InnerStringWithPasswordDataType { [DataType(DataType.Password)] public string Name { get; set; } = string.Empty; }
    private class InnerStringWithHiddenInput { [HiddenInput] public string Name { get; set; } = string.Empty; }
    private class InnerStringWithRequired { [Required] public string Name { get; set; } = string.Empty; }
    private class InnerStringWithRequiredCustom { [Required(ErrorMessage = "Name is mandatory.")] public string Name { get; set; } = string.Empty; }
    private class InnerStringWithEmail { [EmailAddress] public string Address { get; set; } = string.Empty; }
    private class InnerStringWithUrl { [Url] public string Site { get; set; } = string.Empty; }
    private class InnerStringWithPhone { [Phone] public string Phone { get; set; } = string.Empty; }
    private class InnerStringWithRegex { [RegularExpression(@"^\d{3}-\d{4}$")] public string Code { get; set; } = string.Empty; }
    private class InnerStringWithLength { [StringLength(maximumLength: 50, MinimumLength = 3)] public string Name { get; set; } = string.Empty; }
    private class InnerStringWithMaxLengthOnly { [StringLength(maximumLength: 100)] public string Name { get; set; } = string.Empty; }
    private class InnerIntWithRange { [DataAnnotationsRangeAttribute(1, 99)] public int Age { get; set; } }
    private class InnerStringWithDisplayName { [DisplayName("Full Name"), Required] public string Name { get; set; } = string.Empty; }
    private class InnerInt { public int Count { get; set; } }
    private class InnerGuid { public Guid Id { get; set; } }
    private class InnerDate { public DateTime When { get; set; } = new DateTime(2024, 1, 15); }
    private class InnerBool { public bool Flag { get; set; } }
    private class InnerUnsupported { public List<int> Items { get; set; } = new(); }

    private class OuterPlain
    {
        public ModelExpression Field { get; set; } = default!;
    }

    // ============================================================
    //  Infrastructure
    // ============================================================

    private static IModelMetadataProvider CreateMetadataProvider()
    {
        // The concrete IMetadataDetailsProvider implementations that honor [Required],
        // [DataType] and [HiddenInput] are internal to ASP.NET Core. Wiring them through
        // AddMvcCore().AddDataAnnotations() is the supported public path to get a fully
        // configured DefaultModelMetadataProvider.
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
    /// Builds the dual-layer ModelExpression that the tag helper expects:
    /// an outer expression for <typeparamref name="TOuter"/> whose .Model is the inner ModelExpression.
    /// </summary>
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
    //  Type inference from inner model type
    // ============================================================

    [Test]
    public void Process_WithStringModel_InfersTypeText()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("text"));
    }

    [Test]
    public void Process_WithIntModel_InfersTypeNumber()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerInt(), nameof(InnerInt.Count),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("number"));
    }

    [Test]
    public void Process_WithGuidModel_InfersTypeText()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerGuid(), nameof(InnerGuid.Id),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("text"));
    }

    [Test]
    public void Process_WithDateTimeModel_InfersTypeDateAndFormatsValue()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerDate(), nameof(InnerDate.When),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("date"));
        Assert.That(GetAttr(output, "value"), Is.EqualTo("2024-01-15"));
    }

    // ============================================================
    //  Type inference from attributes on the outer ModelExpression
    // ============================================================

    [Test]
    public void Process_WithDataTypePasswordOnInnerProperty_InfersTypePassword()
    {
        // [DataType(Password)] lives on the inner (model) property; the helper reads it
        // through InputExpressionOneLayerDeep.Metadata.DataTypeName.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithPasswordDataType(), nameof(InnerStringWithPasswordDataType.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("password"));
    }

    [Test]
    public void Process_WithHiddenInputOnInnerProperty_InfersTypeHidden()
    {
        // [HiddenInput] sets TemplateHint=HiddenInput on the inner property metadata.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithHiddenInput(), nameof(InnerStringWithHiddenInput.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("hidden"));
    }

    // ============================================================
    //  Explicit "type" attribute precedence
    // ============================================================

    [Test]
    public void Process_WithExplicitType_OverridesInferredTypeFromModelType()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer, Type = "email" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("email"));
    }

    [Test]
    public void Process_WithExplicitType_OverridesDataTypePasswordInference()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithPasswordDataType(), nameof(InnerStringWithPasswordDataType.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer, Type = "text" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("text"));
    }

    [Test]
    public void Process_WithExplicitType_OverridesHiddenInputInference()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithHiddenInput(), nameof(InnerStringWithHiddenInput.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer, Type = "search" };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "type"), Is.EqualTo("search"));
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
            new InnerString(), nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "name"), Is.EqualTo("Name"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("Name"));
    }

    [Test]
    public void Process_SetsIdFromInnerExpressionWithDotsReplacedByUnderscores()
    {
        // We need a proper container-derived ModelExplorer (so PropertyAttributes is non-null
        // inside SetValidationAttributes), but with a dotted Name to exercise sanitization.
        // The trick: build the explorer normally, then construct a ModelExpression that
        // shares that explorer's metadata but has an overridden dotted Name.
        var provider = CreateMetadataProvider();
        var containerExplorer = provider.GetModelExplorerForType(typeof(InnerString), new InnerString());
        var innerExplorer = containerExplorer.GetExplorerForProperty(nameof(InnerString.Name));
        var inner = new ModelExpression("User.Address.Street", innerExplorer);
        var outerContainer = new OuterPlain { Field = inner };
        var outer = BuildExpression(provider, outerContainer, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        // name preserves the dotted path (so model binding can map back to the nested
        // property), while id sanitizes dots/brackets — matching asp-for's behavior.
        Assert.That(GetAttr(output, "name"), Is.EqualTo("User.Address.Street"));
        Assert.That(GetAttr(output, "id"), Is.EqualTo("User_Address_Street"));
    }

    [Test]
    public void Process_WithModelValue_SetsValueAttribute()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString { Name = "hello world" }, nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "value"), Is.EqualTo("hello world"));
    }

    [Test]
    public void Process_WithExplicitValue_PreservesExplicitValue()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString { Name = "model-value" }, nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput(new TagHelperAttributeList { { "value", "explicit-value" } });

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "value"), Is.EqualTo("explicit-value"));
    }

    // ============================================================
    //  Exception cases
    // ============================================================

    [Test]
    public void Process_WithUnsupportedInnerModelType_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerUnsupported(), nameof(InnerUnsupported.Items),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Process_WithBoolInnerModel_ThrowsArgumentException()
    {
        // bool is no longer supported by StaticInputTagHelper — checkbox inputs
        // are handled by StaticCheckboxTagHelper.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerBool(), nameof(InnerBool.Flag),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Process_WithInvalidExplicitType_ThrowsArgumentException()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer, Type = "not-a-real-type" };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Process_WithExplicitTypeCheckbox_ThrowsArgumentException()
    {
        // "checkbox" is not in ValidHtmlInputTypes — use StaticCheckboxTagHelper instead.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer, Type = "checkbox" };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Process_WithExplicitTypeRadio_ThrowsArgumentException()
    {
        // "radio" is not in ValidHtmlInputTypes — use StaticRadioTagHelper instead.
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerString(), nameof(InnerString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer, Type = "radio" };
        var output = CreateTagHelperOutput();

        Assert.That(() => tagHelper.Process(CreateTagHelperContext(), output),
            Throws.TypeOf<ArgumentException>());
    }

    // ============================================================
    //  data-val attribute generation (the new core purpose)
    //
    //  StaticInputTagHelper acts as a replacement for asp-for: validation attributes
    //  on the inner property are translated to data-val-* HTML attributes used by the
    //  jquery.validate.unobtrusive client-side validator.
    // ============================================================

    [Test]
    public void Process_WithRequiredAttribute_SetsDataValAndDataValRequired()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithRequired(), nameof(InnerStringWithRequired.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val"), Is.EqualTo("true"));
        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("The Name field is required."));
    }

    [Test]
    public void Process_WithRequiredCustomErrorMessage_UsesCustomMessage()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithRequiredCustom(), nameof(InnerStringWithRequiredCustom.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("Name is mandatory."));
    }

    [Test]
    public void Process_WithDisplayName_UsesDisplayNameInDefaultErrorMessage()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithDisplayName(), nameof(InnerStringWithDisplayName.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val-required"), Is.EqualTo("The Full Name field is required."));
    }

    [Test]
    public void Process_WithEmailAddressAttribute_SetsDataValEmail()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithEmail(), nameof(InnerStringWithEmail.Address),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val"), Is.EqualTo("true"));
        // EmailAddressAttribute ships a default ErrorMessage of "The {0} field is not a valid
        // e-mail address." (note the hyphen). The helper substitutes {0} with the display name.
        Assert.That(GetAttr(output, "data-val-email"), Is.EqualTo("The Address field is not a valid e-mail address."));
    }

    [Test]
    public void Process_WithUrlAttribute_SetsDataValUrl()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithUrl(), nameof(InnerStringWithUrl.Site),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        // UrlAttribute's default ErrorMessage has a {0} placeholder; the helper substitutes it.
        Assert.That(GetAttr(output, "data-val-url"),
            Is.EqualTo("The Site field is not a valid fully-qualified http, https, or ftp URL."));
    }

    [Test]
    public void Process_WithPhoneAttribute_SetsDataValPhone()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithPhone(), nameof(InnerStringWithPhone.Phone),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        // PhoneAttribute's default ErrorMessage has a {0} placeholder; the helper substitutes it.
        Assert.That(GetAttr(output, "data-val-phone"),
            Is.EqualTo("The Phone field is not a valid phone number."));
    }

    [Test]
    public void Process_WithRegularExpressionAttribute_SetsDataValRegexAndPattern()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithRegex(), nameof(InnerStringWithRegex.Code),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val-regex-pattern"), Is.EqualTo(@"^\d{3}-\d{4}$"));
        Assert.That(GetAttr(output, "data-val-regex"),
            Is.EqualTo(@"The field Code must match the regular expression '^\d{3}-\d{4}$'."));
    }

    [Test]
    public void Process_WithRangeAttribute_SetsDataValRangeBounds()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerIntWithRange(), nameof(InnerIntWithRange.Age),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val-range-min"), Is.EqualTo("1"));
        Assert.That(GetAttr(output, "data-val-range-max"), Is.EqualTo("99"));
        Assert.That(GetAttr(output, "data-val-range"),
            Is.EqualTo("The field Age must be between 1 and 99."));
    }

    [Test]
    public void Process_WithStringLengthAttribute_SetsDataValLengthBoundsAndMaxlength()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithLength(), nameof(InnerStringWithLength.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val-length-min"), Is.EqualTo("3"));
        Assert.That(GetAttr(output, "data-val-length-max"), Is.EqualTo("50"));
        Assert.That(GetAttr(output, "maxlength"), Is.EqualTo("50"));
        Assert.That(GetAttr(output, "data-val-length"),
            Is.EqualTo("The field Name must be between 3 and 50 characters long."));
    }

    [Test]
    public void Process_WithStringLengthMaxOnly_OmitsDataValLengthMin()
    {
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerStringWithMaxLengthOnly(), nameof(InnerStringWithMaxLengthOnly.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(GetAttr(output, "data-val-length-max"), Is.EqualTo("100"));
        Assert.That(GetAttr(output, "maxlength"), Is.EqualTo("100"));
        Assert.That(output.Attributes.ContainsName("data-val-length-min"), Is.False);
        Assert.That(GetAttr(output, "data-val-length"),
            Is.EqualTo("The field Name must be at most 100 characters long."));
    }

    [Test]
    public void Process_WithoutAnyValidationAttributes_DoesNotSetDataValTrue()
    {
        // Use a nullable string so the metadata provider does not infer IsRequired=true from
        // the property's nullability (which would emit data-val-required even without [Required]).
        var provider = CreateMetadataProvider();
        var outer = BuildOuterExpression(
            provider,
            new InnerNullableString(), nameof(InnerNullableString.Name),
            inner => new OuterPlain { Field = inner }, nameof(OuterPlain.Field));

        var tagHelper = new StaticInputTagHelper { InputExpression = outer };
        var output = CreateTagHelperOutput();

        tagHelper.Process(CreateTagHelperContext(), output);

        Assert.That(output.Attributes.ContainsName("data-val"), Is.False);
        Assert.That(output.Attributes.ContainsName("data-val-required"), Is.False);
    }
}
