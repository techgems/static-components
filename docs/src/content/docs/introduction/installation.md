---
title: 'Installation'
---

Installing Static Components is very easy. All you need is a .NET 8 or above project and to download the nuget package into:

```
dotnet add package TechGems.StaticComponents --version 1.0.0
```

Then you will need to go to the `_ViewImports.cshtml` file and add a reference to your project's namespace like so:

```
@addTagHelper *, YourRazorPagesProject.Web
@addTagHelper *, TechGems.StaticComponents
```

And you're done! You can continue in order to understand how to create components.