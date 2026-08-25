# Error codes

`Result<TOk, Error>` is the default failure shape. An `Error` is a record of an
`ErrorCode` and a message, and `ErrorCode` is a string wrapper with implicit
conversions both ways. The codes reach consumers over a wire, so they are a
contract: derive them at compile time, never at run time.

## Declare a catalog

Mark an enum with `[ErrorCodeCatalog]` and a generator emits a companion type
named after the enum plus `Catalog` — nothing is trimmed, so `OrderErrorCode`
gives `OrderErrorCodeCatalog`.

```csharp
[ErrorCodeCatalog(Format = "order.{member:kebab}")]
internal enum OrderErrorCode
{
    NotFound,
    AlreadyShipped,
}
```

Three nested classes hold one member per enum member, named verbatim:

| Generated | Holds | Use for |
| --- | --- | --- |
| `…Catalog.Names.NotFound` | A `const string` — `"order.not-found"` | `case` labels, attributes, anywhere a constant is required |
| `…Catalog.Codes.NotFound` | An `ErrorCode` field | Comparing or passing a code without a message |
| `…Catalog.Errors.NotFound(message)` | An `Error` factory | Constructing the failure to return |

Three extensions handle the case where the member is only known at run time:
`value.ToErrorCodeName()`, `value.ToErrorCode()` and `value.ToError(message)`.
Reach for these at a boundary that receives a bare enum — not as the default way
to build an error, since the nested factories are direct.

```csharp
// The member is known here — use the factory
return Result.Err<Order>(
    OrderErrorCodeCatalog.Errors.NotFound($"no order with id {id}"));

// The member arrived from elsewhere — attach a message with ToError
OrderErrorCode? refusal = AskWarehouse(order);
return refusal.HasValue
    ? Result.Err<Reservation>(refusal.Value.ToError($"cannot reserve {order.Sku}"))
    : Result.Ok<Reservation>(reservation);
```

## Shape the code string

`Format` parses `{enum}` and `{member}`, each taking an optional casing —
`kebab`, `snake`, `lower` or `upper`. Precedence runs: the enum's own `Format`,
then an assembly-level `[assembly: ErrorCodeFormat("...")]`, then the default
`{enum}.{member}`.

Pick the format once per taxonomy and let it apply to every member. A wire code
of `order.not-found` is what most APIs want; the default `OrderErrorCode.NotFound`
leaks a C# type name.

## Switch on codes at the boundary

Because `Names` holds constants, a boundary can translate a code without
knowing which method produced it — the reason to prefer the catalog over any
runtime factory:

```csharp
switch (error.Code.Value)
{
    case OrderErrorCodeCatalog.Names.NotFound: return 404;
    case OrderErrorCodeCatalog.Names.AlreadyShipped: return 409;
    default: return 500;
}
```

## Do not use the runtime factories

`Error.FromEnum`, `ErrorCode.FromEnum`, `Result.Err(Enum, string)` and
`MonadOptions.UseErrorCodeFactory` are obsolete and scheduled for removal in a
future major. They work the code out at run time, which produces no constants,
so nothing downstream can be a `case` label. A custom factory also cannot change
what the generator
emitted — the generated members never consult it — so installing one leaves the
runtime string and the generated string disagreeing.

## Review the codes as a list

A project may add an `ErrorCodes.txt` to opt into reviewing its codes the way
`PublicAPI.Shipped.txt` makes a public API reviewable. Once the file exists,
`WM2019` reports a generated code missing from it and `WM2020` reports an entry
no catalog still generates. Invoke `WM2019`'s code fix, then read the added line
before committing — a new line is a wire contract reaching consumers.

Two catalogs must never generate the same code. Enums sharing a name in
different namespaces collide on every member name they share, which `WM2018`
reports at compile end; the fix is a rename, since nothing in the source says
which enum should keep the code.

## Configure the rest globally

`MonadOptions.Configure` sets library-wide behaviour, and
`MonadOptions.BeginScope` overrides it for a scope. Two settings matter when
building errors:

- `UseFallbackErrorCode` names the code used when one cannot be derived.
- `UseExceptionLogger` receives every exception `Try` and `TryAsync` convert,
  along with caller information. Without it, a converted exception's stack trace
  is not recorded anywhere.

`UseCancellationAsFailure` is a trap worth knowing: by default an
`OperationCanceledException` is **not** caught by `Try`/`TryAsync` and
propagates. Calling it restores pre-6.0.0 behaviour, where a cancellation
becomes a `None` or an `Err` like any other failure. Prefer the default — a
cancelled operation produced no answer, and reporting that as absence hides the
cancellation from the caller that requested it.
