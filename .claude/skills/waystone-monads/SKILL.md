---
name: waystone-monads
description: Write idiomatic Waystone.Monads C# — compose Option<T> and Result<TOk, TErr> with Map, AndThen, Filter and Match rather than IsSome checks, Unwrap calls and nested branching. Use when writing or reviewing C# that returns Option or Result, when porting a nullable return or a thrown exception onto one, when a WM or WMS diagnostic fires, when extracting a reusable chain of fallible steps, when serializing or asserting on a monad, when configuring MonadOptions or observing the exceptions Try swallows, or when the user says "use an Option", "return a Result", "make this monadic", "make this chain reusable".
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

### Design steps so the chain is possible

A chain reads that way only when every step already has the shape a chain
accepts: **one parameter in, one monad out**. That is what makes a step a method
group rather than a lambda, and it is the constraint to design backwards from.

| A step needs | Give it |
| --- | --- |
| A dependency — a repository, a clock, a rate | A field set in the constructor, never a second parameter |
| A value an earlier step produced | A tuple carried forward, so the step still takes one parameter |
| Only to validate what it was handed | The same `T` back, so it slots in anywhere that `T` flows |
| To fail | One error code, so the failure names which step it was |

A **guard step** — `Order → Result<Order, Error>` — is the shape that makes
validation reusable rather than copied, because it fits every chain carrying an
`Order`. A step that takes two parameters fits none of them, and forces the
lambda the chain exists to avoid. A step that answers with `bool` fits only
`Filter`, which discards the reason — so where the reason is what the caller
needs, the guard returns the monad and not the predicate.

Every step in one chain must fail with the **same error type**; `AndThen` fixes
`TErr` and will not accept anything else. Where a step's failures come from
another taxonomy, convert at that seam with `MapErr` rather than widening the
chain's error type to accommodate both.

**A named chain is itself a step.** `Place(int) → Result<Shipment, Error>` has
exactly the shape `AndThen` takes, so a chain composes into a larger chain by
method group with no wrapper and no lambda. That is the whole of chain reuse:
extract what repeats into a method obeying the rules above, and it is available
everywhere the types line up. Read
[references/reusable-chains.md](references/reusable-chains.md) before extracting
one — a chain that must vary by caller and an `async` chain each have a shape
that stops composing, and the async one is a hard constraint rather than a
preference.

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
belongs in shipped code. A test is the one place a panic is a failed assertion —
but where `Waystone.Monads.Shouldly` is available, its assertions beat `Unwrap`
even there, because an `Unwrap` throws before the assertion runs and reports
nothing about what it found. Read
[references/shouldly.md](references/shouldly.md) when writing them.

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

A property pattern is the same check wearing a disguise, and it is worse than
the plain read rather than better:

```csharp
// Poor — a state check nothing recognises as one (WM2021)
if (option is { IsSome: true }) { return option.Unwrap(); }

// Poor — the same, in a switch arm
return option switch { { IsSome: true } => 1, _ => 0 };

// Good
return option.MapOr(0, _ => 1);
```

Reaching for `is { IsSome: true }` usually means the plain check felt wrong —
which it was. The answer is the combinator or `Match`, not a spelling of the
check that the rules cannot read.

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

**Construct through a factory; a bare value does not convert.** There is no
implicit conversion from `T` to a monad, so write `Option.Some(value)`,
`Result.Ok<TOk, TErr>(value)` or `Result.Err<TOk, TErr>(error)`. A `return value;`
carried over from an older version fails as `CS0029` or `CS1503`, and a code fix
sits on both — where a `Result` carries the same type on each side it offers `Ok`
and `Err` and does not choose for you.

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
- `Result<string, string>` leaves `Ok` indistinguishable from `Err` to a
  reader. Give the two sides different types. No rule reports it — the ambiguity
  a rule once caught was the implicit conversions', and those are gone.
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

## See what Try swallowed

`Try` and `TryAsync` catch the exception and hand back a `None` or an `Err`, so it
reaches no caller — and in the `Option` case is gone for good. The library reports
each one it catches on a meter and a `DiagnosticListener` named after itself, both
gated on whether anything is listening. Two things follow for code written here:

- **Configure logging through `Waystone.Monads.Extensions.Logging`,** with
  `UseLoggerFactoryFrom`, `UseLoggerFactory` or `UseLogger`. The hand-written
  `MonadOptions.UseExceptionLogger` hook was removed in `7.0.0`, so a call
  carried over from 6.x fails as `CS1061`.
- **Never write one of the names as a literal.** `MonadDiagnostics` carries a
  constant for every meter, listener, event, instrument and tag name, and a
  *token* per event that pairs the name with its payload type — subscribe through
  the token and use a constant anywhere a bare string is required. A mistyped
  literal subscribes to nothing and fails silently: no exception, no warning, an
  empty dashboard.

Read [references/observability.md](references/observability.md) before wiring up
either channel and before writing a test that asserts a `Try` swallowed
something.

## Where the detail lives

These areas carry more than the chain above needs. Load the one the code is
actually touching. Three further references —
[references/diagnostics.md](references/diagnostics.md) for the full rule table,
[references/state-overloads.md](references/state-overloads.md) for closure
mechanics and [references/observability.md](references/observability.md) for the
metrics, logging and raw-event channels — are pointed to above, where the
situation that needs them arises.

