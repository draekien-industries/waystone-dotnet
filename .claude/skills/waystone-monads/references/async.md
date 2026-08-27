# Async chains

The async surface exists so a chain survives an await boundary. Nothing here
requires awaiting into a local between steps, and doing so is the mistake this
reference exists to prevent.

## The receivers are what make chaining work

The `*Async` members are extension methods on **the task that wraps the monad**,
not on the monad. They extend `Task<Option<T>>`, `ValueTask<Option<T>>`,
`Task<Result<TOk, TErr>>` and `ValueTask<Result<TOk, TErr>>`, and there are
overloads on the bare monad too for a delegate that returns a task.

That is what lets a whole chain stay a single expression with one `await` at the
front:

```csharp
string name = await FetchUserAsync(id)
    .AndThenAsync(LoadProfileAsync)
    .MapAsync(profile => profile.DisplayName)
    .UnwrapOrAsync("anonymous");
```

Written without them, the same logic needs a local and a re-await per step,
which is where `IsSome` checks and `Unwrap` calls creep back in.

Each `*Async` member takes both a synchronous and an asynchronous delegate, so
`MapAsync` accepts `Func<T, TOut>` as well as `Func<T, Task<TOut>>`. A step that
does not itself need to await still belongs in the chain.

## Async members return ValueTask

Every `*Async` member returns `ValueTask` or `ValueTask<T>` — `MapAsync`,
`AndThenAsync`, `MatchAsync`, `UnwrapOrAsync` and the rest. There is no member
left to check.

From 7.0.0 that includes the two that used to be exceptions: `Option.TryAsync`
and `Result.TryAsync`, which are static factories rather than extensions, and
`CollectAsync`, which consumes an `IAsyncEnumerable`. Both returned `Task` up to
6.x. The reason they changed is not consistency — a `TryAsync` is the first link
in most chains, so a `Task` there is the one link that cannot compose.

`.AsTask()` is the conversion where a foreign API genuinely demands a `Task` —
`Should.ThrowAsync` taking a `Func<Task>`, say — and the compiler's own
type-mismatch diagnostic offers it as a fix. It is **not** a way to feed one async
chain into another, which no member here supports.

## A chain trips CA2012, and the chain is still right

`CA2012` fires on every chained `*Async` call: the intermediate `ValueTask` is
used as the receiver of the next member, and the rule only recognises a
`ValueTask` that is awaited, returned, or passed as a named argument. A reduced
extension receiver is none of those to the analyzer, even though it is one in
fact.

Awaiting does not clear it and neither does `ConfigureAwait` — the receiver is
what it reads, not the tail of the expression. Only a chain of a single `*Async`
call escapes.

The code is nonetheless correct: each member awaits its receiver exactly once, so
the one consumption `ValueTask` allows is the one it gets. Suppress `CA2012` where
the chain lives rather than breaking the chain into locals to satisfy it, because
a local is the thing `ValueTask` genuinely must not be stored in — satisfying the
rule that way is what would introduce the bug it warns about.

## An async delegate in a synchronous member is silent

This is the failure mode worth watching hardest, because nothing about it looks
wrong:

```csharp
// The delegate is never awaited. T is inferred as Task<Order>, which satisfies
// notnull, so this compiles and Try catches nothing.
Option<Task<Order>> trapped = Option.Try(async () => await FetchAsync(id));
```

The task ends up *inside* the monad, where nothing awaits it: the work has not
finished, anything it throws is unobserved, and `Try` converts no exception at
all. `WM1011` reports it, and ships no code fix, because renaming to the `Async`
sibling alone would leave the caller with an unawaited task and where the
`await` belongs is not something a fix can decide.

`Match` and `MapOr` hand the task straight back to the caller, who can await it,
so those stay quiet. `Map` and `MapErr` trap it.

## State overloads are only partly converted on the async side

The async surface has fewer state overloads than the synchronous one, and which
families are converted moves as the library grows. Where the `*Async` state
overload does not exist, await first and call the synchronous state overload on
the result rather than reintroducing a closure.

`WM2017` fires on the capture wherever the overload exists, so let it identify
the call rather than assuming symmetry with the sync surface.
