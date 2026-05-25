using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using TechGems.StaticComponents.Headless;
using TechGems.StaticComponents.UnitTests.Utils;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticButtonTests
{
    private static ViewContext CreateViewContext(IHtmlHelper htmlHelper)
    {
        var services = new ServiceCollection();
        services.AddSingleton(htmlHelper);

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
            tagName: "static-headless-button",
            allAttributes: attributes,
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private static TagHelperOutput CreateTagHelperOutput(string childContent = "")
    {
        return new TagHelperOutput(
            "static-headless-button",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var content = new DefaultTagHelperContent();
                content.SetHtmlContent(childContent);
                return Task.FromResult<TagHelperContent>(content);
            });
    }

    [Test]
    public void ProcessAsync_InvalidType_ThrowsArgumentException()
    {
        var tagHelper = new StaticButton
        {
            Type = "invalid"
        };
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("Click");

        var action = async () =>
        {
            tagHelper.Init(context);
            await tagHelper.ProcessAsync(context, output);
        };

        Assert.That(action, Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void ProcessAsync_ValidType_DoesNotThrowExceptions()
    {
        var htmlHelper = new FakeMarkupStaticComponent();
        var viewContext = CreateViewContext(htmlHelper);

        var tagHelper = new StaticButton
        {
            ViewContext = viewContext,
            Type = "submit",
            Disabled = true
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("Click");

        var action = async () =>
        {
            tagHelper.Init(context);
            await tagHelper.ProcessAsync(context, output);
        };

        Assert.That(action, Throws.Nothing);
    }
}