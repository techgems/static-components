---
title: 'Advanced Usage'
---

## Overriding ProcessAsync

As you may be aware, the method `ProcessAsync` is an essential part for implementing the functionality of a StaticComponent, which is ultimately just a fancy [tag helper](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-10.0).

If all you need to do is render a partial view as part of the functionality of your tag helper, then you are fine using the default implementation included in `StaticComponent`. However, if for some reason you need to do more with your tag helper, then you can override the default implementation.

Doing this will cause the rendering of the corresponding partial view to not happen, however, there are a couple of helper functions included in the base class that you can use to do render the partial view. It would look something like this:

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.RazorComponentTagHelpers;

namespace Sample.Views;

[HtmlTargetElement("hello-world")] //Using a custom name instead of the generated default
public class HelloWorldComponent : StaticComponent
{
    public HelloWorldComponent() : base("~/Views/HelloWorld.cshtml") //Specifying a route instead of using the inferred default route view
    {
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        //Your custom logic here...

        await base.RenderPartialView(output);
    }
}
```

The one thing that is worthy of note is that the implementation of `RenderPartialView` method does use the output and sets a couple of things for partial view rendering to work, specifically:

- output.Content: Which is set as the output of the partial view.
- output.TagName: Which is set to null.

If you need to avoid using `RenderPartialView` altogether, you can, the implementation isn't too complex since this library is a single file library.

You can also use the HtmlHelper method inside the base class directly by calling inside your implementation of `ProcessAsync`:

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.RazorComponentTagHelpers;

namespace Sample.Views;

[HtmlTargetElement("hello-world")] //Using a custom name instead of the generated default
public class HelloWorldComponent : StaticComponent
{
    public HelloWorldComponent() : base("~/Views/HelloWorld.cshtml") //Specifying a route instead of using the inferred default route view
    {
    }

    [HtmlAttributeName("render-alternate")]
    public bool RenderAlternateView { get; set; }       

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        //Your custom logic here...

        if(!RenderAlternateView) 
        {
            await base.RenderPartialView(output);
        }
        else 
        {
            var model = new ModelType() {
                Name = "Test",
                Message = "Sample message"
            };

            await base.RenderPartialView("~/Views/DifferentView.cshtml", output, model);
        }
    }
}
```

By overriding the default `ProcessAsync` you can take complete control of the way razor gets processed and how the output gets handled. Something to note is that to do this properly you require proper knowledge of how to write your own [Tag Helpers](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-10.0) in ASP.NET Core.