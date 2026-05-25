using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TechGems.StaticComponents.Headless;
using TechGems.StaticComponents.UnitTests.Utils;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticInputTests
{
    private static ViewContext CreateViewContext(FakeMarkupStaticComponent htmlHelper)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHtmlHelper>(htmlHelper);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        return new ViewContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        };
    }

    private static TagHelperContext CreateTagHelperContext(TagHelperAttributeList? attributes = null)
    {
        attributes ??= new TagHelperAttributeList();
        return new TagHelperContext(
            tagName: "static-input",
            allAttributes: attributes,
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private static TagHelperOutput CreateTagHelperOutput(string childContent = "")
    {
        return new TagHelperOutput(
            "static-input",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var content = new DefaultTagHelperContent();
                content.SetHtmlContent(childContent);
                return Task.FromResult<TagHelperContent>(content);
            });
    }

    private static ModelExpression CreateModelExpression(Type modelType)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var modelExplorer = metadataProvider.GetModelExplorerForType(modelType, null);
        return new ModelExpression("TestProperty", modelExplorer);
    }

    [Test]
    public void ProcessAsync_InvalidInputExpressionModelType_ThrowsArgumentException()
    {
        var tagHelper = new StaticInput
        {
            InputExpression = CreateModelExpression(typeof(List<int>))
        };
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var action = async () =>
        {
            tagHelper.Init(context);
            await tagHelper.ProcessAsync(context, output);
        };

        Assert.That(action, Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void ProcessAsync_ValidInputExpressionModelType_DoesNotThrowException()
    {
        var htmlHelper = new FakeMarkupStaticComponent();
        var tagHelper = new StaticInput
        {
            ViewContext = CreateViewContext(htmlHelper),
            InputExpression = CreateModelExpression(typeof(int))
        };
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var action = async () =>
        {
            tagHelper.Init(context);
            await tagHelper.ProcessAsync(context, output);
        };

        Assert.That(action, Throws.Nothing);
    }
}