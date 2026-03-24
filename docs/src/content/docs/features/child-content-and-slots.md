---
title: 'Child Content and Slots'
---

Similar to how you can have child content and slots in Single Page Application frameworks, Static Components allows you to do this as well. Static Components has the `StaticComponentSlot` component. It can be used only inside classes that inherit from `StaticComponent`.

## Child Content vs Slots

The main difference between child content and slots is the way that they're used. ChildContent is shown by using the expression `@Model.ChildContent` in razor. This expression is only possible to use when the model to the cshtml view inherits from the `StaticComponent` class. Slots are not considered child content and will not be rendered along with child content. Slots are named pieces of HTML that you can invoke in your component wherever you like. They allow for better expression of intent in the UI.

This means you can use both ChildContent and Slots in the same component. Often child content is used when creating things such as button component in which you want to allow the user to customize HTML inside of an existing template.

## Slots

Slots work a little bit different than ChildContent. To use a slot you will need to run the `RenderSlot` function, with a valid slot name in the component. It is important to know that when slots an child content are combined, all slots are popped out of the child content, so they will not render unless `RenderSlot` is called explicitly.

The following sample is a no-styles sidebar component with two slots, one for a mobile and desktop nav items.

```csharp
// ~/Views/Components/SidebarComponent.cshtml.cs
using TechGems.StaticComponents;

namespace YourAssembly.Views.Components;

public class SidebarComponent : StaticComponent
{
    public SidebarComponent()
    {
    }
}
```

Then you need a view like this one:

```razor
<!-- ~/Views/Components/SidebarComponent.cshtml -->
@using YourAssembly.Views.Components;
@model SidebarComponent

<nav class="cardHeader">
    <div id="mobile">
        @Model.RenderSlot("mobileLinks")
    </div>
    <div id="dekstop">
        @Model.RenderSlot("desktopLinks")
    </div>
</nav>
```

Finally, the `SidebarComponent` would be used like this:

```razor
<!-- ~/Pages/YourView.cshtml -->
<sidebar-component>
    <slot name="mobileLinks">
        <a href="#">Home</a>
        <a href="#">About</a>
    </slot>
    <slot name="desktopLinks">
        <a href="#">Home</a>
        <a href="#">About</a>
        <a href="#">Contact</a>
    </slot>
</sidebar-component>
```

You will see that in the `card-sample-component` there is no reference to the slots and there shouldn't be, since slots are a declarative UI concept. But we do see them in the declarative use of the tag helper as well as in the razor template.

The finalized output of the previous declaration is the following:

```html
<nav class="cardHeader">
    <div id="mobile">
        <a href="#">Home</a>
        <a href="#">About</a>
    </div>
    <div id="dekstop">
        <a href="#">Home</a>
        <a href="#">About</a>
        <a href="#">Contact</a>
    </div>
</nav>
```

UI composition with slots can be incredibly useful when building UI elements that need many levels of customization. As I mentioned above, it is also possibly to combine slots with ChildContent in cases where it would benefit the readability of the component. This feature is particularly useful when creating reusable components. 

## Fallback content

Sometimes, when you use slots, you will want content fallbacks in case it doesn't make sense to just not display any content.

Fallbacks exist both for child content and for slots.

We will use the same example component above, but we will only change the razor template to look like this:

```razor
<!-- ~/Views/Components/SidebarComponent.cshtml -->
@using YourAssembly.Views.Components;
@model SidebarComponent

<nav class="cardHeader">
    <div id="mobile">
        @if (Model.IsSlotContentNullOrEmpty("mobileLinks"))
        {
            <a href="#">Home Mobile Fallback</h5>
        }
        else
        {
            @Model.RenderSlot("mobileLinks")
        }
    </div>
    <div id="dekstop">
        @if (Model.IsSlotContentNullOrEmpty("desktopLinks"))
        {
            <a href="#">Home Desktop Fallback</h5>
        }
        else
        {
            @Model.RenderSlot("desktopLinks")
        }
    </div>
</nav>
```

We have some additional functions that allow us to determine if a slot is actually being used or not, the same applies with child content.

By using the function `IsSlotContentNullOrEmpty` you can determine if the slot was declared or if it wasn't. Likewise, normal child content can be validated with the property `IsChildContentNullOrEmpty`.

## Best practice for slot names

A good approach to not end up using magic strings everywhere when writing components that use slots is to use static string variables for the names of the allowed slots.

Going to our first example we can refactor it into this:

```csharp
// ~/Views/Components/SidebarComponent.cshtml.cs
using TechGems.StaticComponents;

namespace YourAssembly.Views.Components;

public class SidebarComponent : StaticComponent
{
    public static readonly string MobileSlot = "mobileLinks";
    public static readonly string DesktopSlot = "desktopLinks";

    public SidebarComponent()
    {
    }
}
```

```razor
<!-- ~/Views/Components/SidebarComponent.cshtml -->
@using YourAssembly.Views.Components;
@model SidebarComponent

<nav class="cardHeader">
    <div id="mobile">
        @if (Model.IsSlotContentNullOrEmpty(SidebarComponent.MobileSlot))
        {
            <a href="#">Home Mobile Fallback</h5>
        }
        else
        {
            @Model.RenderSlot(SidebarComponent.MobileSlot)
        }
    </div>
    <div id="dekstop">
        @if (Model.IsSlotContentNullOrEmpty(SidebarComponent.DesktopSlot))
        {
            <a href="#">Home Desktop Fallback</h5>
        }
        else
        {
            @Model.RenderSlot(SidebarComponent.DesktopSlot)
        }
    </div>
</nav>
```

This is a simple approach to make sure you don't find errors with your slot names due to a typo and also to make your components easy to refactor. Just make sure to also use the static constants anywhere you declare the component:

```razor
<!-- ~/Pages/YourView.cshtml -->
<sidebar-component>
    <slot name="@SidebarComponent.MobileSlot">
        <a href="#">Home</a>
        <a href="#">About</a>
    </slot>
    <h4>Child Content</h4>
    <slot name="@SidebarComponent.DesktopSlot">
        <a href="#">Home</a>
        <a href="#">About</a>
        <a href="#">Contact</a>
    </slot>
</sidebar-component>
```