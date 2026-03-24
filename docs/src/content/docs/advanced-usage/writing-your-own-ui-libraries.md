---
title: 'Writing your own UI libraries'
---

## Razor Class Libraries

Static Components has been optimized for the usage of [Razor Class Libraries](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-10.0&tabs=visual-studio). In fact, as a proof of concept, and for personal use, I wrote an implementation of [DevDojo's PinesUI](https://devdojo.com/pines) by making it native to ASP.NET Core and utilizing all the quality of life features that Static Components allows you.

You can find that library [here](https://pines-ui.techgems.net/), feel free to use it in your applications, but also as a [reference](https://github.com/techgems/PinesUI) of how to use Static Components effectively when building a UI library.

Razor Class Libraries are useful for creating UI component libraries meant to be reused, this can be a great way to isolate your UI-only components from your application's presentation components.

Razor Class Libraries can be combined with your CSS library of choice to make UI elements easier to use.


## Best Practices

This is just a summary of habits you will find useful when writing components that need to be maintained.

- Use [Static Scripts](/features/static-scripts) extensively. They make it easier to keep related code in the same place, while separating it properly for HTML optimization.
- When using [AlpineJS](https://alpinejs.dev/), try to use [`Alpine.data`](https://alpinejs.dev/globals/alpine-data) and [`Alpine.store`](https://alpinejs.dev/globals/alpine-store) over defining in-line state with `x-data`. This will make your code easier to understand and maintain.
- Do not override the Static Component pipeline unless you *really* need to do so. It genuinely fits 99% of the scenarios in which implementing a component makes sense.

#### ASP.NET Core Tag Helper Support

When using Razor Pages or MVC it is important to use ASP.NET Core official tag helpers for link generation, or making forms easier to work with, here are some tips that can make it easier to create your own custom link and form input components

- `asp-for`, used in input tags, can be configured into a custom Static Component by using the type `ModelExpression`. For a clearer example, see [here](https://github.com/techgems/PinesUI/tree/master/Views/Components/PinesInput).
- `asp-controller`, `asp-action` and `asp-page` can also be configured to be used in a custom Static Component. You can find an examples of that [here](https://github.com/techgems/PinesUI/blob/master/Views/Components/PinesSidebar/PinesSidebarLink.cshtml.cs) and [here](https://github.com/techgems/PinesUI/blob/master/Views/Components/PinesSidebar/PinesSidebarLink.cshtml) 
- In general, maintaining ASP.NET Core conventions of usage will make your components better and more consistent for your users.


