---
name: static-components
description: >
  Use this skill whenever the user asks to create, build, or work with UI components in an ASP.NET Core project using the TechGems.StaticComponents library (also known as Static Components). Trigger this skill for any request involving: creating a new Static Component, adding slots or child content to a component, writing a component's Razor view (.cshtml), wiring up scripts with static-script, building form input components or a UI kit with the headless base classes (StaticInput, StaticCheckbox, StaticRadio, StaticSelect, StaticLabel, StaticButton), letting consumers pass HTML attributes through a component with static-attributes, integrating AlpineJS or HTMX with a component, or following the conventions of the HATS stack (HTMX + AlpineJS + TailwindCSS + Static Components). If the user mentions StaticComponent, StaticNode, RenderSlot, static-script, teleport-script, static-for, static-attributes, AdditionalAttributes, class-replace, or JavascriptConvert, always use this skill.
license: MIT
---

# Static Components

Static Components (`TechGems.StaticComponents`) is a minimalistic ASP.NET Core library that lets you write reusable UI components backed by Razor views. It extends ASP.NET Core Tag Helpers and is designed to synergize with AlpineJS, HTMX, and TailwindCSS (the **HATS stack**).

- **NuGet**: `TechGems.StaticComponents` (v{{PACKAGE_VERSION}})
- **Requires**: .NET 6 or above
- **Docs**: https://static-components.techgems.net/

## Reference files

Read the relevant file before answering in depth; do not guess at APIs.

| File | Covers |
| --- | --- |
| `references/headless-components.md` | The headless base classes for form inputs, the `static-*` tag helpers and why they exist, a worked example per element type, the `data-val-*` attributes emitted |
| `references/attribute-passthrough.md` | `AdditionalAttributes` and `static-attributes`, the merge rules, `class-replace`, routing attributes to different elements |
| `references/static-scripts.md` | `static-script`, `render-once`, `teleport-script`, `disable-render`, `<render-static-scripts />`, and `JavascriptConvert` |
| `references/alpine-and-htmx.md` | Passing server state into AlpineJS, Razor's `@@` escaping, where Alpine and HTMX attributes belong on a component |
| `references/overriding-rendering.md` | Overriding `ProcessAsync` and rendering an alternate view or model |

Everything below applies to every component regardless of the task.

---

## Installation

```bash
dotnet add package TechGems.StaticComponents --version {{PACKAGE_VERSION}}
```

In `_ViewImports.cshtml`, register tag helpers:

```cshtml
@addTagHelper *, YourRazorPagesProject.Web
@addTagHelper *, TechGems.StaticComponents
```

---

## Core Convention: Two-File Structure

Every component consists of **two co-located files** inside `Pages/` or `Views/`:

| File | Purpose |
|---|---|
| `HelloWorldComponent.cshtml.cs` | C# class inheriting `StaticComponent` |
| `HelloWorldComponent.cshtml` | Razor view using the class as `@model` |

### Naming & Tag Inference

By default (no `[HtmlTargetElement]`), the library **infers** the tag name and view path from the class name:

- Class `HelloWorldComponent` → tag `<hello-world-component>`
- View auto-discovered at `~/Pages/Components/HelloWorldComponent.cshtml` (must be next to the code-behind)
- Properties become kebab-case HTML attributes: `GreetMessage` → `greet-message`

```csharp
// ~/Pages/Components/HelloWorldComponent.cshtml.cs
using TechGems.StaticComponents;

namespace Sample.Pages.Components;

public class HelloWorldComponent : StaticComponent
{
    public HelloWorldComponent() { }

    public string GreetMessage { get; set; }
}
```

```cshtml
@* ~/Pages/Components/HelloWorldComponent.cshtml *@
@using Sample.Pages.Components
@model HelloWorldComponent

<div>Hello world! @Model.GreetMessage</div>
```

Usage:

```cshtml
<hello-world-component greet-message="Hello there!"></hello-world-component>
```

The view route is derived from the class's full namespace: the assembly name becomes `~`, dots become `/`, and `.cshtml` is appended — so `YourAssembly.Views.Components.AlertComponent` resolves to `~/Views/Components/AlertComponent.cshtml`.

---

## Overriding Tag Name, View Route, or Attribute Names

Use `[HtmlTargetElement]` to set a custom tag name and pass a custom path to `base(...)`:

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents;

namespace Sample.Views;

[HtmlTargetElement("hello-world")]
public class HelloWorldComponent : StaticComponent
{
    public HelloWorldComponent() : base("~/Views/HelloWorld.cshtml") { }

    [HtmlAttributeName("message")]
    public string GreetMessage { get; set; }
}
```

Usage:

```cshtml
<hello-world message="Hello there!"></hello-world>
```

---

## Child Content

Any HTML placed between a component's opening and closing tags becomes `@Model.ChildContent`:

```csharp
public class OutlinedButton : StaticComponent
{
    public OutlinedButton() { }
}
```

```cshtml
@model YourAssembly.Views.OutlinedButton

<button type="button" class="...">
    @Model.ChildContent
