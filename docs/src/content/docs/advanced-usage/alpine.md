---
title: 'Static Components with AlpineJS'
--- 


## Basic Client-side interactions with AlpineJS

This approach isn't unique to this library or to [AlpineJS](https://alpinejs.dev/), but it's an approach that fits this kind of UI composition very well. You can also very easily do this with [Stimulus](https://stimulus.hotwired.dev/) and it should work well.

But essentially, these kinds of JS libraries allow you to write client-side logic without actually writing a script in a very declarative way which meshes very well with server rendered html.

A simple example could be a collapsible section component like the one below, we will be using AlpineJS 3, which means it will not work property if you don't have the script in your Html.

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechGems.RazorComponentTagHelpers;

namespace Sample.Views;

public class CollapsibleSectionComponent : StaticComponent
{
    public CollapsibleSectionComponent()
    {
    }

    [HtmlAttributeName("is-open")]
    public bool IsOpen { get; set; } = false;
}
```

And the partial view:

```razor
@using Sample.Views;
@using TechGems.StaticComponents;
@model CollapsibleSectionComponent


<div x-data="{ open: @JavascriptConvert.SerializeObject(Model.IsOpen, true) }">
    <div x-show="open">
        @Model.ChildContent
    </div>

    <button @@click="open = !open">Toggle</button>
</div>
```
`JavascriptConvert.SerializeObject` is a utility provided by Static Components that allows you to serialize an object into a Javascript Object Literal. This is easier than having to replicate and respect that syntax using razor.  

The component can now be used this way

```razor
<collapsible-section-component is-open="true">This content can be toggled on and off.</collapsible-section-component>
```

This sample doesn't have any proper styling, but it's sufficient enough to show how easy it can be to implement a reusable component.

## Writing a components with complex UI interactions

AlpineJS is a great way to create reusable components with minimal JS, but it's integration with Razor can be a bit clunky sometimes. Static Components provides tools that make this process easier and more maintainable. I'll provide an example from the development of PinesUI, in a step by step fashion, of how to implement such a component.

```razor
<!-- ~/Pages/Components/PinesImageGallery.cshtml -->
@using TechGems.PinesUI.Views.Components.PinesImageGallery;
@model PinesImageGallery;

<div x-data="{
        imageGalleryOpened: false,
        imageGalleryActiveUrl: null,
        imageGalleryImageIndex: null,
        imageGallery: [
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-01.jpeg',
                'alt': 'Photo of Mountains'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-02.jpeg',
                'alt': 'Photo of Mountains 02'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-03.jpeg',
                'alt': 'Photo of Mountains 03'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-04.jpeg',
                'alt': 'Photo of Mountains 04'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-05.jpeg',
                'alt': 'Photo of Mountains 05'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-06.jpeg',
                'alt': 'Photo of Mountains 06'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-07.jpeg',
                'alt': 'Photo of Mountains 07'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-08.jpeg',
                'alt': 'Photo of Mountains 08'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-09.jpeg',
                'alt': 'Photo of Mountains 09'
            },
            {
                'photo': 'https://cdn.devdojo.com/images/june2023/mountains-10.jpeg',
                'alt': 'Photo of Mountains 10'
            }
        ],
        imageGalleryOpen(event) {
            this.imageGalleryImageIndex = event.target.dataset.index;
            this.imageGalleryActiveUrl = event.target.src;
            this.imageGalleryOpened = true;
        },
        imageGalleryClose() {
            this.imageGalleryOpened = false;
            setTimeout(() => this.imageGalleryActiveUrl = null, 300);
        },
        imageGalleryNext(){
            this.imageGalleryImageIndex = (this.imageGalleryImageIndex == this.imageGallery.length) ? 1 : (parseInt(this.imageGalleryImageIndex) + 1);
            this.imageGalleryActiveUrl = this.$refs.gallery.querySelector('[data-index=\'' + this.imageGalleryImageIndex + '\']').src;
        },
        imageGalleryPrev() {
            this.imageGalleryImageIndex = (this.imageGalleryImageIndex == 1) ? this.imageGallery.length : (parseInt(this.imageGalleryImageIndex) - 1);
            this.imageGalleryActiveUrl = this.$refs.gallery.querySelector('[data-index=\'' + this.imageGalleryImageIndex + '\']').src;
            
        }
    }"
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
```

This sample is mostly a copy and pasted version from DevDojo's PinesUI. I took the liberty of making a minor change to make it work by escaping every `@` sign making it double, since both AlpineJS and Razor use the character. Alpine uses it as a short-hand for events and in razor it's how you invoke any kind of C# code.

However, a clever reader will have noticed that this component is completely useless if we can't pass configuration to it, so let's do that:

```csharp
// ~/Pages/Components/PinesImageGallery/GalleryItem.cs
using Newtonsoft.Json;

