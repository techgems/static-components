# AlpineJS and HTMX

Static Components is built for the **HATS stack** — HTMX + AlpineJS + TailwindCSS + Static Components. The server renders the markup; Alpine adds client-side state and HTMX adds requests, both declared as HTML attributes rather than JavaScript.

## Passing server state into AlpineJS

Feed C# values into `x-data` so a component is interactive without writing imperative JS:

```csharp
public class CollapsibleSectionComponent : StaticComponent
{
    public CollapsibleSectionComponent() { }

    [HtmlAttributeName("is-open")]
    public bool IsOpen { get; set; } = false;
}
```

```cshtml
@using Sample.Views
@model CollapsibleSectionComponent
@{
    var isOpenJs = Model.IsOpen.ToString().ToLower();
}

<div x-data="{ open: @isOpenJs }">
    <div x-show="open">
        @Model.ChildContent
    </div>
    <button @@click="open = !open">Toggle</button>
</div>
```

```cshtml
<collapsible-section-component is-open="true">
    This content can be toggled on and off.
</collapsible-section-component>
```

## Razor's `@` escaping

Alpine's shorthand bindings collide with Razor's `@`. **Write `@@` for a literal `@`** — `@@click`, `@@input.debounce`, `@@keydown.escape`. The long forms (`x-on:click`) need no escaping and are worth preferring in templates where the shorthand gets noisy.

## Structured state: `Alpine.data` over inline `x-data`

For anything beyond a couple of fields, declare the component's state with `Alpine.data` in a `static-script` instead of inlining it. It keeps the markup readable and the state in one place.

```cshtml
<div x-data="gallery">…</div>

<script static-script type="text/javascript" render-once="Model" teleport-script>
    Alpine.data('gallery', () => ({
        images: @(JavascriptConvert.SerializeObject(Model.ImageList)),
        index: 0,
        next() { this.index = (this.index + 1) % this.images.length; },
    }))
</script>
```

Use `Alpine.store` for state shared across components. See `static-scripts.md` for `JavascriptConvert` — note the `SerializeObject(value, true)` overload, which emits single-quoted strings and is the one to use when the literal goes inside an HTML attribute like `x-data`.

## A C# class for the state object

Typing the state gives you compile-time safety over what reaches the client:

```csharp
public class PinesBannerState
{
    public bool BannerVisible { get; set; } = false;
    public int BannerVisibleAfter { get; set; } = 1000;
}

public class PinesBanner : StaticComponent
{
    [HtmlAttributeName("initial-state")]
    public PinesBannerState BannerState { get; set; } = new PinesBannerState();
}
```

```cshtml
<div x-data="@JavascriptConvert.SerializeObject(Model.BannerState, true)"
     x-show="bannerVisible"
     x-init="setTimeout(() => { bannerVisible = true }, bannerVisibleAfter);"
     x-cloak>
```

## Where Alpine and HTMX attributes belong on a component

When consumers pass `x-` and `hx-` attributes to a component you wrote, they usually belong on the **wrapper**, not the inner input: `x-show` on the wrapper hides the label along with the field, and HTMX inherits most attributes from ancestors. The exception is `x-model`, which two-way binds a value and must sit on the `<input>` itself.

`AdditionalAttributes.WithPrefix("x-", "hx-")` / `.WithoutPrefix(...)` do that split. See `attribute-passthrough.md`.

## HTMX and scripts

A component rendered into an HTMX partial response re-emits its `static-script` unless you stop it. `render-once` only dedups within a single response, so for partial responses send a custom header on the request, read it in the view and set `disable-render` from it — see `static-scripts.md`.
