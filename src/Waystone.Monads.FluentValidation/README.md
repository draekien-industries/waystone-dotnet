# Waystone.Monads.FluentValidation

An interop package for using FluentValidation with Waystone.Monads

## Configuration

This package's options are configured through `MonadOptions`, so they are set
alongside the core options and respect `MonadOptions.CreateScope`:

```csharp
MonadOptions.Configure(options => options
    .UseValidationErrorCode("validation.failed")
    .UseFallbackValidationErrorMessage("One or more validation errors occurred."));

// or scoped to a region of code
using (MonadOptions.CreateScope(options => options.UseValidationErrorCode("debug.validation")))
{
    Error error = validationErr.ToError(); // code is "debug.validation"
}
```

## ValidationErr

A decorator for the `ValidationResult` provided by FluentValidation which
represents an _Invalid_ `ValidationResult` (e.g. `IsValid: false`). Provides
access to the `ValidationFailures` captured by the invalid result.

## Validate

An extension method that can be invoked on any value (`T`). Accepts an
`IValidator<T>` which will be executed synchronously.

### Example

```csharp
TestClass value = new();
Result<TestClass, ValidationErr> result = value.Validate(new TestClassValidator());

if (result.IsErr)
{
    // validation error
}
```

## ValidateAsync

An extension method that can be invoked on any value (`T`). Accepts an
`IValidator<T>` which will be executed asynchronously.

### Example

```csharp
TestClass value = new();
Result<TestClass, ValidationErr> result = await value.ValidateAsync(new TestClassValidator(), CancellationToken.None);

if (result.IsErr)
{
    // validation error
}
```
