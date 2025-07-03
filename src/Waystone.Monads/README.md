# Waystone.Monads

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

- An `Option<T>` abstract record describes the `Option` type.
- A `Some<T>` record describes the `Some` type.
- A `None<T>` record describes the `None` type.

## Result

The `Result` type is a type used for returning and propagating errors. Every
`Result` is either `Ok`, representing success and containing a value, or `Err`,
representing an error and containing an error value.

### Implemented Types

- An `Result<TOk,TErr>` abstract record describes the `Result` type.
- An `Ok<TOk,TErr>` record describes the `Ok` type.
- An `Err<TOk,TErr>` record describes the `Err` type.

> [!NOTE]
> Each concrete result type requires the other's generic type parameters in
> order to correlate correctly with each other.

## Configuration

You can configure an action to be invoked when an exception is caught and
handled by the library. Invoke the `UseExceptionLogger` function once during the
lifetime of your app:

```csharp
MonadOptions.Configure(options => options.UseExceptionLogger((exception, callerInfo) =>
{
    Log.Error(exception, "Exception when creating monad"); // use Serilog/NLog/Etc
}));
```

There may be times where you want to generate an `ErrorCode` from an `Enum` or an `Exception`.
You can configure the formatting of the generated error codes using the `UseErrorCodeFactory`.

```csharp
class MyErrorCodeFactory : ErrorCodeFactory
{
  // override as needed
}

MonadOptions.Configure(options => options.UseErrorCodeFactory(new MyErrorCodeFactory()));
```

> ![NOTE]
> The `MonadOptions` class acts like a singleton, so you should only configure it once
> in your application's life-cycle.
