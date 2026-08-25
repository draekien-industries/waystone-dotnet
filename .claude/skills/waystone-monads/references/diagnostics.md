# The WM diagnostics

The analyzer ships inside the `Waystone.Monads` package and every consumer gets
it on upgrade, with no opt-out beyond `.editorconfig`. Three tiers, and the tier
sets the severity:

| Tier | Category | Severity | Enabled by default |
| --- | --- | --- | --- |
| Bug (`WM1xxx`) | Reliability | Warning | Yes |
| Idiom (`WM2xxx`) | Usage | Info | Yes |
| Migration (`WM3xxx`) | Design | Info | **No** |

A `WM1xxx` is a defect: the code throws, silently drops a failure, or defeats
the type. A `WM2xxx` is a rewrite into the idiom, and informational severity is
deliberate — it surfaces the context and lets the author decide. A `WM3xxx`
fires on ordinary non-monadic C# and is off until a migration turns it on.

## Bugs

| Id | Flags | Fix |
| --- | --- | --- |
| `WM1001` | `Option.Some` given a value that is null or `default` | `Option.None<T>()` — `Some`'s constructor rejects null, so the call always throws |
| `WM1002` | `null` assigned to an `Option` or `Result` | `None` or `Err`; null throws on the next member access |
| `WM1003` | `default(Option<T>)` or `default(Result<T, E>)` | Construct the case — both are reference types, so their default is null |
| `WM1005` | `Option.Some` given a possibly-null value | `Option.FromNullable`, which maps null onto `None` |
| `WM1006` | A `Result` returned and unused | Match on it or propagate it — a discarded `Result` reports nothing |
| `WM1008` | `Option<T>?` or `Result<T, E>?` declared | Drop the annotation; the type already has exactly the two states it needs |
| `WM1011` | An async delegate passed to a synchronous member | The `Async` sibling — otherwise the task is trapped in the monad, unobserved, and `Try` catches nothing |

## Idioms

| Id | Flags | Fix |
| --- | --- | --- |
| `WM2001` | `Unwrap` | `UnwrapOr`, `UnwrapOrElse`, `UnwrapOrDefault` or `Match` |
| `WM2002` | `Expect` | Same as `WM2001`. Separate rule so a codebase can keep `Expect` where an invariant is genuine and still ban `Unwrap` |
| `WM2003` | A `throw` inside a member returning `Result` | Return an `Err` — the signature already promised failures are values |
| `WM2004` | An `IsSome`/`IsOk` guard whose body unwraps | `Match` or `Inspect`, which express both branches once |
| `WM2005` | `Map` followed by `Flatten` | `AndThen`, which avoids materialising the nested monad |
| `WM2006` | A state check combined with an unwrap of the same instance | `IsSomeAnd`, `IsNoneOr`, `IsOkAnd` or `IsErrAnd` |
| `WM2007` | `UnwrapOr` given the type's default | `UnwrapOrDefault` |
| `WM2008` | An `Option` or `Result` compared to null | `IsNone` / `IsErr` — a null check reads as an absence check but is not one |
| `WM2009` | `Option<Option<T>>` | `Flatten` — the inner/outer distinction is one callers never act on |
| `WM2010` | A `Result` whose `TOk` and `TErr` are the same type | Distinct types; identical ones make both implicit conversions ambiguous |
| `WM2011` | `Some`, `None`, `Ok` or `Err` named in a declaration | The base type, so both cases stay representable |
| `WM2012` | A nullable-returning member on a type that elsewhere returns `Option`/`Result` | One absence convention per type |
| `WM2013` | An `Option` returned and unused | Usually a sign the value was meant to be handled |
| `WM2015` | `UnwrapOrDefault`/`MapOrDefault` on a value type | `UnwrapOrNull`/`MapOrNull`, so the absent case is distinguishable from a real `0` or `false` |
| `WM2016` | An eager argument that is not provably free | The `*Else` sibling. Fires on calls, `new` and `await`; stays silent on constants, locals, parameters, fields and property reads |
| `WM2017` | A delegate capturing a local or parameter | The overload taking state. Capturing only `this` is excluded |
| `WM2018` | Two `[ErrorCodeCatalog]` enums generating the same code | Rename one enum or the colliding member — no two taxonomies should share a wire code |
| `WM2019` | A generated code missing from `ErrorCodes.txt` | Invoke the fix, then read the added line before committing |
| `WM2020` | An `ErrorCodes.txt` entry no catalog generates | Delete the line, or restore the member if the code was removed by mistake |

`WM2007` and `WM2015` point in opposite directions on a value type by design:
the first removes a repeated type from `UnwrapOr`, the second asks whether the
default was meant as a value. Applying `WM2007`'s fix on a value type therefore
produces code `WM2015` reports, and that is intended rather than a bug.

## Migration

Both are off by default and fire on ordinary C#, not only on code already using
the library. Turn them on for a migration, work through them, and turn them off
again.

| Id | Flags | Fix |
| --- | --- | --- |
| `WM3001` | A member returning a nullable type | An `Option<T>` return, which makes the absent case impossible to ignore |
| `WM3002` | A `throw` statement | A `Result<TOk, Error>` return, which states the failure in the signature |
