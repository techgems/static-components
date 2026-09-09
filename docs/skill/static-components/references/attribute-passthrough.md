# Attribute Passthrough

*New in v1.4.0.* A UI kit can't anticipate every attribute its users will need. Instead of adding a C# property for each of `placeholder`, `autocomplete`, `aria-describedby`, `data-*`, `x-on:input` and so on, let them pass straight through.

Every component inheriting `StaticInputBase` exposes `AdditionalAttributes` — a `StaticAttributeCollection` holding whatever the consumer wrote that wasn't bound to one of its properties. Apply it to an element with the `static-attributes` tag helper, which targets **any** element (a wrapper `<div>` just as well as the `<input>`):

```cshtml
@model PinesInput

<input static-for="@Model.InputExpression"
       static-attributes="@Model.AdditionalAttributes"
       placeholder="Type something"
       class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm" />
```

```cshtml
@* consumer view *@
<pines-input asp-for="Form.Email"
             class="mt-4"
             placeholder="you@example.com"
             autocomplete="email"
             data-testid="email"
             required />
```

```html
<!-- rendered -->
<input placeholder="you@example.com"
       class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm mt-4"
       autocomplete="email" data-testid="email" required
       type="email" name="Email" id="Email"
       data-val="true" data-val-email="…" />
```

The consumer's `placeholder` replaced the template's; their `class` was appended to it.

> **Passthrough is opt-in.** Nothing reaches the rendered element until you add `static-attributes` to the template, so adding it to an existing component changes nothing for current users until you do.

## Merge rules

| Attribute | Behavior |
|---|---|
| `class` | **Merged.** Template classes first, consumer's appended, duplicates dropped. |
| `class-replace` | **Replaces.** Discards the template's `class` entirely. Passing both `class` and `class-replace` throws `ArgumentException`. |
| `style` | **Merged**, joined with `;`. Inline styles are last-declaration-wins, so the consumer's values take effect. |
| `type` | **Never captured.** Owned by the `static-*` tag helpers. |
| `name`, `id`, `data-val-*` | **Owned by `static-for`**, which runs after `static-attributes` (`Order => -1000`) and overwrites them. A consumer can't break model binding from the outside. |
| everything else | **Consumer wins.** Template attributes act as defaults they can override. |

Anything bound to a C# property (`asp-for`, `show-label`, `disabled`, and any property you declare yourself) never reaches the collection either — it was consumed during model binding.

## `class-replace`: when merging isn't enough

Merging works when the consumer is *adding* something — a margin, a grid placement. It breaks down when they try to *override* one of your classes, a common Tailwind problem: CSS resolves the conflict by stylesheet order, not attribute order, so both classes end up on the element and the override may not take effect.

```cshtml
<pines-input asp-for="Form.Zip" class="w-32" />
<!-- rendered: class="… w-full … w-32" — still full width -->
```

`class-replace` discards your classes so there's nothing left to conflict with:

```cshtml
<pines-input asp-for="Form.Zip" class-replace="w-32 rounded-md border px-3 py-2" />
<!-- rendered: class="w-32 rounded-md border px-3 py-2" -->
```

It gives up every base style the component defines — note the consumer had to restate `rounded-md`, `border` and the padding to keep them. Reach for it only when a merge genuinely can't express the override.

## `StaticAttributeCollection` members

Namespace: `TechGems.StaticComponents.Headless`. Sealed, immutable, implements `IReadOnlyList<TagHelperAttribute>`. You never construct it — it's populated during rendering.

| Member | Returns |
|---|---|
| `Only(params string[] names)` | A new collection with only the named attributes. |
| `Except(params string[] names)` | A new collection without the named attributes. |
| `WithPrefix(params string[] prefixes)` | Only attributes whose names start with one of the prefixes. |
| `WithoutPrefix(params string[] prefixes)` | Everything but those. |
| `Contains(string name)` | Whether the attribute is present. |
| `this[string name]` | The attribute, or `null`. |
| `this[int index]` | The attribute at that position — order is the order the consumer wrote them in. |
| `Count`, `Empty` | Size, and a shared empty instance whose application is a no-op. |

Name comparison is case-insensitive throughout.

## Routing attributes to different elements

Every filter returns a new collection, so filters chain. This matters when a component renders a wrapper: Alpine and HTMX attributes act on the element they're written on, so not everything belongs on the `<input>`.

| Prefix | Library | What it does |
|---|---|---|
| `x-` | AlpineJS | Declares client-side state and reactivity — `x-show="open"`, `x-model="email"`. |
| `hx-` | HTMX | Turns an element into a request — `hx-get`, `hx-trigger`, `hx-target`. |

Both are identified by their prefix, which makes the usual split a one-liner:

```cshtml
@model PinesInput

<div class="relative" static-attributes="@Model.AdditionalAttributes.WithPrefix("x-", "hx-")">
    <label static-for="@Model.InputExpression" class="block text-sm font-medium"></label>

    <input static-for="@Model.InputExpression"
           static-attributes="@Model.AdditionalAttributes.WithoutPrefix("x-", "hx-")"
           class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm" />
</div>
```

```cshtml
<pines-input asp-for="Form.Email" x-show="wantsUpdates" hx-target="#email-feedback" placeholder="you@example.com" />
```

`x-show` and `hx-target` land on the `<div>`, `placeholder` on the `<input>`. The wrapper is the right home for both: `x-show` there hides the label along with the field instead of leaving a dangling label, and HTMX inherits most of its attributes from ancestors, so a request declared on the wrapper still governs the input inside it.

Use `Only` / `Except` for exact names when an attribute needs different placement than its prefix suggests — `x-model` two-way binds an input's value, so it has to stay on the `<input>` even when the other `x-` attributes belong on the wrapper:

```cshtml
<div static-attributes="@Model.AdditionalAttributes.Only("x-show", "x-transition")">
    <input static-for="@Model.InputExpression"
           static-attributes="@Model.AdditionalAttributes.Except("x-show", "x-transition")" />
</div>
```

## Guidance

- Add `static-attributes` to every input component you ship — you can't anticipate what users will need.
- Only add a C# property for an attribute when you need to **act** on its value (branch in the template, validate it, feed it to another tag helper). Anything you'd merely copy onto an element should pass through instead.
- To let consumers set an attribute the `static-*` helpers own (like `type`), declare a property for it and write it into the template explicitly.
- If you override `ProcessAsync` on a `StaticInputBase` component, call `base.ProcessAsync(context, output)` — that's where the collection is captured. See `overriding-rendering.md`.
