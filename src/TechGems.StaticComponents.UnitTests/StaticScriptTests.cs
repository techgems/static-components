using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using TechGems.StaticComponents;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class StaticScriptTests
{
    private const string ScriptsArrayKey = "_static_scripts_";
    private const string ScriptsOnceKeyPrefix = "_static_scripts_once_";

    #region Helper Methods

    private static ViewContext CreateViewContext(HttpContext? httpContext = null)
    {
        httpContext ??= new DefaultHttpContext();
        return new ViewContext
        {
            HttpContext = httpContext
        };
    }

    private static TagHelperContext CreateTagHelperContext(TagHelperAttributeList? attributes = null)
    {
        attributes ??= new TagHelperAttributeList();
        return new TagHelperContext(
            tagName: "script",
            allAttributes: attributes,
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private static TagHelperOutput CreateTagHelperOutput(string childContent, TagHelperAttributeList? attributes = null)
    {
        attributes ??= new TagHelperAttributeList();
        return new TagHelperOutput(
            tagName: "script",
            attributes: attributes,
            getChildContentAsync: (useCachedResult, encoder) =>
            {
                var content = new DefaultTagHelperContent();
                content.SetContent(childContent);
                return Task.FromResult<TagHelperContent>(content);
            });
    }

    private static StaticScript CreateStaticScript(
        ViewContext viewContext,
        bool teleportScript = false,
        object? renderOnce = null,
        bool disableRender = false)
    {
        return new StaticScript
        {
            ViewContext = viewContext,
            TeleportScript = teleportScript,
            RenderOnce = renderOnce,
            DisableRender = disableRender
        };
    }

    private static List<string>? GetScriptsArray(HttpContext httpContext)
    {
        return httpContext.Items[ScriptsArrayKey] as List<string>;
    }

    private static string GetOnceKey(object renderOnceValue)
    {
        return $"{ScriptsOnceKeyPrefix}_{renderOnceValue.GetType().Name}";
    }

    private static async Task ExecuteTagHelper(StaticScript tagHelper, TagHelperContext context, TagHelperOutput output)
    {
        tagHelper.Init(context);
        await tagHelper.ProcessAsync(context, output);
    }

    #endregion

    #region Output Suppression

    [Test]
    public async Task ProcessAsync_Always_SuppressesOriginalOutput()
    {
        var viewContext = CreateViewContext();
        var tagHelper = CreateStaticScript(viewContext);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("console.log('test');");

        await ExecuteTagHelper(tagHelper, context, output);

        Assert.That(output.TagName, Is.Null);
    }

    #endregion

    #region DisableRender

    [Test]
    public async Task ProcessAsync_DisableRender_NoContentNoArray()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);
        var tagHelper = CreateStaticScript(viewContext, disableRender: true);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("console.log('test');");

        await ExecuteTagHelper(tagHelper, context, output);

        Assert.That(output.Content.GetContent(), Is.Empty);
        Assert.That(httpContext.Items.ContainsKey(ScriptsArrayKey), Is.False);
    }

    [Test]
    public async Task ProcessAsync_DisableRender_WithTeleport_NoContentNoArray()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);
        var tagHelper = CreateStaticScript(viewContext, teleportScript: true, disableRender: true);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("console.log('test');");

        await ExecuteTagHelper(tagHelper, context, output);

        Assert.That(output.Content.GetContent(), Is.Empty);
        Assert.That(httpContext.Items.ContainsKey(ScriptsArrayKey), Is.False);
    }

    [Test]
    public async Task ProcessAsync_DisableRender_WithRenderOnce_NoContentNoMarker()
    {
        var renderOnceObj = new object();
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);
        var tagHelper = CreateStaticScript(viewContext, renderOnce: renderOnceObj, disableRender: true);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("console.log('test');");

        await ExecuteTagHelper(tagHelper, context, output);

        Assert.That(output.Content.GetContent(), Is.Empty);
        Assert.That(httpContext.Items.ContainsKey(ScriptsArrayKey), Is.False);
        Assert.That(httpContext.Items.ContainsKey(GetOnceKey(renderOnceObj)), Is.False);
    }

    #endregion

    #region Inline Render (TeleportScript=false, RenderOnce=null)

    [Test]
    public async Task ProcessAsync_Inline_SetsHtmlContentWithScriptTag()
    {
        var viewContext = CreateViewContext();
        var tagHelper = CreateStaticScript(viewContext);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("console.log(1);");

        await ExecuteTagHelper(tagHelper, context, output);

        var content = output.Content.GetContent();
        Assert.That(content, Does.Contain("<script>"));
        Assert.That(content, Does.Contain("console.log(1);"));
        Assert.That(content, Does.Contain("</script>"));
    }

    [Test]
    public async Task ProcessAsync_Inline_CalledTwice_RendersBothTimes()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);

        var context1 = CreateTagHelperContext();
        var output1 = CreateTagHelperOutput("var a = 1;");
        var tagHelper1 = CreateStaticScript(viewContext);
        await ExecuteTagHelper(tagHelper1, context1, output1);

        var context2 = CreateTagHelperContext();
        var output2 = CreateTagHelperOutput("var b = 2;");
        var tagHelper2 = CreateStaticScript(viewContext);
        await ExecuteTagHelper(tagHelper2, context2, output2);

        Assert.That(output1.Content.GetContent(), Does.Contain("var a = 1;"));
        Assert.That(output2.Content.GetContent(), Does.Contain("var b = 2;"));
    }

    [Test]
    public async Task ProcessAsync_Inline_PreservesStandardAttributes()
    {
        var viewContext = CreateViewContext();
        var attributes = new TagHelperAttributeList { { "type", "module" } };
        var tagHelper = CreateStaticScript(viewContext);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("var x = 1;", attributes);

        await ExecuteTagHelper(tagHelper, context, output);

        var content = output.Content.GetContent();
        Assert.That(content, Does.Contain("type=\"module\""));
    }

    [Test]
    public async Task ProcessAsync_Inline_StripsStaticScriptAttributes()
    {
        var viewContext = CreateViewContext();
        var attributes = new TagHelperAttributeList
        {
            { "static-script", "" },
            { "teleport-script", "false" },
            { "render-once", "" },
            { "type", "text/javascript" }
        };
        var tagHelper = CreateStaticScript(viewContext);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("var x = 1;", attributes);

        await ExecuteTagHelper(tagHelper, context, output);

        var content = output.Content.GetContent();
        Assert.That(content, Does.Not.Contain("static-script"));
        Assert.That(content, Does.Not.Contain("teleport-script"));
        Assert.That(content, Does.Not.Contain("render-once"));
        Assert.That(content, Does.Contain("type=\"text/javascript\""));
    }

    #endregion

    #region Teleport (TeleportScript=true, RenderOnce=null)

    [Test]
    public async Task ProcessAsync_Teleport_AddsScriptToArray()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);
        var tagHelper = CreateStaticScript(viewContext, teleportScript: true);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("alert(1);");

        await ExecuteTagHelper(tagHelper, context, output);

        var scriptsArray = GetScriptsArray(httpContext);
        Assert.That(scriptsArray, Is.Not.Null);
        Assert.That(scriptsArray, Has.Count.EqualTo(1));
        Assert.That(scriptsArray![0], Does.Contain("alert(1);"));
    }

    [Test]
    public async Task ProcessAsync_Teleport_NoInlineContent()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);
        var tagHelper = CreateStaticScript(viewContext, teleportScript: true);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("alert(1);");

        await ExecuteTagHelper(tagHelper, context, output);

        Assert.That(output.Content.GetContent(), Is.Empty);
    }

    [Test]
    public async Task ProcessAsync_Teleport_CalledTwice_AddsBothToArray()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);

        var tagHelper1 = CreateStaticScript(viewContext, teleportScript: true);
        var context1 = CreateTagHelperContext();
        var output1 = CreateTagHelperOutput("first();");
        await ExecuteTagHelper(tagHelper1, context1, output1);

        var tagHelper2 = CreateStaticScript(viewContext, teleportScript: true);
        var context2 = CreateTagHelperContext();
        var output2 = CreateTagHelperOutput("second();");
        await ExecuteTagHelper(tagHelper2, context2, output2);

        var scriptsArray = GetScriptsArray(httpContext);
        Assert.That(scriptsArray, Has.Count.EqualTo(2));
    }

    #endregion

    #region Teleport + RenderOnce

    [Test]
    public async Task ProcessAsync_TeleportRenderOnce_FirstCall_AddsToArrayAndSetsMarker()
    {
        var renderOnceObj = new object();
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);
        var tagHelper = CreateStaticScript(viewContext, teleportScript: true, renderOnce: renderOnceObj);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("initComponent();");

        await ExecuteTagHelper(tagHelper, context, output);

        var scriptsArray = GetScriptsArray(httpContext);
        Assert.That(scriptsArray, Has.Count.EqualTo(1));
        Assert.That(scriptsArray![0], Does.Contain("initComponent();"));
        Assert.That(httpContext.Items.ContainsKey(GetOnceKey(renderOnceObj)), Is.True);
    }

    [Test]
    public async Task ProcessAsync_TeleportRenderOnce_SecondSameType_NoDuplicate()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);

        var tagHelper1 = CreateStaticScript(viewContext, teleportScript: true, renderOnce: new object());
        var context1 = CreateTagHelperContext();
        var output1 = CreateTagHelperOutput("initComponent();");
        await ExecuteTagHelper(tagHelper1, context1, output1);

        var tagHelper2 = CreateStaticScript(viewContext, teleportScript: true, renderOnce: new object());
        var context2 = CreateTagHelperContext();
        var output2 = CreateTagHelperOutput("initComponent();");
        await ExecuteTagHelper(tagHelper2, context2, output2);

        var scriptsArray = GetScriptsArray(httpContext);
        Assert.That(scriptsArray, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ProcessAsync_TeleportRenderOnce_DifferentTypes_AddsBoth()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);

        // First call with object type
        var renderOnce1 = new object();
        var tagHelper1 = CreateStaticScript(viewContext, teleportScript: true, renderOnce: renderOnce1);
        var context1 = CreateTagHelperContext();
        var output1 = CreateTagHelperOutput("initA();");
        await ExecuteTagHelper(tagHelper1, context1, output1);

        // Second call with string type (different type name)
        var renderOnce2 = "a string";
        var tagHelper2 = CreateStaticScript(viewContext, teleportScript: true, renderOnce: renderOnce2);
        var context2 = CreateTagHelperContext();
        var output2 = CreateTagHelperOutput("initB();");
        await ExecuteTagHelper(tagHelper2, context2, output2);

        var scriptsArray = GetScriptsArray(httpContext);
        Assert.That(scriptsArray, Has.Count.EqualTo(2));
        Assert.That(httpContext.Items.ContainsKey(GetOnceKey(renderOnce1)), Is.True);
        Assert.That(httpContext.Items.ContainsKey(GetOnceKey(renderOnce2)), Is.True);
    }

    #endregion

    #region Inline + RenderOnce (TeleportScript=false, RenderOnce=object)

    [Test]
    public async Task ProcessAsync_InlineRenderOnce_FirstCall_SetsContentAndMarker()
    {
        var renderOnceObj = new object();
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);
        var tagHelper = CreateStaticScript(viewContext, renderOnce: renderOnceObj);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("setupOnce();");

        await ExecuteTagHelper(tagHelper, context, output);

        var content = output.Content.GetContent();
        Assert.That(content, Does.Contain("setupOnce();"));
        Assert.That(httpContext.Items.ContainsKey(GetOnceKey(renderOnceObj)), Is.True);
    }

    [Test]
    public async Task ProcessAsync_InlineRenderOnce_SecondSameType_NoContent()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);

        var tagHelper1 = CreateStaticScript(viewContext, renderOnce: new object());
        var context1 = CreateTagHelperContext();
        var output1 = CreateTagHelperOutput("setupOnce();");
        await ExecuteTagHelper(tagHelper1, context1, output1);

        var tagHelper2 = CreateStaticScript(viewContext, renderOnce: new object());
        var context2 = CreateTagHelperContext();
        var output2 = CreateTagHelperOutput("setupOnce();");
        await ExecuteTagHelper(tagHelper2, context2, output2);

        Assert.That(output1.Content.GetContent(), Does.Contain("setupOnce();"));
        Assert.That(output2.Content.GetContent(), Is.Empty);
    }

    [Test]
    public async Task ProcessAsync_InlineRenderOnce_DifferentTypes_BothRender()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);

        var tagHelper1 = CreateStaticScript(viewContext, renderOnce: new object());
        var context1 = CreateTagHelperContext();
        var output1 = CreateTagHelperOutput("initA();");
        await ExecuteTagHelper(tagHelper1, context1, output1);

        var tagHelper2 = CreateStaticScript(viewContext, renderOnce: "a string");
        var context2 = CreateTagHelperContext();
        var output2 = CreateTagHelperOutput("initB();");
        await ExecuteTagHelper(tagHelper2, context2, output2);

        Assert.That(output1.Content.GetContent(), Does.Contain("initA();"));
        Assert.That(output2.Content.GetContent(), Does.Contain("initB();"));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void ProcessAsync_ViewContextNull_ThrowsArgumentNullException()
    {
        var tagHelper = new StaticScript();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("test();");

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await ExecuteTagHelper(tagHelper, context, output));
    }

    [Test]
    public async Task ProcessAsync_EmptyChildContent_StillRendersScriptTag()
    {
        var viewContext = CreateViewContext();
        var tagHelper = CreateStaticScript(viewContext);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("");

        await ExecuteTagHelper(tagHelper, context, output);

        var content = output.Content.GetContent();
        Assert.That(content, Does.Contain("<script>"));
        Assert.That(content, Does.Contain("</script>"));
    }

    [Test]
    public async Task ProcessAsync_Teleport_PreservesCustomAttributes()
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = CreateViewContext(httpContext);
        var attributes = new TagHelperAttributeList { { "src", "app.js" } };
        var tagHelper = CreateStaticScript(viewContext, teleportScript: true);
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("", attributes);

        await ExecuteTagHelper(tagHelper, context, output);

        var scriptsArray = GetScriptsArray(httpContext);
        Assert.That(scriptsArray, Is.Not.Null);
        Assert.That(scriptsArray![0], Does.Contain("src=\"app.js\""));
    }

    #endregion
}
