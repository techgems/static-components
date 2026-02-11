# Static Components

---
Create ⚡️ Static ASP.NET Core Server Components ⚡️

---

Static Components is an extension to ASP.NET Core MVC and Razor Pages. It takes the concept of a tag helper and supercharges it to make it simple to write static server components. It has no dependencies and it is very lightweight, being composed of only 2 files.

Static Components may be small, but don't let that hide it's potential. It is the perfect tool for organizing your UI into decoupled reusable components, complementing HTMX workflows and even writing low-JS component libraries with tools such as [AlpineJS](https://alpinejs.dev/) or [Stimulus JS](https://stimulus.hotwired.dev/).

## Documentation

Simply create a CSHTML file like you would for a partial view. Then create a class attached to that partial view.

In our example we will create the following two files in the folder /Views

- HelloWorldComponent.cshtml
- HelloWorldComponent.cshtml.cs


```
public class HelloWorldComponent : StaticComponent
{
    public HelloWorldComponent()
    {
    }
}
```

```
@using YourAssembly.Web.Views;
@addTagHelper *, TechGems.StaticComponents;
@model HelloWorldComponent

<span>Hello world!</span>
```

With this in place, you can now call your component in any View or Razor Page like this:

```
@page
@model IndexModel
@{
    ViewData["Title"] = "Home page";
}

<h2>Hello World Component</h2>

<hello-world-component></hello-world-component>
```

There are more use cases and configurations you can set with Static Components. To see more, go to our [documentation](https://static-components.techgems.net).

