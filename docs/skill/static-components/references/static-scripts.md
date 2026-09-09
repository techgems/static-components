# Static Scripts

Static Components includes the `static-script` tag helper to manage JavaScript inside components. It solves two problems that ordinary Razor partials cannot — partials can't use Razor sections, which is why scripts in partials are normally discouraged: **deduplication** and **teleportation**.

Use `static-script` as an attribute on a `<script>` tag. It works inside Static Components as well as regular partial views and ViewComponents.

## Attributes

| Attribute | Type | Purpose |
|---|---|---|
| `static-script` | marker | Required. Marks the script for Static Components processing. |
| `render-once` | `object` | Renders the script only once per page, regardless of how many times the component is used. Pass the view model (`Model`) — its type is the dedup key. |
| `teleport-script` | `bool` | Moves the script to `<render-static-scripts />` instead of rendering inline. The bare attribute means `true`. |
| `disable-render` | `bool` | Prevents the script from rendering at all. |

A `static-script` tag must either contain script content **or** carry a `src` attribute — one with neither throws `ArgumentNullException`. Both forms work:

```cshtml
<script static-script src="~/js/my-widget.js" teleport-script render-once="Model"></script>
```

## Behavior matrix

| `teleport-script` | `render-once` | `disable-render` | Result |
|---|---|---|---|
| `false` | unset | `false` | Renders inline at the component's location. |
| `true` | unset | `false` | Teleported to `<render-static-scripts />`. |
| `false` | set | `false` | Renders inline, first occurrence only. |
| `true` | set | `false` | Teleported, first occurrence only. |
| any | any | `true` | Not rendered. |

## `render-once`

Without it, a component used twice on a page emits its script twice — re-running initialization and often breaking the library it wraps.

```cshtml
@model MaskedInputComponent

<input asp-for="Model.InputExpression" class="autocomplete" data-mask />

<script static-script type="text/javascript" render-once="Model" defer>
    var selector = document.querySelector("[data-mask]");
    var im = new Inputmask("(000)-000-0000");
    im.mask(selector);
</script>
```

```cshtml
<masked-input asp-for="MobilePhoneNumber" />
<masked-input asp-for="HomePhoneNumber" />
```

Both inputs render; the script renders once.

## `teleport-script`

Scripts sitting mid-document can execute before the DOM is ready. `defer` helps; teleporting is the robust fix — it moves the script to the bottom of the page while keeping the source next to the markup it belongs to.

```cshtml
<script static-script type="text/javascript" render-once="Model" teleport-script>
    var selector = document.querySelector("[data-mask]");
    var im = new Inputmask("(000)-000-0000");
    im.mask(selector);
</script>
```

Add the collection point to your layout:

```cshtml
@* ~/Pages/Shared/_Layout.cshtml *@
...
@await RenderSectionAsync("Scripts", required: false)
<render-static-scripts />
</body>
```

Teleported scripts render there wrapped in `<!-- Static Scripts -->` / `<!-- Static Scripts End -->` comments.

Use exactly **one** `<render-static-scripts />` per layout — scripts are consumed from a shared list in `HttpContext.Items`, so a second one renders nothing. If nothing was teleported it suppresses its own output entirely.

Note that `teleport-script` alone still renders the script once per component instance. Combine it with `render-once` to also deduplicate.

## `disable-render`

Suppresses the script entirely. The usual case is a component rendered into an HTMX or AJAX partial response, where the page already loaded the script: send a custom header on the request, read it in the view, and set `disable-render` from it.

## Injecting server-side values: `JavascriptConvert`

`JavascriptConvert.SerializeObject()` renders a C# object as a JS Object Literal — not a JSON string, so no `JSON.parse()` is needed.

```cshtml
<script static-script type="text/javascript" teleport-script>
    Alpine.data('gallery', () => ({
        imageGallery: @(JavascriptConvert.SerializeObject(Model.ImageList)),
        // ...
    }))
</script>
```

| Call | Output | Use when |
|---|---|---|
| `SerializeObject(value)` | `{ stringValue: "Test", items: ["One", "Two"] }` | Outside HTML attributes, where double quotes don't disrupt the markup. |
| `SerializeObject(value, true)` | `{ stringValue: 'Test', items: ['One', 'Two'] }` | **Inside HTML attributes** — e.g. an Alpine `x-data="…"` — where single-quoted strings are required. |

```cshtml
<div x-data="@JavascriptConvert.SerializeObject(Model.BannerState, true)" x-show="bannerVisible">
```

Property names are camel-cased in the output.

> Uses Newtonsoft.Json internally. Apply `Newtonsoft.Json` attributes (not `System.Text.Json`) for serialization control — the latter are ignored.

**Security**: always encode untrusted or user-supplied values before embedding them in a JS object literal, or you're writing an XSS hole.
