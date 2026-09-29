---
name: waystone-monads
description: Write idiomatic Waystone.Monads C# — compose Option<T> and Result<TOk, TErr> with Map, AndThen, Filter and Match rather than IsSome checks, Unwrap calls and nested branching. Use when writing or reviewing C# that returns Option or Result, when porting a nullable return or a thrown exception onto one, when a WM, WMG, WMS or WMSC diagnostic fires, when extracting a reusable chain of fallible steps, when parsing untrusted input with Waystone.Monads.Schemas or adding a schema rule of your own, when serializing or asserting on a monad, when configuring MonadOptions or observing the exceptions Try swallows, or when the user says "use an Option", "return a Result", "make this monadic", "make this chain reusable", "parse this payload", "add a schema rule".
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

## Look the surface up on the site

This skill carries how the library is meant to be written; the documentation site
carries what the library is. Before relying on a member's exact signature, a
diagnostic's meaning or a companion package's surface, read the **docs index** at
`https://draekien-industries.wpei.me/llms.txt` and fetch the markdown page it
names. Distrust recalled API: the surface has changed across majors, and the site
tracks the current release.

`https://draekien-industries.wpei.me/llms-full.txt` is every page concatenated,
far more than a context should hold. Reach for it only when the docs index names no
page that answers the question, and search it for the member or diagnostic id
rather than reading it whole. If neither can be fetched, say so and treat any
signature or diagnostic meaning written from recall as unverified.

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
`Option<Result<T, E>>`. Nothing in the build checks the choice. The outer monad
is the question the caller answers first: a lookup that can fail and may find
nothing is `Result<Option<User>, Error>`. Keep the nesting only where a caller
acts on the empty case differently from the failed one — if every caller would
write the same `OkOr`, resolve it once inside the method with `OkOrElse`, not
with `Transpose`, which preserves the distinction.

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
| A dependency — a repository, a clock, a rate | A readonly field assigned in the constructor, never a second parameter |
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
everywhere the types line up. Three shapes stop a chain composing:

- **An async step declared `Task`.** `AndThenAsync` and `OrElseAsync` take
  `ValueTask` steps, so a `Task` method group fails as `CS0411`, which reads as a
  generics problem (`WM2022`). Redeclare the step `ValueTask`.
- **A variation point passed as a parameter.** Hold the varying `Func` in a
  readonly field, like any dependency — never compose a collection of `Func`
  values, which reads worse than `AndThen` and loses the method names.
- **A chain that gathers its own results.** `Collect` or `Partition` is the
  caller's choice: `orders.Select(Bill).Collect()` at the call site.

**Where the project raises `CA2012`, await each step into a local; never
suppress it.** A chained `*Async` call consumes the `ValueTask` before it as an
extension receiver, which `CA2012` does not recognise, so under `AnalysisMode: All`
or a raised severity it reports every link. Store the awaited monad, never the
un-awaited `ValueTask`:

```csharp
Result<Order, Error> order = await FindAsync(id);
return await order.AndThenAsync(Charge);
```

Test each step, then the chain once per short-circuit point: the error reaching the caller is the failing step's, **unchanged**.

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
nothing about what it found. Read the Shouldly page in the docs index when
writing them.

## Traps

Each of these is a real failure mode with a diagnostic behind it. When a `WM`,
`WMG`, `WMS` or `WMSC` code fires and its meaning is not obvious, look it up in
the docs index: the Analyzers pages carry one page per `WM` tier plus Assertion
rules for `WMS`, and Generator diagnostics carries `WMG` and `WMSC`.

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

The pairing runs both ways. A delegate whose body is a literal, a constant or a
variable already in scope defers nothing — the value was built before the
delegate was handed over, so the call allocates a delegate for no gain and tells
its reader the fallback is costly when it is not (`WM2024`):

```csharp
option.UnwrapOrElse(() => fallback);  // defers a value already built
option.UnwrapOr(fallback);            // takes it directly
```

### Allocating a closure per call

A lambda that captures a local or a parameter allocates a display class on every
call. Bind the value as **state** with `With` and call the member on the binder
it returns; the lambda then closes over nothing (`WM2017`):

```csharp
option.With(multiplier).Map(static (value, m) => value * m);
```

Mark every such lambda `static`, so a later edit cannot silently reintroduce the
capture. Never bind an `Option` as state: the binder hands it over untouched, so
the second absence is the author's to remember, while `Zip` and `ZipWith` answer
`None` whenever either side is absent (`WM2023`). Read the State overloads page
in the docs index when rewriting one — where the state argument goes differs by branch, and an older form passing
state as the call's first argument is still supported but reaches less of the
async surface.

### An async delegate handed to a synchronous member

```csharp
// Compiles. T is inferred as Task<Order>, which satisfies notnull.
Option<Task<Order>> trapped = Option.Try(async () => await FetchAsync(id));
```

The task ends up *inside* the monad, where nothing awaits it: the work has not
finished, anything it throws is unobserved, and `Try` converts no exception at
all. Nothing about the call site looks wrong, which is what makes it the async
failure worth watching hardest. Use the `Async` sibling (`WM1011`).

### Over-wrapping

The type earns its place only where absence or failure is real.

- `Option<bool>` has three states and almost always wants to be two — model it
  as an enum or split the question.
- `Result<string, string>` leaves `Ok` indistinguishable from `Err` to a
  reader. Give the two sides different types; no rule reports it.
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
each one on a meter and a `DiagnosticListener`. Two things follow:

- **Configure logging through `Waystone.Monads.Extensions.Logging`,** with
  `UseLoggerFactoryFrom`, `UseLoggerFactory` or `UseLogger`. The hand-written
  `MonadOptions.UseExceptionLogger` hook was removed in `7.0.0`, so a call
  carried over from 6.x fails as `CS1061`.
