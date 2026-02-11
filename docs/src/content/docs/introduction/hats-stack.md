---
title: "What is the HATS Stack"
---

The HATS stack is a set of technologies that synergize with Static Components and allow for a new paradigm of Razor Pages applications to exist. Many ASP.NET applications need rich interactivity, but not the complexity, tooling overhead and cognitive load of a full SPA.

The HATS Stack is comprised of the following:

- HTMX
- AlpineJS and ASP.NET Core 
- TailwindCSS
- Static Components

This stack is an approach to include modern web tools into Razor Pages and MVC style development. This is the core of writing "low-JS" applications that can have rich UI interactions without the cost of using tools such as Blazor and React. Static Components itself is fairly non-opinionated, but the stack as a whole is intentionally opinionated—designed to combine these tools into a **fast**, **powerful**, and **secure** way to build applications.

I will now explain the purpose of each part of the stack.

### HTMX

HTMX is a JS library that allows any HTML element to make AJAX requests by using specific HTML attributes. Its main purpose is to support traditional server rendered applications by making it simple to load portions of a page incrementally instead of refreshing the entire document.

Khalid Abuhakmeh is the writer of an amazing [ASP.NET Core library](https://github.com/khalidabuhakmeh/Htmx.Net) that makes it easier to write forms powered by HTMX and it is an important component of this stack, and it is an important component of this stack, especially for convenience and ergonomics.

### AlpineJS

AlpineJS is similar to HTMX in approach, but it's different in purpose. AlpineJS defines HTML attributes that allow the declaration of web interactivity, events and DOM modification without using an imperative approach, like you would do with JQuery or Vanilla JS. AlpineJS is particularly useful for the creation of reusable UI components.

In a complete application built in the HATS stack, it would be expected that most of the application interactions related to data flow and application state (form submissions, storing of data, deletions, updates, etc) are powered by HTMX, but that cosmetic UI flair such as alerts, toasts, tooltips, and modals would be powered by AlpineJS. [Stimulus](https://stimulus.hotwired.dev/) is another library that functions in a slightly different way, but can serve as an alternative to AlpineJS.

As a rule of thumb, expect roughly 70% of your UI interactions using HTMX and use AlpineJS for that last 30% in which HTMX just isn't enough.

### Tailwind CSS

Tailwind CSS is infamous for its large class strings in CSS, and even more famous for how quickly it became an industry standard and how addictive it is to use. It's especially loved by backend developers and I personally became better at styling websites because of it. Jon Hilton's [Nuget Package](https://github.com/Practical-ASP-NET/Tailwind.Extensions.AspNetCore) is a great way to set up Tailwind with very little hassle and npm is not required. 

This however is the most easily replaceable part of the stack, feel free to use whichever CSS library you feel comfortable using.

### Static Components

You're looking at it! Static Components was written by me to make the previously mentioned tools synergize better together. ASP.NET Core already has Partials and ViewComponents, but having to specify a route every time made it harder to use than I liked for composing reusable UI. ViewComponents were lacking basic features like ChildContent and Slots which felt like a massive oversight from the ASP.NET Core team. 

I developed what became Static Components after being tasked to migrate an on-prem application to the cloud. I was given a very tight deadline and a team that wasn't fluent in React, but knew MVC decently well. After having worked with UI components of different kinds for many years I felt like I was missing an essential part of how to write UIs cleanly, which led to me develop this library and the early ideas of this stack at the same time as the other project I was working on. 

Static Components was born in the heat of battle and had kills before it even had a name. I hope you enjoy using it as much as I do.
