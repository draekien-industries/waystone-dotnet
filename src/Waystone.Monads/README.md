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

### Creating Results

Supply both type parameters when you provide your own error type:

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
Result<int, string> error = Result.Err<int, string>("something went wrong");
```

If you are happy with the built in `Error` type, use the single type parameter
overloads instead. These default `TErr` to `Error`:

```csharp
Result<int, Error> result = Result.Ok<int>(1);
Result<int, Error> error = Result.Err<int>(new Error("MyCode", "something went wrong"));
```

An `Error` code can be derived from an enum value, which keeps the code stable
across occurrences of the same error type:

```csharp
enum UserErrors
{
    NotFound,
}

// code becomes "UserErrors.NotFound"
Result<User, Error> error = Result.Err<User>(UserErrors.NotFound, "the user was not found");

// or, when you need the error on its own
Error err = Error.FromEnum(UserErrors.NotFound, "the user was not found");
```

Use `Try` to capture the value of a factory that may throw. The single type
parameter overload converts the exception using `Error.FromException`:

```csharp
Result<int, string> custom = Result.Try(() => int.Parse(input), ex => ex.Message);
Result<int, Error> parsed = Result.Try<int>(() => int.Parse(input));
```

## Async

Both monads provide `TryAsync` for factories that return a `Task`:

```csharp
Result<User, Error> result = await Result.TryAsync<User>(() => FetchUserAsync(id));
Option<User> option = await Option.TryAsync(() => FetchUserAsync(id));
```

> [!NOTE]
> The `Try` overloads that accept an async factory are obsolete and will be
> removed in v6. Call `TryAsync` instead.

The terminal operations are also available on `Task` and `ValueTask` receivers,
so you do not have to await the monad before unwrapping it:

```csharp
User user = await FetchUserAsync(id).UnwrapAsync();
User userOrGuest = await FetchUserAsync(id).UnwrapOrAsync(Guest);
User expected = await FetchUserAsync(id).ExpectAsync("the user must exist");
Error error = await FetchUserAsync(id).UnwrapErrAsync();
```

`Result` provides `UnwrapAsync`, `UnwrapErrAsync`, `UnwrapOrAsync`,
`UnwrapOrDefaultAsync`, `ExpectAsync` and `ExpectErrAsync`. `Option` provides
`UnwrapAsync`, `UnwrapOrAsync`, `UnwrapOrDefaultAsync` and `ExpectAsync`.

## Configuration

### Observability

The library reports the exceptions it swallows through sources named after
itself, so most of this needs no configuration at all.

**Metrics need nothing installed.** A `Meter` named `Waystone.Monads` publishes a
`waystone.monads.exceptions_handled` counter, tagged with `error.type` and with
`waystone.monads.monad` to separate the `Option` case from the `Result` one. Add
the name to the meters your pipeline already collects:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter("Waystone.Monads"));
```

**Logs need one call**, because `Microsoft.Extensions.Logging` publishes no
ambient logger to discover. Install
[`Waystone.Monads.Extensions.Logging`](https://www.nuget.org/packages/Waystone.Monads.Extensions.Logging)
and point it at your own `ILogger` once during start-up:

```csharp
MonadOptions.Configure(options => options.UseLoggerFactoryFrom(app.Services));
```

`UseLogger(logger)` and `UseLoggerFactory(factory)` are there for an application
with no service provider.

`MonadOptions.UseExceptionLogger` did this with a hand-written delegate. It is
obsolete and removed in `7.0.0`; configuring both logs everything twice.

### Error codes

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

### Scoped Configuration

When you need different options for one region of code - a single request, a
test, or a block you are debugging - create a scope instead of reconfiguring the
whole application:

```csharp
using (MonadOptions.BeginScope(options => options.UseFallbackErrorCode("Debug")))
{
    // reads inside here, including after an await, see "Debug"
    var result = Result.Try<int>(() => int.Parse(input));
}

// the global configuration is unchanged out here
```

Options you do not set are inherited from the configuration in effect when the
scope is created, and the scope is a snapshot, so a later `Configure` call does
not change an open scope. Scopes nest, and disposing one restores the scope
around it.

Scopes accept the same configuration methods as `Configure`, so an override can
change any option:

```csharp
using (MonadOptions.BeginScope(options => options
    .UseErrorCodeFactory(new MyErrorCodeFactory())
    .UseFallbackErrorMessage("Something went wrong while debugging.")))
{
    // ...
}
```

Because a scope applies to the current asynchronous flow, concurrent flows each
see their own options. This makes scopes safe to use in parallel tests.

> [!NOTE]
> A scope affects work started inside it. It does not affect work that was
> already running when the scope was created.
