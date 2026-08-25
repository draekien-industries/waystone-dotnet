---
name: waystone-monads
description: Write idiomatic Waystone.Monads C# — compose Option<T> and Result<TOk, TErr> with Map, AndThen, Filter and Match rather than IsSome checks, Unwrap calls and nested branching. Use when writing or reviewing C# that returns Option or Result, when porting a nullable return or a thrown exception onto one, when a WM diagnostic fires, or when the user says "use an Option", "return a Result", "make this monadic".
---

# Waystone.Monads

`Option<T>` and `Result<TOk, TErr>` are closed two-case records: `Some`/`None`
and `Ok`/`Err`. Both are deliberate ports of the Rust types of the same name.
Write them the way Rust code is written — a value flows through a chain of
combinators and the two cases are collapsed once, at the end — not the way
nullable C# is written, where every step re-asks whether the value is there.

The whole payoff is that absence and failure become **unignorable and
uninspected**. Code that reaches for `IsSome` or `Unwrap` has kept the check
and paid for the type anyway.

## Reach for the right type

| Situation | Use |
| --- | --- |
| A value may legitimately be absent, and why does not matter | `Option<T>` |
| An operation may fail, and the caller must know why | `Result<TOk, Error>` |
| An operation may fail with a domain-specific failure value | `Result<TOk, TErr>` |
| A failure no caller can act on (programmer error, corrupt state) | Throw |

`Option` answers *is there one*; `Result` answers *why not*. Converting
between them is explicit: `OkOr`/`OkOrElse` turns a `None` into an `Err`, and
`GetOk`/`GetErr` turns a `Result` into an `Option` of one side. Where both
questions are live at once, the answer is a nested `Result<Option<T>, E>` or
`Option<Result<T, E>>` — read [references/nesting.md](references/nesting.md)
before choosing, since the outer monad decides which question the caller must
answer first and nothing in the build checks the choice.

Never mix conventions inside one type. A type that already returns `Option` from
some members and `T?` from others makes callers guess which absence convention
applies — `WM2012` reports it.

## Compose, do not inspect

The canonical shape is a chain of **named method groups**, each returning the
same monad, with no intermediate locals and no lambda where a method will do:

```csharp
public Result<Shipment, Error> Place(int orderId) =>
    Find(orderId)
       .AndThen(NotYetShipped)
       .AndThen(Deliverable)
       .AndThen(Reserve)
       .AndThen(Charge)
       .AndThen(Dispatch);
```

Every step returns `Result<T, Error>`, so the first `Err` short-circuits the
rest and arrives at the caller unchanged. This is what Rust's `?` operator
produces, and it is the shape to aim for whenever several fallible steps run in
sequence. C# has no `?` operator; `AndThen` is the substitute, not a `try`/`catch`
or an early `return` after an `IsOk` check.

Pick the combinator by what the delegate returns:

| The delegate returns | Use | Rust |
| --- | --- | --- |
| A plain value | `Map` | `map` |
| Another `Option`/`Result` | `AndThen` | `and_then` |
| Nothing — a side effect only | `Inspect` / `InspectErr` | `inspect` |
| A `bool` narrowing the value | `Filter` (Option only) | `filter` |
| A replacement error type | `MapErr` | `map_err` |
| A fallback monad | `OrElse` | `or_else` |

`Map` that returns a monad produces a nested monad and forces a `Flatten`;
that pair is exactly `AndThen` (`WM2005`).

## Collapse once, at the end

A chain ends in exactly one place, and that is the only place both cases are
named. Choose by what the caller needs:

| Need | Use |
| --- | --- |
| Both cases produce a value | `Match(onSome, onNone)` |
| A fixed fallback | `UnwrapOr(value)` |
| A computed fallback | `UnwrapOrElse(factory)` |
| `null` for the absent case | `UnwrapOrNull()` |
| The caller should decide | Return the monad — do not collapse at all |

**Propagating beats collapsing.** A method that collapses a `Result` only to
rebuild one has done nothing; return the monad and let the boundary — a
controller, a handler, a `Main` — collapse it once.

`Unwrap` and `Expect` throw, which converts a handled absence back into the
unhandled exception the type exists to prevent (`WM2001`, `WM2002`). Neither
belongs in shipped code. Tests are where `Unwrap` is defensible, because a
panic there is a failed assertion.

## Traps

