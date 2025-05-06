[!INCLUDE [waystone-monads](../../src/Waystone.Monads/README.md)]

## Common Concepts

### Try

Provides a way to store the result of a function into an `Option` or `Result`
type.

> [!NOTE]
> This was previously named `Bind` in v1.x

### Match

Performing a `Match` will ensure you handle all possible states of an `Option`
or `Result`.

### Unwrap

Unwrapping an `Option` or `Result` will allow you to access the value without
handling the possible states of the monad, but this will throw an exception if
you use one of the methods without a fallback value provider.

### Expect

`Expect` is similar to `Unwrap` and shares in its pitfalls, but allows you to
provide a custom exception message.

### Map

Performing a `Map` will alter the internal value of the Monad. This can be used
to compose the results of two functions.

### Inspect

Performing an `Inspect` will give you access to the value inside a Monad without
altering the monad itself.

## Async Overloads

Almost all methods which accept delegates have overloads that support async
methods which return `Task` and `ValueTask`.

### Example

```csharp
Option<string> fromAsyncFactory = await Option.Try(() => Task.FromResult("John Smith"));
```

> [!NOTE]
>
> Do not use the `async` keyword when declaring your lambda expressions. This
> prevents the .NET compiler from identifying if the return type of the lambda
> is a `Task` or `ValueTask`
