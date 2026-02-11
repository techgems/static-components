using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGems.StaticComponents;

/// <summary>
/// The base class for a component tag helper that leverages a razor partial view. Sends itself as the view model for the razor template.
/// </summary>
public abstract class StaticComponent : TagHelper
{
    protected readonly string _razorViewRoute;
    protected readonly string _componentStackKey = "StackKey";

    [HtmlAttributeNotBound]
    protected StaticComponent? ParentComponent { get; set; }

    /// <summary>
    /// Creates the tag helper with a razor view route using default route.
    /// </summary>
    public StaticComponent()
    {
        var type = GetType();
        var assemblyName = type.Assembly.GetName().Name;
        _razorViewRoute = $"{type.FullName!.Replace(assemblyName!, "~").Replace(".", "/")}.cshtml";
    }

    /// <summary>
    /// Creates the tag helper with a razor view route that overrides the default route.
    /// </summary>
    /// <param name="razorViewRoute"></param>
    public StaticComponent(string razorViewRoute)
    {
        _razorViewRoute = razorViewRoute;
    }

    /// <summary>
    /// The View Context necessary to get the Html Helper that renders the partial views.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    /// <summary>
    /// Child content for rendering in the razor template.
    /// </summary>
    [HtmlAttributeNotBound]
    public TagHelperContent? ChildContent { get; set; }

    [HtmlAttributeNotBound]
    internal Dictionary<string, TagHelperContent> NamedSlots { get; set; } = new Dictionary<string, TagHelperContent>();

    /// <summary>
    /// Property used for determining if you need a fallback on your child content.
    /// </summary>
    [HtmlAttributeNotBound]
    public bool IsChildContentNullOrEmpty
    {
        get
        {
            if (ChildContent is null)
                return true;

            if (ChildContent.IsEmptyOrWhiteSpace)
                return true;

            return false;
        }
    }

    public bool IsSlotContentNullOrEmpty(string slotName)
    {
        if (!NamedSlots.ContainsKey(slotName))
            return true;

        if (NamedSlots[slotName] is null)
            return true;

        if (NamedSlots[slotName].IsEmptyOrWhiteSpace)
            return true;

        return false;
    }

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


    private void SetParentComponentStack(TagHelperContext context, Stack<StaticComponent> parentComponentStack)
    {
        context.Items[_componentStackKey] = parentComponentStack;
    }

    private Stack<StaticComponent> GetParentComponentStack(TagHelperContext context)
    {
        return (context.Items[_componentStackKey] as Stack<StaticComponent>)!;
    }

    /// <summary>
    /// Render the content of a slot in the base razor view.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public TagHelperContent RenderSlot(string name)
    {
        var result = NamedSlots.TryGetValue(name, out var slot);

        if (!result)
            throw new ArgumentException("The slot could not be rendered because it doesn't exist in the slot dictionary in the parent component. To fix this, make sure that the slot name exists in your markup.");

        return slot!;
    }

    public override sealed void Init(TagHelperContext context)
    {
        if (!context.Items.ContainsKey(_componentStackKey))
        {
            var parentComponentStack = new Stack<StaticComponent>();

            ParentComponent = null;
            parentComponentStack.Push(this);

            SetParentComponentStack(context, parentComponentStack);
        }
        else
        {
            var parentComponentStack = GetParentComponentStack(context);

            ParentComponent = parentComponentStack.Peek();

            if (this is not StaticComponentSlot)
            {
                parentComponentStack.Push(this);
            }
        }

        base.Init(context);
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
        else
        {
            await RenderPartialView(_razorViewRoute, output);
        }

        if (this is not StaticComponentSlot)
        {
            var stack = GetParentComponentStack(context);
            if (stack.Count > 0 && stack.Peek() == this)
            {
                stack.Pop();
            }
        }
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
    /// Uses the HtmlHelper to render the partial view. Defaults tag name to null and adds child content if there was any.
    /// Will send the child class as the view model for the partial view.
    /// </summary>
    /// <param name="viewRoute"></param>
    /// <param name="output"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    protected async Task RenderPartialView<T>(string viewRoute, TagHelperOutput output, T model)
    {
        var childContent = await output.GetChildContentAsync();

        if (childContent is not null)
        {
            ChildContent = childContent;
        }

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

