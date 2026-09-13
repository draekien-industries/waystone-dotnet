# Writing an async member on a state binder

Read before editing an async member of `Option<T>.Bound<TState>` or
`Result<TOk, TErr>.Bound<TState>` — `src/Waystone.Monads/Options/OptionBound.cs` and
`src/Waystone.Monads/Results/ResultBound.cs`.

## A short-circuit branch carries no `async` keyword

Per-case overrides on the monad drop `async` altogether on the trivial branch —
`None<T>.IsSomeAndAsync` is `=> new ValueTask<bool>(false)` — because virtual dispatch
has already chosen the case. The binder has no such dispatch: it tests
`Source is Some<T>` inside one method. Written `async`, that method builds a state
machine even on the branch that never awaits, and DRA-201 measured the cost at
**1.76x a closure** on `IsSomeAndAsync` over a `None`, the cheapest member in the set.

Removing that machine takes no change to the monads. The outer method is not `async`
and returns a completed `ValueTask` on the short-circuit branch; the await sits in a
private `async` helper reached only from the branch that awaits:

```csharp
public ValueTask<bool> IsSomeAndAsync(Func<T, TState, Task<bool>> predicate) =>
    Source is Some<T> some
        ? Awaited(predicate(some.Value, _state))
        : new ValueTask<bool>(false);

private static async ValueTask<bool> Awaited(Task<bool> task) =>
    await task.ConfigureAwait(false);
```

**The rule for which members convert is mechanical: count the awaits.** One await means
a branch returns without awaiting, and it converts. Two awaits means both branches build
a machine anyway, so converting buys nothing and only adds an indirection.
`AndThenAsync` and `OrElseAsync` have the most to gain: their delegates return
`ValueTask`, so an awaiting branch can return it straight through without building a
machine.

**Both give half of that back on purpose, and taking it back is not a fix.** Each wraps
the delegate's task in `NotNullAsync`, which keeps the property for a step whose task has
already completed — the guard unwraps and rewraps it synchronously — and pays one machine
for a step that has not. That is the cost the non-bound `Some.AndThenAsync` and
`Ok.AndThenAsync` pay too, on a branch already awaiting a pending task, and it buys an
`ArgumentNullException` naming the factory in place of a `NullReferenceException` at
whatever read the monad next.

The short-circuit branch is untouched in all four, so the property this rule is about —
no machine on the branch that does not await — still holds everywhere.

**Count per overload, not per member.** `MatchAsync` and `MapOrElseAsync` are where this
bites: their both-asynchronous overloads await twice and keep `async`, while the mixed
ones await once and drop it. So the two forms sit adjacent in the file, spelled
differently, and the difference is the rule applying — not drift. Read the delegate types
before concluding one of them is wrong.

## A one-await branch does not need the helper either — wrap the task

`Awaited` exists to move the `await` off the outer method, but `new ValueTask<T>(task)`
removes it altogether. Measured on an incomplete task, 100k calls:

```
Awaited(task)             136.0 bytes per call
new ValueTask<int>(task)   16.0 bytes per call
```

The 120 bytes are the async state machine box. The two are otherwise indistinguishable —
same `IsCompleted`, same result, and the same exception timing in both of the cases that
get confused for each other:

| What fails | `Awaited` | wrapped |
| --- | --- | --- |
| Delegate throws *before* returning its task | at the call | at the call |
| Delegate returns an already-faulted task | at the await | at the await |

The eagerness the two `…ThrowsFromTheCall` tests pin is the first row, and neither form
changes it — the delegate is invoked before either wrapper sees anything. Do not read
those tests as a claim about the second row; a faulted task surfaces at the await under
both.

`ConfigureAwait(false)` inside `Awaited` is not a reason to keep it either. It governs
that method's own continuation, and the wrap has none; where the *caller* resumes is
decided by the caller's own await either way.

Both forms are in the tree; count them rather than trusting a figure here:

```
grep -c 'Awaited(' src/Waystone.Monads/Options/OptionBound.cs src/Waystone.Monads/Results/ResultBound.cs
```

**Converting the remainder is a uniform change wanting its own benchmark run against
`artifacts/dra-205/`**, not a line in a PR about binder shapes. Do not read the mixture
as a decision that `Awaited` is preferred anywhere; prefer the wrap in anything new.

## What conversion costs

**It costs no public API.** `async` is not part of a signature, so the conversion moves
zero baseline rows. Verify with `git diff --stat -- '*PublicAPI*'` rather than assuming.

**It does change when exceptions surface, which is the part to be careful with.** A
non-`async` method reads `Source` and invokes the delegate eagerly, so a `default` binder
and a delegate that throws before returning its `Task` both throw *at the call* rather
than yielding a faulted task. `ADefaultBoundThrowsFromTheCallRatherThanFromTheAwait` and
`ADelegateThrowingBeforeItsTaskThrowsFromTheCall` pin it in both async test classes,
asserting with the synchronous `Should.Throw` and no `await` — which is what makes them
fail against the `async` form rather than passing either way.

The conversion moved every case it touched, and moved most of them past the closure rather
than merely level with it. `artifacts/dra-205/` against `artifacts/dra-201/`: `MapAsync`
on a `None` went 1.11x to **0.73x**, `MapAsync` on an `Err` 1.08x to **0.56x**,
`AndThenAsync` on a `Some` 0.98x to **0.57x** — that last one being the member that now
builds no machine on either branch. Six of the eight categories are faster than the
closure, at zero allocation where the closure pays 88 bytes.

Two figures are deliberately still above 1.0 and neither is a defect. `IsSomeAndAsync`
over a `None` sits at **1.10x**: the residue of testing the case at all, on the cheapest
member in the set, and the floor for anything short of adding async state overloads to the
monads. `MatchAsync` sits at 1.04x because it awaits on both branches and was never
converted. Read an allocation claim off the predicate categories rather than off
`MapAsync`, whose non-zero figure is the new result instance and not a state machine.

## Match the success case, never the failure case

The async bodies read `Source is Some<T>` and `Source is Ok<TOk, TErr>`, and reach the
other side through `UnwrapErr()`. Matching `Err<TOk, TErr>` directly and reading
`err.Value` looks tidier on a member that needs only the error, and is wrong to reach for:
most need *both* values, so one pattern cannot serve them, and nothing in this library
matches `Err<TOk, TErr>` or `None<T>` anywhere. `UnwrapErr()` also avoids the alternative —
a cast, or a `_ => throw` arm that is a line coverage can never reach.

Read the current split rather than trusting a count here:

```
grep -c 'UnwrapErr()' src/Waystone.Monads/Results/ResultBound.cs
grep -c 'Match(' src/Waystone.Monads/Results/ResultBound.cs
```

**That argument answers consistency and coverage, never safety.** `UnwrapErr()` is a
partial member used where the type test above it has already proved totality, and nothing
reports it if that stops being true; `Match` is total and does not have the problem.
Whichever way a member is written, do not reach for a `switch` expression — the compiler
cannot see the closed hierarchy and `CS8509` demands the uncoverable arm.

## Do not close the gap with async state overloads

Each one costs three declarations — abstract plus two overrides — so the 27 the binder
needs is 81, and the abstract ones land in the baseline where **deprecate; never remove**
locks them until the next major. They would also be 27 more of exactly the surface `With`
exists to replace.
