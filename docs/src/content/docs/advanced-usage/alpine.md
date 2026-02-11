---
title: 'Static Components with AlpineJS'
--- 


## Client-side interactions with AlpineJS

This approach isn't unique to this library or to [AlpineJS](https://alpinejs.dev/), but it's an approach that fits this kind of UI composition very well. You can also very easily do this with [Stimulus](https://stimulus.hotwired.dev/) and it should work well.

But essentially, these kinds of JS libraries allow you to write client-side logic without actually writing a script in a very declarative way which meshes very well with server rendered html.

A simple example could be a collapsible section component like the one below, we will be using AlpineJS 3, which means it will not work property if you don't have the script in your Html.

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.RazorComponentTagHelpers;

namespace Sample.Views;

public class CollapsibleSectionComponent : StaticComponent
{
    public CollapsibleSectionComponent()
    {
    }

    [HtmlAttributeName("is-open")]
    public bool IsOpen { get; set; } = false;
}
```

And the partial view:

```html
@using Sample.Views;
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

And you should be able to call it this way:

```html
<collapsible-section-component is-open="true">This content can be toggled on and off.</collapsible-section-component>
```

This sample doesn't have any proper styling, but it's sufficient enough to show how easy it can be to implement a reusable component.