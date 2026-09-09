# Overriding Default Rendering

Every Static Component renders its Razor partial through `ProcessAsync`. Override it when the component needs pre-render logic — fetching data, choosing between views, or building a different model.

**Don't reach for this by default.** The standard pipeline fits the overwhelming majority of components; an override is a maintenance cost and easy to get subtly wrong.

## Overriding `ProcessAsync`

Call `base.RenderPartialView(output)` to trigger the default rendering:

```csharp
[HtmlTargetElement("hello-world")]
public class HelloWorldComponent : StaticComponent
{
    public HelloWorldComponent() : base("~/Views/HelloWorld.cshtml") { }

    [HtmlAttributeName("render-alternate")]
    public bool RenderAlternateView { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!RenderAlternateView)
        {
            await base.RenderPartialView(output);
        }
        else
        {
            var model = new ModelType() { Name = "Test" };
            await base.RenderPartialView("~/Views/DifferentView.cshtml", output, model);
        }
    }
}
```

After rendering:

- `output.Content` holds the rendered partial output.
- `output.TagName` is set to `null`, so the component's own tag doesn't appear in the HTML.

## `RenderPartialView` overloads

| Overload | Renders | Model |
|---|---|---|
| `RenderPartialView(TagHelperOutput output)` | The component's default view route | `this` |
| `RenderPartialView(string viewRoute, TagHelperOutput output)` | The given route | `this` |
| `RenderPartialView<T>(string viewRoute, TagHelperOutput output, T model)` | The given route | The `model` you pass |

The generic overload is the one to use when the alternate view has its own `@model` type. All three are `protected` and available on both `StaticComponent` and `StaticNode`.

## Other members available to an override

| Member | Notes |
|---|---|
| `ViewContext` | Injected by ASP.NET Core; gives access to `HttpContext`. Never set it manually. |
| `ParentComponent` | `protected`. The enclosing `StaticComponent`, set during `Init`. `null` at the top level. |
| `GetHtmlHelper()` | `protected`. Pulls `IHtmlHelper` from `ViewContext`; throws `ArgumentNullException` when `ViewContext` is null. |

## Overriding on a headless input component

`StaticInputBase.ProcessAsync` captures `AdditionalAttributes` before delegating to `StaticComponent.ProcessAsync` — the output attributes at that moment are exactly the ones that weren't bound to a C# property, and the Razor render consumes them.

So on a component inheriting `StaticInputBase`, an override must call `base.ProcessAsync(context, output)`:

```csharp
public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
{
    // your pre-render logic here

    await base.ProcessAsync(context, output);   // captures AdditionalAttributes, then renders
}
```

Calling `RenderPartialView` directly instead skips the capture and leaves `AdditionalAttributes` empty — attribute passthrough silently stops working with no error.
