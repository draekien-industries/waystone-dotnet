﻿# Waystone.Monads

A .NET implementation of
the [std::option](https://doc.rust-lang.org/std/option/)
and [std::result](https://doc.rust-lang.org/std/result/index.html) modules
from [the Rust Standard Library](https://doc.rust-lang.org/std/index.html).

## Option

The `Option` type represents an optional value: every `Option` is either `Some`
and contains a value, or `None`, and does not. It provides similar functionality
to the built in
[nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
offered in C#, but provides a more rigid structure for handling the "null"
scenario.

### Implemented Types

- An `IOption<T>` interface describes the `Option` type.
- A `Some<T>` record describes the `Some` type.
- A `None<T>` record describes the `None` type.

## Result

The `Result` type is a type used for returning and propagating errors. Every
`Result` is either `Ok`, representing success and containing a value, or `Err`,
representing an error and containing an error value.

### Implemented Types

- An `IResult<TOk,TErr>` interface describes the `Result` type.
- An `Ok<TOk,TErr>` record describes the `Ok` type.
- An `Err<TOk,TErr>` record describes the `Err` type.

> [!NOTE]
> Each concrete result type requires the other's generic type parameters in
> order to correlate correctly with each other.