---
icon: bullseye-arrow
layout:
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

# Quickstart

Ready to eliminate `null` checks and stop catching exceptions? Here's how to get up and running with Waystone.Monads in less than a minute.

## Installation

Install via NuGet:

```sh
dotnet add package Waystone.Monads
```

Or using the Package Manager:

```sh
Install-Package Waystone.Monads
```

## Using Option\<T>

`Option<T>` represents a value that may or may not exist. Think of it as an explicit alternative to `null`.

```csharp
Option<string> name = Option.Some("Liam O'Brian");
Option<string> missing = Option.None<string>();

string greeting = name.Match(
    some => $"Hello, {some}!",
    () => "Hello, stranger!"
);

// Output: "Hello, Liam O'Brian!"
```

{% hint style="info" %}
Use `Map`, `Inspect`, `Match` , and more to work with values safely and fluently
{% endhint %}

## Using Result\<T, E>

`Result<T, E>` represents the outcome of an operation that can succeed or fail. Use it instead of throwing exceptions for recoverable failures.

```csharp
Result<int, string> ParseInt(string input)
{
    return int.TryParse(input, out var value)
        ? Result.Ok<int, string>(value)
        : Result.Err<int, string>($"Input '{input}' is not a valid number");
}

var result = ParseInt("42");

int value = result.Match(
    ok => ok,
    err => -1
);

// Output: 42
```

{% hint style="success" %}
No exceptions, no `try/catch`, just predictable control flow
{% endhint %}

## Want more?

* Explore `Option<T>` methods
* Explore `Result<T, E>` methods
* See real-world usage examples
