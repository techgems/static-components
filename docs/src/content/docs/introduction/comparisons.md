---
title: 'Comparisons'
---

## React

Using React with ASP.NET Core has become the standard approach in the industry over the last 7 years. It is however not without drawbacks. Using React with an ASP.NET Core API forces you to have 2 codebases in 2 different programming languages that use very different paradigms.

React requires you to become fluent enough in the node and npm space to understand it's errors and hosting it creates a lot of questions without easy answers depending on what you want to do. Writing authentication properly for a React application and an ASP.NET Core API is both an amazing learning experience and a great opportunity for creating security holes you might not know you are creating.

React is a very useful tool that can do a lot of things very well, but it isn't for everyone. In my experience, building forms with robust validation and backend integration in React almost always requires you to write the same form validation logic in two different programming languages. 

Static Components makes writing secure applications trivial and forms become easy once again.

## Blazor

Blazor, even with all of its recent improvements has to make a choice between running on the server and keeping a Websocket connection alive and dealing with an unavoidable massive initial payload that slows down the first request.

Blazor Server can certainly scale up, but scaling it will take more thought and it will be preferred to scale vertically before you try to scale horizontally, as you need to make sure that load balancing is configured properly to keep persistent connections.

Blazor WASM is in a better position now that WASM 3.0 has been rolled out, but its main drawback, a large initial payload, remains at large. This hurts user experience, primarily in mobile first workloads. Prerendering helps improve load time perception, but it doesn't make the problem disappear.

## Hydro

[Hydro](https://usehydro.dev/) and the Hydro stack is very similar in approach to the HATS stack in many ways, and it is a very innovative way to create rich web UI in applications. 

Hydro relies entirely on AlpineJS and handles state and DOM manipulation automatically. Having used HTMX and having weighed it against AlpineJS, I came to the conclusion that HTMX is superior for handling application state in a way that makes it unnecessary to deal with the DOM almost entirely barring basic UI interactions.

The main difference between Hydro and the HATS stack is that the HATS stack encourages you to manage application state with HTMX, which reduces significantly the amount of AJAX calls to update the UI. Hydro also has a higher learning curve as it is a larger library.

The HATS stack is a superior approach if you want to have control of when AJAX calls happen. In general, both approaches are similar and perhaps even compatible with one another. 
