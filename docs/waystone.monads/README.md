---
description: Keep exceptions exceptional
icon: hand-wave
cover: https://gitbookio.github.io/onboarding-template-images/header.png
coverY: 0
layout:
  cover:
    visible: true
    size: full
  title:
    visible: true
  description:
    visible: true
  tableOfContents:
    visible: true
  outline:
    visible: true
  pagination:
    visible: true
---

# Welcome

Welcome to the documentation for [Waystone.Monads](https://github.com/draekien-industries/waystone-dotnet/tree/main/src/Waystone.Monads), a C# library that brings principled, functional error handing, and optional values to the .NET ecosystem.

{% hint style="info" %}
At its core, this library provides two main types:

* `Option<T>`: an optional value - either `Some(T)` or `None`.
* `Result<T,E>`: the result of a computation that can succeed - `Ok(T)`, or fail - `Err(E)`.
{% endhint %}

{% hint style="success" %}
The aim of this library is to enable expressive and predictable composition of operations while reducing the risk of null-related bugs and scattered exception handling.
{% endhint %}

### :bulb: Inspired by Rust

Rust's [`Option<T>`](https://doc.rust-lang.org/std/option/) and [`Result<T,E>`](https://doc.rust-lang.org/std/result/index.html) have become gold standards for safe, composable programming. This library brings their elegance and power to C#.

### :thinking: Why use this library?

C# has evolved to reduce null-related bugs, most notably through the introduction of [nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references). However, nullable reference types have limitations:

{% hint style="success" %}
* Integrated with the compiler and static analysis
* Helps surface null-safety issues at compile time
* No need to introduce new types - easy adoption
{% endhint %}

{% hint style="danger" %}
* `null` still exists at runtime - nullable reference types are just a hint, not a runtime guarantee
* Not enforced at runtime, making it possible to misuse or ignore annotations
* Inadequate for modelling _intentional absence_ or _recoverable failures_ in a composable way
* No built-in mechanism for chaining operations safely
{% endhint %}

This library contains implementations of the monadic types `Option<T>` and `Result<T,E>`. By using these types, your code gains:

{% hint style="success" %}
* The presence/absence of a value, or success/failure of an operation becomes part of the type system
* Methods such as `Map` and `Bind` allows for the safe chaining of operations
* Methods such as `Match` enforce the handling of branch logic
* Encourages a more functional, declarative, and predictable coding style
{% endhint %}

{% hint style="danger" %}
* Slightly more verbose than nullable types or exceptions for simple cases
* May require learning functional programming concepts
* Not as idiomatic in traditional .NET ecosystems
{% endhint %}

### When to use this library?

Use this library when you want to:

* Replace `null` with explicit optional values
* Avoid exceptions for expected error cases
* Enforce the handling of errors and the absence of values
* Work in a functional or domain-driven design style

This package is especially useful in:

* Operations that return optional results or fallible operations
* Business logic where error propagation needs to be explicit and safe
* Systems that aim to be robust, testable, and expressive

***

Ready to dive in? Head over to the [Quickstart ](getting-started/quickstart.md)section to learn how to use this library in your projects.
