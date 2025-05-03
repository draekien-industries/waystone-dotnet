---
description: Learn about the option monad and the types that implement it in this library
icon: option
---

# Option

## Overview

The `Option` type represents an optional value. Every `Option` is ether a

* `Some` - contains a value, or
* `None` - contains no value

It provides similar functionality to C# [nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references), but provides the following benefits:

* enforces null handling, preventing accidental null reference exceptions
* all [default values](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/default-values) are treated as `None`&#x20;
* provides a way to mark a value as [intentionally absent](intentional-absence.md)

## Creating an Option

Use the factory methods found in the `Option` class to create options.

```csharp
Option<string> some = Option.Some("value");
Option<string> none = Option.None<string>();
```

{% hint style="warning" %}
An `InvalidOperationException` will be thrown when attempting to create a `Some` from a `null` or `default` value
{% endhint %}

To avoid the possibility of an `InvalidOperationException` being thrown when creating a `Some` option, use the `Bind` factory method instead.

```csharp
Option<int> some = Option.Bind(() => 1);
Option<int> none = Option.Bind(() => 0);
```

An optional error handler may be provided to the `Bind` method if you would like to perform actions on a `None` outcome. The example below writes the exception message to the console.

```csharp
Option<int> none = Option.Bind(() => 0, (ex) => Console.WriteLine(ex.Message));
```

### Using an Option

