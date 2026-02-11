---
title: 'Basic Usage'
---

Static Components are extensions of ASP.NET Core Tag Helpers. 

To build a component you will need two files:

- A C# View (cshtml file)
- A code-behind file that inherits from `StaticComponent`

Following ASP.NET Core convention, these files must be stored inside the `Pages` or `Views` folders.


```csharp
// ~/Pages/Components/HelloWorldComponent.cshtml.cs

using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents;

namespace Sample.Views.Components;

public class HelloWorldComponent : StaticComponent
{
    public HelloWorldComponent()
    {
    }

    public string GreetMessage { get; set; }
}
```

```razor 
<!-- ~/Pages/Components/HelloWorldComponent.cshtml -->

@using Sample.Views.Components;
@model HelloWorldComponent

<div>Hello world! @Model.GreetMessage</div>
```

You would then be able to use your component like this:

```razor
<!-- ~/Pages/TestYourComponents.cshtml -->

<h2>Component Example</h2>

<hello-world-component greet-message="Hello there!"></hello-world-component>
```

It is possible to override the default view route, tag name and parameter name by specifying the route to the razor view of your choosing in the base constructor, like this:

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents;

namespace Sample.Views;

[HtmlTargetElement("hello-world")]
public class HelloWorldComponent : StaticComponent
{
    public HelloWorldComponent() : base("~/Views/HelloWorld.cshtml")
    {
    }

    [HtmlAttributeName("message")]
    public string GreetMessage { get; set; }
}
```

This should in turn be called like this:

```razor
<!-- ~/Pages/TestYourComponents.cshtml -->

<h2>Component Example</h2>

<hello-world message="Hello there!"></hello-world>

```

It's important to note that your razor template will be rendered as a partial that must use the component class as it's model. It is possible to override this, but more on that can be found in [Advanced usage](/advanced-usage).

## Child Content

Let's now build a more useful component.

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents;

namespace YourAssembly.Views;

public class OutlinedButton : StaticComponent
{
    public OutlinedButton() 
    {
    }
}
```

The html in the view includes a bit of TailwindCSS code, but you can choose not to use it if you like:

```razor
@model YourAssembly.Views.OutlinedButtonComponent
@{
    var colorClasses = "text-neutral-900 hover:text-white border-neutral-900 hover:bg-neutral-900";
    var outlinedStyles = "inline-flex items-center justify-center px-4 py-2 text-sm font-medium tracking-wide transition-colors rounded-md bg-white border-2 text-neutral-900 hover:text-white border-neutral-900 hover:bg-neutral-900";
}

<button type="button" class="@outlinedStyles @colorClasses duration-100">
    @Model.ChildContent
</button>
```

and you'd be able to invoke it in your code like this:

```razor
<outlined-button>
  <strong>Click Me!</strong>
</outlined-button>
```

### Summary

These examples should cover most of use cases for this library. However, you can override default behaviour should you need to. Look into [Advanced Usage](/docs/advanced-usage) for more information about overriding more defaults.