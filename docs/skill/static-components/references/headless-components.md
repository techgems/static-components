# Headless Components (form inputs & UI kits)

Headless components are abstract base classes that give you the scaffolding for input-like components — `name`, `id`, the bound value and the `data-val-*` validation attributes are wired for you. Inherit one, write the template once, and consumers bind with `asp-for` exactly as they would on a native input.

Namespace: `TechGems.StaticComponents.Headless`

## Base classes

| Class | Inherits | Purpose | Adds |
|---|---|---|---|
| `StaticInputBase` | `StaticComponent` | Root for any input-like component. | `InputExpression` (`asp-for`), `ShowLabel` (default `true`), `Disabled`, `AdditionalAttributes` |
| `StaticInput` | `StaticInputBase` | Text / number / date / email / password inputs. | — |
| `StaticCheckbox` | `StaticInputBase` | Checkbox inputs. | `Value`, `Checked` |
| `StaticRadio` | `StaticInputBase` | Radio inputs (one instance = one option). | `Value` (required) |
| `StaticSelect` | `StaticInputBase` | `<select>` elements. | `Items` (mapped from `asp-items`) |
| `StaticLabel` | `StaticComponent` | Standalone `<label>` components. | `For` (mapped from `asp-for`) |
| `StaticButton` | `StaticComponent` | `<button>` components. | `Type` (`button`/`submit`/`reset`), `Disabled` |

> `StaticLabel` and `StaticButton` do **not** inherit `StaticInputBase`, so they have no `AdditionalAttributes`.

## Why the `static-*` tag helpers exist

When a consumer writes `<pines-input asp-for="Form.Email">`, ASP.NET Core resolves the expression once and assigns the resulting `ModelExpression` to `InputExpression`. If you then write `asp-for="@Model.InputExpression"` on the inner `<input>`, the built-in helper resolves it a **second** time and reads the metadata of `ModelExpression` itself — not of `Form.Email`. The `name`, `id` and `data-val-*` all come out wrong.

The `static-*` helpers expect that doubly-wrapped expression, reach one layer deeper (`InputExpression.ModelExplorer.Model` is itself a `ModelExpression`) and emit the correct attributes against the original property.

**Inside a headless component's template, always use `static-for` (not `asp-for`).** Using `asp-for` there compiles and renders, but silently binds against the wrong metadata. Regular Razor views and pages keep using `asp-for` as normal.

## Matching tag helpers

Namespace: `TechGems.StaticComponents.Headless.TagHelpers`

| Tag helper | Target | What it does |
|---|---|---|
| `static-for` | `<input>` | Sets `type` (inferred from the model property or overridden via `type="…"`), `name`, `id`, `value` and `data-val-*`. Does **not** support checkbox or radio. |
| `static-checkbox-for` | `<input>` | Always emits `type="checkbox"`, `value="true"`, a paired hidden `value="false"` so unchecked submits, and `checked` from the bool model value. Only `bool` is allowed. |
| `static-radio-for` | `<input>` | Emits `type="radio"`. Requires an explicit `value="…"` and adds `checked` when the bound value matches it. |
| `static-for` | `<label>` | Sets `for` from the inner expression and uses the property's `[DisplayName]` (falling back to the property name) as the label text. |
| `static-for` | `<textarea>` | Same as on `<input>`, but only `string` is supported. |
| `static-for` + `static-items` | `<select>` | Renders `<option>` / `<optgroup>`. `static-items` takes an `IEnumerable<SelectListItem>` just like `asp-items`. |
| `static-attributes` | any element | Applies the consumer's passed-through attributes. See `attribute-passthrough.md`. |

`static-for` **owns the `type` attribute on inputs** — it always overwrites it. If you want consumers to lock an input to a specific HTML type, declare a property for it and write it into the template yourself.

## Building a text input

```csharp
// ~/Views/Components/PinesInput.cshtml.cs
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents.Headless;

namespace TechGems.PinesUI.Views.Components;

public class PinesInput : StaticInput
{
    [HtmlAttributeName("type")]
    public string? Type { get; set; }
}
```

```cshtml
@* ~/Views/Components/PinesInput.cshtml *@
@model PinesInput

@if (Model.ShowLabel)
{
    <label static-for="@Model.InputExpression"
           class="block text-sm font-medium text-gray-900"></label>
}

<input static-for="@Model.InputExpression"
       static-attributes="@Model.AdditionalAttributes"
       type="@Model.Type"
       disabled="@(Model.Disabled ? "disabled" : null)"
       class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm" />
```

```cshtml
<pines-input asp-for="Form.FullName"></pines-input>
<pines-input asp-for="Form.SignupEmail"></pines-input>   @* [DataType(EmailAddress)] → type="email" *@
<pines-input asp-for="Form.Password" type="password"></pines-input>
```

## Building a checkbox

```csharp
public class PinesCheckbox : StaticCheckbox { }
```

