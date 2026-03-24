---
title: 'Static Scripts'
---

Static Components gives you a set of tools to do more with scripts than ASP.NET Core allows you. Static Components are essentially compiler enhanced versions of Razor Partials. Normally partials cannot use Razor sections, which is why using scripts in partials is normally discouraged.

The `static-script` tag helper was created to alleviate that problem. It allows you to "teleport" a script so that it is not rendered inline. It allows you to tell a component when a script inside a static component should only be rendered once. This gives you a lot of flexibility when it comes to adding javascript to your static components, particularly by keeping related code in the same place. Static scripts should work both when used inside Static Components as well as regular partial views or ViewComponents.

The `static-script` tag helper on it's own does nothing though, but by including it allows you to use the following properties on a regular `<script>` tag.

- `render-once`
- `teleport-script`
- `disable-render`

We will cover these properties shortly, but this would be an example of what a `static-script` looks like:

```razor
@model MyComponentClass
<div id="my-component">
    <!-- component markup -->
</div>

<script static-script type="text/javascript" render-once="Model" teleport-script disable-render="false">
    document.getElementById('my-component').style.color = 'red';
</script>
```

## `render-once`

One of the most important behaviors of `static-script` is script deduplication via the `render-once` attribute. `render-once` is supposed to receive the view model as a parameter, which will extract the type and make sure that the given script only gets rendered once regardless of the amount of times that the static component gets rendered.

To provide a better example, let's do an integration with the [Input Mask](https://robinherbots.github.io/Inputmask/#/documentation) library, for this we will assume that the JS and CSS dependencies are already in place in the project:

```csharp
using TechGems.StaticComponents;

[HtmlTargetElement("masked-input")]
public class MaskedInputComponent : StaticComponent {

    //Allows the usage of this component similarly to how you would use the asp-for tag helper in any other input tag.
    //This is a useful way of creating input based controllers without losing the ergonomics of asp-for
    [HtmlAttributeName("asp-for")]
    public ModelExpression? InputExpression { get; set; }
}

```

```razor
@model MaskedInputComponent

<input asp-for="Model.InputExpression" class="autocomplete" data-mask />

<script static-script type="text/javascript" render-once="Model" defer>
    var selector = document.querySelector("[data-mask]");

    var im = new Inputmask("(000)-000-0000");
    im.mask(selector);
</script>
```

Let us now use this component like this:

```razor
<!-- ~/Views/Test/Index.cshtml -->

@model MyViewModel

<masked-input asp-for="MobilePhoneNumber">
<masked-input asp-for="HomePhoneNumber">
```

Using a regular script, this would have resulted in the script being rendered twice, leading to issues with the script used. However, because we used `render-once`, the HTML output of our test view will be something like this:

```html
<input name="MobilePhoneNumber" />
<script type="text/javascript" defer> 
    var selector = document.querySelector("[data-mask]");

    var im = new Inputmask("(000)-000-0000");
    im.mask(selector);
</script>
<input name="HomePhoneNumber" />
```

Thus not resulting in script duplication, reducing JavaScript runtime errors and leading to a more intuitive code structure. 

One thing you might have noticed is that it's not a great practice to have scripts in the middle of your HTML, since they can get executed before you want them to be executed, you can use the defer attribute for that, but in the next section we will cover a different solution for that same purpose.

## `teleport-script`

The `teleport-script` attribute allows us to 'teleport' a script from the place in the HTML in which the static component gets called, to rendering the scripts for any component at the bottom of the HTML page. Let's use our previous input mask component example:

```razor
@model MaskedInputComponent

<input asp-for="Model.InputExpression" class="autocomplete" data-mask />

<script static-script type="text/javascript" render-once="Model" teleport-script>
    var selector = document.querySelector("[data-mask]");

    var im = new Inputmask("(000)-000-0000");
    im.mask(selector);
</script>
```

`teleport-script` is a boolean attribute, but it will be marked as true even without using `teleport-script="true"`. Now, the code above will work, but we will need to tell the application's layout where we want to place all the teleported scripts.

We can do this the following way:

```razor
<!-- ~/Pages/Shared/Layout.cshtml -->
<!DOCTYPE html>
<html lang="en">
    ...
    <!--Previous html here-->
    ...

    @await RenderSectionAsync("Scripts", required: false)
    <render-static-scripts />
</body>
</html>
```

Static Components includes a custom-built static component called `render-static-scripts` which renders all scripts that are meant to be teleported.

The final HTML output would then be something like this:

```html
<!DOCTYPE html>
<html lang="en">
    ...
    <!--Previous html here-->
    ...

    <!--Scripts from Scripts section would go here-->
    <!--Static Scripts Start-->
    <script type="text/javascript">
        var selector = document.querySelector("[data-mask]");

        var im = new Inputmask("(000)-000-0000");
        im.mask(selector);
    </script>
    <!--Static Scripts End-->
</body>
</html>
```

The teleported script in this example gets rendered only once due to the `render-once` attribute, without it, it will be teleported, but rendered as many times as the containing component got rendered.

## `disable-render`

Disable render is a boolean attribute meant to stop the script from rendering at all. This can be useful if you want to prevent the script from rendering for any reason. 

A potential use case might be not rendering a script when a static component is being used in an AJAX or HTMX request, at which point you can send custom headers in the request and read them in the view itself to set `disable-render` to true.

