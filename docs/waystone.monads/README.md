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

Waystone.Monads is a lightweight, idiomatic C# library that implements two fundamental functional types: `Option<T>` and `Result<T, E>`. Inspired by Rust's standard library but designed to feel natural in C#, this package brings safer and more expressive handling of optional and fallible data - without the overhead or jargon of most functional programming libraries.

## Why this library exists

Most C# codebases default to `null` and exceptions for absence and failure. That's fine, until it isn't.

{% hint style="warning" %}
`null` and exceptions result in guard clauses everywhere, unpredictable runtime crashes, and unclear API intent.
{% endhint %}

Waystone.Monads replaces that with explicit types that make the intent clear at the type level:

* `Option<T>` means a value might be there.
* `Result<T, E>` means a computation might fail.

## Who should use this

You should use this library if:

* You want to eliminate `null` and exceptions from business logic
* You prefer expressive, explicit code over defensive boilerplate
* You appreciate functional patterns but still want to write C#, not Haskell

{% hint style="success" %}
If you've ever used `Option` and `Result` in Rust or F#, you'll fee right at home. If you haven't, you'll pick it up quickly - and wonder how you ever lived without it.
{% endhint %}

## Core types

### Option\<T>

Represents a value that may or may not be present. Eliminates the ambiguity and risk of `null`, while remaining easy to compose.

```csharp
Option<string> name = Option.Some("Laura Bailey");
Option<string> empty = Option.None<string>();
```

### Result\<T, E>

Represents success or failure as a value instead of throwing exceptions. Encourages you to handle both outcomes explicitly.

```csharp
Result<int, string> parsed = Parse("123"); // Ok(123)
Result<int, string> failed = Parse("abc"); // Err("Input 'abc' is not a number")
```

## Key features

✅ Fluent and idiomatic API\
✅ No obscure functional programming terms like `Functor` or `Applicative`\
✅ Lightweight: zero dependencies\*, zero magic\
✅ Great DX: pattern matching, deconstruction, and clear errors

{% hint style="info" %}
Zero dependencies when using versions of .NET greater than or equal to .NET 8.

The .NET standard version of the package depends on [PolySharp](https://github.com/Sergio0694/PolySharp) internally to enable `record` types.
{% endhint %}

## Next steps

1. [Install the package](getting-started/quickstart.md)
2. Learn how to use `Option<T>`
3. Dive into `Result<T, E>`
4. Check out usage examples for real-world scenarios
