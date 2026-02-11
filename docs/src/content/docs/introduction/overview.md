---
title: 'Overview'
---

## Static Components

Static Components is a minimalistic ASP.NET Core library that allows you to write UI components while maintaining compatibility with Razor Pages and MVC. With this library you can create your own static components which makes it synergize perfectly with AlpineJS, HTMX and the "low-JS" approach of writing applications in general.


## How it works

Here is a sample of a basic component.

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