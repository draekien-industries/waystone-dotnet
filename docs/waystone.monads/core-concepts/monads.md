---
description: Learn about monads and why they are important to use in your code
icon: question
---

# Monads

## Introduction

A monad is a design pattern used to model computations that include context - like "might fail", "might be missing", or "produces side effects" - in a consistent, composable way.

If that sounds a bit abstract, don't worry. In this library, you've already seen two concrete examples of monads:

* `Option<T>`: a computation that may or may not return a value
* `Result<T,E>`: a computation that may succeed and return `T` or fail with an error `E`

## The Rule of Thumb

{% hint style="info" %}
A monad is a container that lets you chain operations while preserving context
{% endhint %}

If you are new to functional programming, think of monads as _pipelines for values that might have extra "baggage" - like being optional or fallible_. They help you write clear, linear code even when those values come with conditions.

## Core Capabilities

### Added Context

Monads wrap raw values to provide a level of additional context to them, e.g.

```csharp
Option.Some("Grace"); // there is definitely a string here
Option.None<string>(); // this string is intentionally absent

Result.Ok<string, string>("some random string"); // this string now means a success
Result.Err<string, string>("some other string"); // this one now means an error
```

### Safe Operation Chaining

Monads let you chain operations using methods like `Map`, `Bind`, etc. This lets you build pipelines of operations without needing to manually check for `null`, exceptions, or missing data.

```csharp
Option<User> user = GetUserById(id);
Option<string> email = user.Map(u => u.Email);
```

{% hint style="success" %}
This avoid nested `if` statements, `try` blocks, or repeated null checks
{% endhint %}

### Containing Failures and Absences

Instead of throwing exceptions or returning `null`, monads encapsulate failures - i.e. `Err(...)` - or absences - i.e. `None` - inside the type system. That makes it impossible to ignore errors or missing values without explicitly handling them.

## Why Monads Matter

Monads bring structure to your code. They let you:

* Write safer code that won't fail silently
* Avoid repetitive error checking and null-handling
* Focus on the core logic, rather than defensive programming
* Make failure and absence part of your types - not runtime surprises

## Example: The Railway Tracks

Imagine your program as a train moving along a railway track. Each step of your logic is a station. You want the train to move from station to station - loading data, transforming it, performing checks, etc, but things can go wrong.

{% hint style="danger" %}
* The passenger might be missing
* A validation might fail
* A file might not be found, or an input might be malformed
{% endhint %}

### Without Monads (Derailment Everywhere)

In regular C# code, errors or missing values derail the train. You get thrown into:

* null checks everywhere
* try/catch blocks scattered across your code
* unpredictable paths - some functions might return `null`, some throw exceptions, some return data

It becomes hard to know what to expect, and even harder to safely chain operations together.

### With Monads (Safe, Predictable Tracks)

Monads like `Option<T>` and `Result<T,E>` put your logic on two parallel railway tracks:

* :train2: success track - your train keeps moving smoothly
* :construction: failure or none track - your train gets rerouted safely to a dead-end, without crashing

The train never jumps tracks randomly, it stays on one track or ther other, and every station (function) is built to handle both cases.

```csharp
Option<string> email = GetUserById(id)
    .Map(u => u.Profile)
    .Map(p => p.Email);
```

If the user doesn't exist, or the profile is missing, the train never crashes. It just stops safely at `None`. You can later decide what to do (show an error message, fall back to defaults, etc).

```csharp
string displayEmail = email.Match(
    some => some,
    none => "[Not Provided]"
);
```

## Summary

By using monads, you build defensive, predictable programs and your logic stays clear, even in the face of complexity.

{% hint style="success" %}
* The train keeps moving automatically through the pipeline of steps
* The train stops as soon as a failure or absence is encountered
* No suproses - you always know which track you're on
{% endhint %}
