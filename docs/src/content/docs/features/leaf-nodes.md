---
title: 'Leaf Nodes'
---

Leaf nodes are an implementation of a Static Component that doesn't allow the use of Child Content or Slots. This is useful to avoid clutter and to make developer intent clear when authoring a Static Component. Other than this they behave in the same way that Static Components do.

Leaf nodes use the `StaticNode` as a base class instead of `StaticComponent`. 

They can be implemented like this:

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.StaticComponents;

namespace Sample.Views.Components;

public class HelloWorldComponent : StaticNode
{
    public string GreetMessage { get; set; }
}
```

```razor 
<!-- ~/Pages/Components/HelloWorldComponent.cshtml -->

@using Sample.Views.Components;
@model HelloWorldComponent

<div>Hello world! @Model.GreetMessage</div>
```


