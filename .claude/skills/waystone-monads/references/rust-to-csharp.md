# Rust habits in C#

The library is a deliberate port, so most Rust reflexes carry over unchanged and
the member names were chosen to match. The places worth care are the three where
C# has no equivalent construct, and the one where a Rust habit is actively
harmful in shipped C#.

## Names that are not just a casing change

Most members keep their meaning and change only `snake_case` to `PascalCase` —
`map` to `Map`, `and_then` to `AndThen`, `unwrap_or_else` to `UnwrapOrElse`.
Convert those on sight. These are the ones that do not follow:

| Rust | Waystone.Monads | Why it moved |
| --- | --- | --- |
| `ok()` | `GetOk()` | `Ok` is the case type, so the name was taken |
| `err()` | `GetErr()` | `Err` is the case type |
| `iter()` | `AsEnumerable()` | .NET convention |
| `contains(x)` | `IsSomeAnd(v => v == x)` | Not ported; unstable in Rust too |

`ok()` and `err()` are the pair that catches people out: searching for `Ok` and
`Err` finds the case types instead.

## What is not ported at all

| Rust | Why |
| --- | --- |
| `?` | A language feature — see below |
| `take`, `replace`, `insert`, `get_or_insert`, `as_mut`, `iter_mut` | These mutate in place; both types here are immutable records |
| `as_ref`, `as_deref`, `copied`, `cloned` | Borrow and ownership projections that C# reference semantics make unnecessary |
| `unwrap_unchecked`, `unwrap_err_unchecked` | No unsafe-contract idiom to hang them on |

## What has no Rust original

Do not go looking for the counterpart — there is none.

| Member | What it does |
| --- | --- |
| `Reduce` | Merges two `Option<T>`, and a present value survives an absent one — unlike `ZipWith`, which needs both |
| `FromNullable` | Builds an `Option<T>` from a `T?` |
| `UnwrapOrNull`, `MapOrNull` | Return `null` rather than `default` on a value type |
| `Try`, `TryAsync` | Run a delegate and turn a throw into a `None` or an `Err` |
| `Partition` | Splits a sequence into successes and failures |
| The `*Async` surface | Every operation over a `Task` or `ValueTask` receiver |
| The state overloads | Pass a captured value as an argument so the delegate allocates nothing |

## There is a fourth state here: null

Rust's `Option<T>` is a value type and cannot be uninitialised, so a Rust
instinct gives no warning at all about this one. Here both types are reference
types, so null is reachable and the two-state guarantee is enforced by the
analyzer rather than by the type system. A further consequence with no Rust
parallel: null is rejected at *run* time — `Option.Some(null)` throws rather
than failing to compile, because nullable reference types are annotations rather
than guarantees.

See "Treating the monad as nullable" in the body for the forms this takes.

## The `?` operator becomes `AndThen`

This is the biggest translation. Rust's `?` unwraps or returns early, so a
sequence of fallible calls reads as straight-line code:

```rust
fn place(id: u32) -> Result<Shipment, Error> {
    let order = find(id)?;
    let order = not_yet_shipped(order)?;
    let reservation = reserve(order)?;
    dispatch(reservation)
}
```

C# has no `?`. The equivalent is a chain of `AndThen` over **method groups**,
which produces the same short-circuiting and reads about as well:

```csharp
public Result<Shipment, Error> Place(int id) =>
    Find(id)
       .AndThen(NotYetShipped)
       .AndThen(Reserve)
       .AndThen(Dispatch);
```

Do not simulate `?` with exceptions, with an early `return` after an `IsErr`
check, or with a `Match` per step. Each of those reintroduces the branching
`AndThen` removes.

Where a step needs a value from an earlier step as well as the current one, keep
the chain and carry a tuple, or use the state overload rather than capturing:
`AndThen(order, (reservation, o) => …)`.

## `?` does not convert error types for you

Rust's `?` applies a `From` conversion, so a function returning
`Result<T, AppError>` can `?` a `Result<T, IoError>`. Nothing in C# does that
implicitly. Convert explicitly with `MapErr` at the point the error crosses
into a different taxonomy:

```csharp
return ReadFile(path)
    .MapErr(io => AppErrorCodeCatalog.Errors.Unreadable(io.Message))
    .AndThen(Parse);
```

## `if let` and `match` become `Match` — not a type pattern

Rust's `if let Some(x) = opt` has two C# translations depending on what the
branch does:

| The branch | Use |
| --- | --- |
| Produces a value in both cases | `Match(onSome, onNone)` |
| Only runs a side effect on the present case | `Inspect` |
| Only computes a `bool` | `IsSomeAnd` / `IsNoneOr` |
| Supplies a fallback | `UnwrapOr` / `UnwrapOrElse` |

Do not reach for a C# type pattern over `Some<T>`/`None<T>`. The hierarchy is
closed to outside assemblies, but the compiler still cannot prove a switch over
it exhaustive, so every such switch needs a discard arm that can never run —
`Match` is total and takes both branches by construction. `WM2011` reports the
related mistake of naming a case in a declaration.

## `unwrap()` in a prototype is not `Unwrap` in C#

Rust culture tolerates `unwrap()` in examples and prototypes because a panic is
loud, local and expected to be removed. The same habit in C# ships, and
`Unwrap` there is an unhandled exception in production — the exact outcome the
type was adopted to prevent. `WM2001` and `WM2002` report both `Unwrap` and
`Expect` by default.

Keep `Unwrap` to tests, where a throw is a failed assertion. Everywhere else,
collapse with `UnwrapOr`, `UnwrapOrElse` or `Match`, or propagate the monad and
let a boundary collapse it.

## `unwrap_or_default` on a value type

Rust gates `unwrap_or_default` behind a `T: Default` bound, so a type opts in by
implementing the trait. In C# `default(T)` always exists, and `T?` on a type
parameter constrained only to `notnull` is an annotation rather than a
`Nullable<T>` — so `UnwrapOrDefault` on `Option<int>` returns `0` with nothing
marking it absent, and a missing count is indistinguishable from a real zero
afterwards.

`UnwrapOrNull` and `MapOrNull` return `null` instead. Prefer them on value types
unless the default is genuinely the wanted answer; `WM2015` points at the choice.

## Iterator chains

Rust's iterator adapters map onto LINQ, with the monad-aware operations supplied
as collection extensions rather than as LINQ operators. Three differences catch
a Rust reader:

- `filter_map`-style dropping of absent elements is `Flatten`. The
  collection-level `Filter` is *not* its equivalent — it preserves length,
  turning failing elements into absent ones in place.
- `Partition` is closest to itertools' `partition_result`, not to any `std`
  member. `Collect` keeps only the first failure; `Partition` keeps them all.
- `Collect` short-circuits exactly as Rust's does, but enumerates eagerly when
  called rather than lazily when read.
