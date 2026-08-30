# Waystone.Monads.FluentValidation

An interop package for using FluentValidation with Waystone.Monads

## Namespaces

The package shadows FluentValidation's own namespaces, so its types sit where a
consumer already looks for them rather than under a parallel `Waystone` tree:

| Member | Namespace |
| --- | --- |
| `ValidationError` | `FluentValidation` |
| `Validate`, `ValidateAsync` | `FluentValidation.Extensions` |
| `UseValidationErrorCode` | `FluentValidation.Configs` |

Before v7.0.0 these lived under `Waystone.Monads.FluentValidation.*`. Only the
`using` directives change; every type and member name is the same. There is no
compatibility shim, because a forwarding type in the old namespace would be a
different type at run time and a `value is ValidationError` pattern would
silently stop matching.

## Supported FluentValidation versions

`FluentValidation >= 11.1.0 && < 13.0.0`. Bring your own version inside that
range; the package does not pin you to one.

The floor is 11.1.0 because that is the first release where
`ValidationResult.ToDictionary()` is an instance method — before it, the same
call was an extension method, so a build compiled against 11.1.0 or later fails
at run time against an older assembly.

FluentValidation 12 targets `net8.0` only, so a consumer on .NET Framework or
`netstandard2.0` cannot resolve it. That is FluentValidation's constraint, not
this package's.

## Configuration

This package's options are configured through `MonadOptions`, so they are set
alongside the core options and respect `MonadOptions.BeginScope`:

```csharp
MonadOptions.Configure(options => options.UseValidationErrorCode("validation.failed"));

// or scoped to a region of code
using (MonadOptions.BeginScope(options => options.UseValidationErrorCode("debug.validation")))
{
    Result<TestClass, Error> result = value.Validate(new TestClassValidator());
    // the err's code is "debug.validation"
}
```

The code is read when the validation runs, not when the error is later read.

## ValidationError

The `Error` a validator's failures produce. It derives from `Error`, so a
validation step composes with every other step in a `Result` chain — there is no
conversion at the seam.

`Failures` carries the `ValidationFailure` list FluentValidation reported, and is
never empty. `ToDictionary()` groups the messages by property name, ready for a
problem-details payload.

```csharp
if (error is ValidationError validationError)
{
    return ValidationProblem(validationError.ToDictionary());
}
```

## Validate

An extension method that can be invoked on any value (`T`). Accepts an
`IValidator<T>` which will be executed synchronously.

### Example

```csharp
TestClass value = new();
Result<TestClass, Error> result = value.Validate(new TestClassValidator());

if (result.IsErr)
{
    // validation error
}
```

## ValidateAsync

An extension method that can be invoked on any value (`T`). Accepts an
`IValidator<T>` which will be executed asynchronously. Returns a `ValueTask`, so
the call composes as a step in an async `Result` chain.

### Example

```csharp
TestClass value = new();
Result<TestClass, Error> result = await value.ValidateAsync(new TestClassValidator(), CancellationToken.None);

if (result.IsErr)
{
    // validation error
}
```
