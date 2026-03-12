using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TechGems.StaticComponents;

[HtmlTargetElement("script", Attributes = $"{StaticComponentsConstants.StatictScriptAttributeName}")]
public class StaticScript : StaticComponent
{

    /// <summary>
    /// HttpContext
    /// </summary>
    [HtmlAttributeNotBound]
    public HttpContext HttpContext => ViewContext.HttpContext;


    /// <summary>
    /// Teleport Script tells StaticScript whether the script should be outputed inline or if it should be moved to a different location via StaticScriptRenderer.
    /// </summary>
    [HtmlAttributeName(StaticComponentsConstants.TeleportScriptAttributeName)]
    public bool TeleportScript { get; set; }

    /// <summary>
    /// Render Once tells StaticScript whether a script needs to be rendered only once despite the component being used multiple times in the same Razor View. The type of component must be specified.
    /// </summary>
    [HtmlAttributeName(StaticComponentsConstants.RenderOnceAttributeName)]
    public object? RenderOnce { get; set; }

    /// <summary>
    /// Should not render disables the rendering of a script entirely when the value is true. This is useful if there are circumstances, such as custom AJAX requests in which you don't want the script to render.
    /// </summary>
    [HtmlAttributeName(StaticComponentsConstants.DisableRenderAttributeName)]
    public bool DisableRender { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext is null)
        {
            throw new ArgumentNullException(nameof(ViewContext));
        }

        var childContent = (await output.GetChildContentAsync()).GetContent();


        if (childContent is null)
            throw new ArgumentNullException("A static-script attributed script tag must not be empty.");

        ProcessScripts(output, childContent);
    }

    private void ProcessScripts(TagHelperOutput output, string scriptContent)
    {
        var script = RecreateHtmlScript(scriptContent, output.Attributes);
        var componentTypeInOnceArray = ComponentTypeExistsInOnceArray();

        var renderOnceTypeName = RenderOnce?.GetType().Name;

        output.SuppressOutput();

        if(DisableRender)
        {
            return;
        }

        //Render multiple and don't teleport.
        if (RenderOnce is null && !TeleportScript)
        {
            output.Content.SetHtmlContent(RecreateHtmlScript(scriptContent, output.Attributes));
            return;
        }

        if (TeleportScript)
        {
            if (RenderOnce is null)
            {
                AddScriptToArray(scriptContent, output.Attributes);
                return;
            }

            if (RenderOnce is not null && !componentTypeInOnceArray)
            {
                AddScriptToArray(scriptContent, output.Attributes);
                ViewContext.HttpContext.Items[$"{StaticComponentsConstants.StaticScriptOnceKey}_{renderOnceTypeName}"] = true;
            }

            return;
        }

        if(RenderOnce is not null && !componentTypeInOnceArray)
        {
            ViewContext.HttpContext.Items[$"{StaticComponentsConstants.StaticScriptOnceKey}_{renderOnceTypeName}"] = true;
            output.Content.SetHtmlContent(RecreateHtmlScript(scriptContent, output.Attributes));
            return;
        }
    }

    private void AddScriptToArray(string scriptContent, TagHelperAttributeList attributes)
    {
        var scriptsArray = ViewContext.HttpContext.Items[StaticComponentsConstants.StaticScriptKey] as List<string>;

        if (scriptsArray != null)
        {
            scriptsArray.Add(RecreateHtmlScript(scriptContent, attributes));
            ViewContext.HttpContext.Items[StaticComponentsConstants.StaticScriptKey] = scriptsArray;
        }
        else
        {
            ViewContext.HttpContext.Items[StaticComponentsConstants.StaticScriptKey] = new List<string> { RecreateHtmlScript(scriptContent, attributes) };
        }
    }

    private bool ComponentTypeExistsInOnceArray()
    {
        if(RenderOnce is null)
        {
            return false; 
        }

        var renderOnceTypeName = RenderOnce?.GetType().Name;

        var keyExists = ViewContext.HttpContext.Items.ContainsKey($"{StaticComponentsConstants.StaticScriptOnceKey}_{renderOnceTypeName}");

        return keyExists;
    }

    private string RecreateHtmlScript(string content, TagHelperAttributeList attributes)
    {
        var sb = new StringBuilder();
        sb.Append("<script");

        foreach (var attribute in attributes)
        {
            if(attribute.Name != StaticComponentsConstants.RenderOnceAttributeName && attribute.Name != StaticComponentsConstants.TeleportScriptAttributeName && attribute.Name != StaticComponentsConstants.StatictScriptAttributeName) { 
                sb.Append(@$" {attribute.Name}=""{attribute.Value}""");
            }
        }
        sb.Append(">");

        sb.AppendLine(content);
        sb.AppendLine("</script>");

        return sb.ToString();
    }
}
