---
title: JavaScript Object Serialization
---

## How to properly inject Server-Side values into a script

Injecting server-side state into your JavaScript is both a great way to add additional flexibility to your applications and a great way to create messy code. From version onwards 1.1.0, Static Components contains additional functionality to streamline the process of adding server-side values into your JS code.

Let's implement a component from [Pines UI](https://devdojo.com/pines/docs/banner) for this example (this assumes you have a valid installation of TailwindCSS and AlpineJS):

```csharp
using System;
using System.Collections.Generic;
using TechGems.StaticComponents;

namespace TechGems.PinesUI.Views.Components.PinesBanner;

//AlpineJS State Class
public class PinesBannerState
{
    public bool BannerVisible { get; set; } = false;

    public int BannerVisibleAfter { get; set; } = 1000;
}

public class PinesBanner : StaticComponent
{
    [HtmlAttributeName("head")]
    public string BannerHead { get; set; }


    [HtmlAttributeName("body")]
    public string BannerBody { get; set; }


    [HtmlAttributeName("initial-state")]
    public PinesBannerState BannerState { get; set; } = new PinesBannerState();
}

```

```razor
<div x-data="@JavascriptConvert.SerializeObject(Model.BannerState)"
     x-show="bannerVisible"
     x-transition:enter="transition ease-out duration-500"
     x-transition:enter-start="-translate-y-10"
     x-transition:enter-end="translate-y-0"
     x-transition:leave="transition ease-in duration-300"
     x-transition:leave-start="translate-y-0"
     x-transition:leave-end="-translate-y-10"
     x-init="setTimeout(() => { bannerVisible = true }, bannerVisibleAfter);"
    class="fixed top-0 left-0 w-full h-auto py-2 duration-300 ease-out bg-white shadow-sm sm:py-0 sm:h-10 z-50" x-cloak>
    <div class="flex items-center justify-between w-full h-full px-3 mx-auto max-w-7xl ">
        <a href="#" class="flex flex-col w-full h-full text-xs leading-6 text-black duration-150 ease-out sm:flex-row sm:items-center opacity-80 hover:opacity-100">
            <span class="flex items-center">
                <svg class="w-4 h-4 mr-1" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g fill="none" stroke="none"><path d="M10.1893 8.12241C9.48048 8.50807 9.66948 9.5633 10.4691 9.68456L13.5119 10.0862C13.7557 10.1231 13.7595 10.6536 13.7968 10.8949L14.2545 13.5486C14.377 14.3395 15.4432 14.5267 15.8333 13.8259L17.1207 11.3647C17.309 11.0046 17.702 10.7956 18.1018 10.8845C18.8753 11.1023 19.6663 11.3643 20.3456 11.4084C21.0894 11.4567 21.529 10.5994 21.0501 10.0342C20.6005 9.50359 20.0352 8.75764 19.4669 8.06623C19.2213 7.76746 19.1292 7.3633 19.2863 7.00567L20.1779 4.92643C20.4794 4.23099 19.7551 3.52167 19.0523 3.82031L17.1037 4.83372C16.7404 4.99461 16.3154 4.92545 16.0217 4.65969C15.3919 4.08975 14.6059 3.39451 14.0737 2.95304C13.5028 2.47955 12.6367 2.91341 12.6845 3.64886C12.7276 4.31093 13.0055 5.20996 13.1773 5.98734C13.2677 6.3964 13.041 6.79542 12.658 6.97364L10.1893 8.12241Z" stroke="currentColor" stroke-width="1.5"></path><path d="M12.1575 9.90759L3.19359 18.8714C2.63313 19.3991 2.61799 20.2851 3.16011 20.8317C3.70733 21.3834 4.60355 21.3694 5.13325 20.8008L13.9787 11.9552" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"></path><path d="M5 6.25V3.75M3.75 5H6.25" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"></path><path d="M18 20.25V17.75M16.75 19H19.25" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"></path></g></svg>
                <strong class="font-semibold">@Model.BannerHead</strong><span class="hidden w-px h-4 mx-3 rounded-full sm:block bg-neutral-200"></span>
            </span>
            <span class="block pt-1 pb-2 leading-none sm:inline sm:pt-0 sm:pb-0">@Model.BannerBody</span>
        </a>
        <button @@click ="bannerVisible=false;" class="flex items-center flex-shrink-0 translate-x-1 ease-out duration-150 justify-center w-6 h-6 p-1.5 text-black rounded-full hover:bg-neutral-100">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-full h-full"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
        </button>
    </div>
</div>
```

Static Components contains the helper function `JavascriptConvert.SerializeObject()`, which will serialize any C# object into [JS Object Literals](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Working_with_objects#using_object_initializers). JSON serialization is possible, but insufficient when writing scripts, since it is not immediately compatible with JS unless you call `JSON.parse()`. `JavascriptConvert.SerializeObject()` exists in order to skip that step and add better ergonomics when mixing C# server side values into a script that will render on the client.

The following table is to show the different ways in which `JavascriptConvert.SerializeObject()` can be called and what the output is. For this we will assume the following C# class:

```csharp

public class SampleClass
{
    public bool BooleanValue { get; set; }

    public int NumberValue { get; set; }

    public string StringValue { get; set; }

    public List<string> StringArray { get; set; }
}

var value = new SampleClass() {
    BooleanValue = true,
    NumberValue = 500,
    StringValue = "Test String",
    StringArray = new List<string>() {
        "One",
        "Two"
    };
}
```

| Function Call | Result | Use Case |
|:---|:---|:---|
| SerializeObject(value) | `{ booleanValue: true, numberValue: 500, stringValue: "Test String", stringArray: ["One", "Two"] }` | Use this outside of HTML Attributes, where double quotes do not disrupt your markup. | 
| SerializeObject(value, true) | `{ booleanValue: true, numberValue: 500, stringValue: 'Test String', stringArray: ['One', 'Two'] }` | Use this inside of HTML Attributes or in any place in which single quotes are preferrable for serializing string values. |

One important thing to consider is that Static Components uses [Newtonsoft.Json](https://www.newtonsoft.com/json) internally to serialize your CLR objects into JS objects, if you want to customize the way that the serialization happens, you will need to use Newtonsoft.Json attributes, not `System.Text.Json` attributes, as those will be ignored.

## Security

**Be careful**  when injecting **user-supplied** or **untrusted** values. If a server-side value could contains special characters, code or HTML that you didn't write, make sure it is properly encoded before embedding it in a JavaScript Object Literal to avoid XSS attacks and other vulnerabilities.
