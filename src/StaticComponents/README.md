# Static Components

Static Components is a minimalistic ASP.NET Core library that allows you to write UI components while maintaining full compatibility with Razor Pages and MVC. With this library you can create your own static components, which makes it synergize perfectly with AlpineJS, HTMX, and the "low-JS" approach to building web applications.


## How it works

A component consists of two files: a C# code-behind class that inherits from `StaticComponent`, and a Razor view (`.cshtml`) that serves as its template. The class is automatically available as a tag helper in your Razor pages.

```
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

```
<!-- ~/Pages/Components/HelloWorldComponent.cshtml -->

@using Sample.Views.Components;
@model HelloWorldComponent

<div>Hello world! @Model.GreetMessage</div>
```


There are more use cases and configurations you can set with Static Components. To see more, go to our [documentation](https://static-components.techgems.net).