Each of these is a real failure mode with a diagnostic behind it. When a `WM`
code fires and its meaning is not obvious, look it up in
[references/diagnostics.md](references/diagnostics.md), which carries all of
them with the tier and the fix.

### Nested matching

A `Match` whose branches contain another `Match` is a nested `if` wearing a
different hat, and it grows quadratically with each fallible step.

```csharp
// Poor — the failure branch is written three times
return FindUser(id).Match(
    user => LoadAccount(user).Match(
        account => Charge(account).Match(
            receipt => Result.Ok<Receipt>(receipt),
            err => Result.Err<Receipt>(err)),
        err => Result.Err<Receipt>(err)),
    err => Result.Err<Receipt>(err));

// Good
return FindUser(id).AndThen(LoadAccount).AndThen(Charge);
```

Whenever a `Match` branch reconstructs the same case it received, the `Match`
was `AndThen`, `Map`, `OrElse` or `MapErr`.

### Boolean checks standing in for combinators

`IsSome`, `IsNone`, `IsOk` and `IsErr` exist for the rare genuine question. They
are not the way to get at the value.

```csharp
// Poor — asks the same question twice (WM2004)
if (option.IsSome) { return option.Unwrap(); }
return 0;

// Good
return option.UnwrapOr(0);

// Poor — check combined with an unwrap (WM2006)
bool big = option.IsSome && option.Unwrap() > 2;

// Good
bool big = option.IsSomeAnd(value => value > 2);
```

`IsSomeAnd`, `IsNoneOr`, `IsOkAnd` and `IsErrAnd` take the predicate and supply
the value, so no unwrap is needed. Where a chain ends in a `bool`, one of these
is almost always the ending.

### Treating the monad as nullable

`Option` and `Result` are records, so the compiler permits `null`, `default`
and a `?` annotation on all of them. Every one of these is wrong:

```csharp
Option<int> a = null;             // WM1002 — throws on next member access
var b = default(Result<int, string>);  // WM1003 — default is null, not Err
Option<int>? c = null;            // WM1008 — three states where two are meaningful
if (option == null) { }           // WM2008 — tests the wrong thing
```

The absent case is `None`, the failed case is `Err`, and `Option.Some(null)`
throws — use `Option.FromNullable` when the value may be null (`WM1001`,
`WM1005`).

Likewise, declare the base type, never a case. `Some<int>` or `Ok<T, E>` in a
signature can only hold one of the two states, which defeats the type
(`WM2011`).

### Discarding the answer

```csharp
SaveOrder(order);     // WM1006 — returns Result; the failure vanishes
FindDiscount(order);  // WM2013 — returns Option; the value is silently dropped
```

A discarded `Result` throws nothing and reports nothing. Match on it or
propagate it. This survives `await ... .ConfigureAwait(false)`, so an
un-awaited-looking async call is caught too. A discarded `Option` is less
harmful but usually means the value was meant to be handled.

### Doing eager work for a branch not taken

`And`, `Or`, `UnwrapOr`, `MapOr` and `OkOr` evaluate their argument before
checking whether it is needed. Where the argument is a call, a `new` or an
`await`, use the lazy sibling — `AndThen`, `OrElse`, `UnwrapOrElse`,
`MapOrElse`, `OkOrElse` (`WM2016`).

```csharp
option.UnwrapOr(BuildExpensiveDefault());     // runs always
option.UnwrapOrElse(BuildExpensiveDefault);   // runs only when None
```

A constant, a field read or a bare local is free; leave those on the eager form.

### Allocating a closure per call

A lambda that captures a local or a parameter allocates a display class on every
call. Nearly every delegate-taking member has an overload that takes the value
as **state** instead, and the lambda then closes over nothing (`WM2017`):

```csharp
option.Map(multiplier, static (value, m) => value * m);
```

Mark every such lambda `static`, so a later edit cannot silently reintroduce the
capture. Read [references/state-overloads.md](references/state-overloads.md)
when rewriting one — where the state argument goes differs by branch, and two
members deliberately have no state overload.

### An async delegate handed to a synchronous member

```csharp
// Compiles. T is inferred as Task<Order>, which satisfies notnull.
Option<Task<Order>> trapped = Option.Try(async () => await FetchAsync(id));
```

The task ends up *inside* the monad, where nothing awaits it: the work has not
finished, anything it throws is unobserved, and `Try` converts no exception at
all. Nothing about the call site looks wrong, which is what makes it the async
failure worth watching hardest. Use the `Async` sibling (`WM1011`), and read
[references/async.md](references/async.md) for why it ships no code fix.