- **Never write one of the names as a literal.** Subscribe through the
  `MonadDiagnostics` token for the event, and use its name constant anywhere a
  bare string is required. A mistyped literal subscribes to nothing and fails
  silently: no exception, no warning, an empty dashboard.

Read the Observability guide and the Logging page in the docs index before wiring
up either channel or asserting in a test that a `Try` swallowed something.

## Parse at the boundary

`Waystone.Monads.Schemas` is a separate package that **parses rather than
validates**: it does not check an object you already built, it builds one. `Parse`
hands back `Result<TOut, SchemaViolation>` and reports every failure at once, so a
parse collapses and composes like any other `Result` — a step in a chain, not a
parallel mechanism.

```csharp
Result<Quest, SchemaViolation> quest = QuestSchema.Instance.Parse(posting);
```

Two things decide most of what the code looks like. **A schema is composition all
the way down** — there is no rule interface and no schema to subclass, so a rule
of your own is an extension method returning `schema.Check(...)`. And **a field
set is synchronous**: `Configure` returns a value rather than a task, so an
asynchronous rule reached from one throws `InvalidOperationException` however the
caller parsed, which is why `WMSC0006` is an error rather than advice.

Read the Schemas guide and the Schemas reference pages in the docs index before
writing one — the traps that bite hardest are silent: a message token that
renders literally rather than failing, a `Schema.Uuid` that accepts `Guid.Empty`,
and a `Refine` that discards a value you meant to keep.

## Where the detail lives

**Every code sample here is illustrative.** The recurring `Order`, `Quote`,
`Invoice` and `Shipment` types are not types this library ships. Substitute the
domain types of the codebase being worked in and keep the shape — a chain copied
verbatim compiles against nothing.

Fetch the page from the docs index for the area the code is actually touching:

| Fetch | When |
| --- | --- |
| The Option API or Result API page and the sub-page for the member's group | Checking a member's signature or overloads before calling it |
| The Async guide | The chain crosses an `await`, or `Try`/`TryAsync` is involved. The `*Async` members extend `Task<Option<T>>`, so a chain need not be broken into locals, and an async delegate handed to a synchronous member compiles silently while catching nothing |
| The Collections page for the monad in hand | Working over an `IEnumerable` of monads — `Collect`, `Partition`, `Flatten` — or combining two with `Zip`, `Reduce` or `Xor`, several of which invert the obvious expectation |
| The Nesting and conversion page for the outer monad | A monad has ended up inside another, and what `Transpose` maps to what matters |
| The Errors guide and the Source generation pages | Building an `Error`, or adding or shaping an error code. Codes come from an enum marked `[ErrorCodeCatalog]`, which generates compile-time constants. Construct failures through `{EnumName}Catalog.Errors.{Member}(message)` rather than the `ToError` extension |
| Coming from Rust | Porting Rust, or a Rust idiom has no obvious C# spelling |

Most of the surface — `With`, every `*Async` member and every collection
operation — is extension methods in `Waystone.Monads.Options.Extensions` and
`Waystone.Monads.Results.Extensions`. Add that `using` before concluding a
member is missing.

## The companion packages

Core ships the monads, the analyzer and the error-code generator. Everything else
is a package a project installs deliberately. Check which are referenced before
concluding a shape is unavailable, and fetch the page for the one being used
rather than guessing at its surface — the docs index lists them under Add-ons,
which extend the library itself, and Integrations, which connect it to a
third-party library. Reach for one when a validator has to become a chain step,
when later steps need values earlier ones produced (LINQ query syntax), when
`MonadOptions` is configured from a container or host, or when a monad is
serialized.

An Integrations package **shadows the namespace of the library it companions**
rather than sitting under a parallel `Waystone` tree, so its types appear under a
`using` the file already has. An Add-on keeps its own `Waystone.Monads.*`
namespace.

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
      the `*Async` chain, unless the project raises `CA2012`
- [ ] Every eager argument that is a call — moved to the `*Else` sibling
- [ ] Every `*Else` delegate whose body is a literal, a constant or a variable
      already in scope — moved back to the eager sibling, which takes the value
      directly (`WM2024`)
- [ ] Every capturing lambda — bound with `With` and called on the binder, on the
      `*Async` surface as readily as on the synchronous one
- [ ] Every `Option` bound as state — replaced with `Zip` or `ZipWith`, so the
      second absence cannot slip past the delegate (`WM2023`)
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

Where a schema was written or edited, four more:

- [ ] Every `Schema.For<T>()` that has a named spelling — replaced with it
      (`WMSC0009`)
- [ ] Every message template — read for a token that is not `{Path}`,
      `{Received}`, `{Predicate}` or `{Code}`, since anything else reaches the
      caller verbatim and `{Expected}` cannot be filled from `Check` at all
- [ ] Every field passed to `Refine` that produces a value — listed in
      `Schema.Fields` if the value was wanted, `.AsChecked()` if it was not
      (`WMSC0005`)
- [ ] Every field whose path came from something other than a member access —
      given a `.Named(...)` (`WMSC0008`)

The build is the check that this landed. A clean build with no `WM` or `WMG`
diagnostic — no `WMS` where the assertions package is referenced, and no `WMSC`
where the schemas package is — is the completion bar. A `WMG` error means no
catalog was generated at all, so every call site reaching for a generated member
fails alongside it. `WMSC0009` is the one rule a build never shows, so it takes a
read rather than a build to clear. `WM3001` and `WM3002` are **disabled by
default** — enable them only when migrating a codebase onto the library, since
they fire on every nullable return and every throw. Read the Severity presets page
in the docs index before setting `WaystoneMonadsRuleset`, since `strict` turns
that pair on.
