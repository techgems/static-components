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

## Best practices for injecting Server-Side values into a script

Injecting server-side state into your JavaScript is both a great way to add additional flexibility to your applications and a great way to create messy code. From version 1.1.0, Static Components contains additional functionality to streamline the process of adding server-side values into your JS code.

Let's implement a component from [Pines UI](https://devdojo.com/pines/docs/image-gallery) for this example (this assumes you have a valid installation of TailwindCSS and AlpineJS):

```csharp
using System;
using System.Collections.Generic;
using TechGems.StaticComponents;

namespace PinesUI.StaticComponents.Views.Components.PinesImageGallery;

public class PinesImageGallery : StaticComponent
{
    public List<GalleryItem> ImageList { get; set; } = new List<GalleryItem>();
}
```

```razor
@using PinesUI.StaticComponents.Views.Components.PinesImageGallery;
@using TechGems.StaticComponents;
@model PinesImageGallery;

<div x-data="gallery"
    @@image-gallery-next.window="imageGalleryNext()"
    @@image-gallery-prev.window="imageGalleryPrev()"
    @@keyup.right.window="imageGalleryNext()"
    @@keyup.left.window="imageGalleryPrev()"
    class="w-full h-full select-none">
    <div class="mx-auto max-w-6xl duration-1000 delay-300 select-none ease animate-fade-in-view" style="translate: none; rotate: none; scale: none; opacity: 1; transform: translate(0px, 0px);">
        <ul x-ref="gallery" id="gallery" class="grid grid-cols-2 gap-5 lg:grid-cols-5">
            <template x-for="(image, index) in imageGallery">
                <li><img x-on:click="imageGalleryOpen" :src="image.photo" :alt="image.alt" :data-index="index+1" class="object-cover select-none w-full h-auto bg-gray-200 rounded cursor-zoom-in aspect-[5/6] lg:aspect-[2/3] xl:aspect-[3/4]"></li>
            </template>
        </ul>
    </div>
    <template x-teleport="body">
        <div x-show="imageGalleryOpened"
             x-transition:enter="transition ease-in-out duration-300"
             x-transition:enter-start="opacity-0"
             x-transition:leave="transition ease-in-in duration-300"
             x-transition:leave-end="opacity-0"
             @@click ="imageGalleryClose"
             @@keydown.window.escape ="imageGalleryClose"
             x-trap.inert.noscroll="imageGalleryOpened"
             class="fixed inset-0 z-[99] flex items-center justify-center bg-black/50 select-none cursor-zoom-out" x-cloak>
            <div class="flex relative justify-center items-center w-11/12 xl:w-4/5 h-11/12">
                <div @@click ="$event.stopPropagation(); $dispatch('image-gallery-prev')" class="flex absolute left-0 justify-center items-center w-14 h-14 text-white rounded-full translate-x-10 cursor-pointer xl:-translate-x-24 2xl:-translate-x-32 bg-white/10 hover:bg-white/20">
                    <svg class="w-6 h-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" /></svg>
                </div>
                <img x-show="imageGalleryOpened"
                     x-transition:enter="transition ease-in-out duration-300"
                     x-transition:enter-start="opacity-0 transform scale-50"
                     x-transition:leave="transition ease-in-in duration-300"
                     x-transition:leave-end="opacity-0 transform scale-50"
                     class="object-contain object-center w-full h-full select-none cursor-zoom-out" :src="imageGalleryActiveUrl" alt="" style="display: none;">
                <div @@click ="$event.stopPropagation(); $dispatch('image-gallery-next');" class="flex absolute right-0 justify-center items-center w-14 h-14 text-white rounded-full -translate-x-10 cursor-pointer xl:translate-x-24 2xl:translate-x-32 bg-white/10 hover:bg-white/20">
                    <svg class="w-6 h-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" /></svg>
                </div>
            </div>
        </div>
    </template>
</div>
<script static-script type="text/javascript" teleport-script>
    document.addEventListener('alpine:init', () => {
        Alpine.data('gallery', () => ({
            imageGalleryOpened: false,
            imageGalleryActiveUrl: null,
            imageGalleryImageIndex: null,
            imageGallery: @(JavascriptConvert.SerializeObject(Model.ImageList)),
            imageGalleryOpen(event) {
                this.imageGalleryImageIndex=event.target.dataset.index;
                this.imageGalleryActiveUrl=event.target.src;
                this.imageGalleryOpened=true;
            },
            imageGalleryClose() {
                this.imageGalleryOpened=false;
                setTimeout(()=>
                this.imageGalleryActiveUrl = null, 300);
            },
            imageGalleryNext(){
                this.imageGalleryImageIndex = (this.imageGalleryImageIndex == this.imageGallery.length) ? 1 : (parseInt(this.imageGalleryImageIndex) + 1);
                this.imageGalleryActiveUrl = this.$refs.gallery.querySelector('[data-index=\'' + this.imageGalleryImageIndex + '\']').src;
            },
            imageGalleryPrev() {
                this.imageGalleryImageIndex = (this.imageGalleryImageIndex == 1) ? this.imageGallery.length : (parseInt(this.imageGalleryImageIndex) - 1);
                this.imageGalleryActiveUrl = this.$refs.gallery.querySelector('[data-index=\'' + this.imageGalleryImageIndex + '\']').src;
            }
        }))
    })
</script>
```

Static Components contains the helper function `JavascriptConvert.SerializeObject()`, which will serialize any C# object into [JS Object Literals](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Working_with_objects#using_object_initializers). JSON serialization is possible, but insufficient when writing scripts, since it is not immediately compatible with JS unless you call `JSON.parse()`. `JavascriptConvert.SerializeObject()` exists in order to skip that step and add better ergonomics when mixing C# server side values into a script that will render on the client. 

One important thing to consider is that Static Components uses [Newtonsoft.Json](https://www.newtonsoft.com/json) internally to serialize your CLR objects into JS objects, if you want to customize the way that the serialization happens, you will need to use Newtonsoft.Json attributes, not `System.Text.Json` attributes, as those will be ignored.

> **Note:** Be careful when injecting user-supplied or untrusted values. If a server-side value could contain single quotes, special characters, or HTML, make sure it is properly encoded before embedding it in a JavaScript string literal to avoid syntax errors or XSS vulnerabilities.