```cshtml
@model PinesCheckbox

<div class="flex items-center mb-4">
    <input static-checkbox-for="@Model.InputExpression"
           checked="@Model.Checked"
           disabled="@(Model.Disabled ? "disabled" : null)"
           class="w-4 h-4 rounded border-gray-300" />

    @if (Model.ShowLabel)
    {
        <label static-for="@Model.InputExpression"
               class="ml-2 text-sm font-medium text-gray-900"></label>
    }
</div>
```

```cshtml
<pines-checkbox asp-for="Form.AcceptsTerms"></pines-checkbox>
<pines-checkbox asp-for="Form.SendNewsletter" show-label="false"></pines-checkbox>
```

The rendered HTML carries the bound `name`, the sanitized `id`, a paired hidden input so the form posts a value even when unchecked, and `data-val-*` from any validation attributes on the property.

## Building a radio

One component instance represents a single option, so `value` is mandatory and `checked` is derived by comparing it to the bound model value.

```csharp
public class PinesRadio : StaticRadio { }
```

```cshtml
@model PinesRadio

<label class="flex items-center gap-2">
    <input static-radio-for="@Model.InputExpression"
           value="@Model.Value"
           disabled="@(Model.Disabled ? "disabled" : null)"
           class="h-4 w-4 border-gray-300" />
    @if (Model.ShowLabel)
    {
        <span class="text-sm">@Model.Value</span>
    }
</label>
```

```cshtml
<pines-radio asp-for="Form.Plan" value="basic"></pines-radio>
<pines-radio asp-for="Form.Plan" value="pro"></pines-radio>
<pines-radio asp-for="Form.Plan" value="enterprise"></pines-radio>
```

## Building a select

```csharp
public class PinesSelect : StaticSelect { }
```

```cshtml
@model PinesSelect

@if (Model.ShowLabel)
{
    <label static-for="@Model.InputExpression"
           class="block text-sm font-medium text-gray-900"></label>
}

<select static-for="@Model.InputExpression"
        static-items="@Model.Items"
        disabled="@(Model.Disabled ? "disabled" : null)"
        class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm">
</select>
```

```cshtml
<pines-select asp-for="Form.Department" asp-items="@Model.DepartmentOptions"></pines-select>
```

Items sharing a `SelectListGroup` reference render together inside a single `<optgroup>`, at the position of the group's first occurrence — matching the built-in `select` tag helper.

**Selection priority**: a bound model value matching an item wins, and every `SelectListItem.Selected` flag is ignored. Only when the model value is `null` or matches nothing does the helper honor `Selected`.

## Building a label and a button

```csharp
public class PinesLabel : StaticLabel { }   // exposes For, not InputExpression
public class PinesButton : StaticButton { }
```

```cshtml
@model PinesLabel
<label static-for="@Model.For" class="block text-sm font-medium text-gray-900"></label>
```

```cshtml
@model PinesButton
<button type="@Model.Type"
        disabled="@(Model.Disabled ? "disabled" : null)"
        class="inline-flex items-center rounded-md bg-neutral-900 px-4 py-2 text-sm text-white">
    @Model.ChildContent
</button>
```

```cshtml
<pines-button type="submit">Save changes</pines-button>
<pines-button type="button" disabled="true">Loading…</pines-button>
```

`StaticButton.Type` only accepts `"button"`, `"submit"` or `"reset"` — anything else throws at render time, so typos surface immediately instead of producing broken HTML.

## `HeadlessUtils.GetLabelContent(ModelExpression)`

Resolves label text the same way `static-for` on a `<label>` does — `[DisplayName]`, then the property name, then `string.Empty`. Useful when the label text goes somewhere that isn't a `<label>` element:

```cshtml
@using TechGems.StaticComponents.Headless
<span class="label">@HeadlessUtils.GetLabelContent(Model.InputExpression)</span>
```

## Validation attributes emitted

Every input/checkbox/radio/select/textarea helper reads the validation attributes on the **inner** property (the one the consumer targeted with `asp-for`) and emits:

| C# attribute | Emits |
|---|---|
| `[Required]` (and non-nullable value types) | `data-val-required` |
| `[EmailAddress]` | `data-val-email` |
| `[Url]` | `data-val-url` |
| `[Phone]` | `data-val-phone` |
| `[RegularExpression]` | `data-val-regex`, `data-val-regex-pattern` |
| `[Range]` | `data-val-range`, `data-val-range-min`, `data-val-range-max` |
| `[StringLength]` | `data-val-length`, `data-val-length-max`, `data-val-length-min` (when a minimum is set), `maxlength` |

`data-val="true"` is emitted whenever at least one of these is present, so jquery.validate.unobtrusive picks the field up.

## Reference implementation

[PinesUI](https://github.com/techgems/PinesUI) is a complete UI kit built on these base classes — a port of DevDojo's Pines to ASP.NET Core. It's the most useful reference for how the pieces fit together in a real library.