namespace TechGems.PinesUI.Views.Components.PinesImageGallery;

public class GalleryItem
{
    [JsonProperty("photo")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonProperty("alt")]
    public string ImageAlt { get; set; } = string.Empty;
}
```

```csharp
// ~/Pages/Components/PinesImageGallery/PinesImageGallery.cs
using System.Collections.Generic;
using TechGems.StaticComponents;

namespace TechGems.PinesUI.Views.Components.PinesImageGallery;

public class PinesImageGallery : StaticComponent
{
    public List<GalleryItem> ImageList { get; set; } = new List<GalleryItem>();
}

```

We now have a list parameter, we can use it the razor view to make the component dynamic:

```razor
<!-- ~/Pages/Components/PinesImageGallery.cshtml -->
@using TechGems.PinesUI.Views.Components.PinesImageGallery;
@using TechGems.StaticComponents;
@model PinesImageGallery;

<div x-data="{
        imageGalleryOpened: false,
        imageGalleryActiveUrl: null,
        imageGalleryImageIndex: null,
        imageGallery: @(JavascriptConvert.SerializeObject(Model.ImageList, true)),
        imageGalleryOpen(event) {
            this.imageGalleryImageIndex = event.target.dataset.index;
            this.imageGalleryActiveUrl = event.target.src;
            this.imageGalleryOpened = true;
        },
        ...
    }"
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
```

This will work for this component, particularly since we are using `JavascriptConvert.SerializeObject(Model.ImageList, true)`, by using the second parameter as `true`, we are telling the serialization to not produce double-quotes when serializing strings, which would break the configuration in `x-data`. However, on a larger component, with more parameters and or interactions, the values in `x-data` will eventually become a large and mostly unmaintainable list. To solve that problem, AlpineJS allows us to define our own reusable data objects:

```razor
@using TechGems.PinesUI.Views.Components.PinesImageGallery;
@using TechGems.StaticComponents;
@model PinesImageGallery;

<script static-script type="text/javascript" teleport-script render-once="Model">
    document.addEventListener('alpine:init', () => {
        Alpine.data('gallery', (imagesArray) => ({
            imageGalleryOpened: false,
            imageGalleryActiveUrl: null,
            imageGalleryImageIndex: null,
            imageGallery: imagesArray,
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

<div x-data="gallery(@JavascriptConvert.SerializeObject(Model.ImageList, true))"
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
```

By using the `alpine:init` event and the function `Alpine.data` we can remove the limitations of having an HTML property be a large clunky javascript object and making sure we are escaping all the characters and whatnot. One thing that must be highlighted is that AlpineJS has allowed us to create a `gallery` function, which receives an array. 

Now `x-data` uses the value `gallery(@JavascriptConvert.SerializeObject(Model.ImageList, true))`. This is much more easy to maintain and understand than the original component was, and by using `teleport-script` and `render-once`, we don't need to worry about the script getting duplicated if this component gets used many times in application code.

