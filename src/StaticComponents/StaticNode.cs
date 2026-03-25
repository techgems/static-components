using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace TechGems.StaticComponents;

/// <summary>
/// A base class for a StaticNode that leverages a razor partial view but does not support ChildContent or Slots.
/// Sends itself as the view model for the razor template.
/// </summary>
public abstract class StaticNode : TagHelper
{
    protected readonly string _razorViewRoute;

    /// <summary>
    /// Creates the tag helper with a razor view route using default route.
    /// </summary>
    public StaticNode()
    {
        var type = GetType();
        var assemblyName = type.Assembly.GetName().Name;
        _razorViewRoute = $"{type.FullName!.Replace(assemblyName!, "~").Replace(".", "/")}.cshtml";
    }

    /// <summary>
    /// Creates the tag helper with a razor view route that overrides the default route.
    /// </summary>
    /// <param name="razorViewRoute"></param>
    public StaticNode(string razorViewRoute)
    {
        _razorViewRoute = razorViewRoute;
    }

    /// <summary>
    /// The View Context necessary to get the Html Helper that renders the partial views.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { protected get; set; }

    /// <summary>
    /// Gets the Html Helper from the View Context. Used for rendering partial views.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected IHtmlHelper GetHtmlHelper()
    {
        if (ViewContext is null)
        {
            throw new ArgumentNullException(nameof(ViewContext));
        }

        IHtmlHelper? htmlHelper = ViewContext.HttpContext.RequestServices.GetService<IHtmlHelper>();
        ArgumentNullException.ThrowIfNull(htmlHelper);

        (htmlHelper as IViewContextAware)!.Contextualize(ViewContext);

        return htmlHelper;
    }

    /// <summary>
    /// Default ProcessAsync method. Will render the default razor view if a route is not provided in the base class.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext is null)
        {
            throw new ArgumentNullException(nameof(ViewContext));
        }

        if (_razorViewRoute is null)
        {
            throw new ArgumentNullException($"{nameof(_razorViewRoute)} cannot be null.");
        }

        await RenderPartialView(_razorViewRoute, output);
    }

    /// <summary>
    /// Uses the HtmlHelper to render the partial view. Defaults tag name to null and adds child content if there was any.
    /// Will send the child class as the view model for the partial view.
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    protected async Task RenderPartialView(TagHelperOutput output)
    {
        await RenderPartialView(_razorViewRoute, output, this);
    }

    /// <summary>
    /// Uses the HtmlHelper to render the partial view. Defaults tag name to null and adds child content if there was any.
    /// Will send the child class as the view model for the partial view.
    /// </summary>
    /// <param name="viewRoute"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    protected async Task RenderPartialView(string viewRoute, TagHelperOutput output)
    {
        await RenderPartialView(viewRoute, output, this);
    }

    /// <summary>
    /// Uses the HtmlHelper to render the partial view. Defaults tag name to null.
    /// Will send the provided model as the view model for the partial view.
    /// </summary>
    /// <param name="viewRoute"></param>
    /// <param name="output"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    protected async Task RenderPartialView<T>(string viewRoute, TagHelperOutput output, T model)
    {
        var htmlHelper = GetHtmlHelper();

        try
        {
            var content = await htmlHelper.PartialAsync(viewRoute, model);
            output.Content.SetHtmlContent(content);
            output.TagName = null;
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                throw new InvalidOperationException("The default view was not found. Make sure that the namespace used in the ViewModel is consistent with the assembly name.", ex);
            }

            throw new Exception("An unexpected error has occurred while rendering the component. See inner exception.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error has occurred while rendering the component. See inner exception.", ex);
        }
    }
}