### Over-wrapping

The type earns its place only where absence or failure is real.

- `Option<bool>` has three states and almost always wants to be two — model it
  as an enum or split the question.
- `Result<string, string>` makes both implicit conversions ambiguous and `Ok`
  indistinguishable from `Err` to a reader (`WM2010`). Give the two sides
  different types.
- `Option<Option<T>>` distinguishes an absent outer from an absent inner, which
  callers never act on — `Flatten` it (`WM2009`).
- A helper that only wraps a value already known to be present is indirection,
  not safety. Construct `Some` at the boundary where absence is genuinely
  possible and let it flow.
- Do not throw from a member returning `Result` (`WM2003`) — its signature has
  already promised failures are values, and a throw leaves callers handling two
  mechanisms.

### `UnwrapOrDefault` on a value type

For a value type, `UnwrapOrDefault` returns `0`, `false` or `default(Guid)`, and
nothing distinguishes that from a real one. `UnwrapOrNull` returns `null`
instead (`WM2015`). The default is fine when the caller genuinely wants it; the
point is to make that a decision rather than an accident.

## Where the detail lives

These areas carry more than the chain above needs. Load the one the code is
actually touching. Two further references —
[references/diagnostics.md](references/diagnostics.md) for the full rule table
and [references/state-overloads.md](references/state-overloads.md) for closure
mechanics — are pointed to above, where the situation that needs them arises.

| Read | When |
| --- | --- |
| [references/async.md](references/async.md) | The chain crosses an `await`, or `Try`/`TryAsync` is involved. The `*Async` members extend `Task<Option<T>>`, so a chain need not be broken into locals — and an async delegate handed to a synchronous member compiles silently while catching nothing |
| [references/sequences.md](references/sequences.md) | Working over an `IEnumerable` of monads — `Collect`, `Partition`, `Flatten` — or combining two with `Zip`, `Reduce` or `Xor`, several of which invert the obvious expectation |
| [references/nesting.md](references/nesting.md) | A monad has ended up inside another. Which shape to reach for, what `Transpose` maps to what in both directions, and when the nesting should be resolved with `OkOr` instead of preserved |
| [references/error-codes.md](references/error-codes.md) | Building an `Error`, or adding or shaping an error code. Codes come from an enum marked `[ErrorCodeCatalog]`, which generates compile-time constants; the runtime `FromEnum` factories are obsolete |
| [references/rust-to-csharp.md](references/rust-to-csharp.md) | Porting Rust, or a Rust idiom has no obvious C# spelling |

Most of the surface — every `*Async` member and every collection operation — is
extension methods in `Waystone.Monads.Options.Extensions` and
`Waystone.Monads.Results.Extensions`. Without that `using`, the methods do not
appear and the chain looks impossible to write. Add it before concluding a
member is missing.

## Sweep before finishing

Run over the code just written and rewrite each of these where it appears:

- [ ] Every `Unwrap` and `Expect` outside a test — replaced or propagated
- [ ] Every `IsSome`/`IsOk` followed by an unwrap — collapsed to `Match`,
      `IsSomeAnd`, `IsOkAnd` or `UnwrapOr`
- [ ] Every nested `Match` — flattened into `AndThen`/`Map`/`OrElse`
- [ ] Every `Match` branch that rebuilds the case it received — replaced with
      the combinator it was imitating
- [ ] Every `Map(...).Flatten()` — replaced with `AndThen`
- [ ] Every nested monad — accidental nesting flattened, and each deliberate
      `Result<Option<T>, E>` or `Option<Result<T, E>>` justified by a caller that
      acts on the empty case differently from the failed one
- [ ] Every `null`, `default` or `?` on a monad — replaced with `None`/`Err`
- [ ] Every awaited intermediate that only feeds the next step — rejoined with
      the `*Async` chain
- [ ] Every eager argument that is a call — moved to the `*Else` sibling
- [ ] Every capturing lambda — moved to the state overload
- [ ] Every discarded `Result` or `Option`

The build is the check that this landed: `WM1xxx` rules are warnings and
`WM2xxx` are informational, both enabled by default, and both ship inside the
`Waystone.Monads` package. A clean build with no `WM` diagnostics is the
completion bar. `WM3001` and `WM3002`, which flag nullable returns and throws
that could become monads, are **disabled by default** — enable them
deliberately when migrating a codebase onto the library, since they fire on
every nullable return and every throw in the project.
