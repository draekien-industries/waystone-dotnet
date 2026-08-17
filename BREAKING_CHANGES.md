# Breaking Changes

This file tracks API that is deprecated in the current release line and will be
removed in the next major release, **v6.0.0**.

Nothing listed here is broken yet. Every entry still compiles and behaves as it
did before, but emits a `CS0618` obsolete warning. Migrate before upgrading to
v6.0.0, where the listed members are deleted.

Latest release: **v5.1.1**. Packages in this repository share a single version,
calculated by GitVersion.

## Waystone.Monads

### `Try` overloads that accept an async factory

**Deprecated in:** 5.2.0 · **Removed in:** 6.0.0 · **Replacement:** `TryAsync`

A lambda whose body is a `throw` expression is convertible to both `Func<T>` and
`Func<Task<T>>`, so the sync and async `Try` overloads are ambiguous at those
call sites and the caller has to declare the delegate type to disambiguate.
Giving the async overloads their own name removes the ambiguity.

Affected members:

| Deprecated | Replacement |
| --- | --- |
| `Option.Try<T>(Func<Task<T>>, …)` | `Option.TryAsync<T>(Func<Task<T>>, …)` |
| `Result.Try<TOk, TErr>(Func<Task<TOk>>, Func<Exception, TErr>, …)` | `Result.TryAsync<TOk, TErr>(Func<Task<TOk>>, Func<Exception, TErr>, …)` |

Migration is a rename. Behaviour, parameters and return types are unchanged:

```csharp
// before
Option<int> option = await Option.Try(() => FetchAsync());
Result<int, string> result = await Result.Try(() => FetchAsync(), ex => ex.Message);

// after
Option<int> option = await Option.TryAsync(() => FetchAsync());
Result<int, string> result = await Result.TryAsync(() => FetchAsync(), ex => ex.Message);
```

The synchronous `Option.Try<T>(Func<T>, …)` and
`Result.Try<TOk, TErr>(Func<TOk>, Func<Exception, TErr>, …)` overloads are not
deprecated and keep the `Try` name.

> [!NOTE]
> `Result.TryAsync<TOk>(Func<Task<TOk>>, …)` — the overload that defaults the
> error type to `Error` — was introduced as `TryAsync` and never carried a `Try`
> spelling, so there is nothing to migrate.

## Adding an entry

When you deprecate something, mark it `[Obsolete]` in the same change and add it
here. The attribute message should name the replacement and the removal version,
so it reads the same in the IDE as it does in this file:

```csharp
[Obsolete(
    "Use TryAsync instead. This overload will be removed in v6 of Waystone.Monads.")]
```

Group entries under the owning package, and record the version the deprecation
shipped in as well as the version that removes it. When v6.0.0 is cut, delete
the removed members, move their entries into the release notes, and clear this
file for the next major.