**Every code sample, here and in the references, is illustrative.** The recurring
`Order`, `Quote`, `Invoice` and `Shipment` types are there to make a shape legible
in isolation, and none of them is a type this library ships. Substitute the domain
types of the codebase being worked in and keep the shape — a chain copied verbatim
compiles against nothing.

| Read | When |
| --- | --- |
| [references/async.md](references/async.md) | The chain crosses an `await`, or `Try`/`TryAsync` is involved. The `*Async` members extend `Task<Option<T>>`, so a chain need not be broken into locals — and an async delegate handed to a synchronous member compiles silently while catching nothing |
| [references/sequences.md](references/sequences.md) | Working over an `IEnumerable` of monads — `Collect`, `Partition`, `Flatten` — or combining two with `Zip`, `Reduce` or `Xor`, several of which invert the obvious expectation |
| [references/reusable-chains.md](references/reusable-chains.md) | Extracting a chain for reuse, or a chain has to vary by caller. Why an async step must be declared `ValueTask` for a chain to compose as one and which parameters take that shape, where a variation point goes, why a library of composed `Func` values is worse than the chain, and how many tests a chain needs once its steps are tested |
| [references/nesting.md](references/nesting.md) | A monad has ended up inside another. Which shape to reach for, what `Transpose` maps to what in both directions, and when the nesting should be resolved with `OkOr` instead of preserved |
| [references/error-codes.md](references/error-codes.md) | Building an `Error`, or adding or shaping an error code. Codes come from an enum marked `[ErrorCodeCatalog]`, which generates compile-time constants. Construct failures through `{EnumName}Catalog.Errors.{Member}(message)` rather than the `ToError` extension |
| [references/rust-to-csharp.md](references/rust-to-csharp.md) | Porting Rust, or a Rust idiom has no obvious C# spelling |

Most of the surface — every `*Async` member and every collection operation — is
extension methods in `Waystone.Monads.Options.Extensions` and
`Waystone.Monads.Results.Extensions`. Without that `using`, the methods do not
appear and the chain looks impossible to write. Add it before concluding a
member is missing.

## The companion packages

Core ships the monads, the analyzer and the error-code generator. Everything else
is a package a project installs deliberately, and each shadows the namespace of
the library it companions rather than sitting under a parallel `Waystone` tree —
so the types appear under a `using` the file already has. Check which are
referenced before concluding a shape is unavailable, and read the one being used
rather than guessing at its surface.

| Read | When |
| --- | --- |
| [references/shouldly.md](references/shouldly.md) | Writing or reviewing a test that asserts on a monad |
| [references/fluent-validation.md](references/fluent-validation.md) | A validator has to become a step in a `Result` chain, or a validation failure has to reach a problem-details payload |
| [references/linq.md](references/linq.md) | Query syntax is in play, or a chain's later steps each need a value an earlier one produced |
| [references/dependency-injection.md](references/dependency-injection.md) | `MonadOptions` is configured from a container rather than by a static call |
| [references/hosting.md](references/hosting.md) | That container is a host, and the install should run from its start-up |
| [references/system-text-json.md](references/system-text-json.md) | A monad is serialized with `System.Text.Json` |
| [references/newtonsoft-json.md](references/newtonsoft-json.md) | A monad is serialized with `Newtonsoft.Json` |

`Waystone.Monads.Extensions.Logging` is a companion package too;
[references/observability.md](references/observability.md) covers it beside the
metric and event channels it belongs with.

## Sweep before finishing

Run over the code just written and rewrite each of these where it appears:

- [ ] Every `Unwrap` and `Expect` outside a test — replaced or propagated
- [ ] Every `IsSome`/`IsOk` followed by an unwrap — collapsed to `Match`,
      `IsSomeAnd`, `IsOkAnd` or `UnwrapOr`
- [ ] Every `is { IsSome: true }` or equivalent property pattern — replaced with
      the combinator or `Match` the check was avoiding
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
- [ ] Every capturing lambda — moved to the state overload, on the `*Async`
      surface as readily as on the synchronous one
- [ ] Every discarded `Result` or `Option`
- [ ] Every step taking two parameters — reshaped to one in, one monad out, so
      the chain takes it as a method group rather than a lambda
- [ ] Every run of steps repeated across chains — extracted into a named chain
      and reused as a method group, whether or not any step awaits
- [ ] Every async step declared `Task` — redeclared `ValueTask`, so a chain can
      take it by name (`WM2022`); only a delegate returning a non-monad keeps
      `Task`
- [ ] Every `.AsTask()` reached for to make an async chain composable — removed,
      since it was never the conversion that shape needed
- [ ] Every observability name written as a literal — replaced with the
      `MonadDiagnostics` token where it subscribes to an event, and with the name
      constant where a bare string is required
- [ ] Every assertion on `IsSome`/`IsOk` or on an `Unwrap` in a test — replaced
      with the assertion that reports the monad (`WMS2001`)

The build is the check that this landed: `WM1xxx` rules are warnings and
`WM2xxx` are informational, both enabled by default, and both ship inside the
`Waystone.Monads` package. A clean build with no `WM` diagnostic — and no `WMS`
diagnostic where the assertions package is referenced — is the completion bar.
`WM3001` and `WM3002`, which flag nullable returns and throws that could become
monads, are **disabled by default** — enable them deliberately when migrating a
codebase onto the library, since they fire on every nullable return and every
throw in the project.

A project can raise the whole tier at once with the `WaystoneMonadsRuleset`
property rather than rule by rule; see
[references/diagnostics.md](references/diagnostics.md) before setting it, since
`strict` turns the migration pair on.