</button>
```

```cshtml
<outlined-button>
    <strong>Click Me!</strong>
</outlined-button>
```

- Check `Model.IsChildContentNullOrEmpty` to conditionally render fallback content.

---

## Slots

Slots are **named regions** of HTML within a component — separate from `ChildContent`. They are declared at call-site with `<slot name="...">` and rendered in the view via `@Model.RenderSlot("slotName")`.

### Component class (best practice: use `static readonly` constants for slot names)

```csharp
using TechGems.StaticComponents;

namespace YourAssembly.Views.Components;

public class SidebarComponent : StaticComponent
{
    public static readonly string MobileSlot = "mobileLinks";
    public static readonly string DesktopSlot = "desktopLinks";

    public SidebarComponent() { }
}
```

### Razor view

```cshtml
@using YourAssembly.Views.Components
@model SidebarComponent

<nav>
    <div id="mobile">
        @if (Model.IsSlotContentNullOrEmpty(SidebarComponent.MobileSlot))
        {
            <a href="#">Home (Fallback)</a>
        }
        else
        {
            @Model.RenderSlot(SidebarComponent.MobileSlot)
        }
    </div>
    <div id="desktop">
        @Model.RenderSlot(SidebarComponent.DesktopSlot)
    </div>
</nav>
```

### Usage

```cshtml
<sidebar-component>
    <slot name="@SidebarComponent.MobileSlot">
        <a href="#">Home</a>
    </slot>
    <slot name="@SidebarComponent.DesktopSlot">
        <a href="#">Home</a>
        <a href="#">About</a>
    </slot>
</sidebar-component>
```

> **Key rule**: Slots are popped out of `ChildContent` — they will NOT render unless `RenderSlot` is explicitly called. Both slots and `ChildContent` can coexist in the same component.

Slot names must be unique within a single parent component, and `<slot>` must carry a non-empty `name` — both throw `ArgumentException` at runtime otherwise. `RenderSlot` throws `ArgumentException` for a name that was never declared, so guard optional slots with `IsSlotContentNullOrEmpty`.

---

## Leaf Nodes (`StaticNode`)

`StaticNode` is a Static Component without composition: **no `ChildContent`, no slots**. Use it for self-contained components so the API makes the intent obvious — consumers can see at a glance that no inner HTML is expected. Everything else (view route convention, `@model`, kebab-case attributes, `RenderPartialView`) behaves identically to `StaticComponent`.

```csharp
using TechGems.StaticComponents;

namespace Sample.Views.Components;

public class AvatarComponent : StaticNode
{
    public string ImageUrl { get; set; }
    public string Name { get; set; }
}
```

```cshtml
@* ~/Views/Components/AvatarComponent.cshtml *@
@using Sample.Views.Components
@model AvatarComponent

<div class="avatar">
    <img src="@Model.ImageUrl" alt="@Model.Name" />
    <span>@Model.Name</span>
</div>
```

```cshtml
<avatar-component image-url="/img/photo.jpg" name="Jane" />
```

If the component later needs child content or slots, switch the base class to `StaticComponent`.

---

## Choosing a base class

| Building | Base class | Reference |
|---|---|---|
| A component that wraps inner HTML | `StaticComponent` | this file |
| A self-contained component with no inner HTML | `StaticNode` | this file |
| A text/number/date/email input for a UI kit | `StaticInput` | `references/headless-components.md` |
| A checkbox, radio or select | `StaticCheckbox` / `StaticRadio` / `StaticSelect` | `references/headless-components.md` |
| A standalone label or button | `StaticLabel` / `StaticButton` | `references/headless-components.md` |

---

## Quick Reference Checklist

When creating a Static Component, always verify:

- [ ] Class inherits `StaticComponent` (or `StaticNode` for a leaf component) and lives in `Pages/` or `Views/`
- [ ] A corresponding `.cshtml` file exists next to the code-behind
- [ ] The `.cshtml` has `@model YourComponentClass` at the top
- [ ] `_ViewImports.cshtml` includes both `@addTagHelper` lines
- [ ] Slot names use `static readonly string` constants (not magic strings)
- [ ] `<render-static-scripts />` is in `_Layout.cshtml` when using `teleport-script`
- [ ] `@@` is used instead of `@` for AlpineJS event bindings in Razor views

When building an input component for a UI kit, also verify:

- [ ] The class inherits the right headless base (`StaticInput` / `StaticCheckbox` / `StaticRadio` / `StaticSelect` / `StaticLabel` / `StaticButton`)
- [ ] The template uses `static-for` / `static-checkbox-for` / `static-radio-for` / `static-items` — never `asp-for` on the inner element
- [ ] `static-attributes="@Model.AdditionalAttributes"` is applied so consumers can pass HTML through
- [ ] Alpine/HTMX attributes are routed to the wrapper with `WithPrefix("x-", "hx-")` when the component renders one
- [ ] `Model.ShowLabel` and `Model.Disabled` are honored in the template
- [ ] A C# property was only added for attributes the component actually acts on
