# Nested monads

Most nesting is an accident and wants removing. The two mixed shapes are the
exception: they are the only ones that say something a single monad cannot.

| Shape | Says | Usually |
| --- | --- | --- |
| `Option<Option<T>>` | An absent outer and an absent inner, which no caller distinguishes | Accidental (`WM2009`) |
| `Result<Result<T, E>, E>` | Two failures of the same taxonomy, arbitrarily ordered | Accidental |
| `Result<Option<T>, E>` | The operation succeeded, and found nothing | Deliberate |
| `Option<Result<T, E>>` | There may have been no work; if there was, it may have failed | Deliberate |

## Accidental nesting comes from Map

A `Map` whose delegate itself returns a monad produces one level of nesting, and
the next line is almost always a `Flatten`. That pair is `AndThen` (`WM2005`),
which never materialises the nested value at all. Reach for `Flatten` only where
the nesting arrived from somewhere you do not control.

`Flatten` removes **one** level, and only where both levels are the same monad —
`Result<Result<TOk, TErr>, TErr>` needs the same `TErr` on both, so it cannot
merge two error taxonomies. Convert one with `MapErr` first.

Only `Option<Option<T>>` has a diagnostic behind it. Nested `Result` and both
mixed shapes are reported by nothing, so whether the nesting is meant is a
judgement no build will make for you.

## Transpose swaps the two monads

`Transpose` is the only way between the two deliberate shapes, and it exists on
both, so it reads the same in either direction.

`Option<Result<TOk, TErr>>` to `Result<Option<TOk>, TErr>`:

| From | To |
| --- | --- |
| `None` | `Ok(None)` |
| `Some(Ok(v))` | `Ok(Some(v))` |
| `Some(Err(e))` | `Err(e)` |

`Result<Option<TOk>, TErr>` to `Option<Result<TOk, TErr>>`:

| From | To |
| --- | --- |
| `Ok(None)` | `None` |
| `Ok(Some(v))` | `Some(Ok(v))` |
| `Err(e)` | `Some(Err(e))` |

The two are exact inverses: transposing twice returns what you started with,
from either side, and no error is ever discarded. The row that looks lossy —
`Some(Err(e))` becoming a bare `Err(e)` — drops only the outer wrapper, which
carried nothing beyond "there was a result".

That exactness is what makes `Transpose` safe to apply wherever the shape is
inconvenient. It is also why it is the wrong tool when the shapes are not
equivalent: see below.

## Choosing which absence is inner

Pick by which of the two questions the caller must answer first, because that is
what the outer monad forces.

```csharp
// The query is what can fail; finding nothing is a normal answer.
Result<Option<User>, Error> FindUser(int id);

// There may be nothing due at all; a charge that runs can fail.
Option<Result<Receipt, Error>> ChargeIfDue(Account account);
```

Inverting either one makes the caller handle the wrong thing first —
`Option<Result<User, Error>>` from a lookup asks the caller to decide what an
outer `None` means before they can see whether the query even ran.

## Do not nest when one level was enough

The most common mistake is not choosing the wrong nesting but keeping a nesting
the caller never wanted. If every caller of a `Result<Option<T>, E>` treats
`None` as a failure, the absence was an error all along:

```csharp
// Poor — every caller writes the same OkOr
Result<Option<User>, Error> FindUser(int id);

// Good — the method makes the decision once
public Result<User, Error> FindUser(int id) =>
    Query(id).AndThen(
        static user => user.OkOrElse(
            static () => UserErrorCodeCatalog.Errors.NotFound("No such user.")));
```

Keep the nested shape only where a caller genuinely acts on the empty case
differently from the failed one. `AndThen` with `OkOr` inside is how the
decision is made — not `Transpose`, which preserves the distinction rather than
resolving it.

## Converting a single level

| From | To | Use |
| --- | --- | --- |
| `Option<T>` | `Result<T, TErr>` | `OkOr(error)` / `OkOrElse(factory)` |
| `Result<TOk, TErr>` | `Option<TOk>` | `GetOk()` |
| `Result<TOk, TErr>` | `Option<TErr>` | `GetErr()` |

The two directions are not symmetric, and neither round-trips:

- `OkOr` **adds** the reason absence was a failure, so it needs one supplied. It
  evaluates its argument eagerly — use `OkOrElse` whenever the error is built by
  a call rather than read from a field (`WM2016`).
- `GetOk` and `GetErr` **discard** the other side. `result.GetOk().OkOr(fallback)`
  compiles and looks like a round trip, but the original error is gone and every
  failure now reports `fallback`. Reach for `MapErr` when the intent was to
  change the error rather than lose it.

`GetOk` is worth its lossiness only where the caller has already handled the
error, or where the reason genuinely does not matter — logging a count, or
feeding a collection that drops absent elements.
