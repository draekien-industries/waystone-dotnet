# Validation as a step

`Waystone.Monads.FluentValidation` runs a validator and hands back a `Result`,
which is what lets validation stop being a branch before the chain and become a
guard step inside it:

```csharp
Result<Order, Error> validated = order.Validate(new OrderValidator());
```

`Validate` runs the validator synchronously; `ValidateAsync` takes a
`CancellationToken` and returns a `ValueTask`, which is already the shape an
async chain takes.

Both hand back the *same* value on success, which is what makes a validation slot
in anywhere that value flows:

```csharp
public Result<Invoice, Error> Bill(Order order) =>
    order.Validate(_validator)
         .AndThen(Price)
         .Map(Render);
```

**Neither is a method group,** because both take the validator as a second
parameter and a step takes one. Hold the validator in a field and wrap the call
in a named guard, which is the same move any variation point takes:

```csharp
private Result<Order, Error> Validated(Order order) => order.Validate(_validator);

public Result<Invoice, Error> Bill(Order order) =>
    Find(order).AndThen(Validated).AndThen(Price);
```

The types live in FluentValidation's own namespaces — `ValidationError` in
`FluentValidation`, the two extensions in `FluentValidation.Extensions`, and
`UseValidationErrorCode` in `FluentValidation.Configs` — so a file that already
validates needs at most one extra `using`.

## ValidationError is an Error

The failure is a `ValidationError`, which derives from `Error`. That is the whole
reason the step composes: no `MapErr` is needed at the seam, because the chain's
error type already covers it.

`Failures` carries the `ValidationFailure` list and is never empty. `ToDictionary()`
groups the messages by property name, which is the shape a problem-details payload
wants. Recover the detail at the boundary with a type test:

```csharp
if (error is ValidationError validationError)
{
    return ValidationProblem(validationError.ToDictionary());
}
```

That test is the reason the package ships no forwarding types in a legacy
namespace — a forwarded type would be a different type at run time and the
pattern would silently stop matching.

## Configure the code, not the error

Unconfigured, every `ValidationError` carries the code `validation.failed`. That
is a usable default and needs no call — override it only where the wire contract
wants a code of your own taxonomy:

```csharp
MonadOptions.Configure(options => options.UseValidationErrorCode("contoso.invalid"));
```

The setting sits beside the core options and respects `BeginScope`. **The code is
captured when `Validate` runs, not when a caller later reads the resulting
`Error`** — so a scope has to be open around the `Validate` call itself, not
around the handler inspecting what it returned.
