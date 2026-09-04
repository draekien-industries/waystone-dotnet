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

## What stops a catalog generating

Six constraints, each an **error** under `WMG`, and each meaning the generator
emitted *nothing*. The three nested classes above never appear, so the same build
also reports a `CS0117` at every call site reaching for one — read the `WMG` and
ignore the crowd.

- The enum must not be `[Flags]` (`WMG0001`) — a combined value has no single code.
- No two members may share a value (`WMG0002`).
- No member may be named `Names`, `Codes` or `Errors` (`WMG0003`), which are the
  nested types above.
- The `Waystone.Monads` error types must be resolvable in the compilation
  (`WMG0004`).
- The format must parse (`WMG0005`) and must contain `{member}` (`WMG0006`), or
  every member would get the same code.

`WMG0002` and `WMG0003` are collected across the whole enum, so one offending
member suppresses the source for every member rather than for itself alone.

## Build errors through the generated factory

`{EnumName}Catalog.Errors.{Member}(message)` is the default way to construct a
failure. Use it wherever you know the member as you write the line:

```csharp
// Good
return Result.Err<Order>(
    OrderErrorCodeCatalog.Errors.NotFound($"no order with id {id}"));

// Poor — names the same member, then routes it through a runtime switch
return Result.Err<Order>(
    OrderErrorCode.NotFound.ToError($"no order with id {id}"));
```

Three extensions exist for the case the factory cannot cover, where the member
arrives as a value rather than as something you type: `value.ToErrorCodeName()`,
`value.ToErrorCode()` and `value.ToError(message)`. Reach for one only at a
boundary that hands you a bare enum.

```csharp
// The member arrived from elsewhere — this is what ToError is for
OrderErrorCode refusal = AskWarehouse(order);

return Result.Err<Reservation>(
    refusal.ToError($"cannot reserve {order.Sku}"));
```

The reason to keep them to that case is what happens to a value that is not a
declared member. `ToErrorCode` is a generated switch whose default arm applies
the catalog's format to the value's `ToString()`, so `((OrderErrorCode)99)`
yields the code `order.99` rather than throwing. Nothing catches it: the code
was never generated, so `ErrorCodes.txt` never lists it and `WM2019` never
reports it. A cast, a deserialised value or a stale integer in a database
becomes a wire contract no catalog declares.

The factory has no such arm — it takes no value, so there is nothing to be out
of range. Prefer it whenever you have the choice, and validate the enum before
calling `ToError` when you do not.

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

## The runtime enum factories are gone

`Error.FromEnum`, `ErrorCode.FromEnum`, `ErrorCodeFactory.FromEnum` and
`Result.Err(Enum, string)` were removed in 7.0.0. A call site carried over from
6.x fails as `CS0117` or `CS1061`, and there is no code fix to lean on — the one
that existed was registered on the deprecation warning and went with the members
it rewrote. Rewrite onto `{EnumName}Catalog.Errors.{Member}(message)`, or onto
`value.ToError(message)` where the member arrives as a value.

They went because working a code out by reflection produces no constant: the
compiler cannot see the code, so a renamed member changes the wire contract
silently, the declared `Format` cannot apply because it is read at compile time,
and neither the analyzers nor `ErrorCodes.txt` can review a string nothing in the
build can see.

`MonadOptions.UseErrorCodeFactory` survives, and its scope is narrower than the
name suggests: it reaches only codes derived from *exceptions*, through
`ErrorCode.FromException`. It cannot change what the generator emitted, because
the generated members never consult it.

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
- `UseLoggerFactoryFrom`, from `Waystone.Monads.Extensions.Logging`, sends every
  exception `Try` and `TryAsync` convert to the application's own `ILogger`,
  along with caller information. Without it, a converted exception's stack trace
  reaches no log. Counts of those exceptions need nothing configured — core
  publishes them on a `Meter` named `Waystone.Monads`.

`UseCancellationAsFailure` is a trap worth knowing: by default an
`OperationCanceledException` is **not** caught by `Try`/`TryAsync` and
propagates. Calling it restores pre-6.0.0 behaviour, where a cancellation
becomes a `None` or an `Err` like any other failure. Prefer the default — a
cancelled operation produced no answer, and reporting that as absence hides the
cancellation from the caller that requested it.
