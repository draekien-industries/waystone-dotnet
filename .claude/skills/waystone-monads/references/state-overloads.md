# State overloads

There are two ways to hand a value to a delegate rather than let it capture one,
and both remove the closure. Bind the value to the receiver with `With`, or pass
it as the call's first argument. The point is allocation: a lambda that captures
a local or a parameter forces a display class on every call, while a delegate
that closes over nothing is cached by the compiler into a static field.

```csharp
// Poor — captures multiplier, allocating per call
option.Map(value => value * multiplier);

// Good
option.With(multiplier).Map(static (value, m) => value * m);
```

**Reach for `With` first.** It reads in call order, it is the only form that
takes an asynchronous delegate, and it is what `WM2017` names. The older form is
supported, is not going away, and does marginally less work for a single call
because there is no binder to build — prefer it only where that is the point.

## `With` comes from the extensions namespace

`With` is an extension method, so it needs `Waystone.Monads.Options.Extensions`
for an `Option<T>` and `Waystone.Monads.Results.Extensions` for a
`Result<TOk, TErr>` — the same usings most of the surface needs.

Without that `using`, `With` does not appear on the receiver and the API reads as
though the method were never shipped. It is the first thing about this shape that
looks broken.

## Always mark the lambda `static`

A lambda that merely happens not to capture measures the same as a `static` one,
because the compiler caches both. `static` is what stops the next edit from
silently reaching for an outer variable and bringing the allocation back with no
warning. The compiler rejects any capture inside it.

Write `static` in every state-taking lambda, in either form, without exception —
it costs nothing and it is the only part of this that is enforced.

## Where the state argument goes

State is the delegate's extra parameter, and the shape follows the branch:

- Members that hand the delegate a value pass `(value, state)` — `Map`,
  `Filter`, `AndThen`, `IsSomeAnd`, `Inspect`, and their `Result` equivalents,
  where the error-side delegates receive `(error, state)`.
- Branches with no value to give pass state alone. On `Option` that is `Match`'s
  `onNone`, and all of `UnwrapOrElse`, `OrElse` and `OkOrElse`. Every `Result`
  delegate receives a value first, so this case does not arise there.
- `MapOrElse` threads the **same** state through both of its delegates.

The two forms differ in where the state sits at the *call*, not in what the
delegate receives: the binder carries it, while the older form passes it as the
call's first argument.

Pack a tuple when more than one value would be captured. C# names tuple members
after the variables put into them, so the delegate reads `state.partySize`
because the caller wrote `partySize` and no name has to be invented.

## The state is spent, not sticky

Every member on the binder returns a plain `Option<T>` or `Result<TOk, TErr>`,
so the state is gone the moment one call uses it and the rest of the chain is
ordinary. Bind again where it is needed again. There is nothing to call to get
off the binder, because no call leaves you on it.

## The static factories bind too

`Try` and `TryAsync` have no receiver to bind to, so the bind goes on the factory
instead — `Option.With(text).Try(static s => …)`.

## What to rewrite and what to leave

`Match` repays the rewrite most: its two branches share one display class but
need a delegate each, so a capturing `Match` is the most expensive call in the
library.

Leave a lambda that captures only `this`. That allocates a delegate rather than
a display class — a smaller cost, and rewriting every ordinary instance-method
call site would drown the signal. `WM2017` excludes it deliberately.

`ZipWith` and `Reduce` take neither form and never will: their delegates already
receive every operand the call involves, so there is nothing left to capture. The
binder does not carry them either. Do not go hunting for one.

**Never bind an `Option` as state.** `With` constrains its type parameter to
nothing, so an `Option` is as acceptable to it as an `int` and the signature
gives no hint that the pairing is wrong. It is wrong because the binder hands the
state over untouched: the delegate runs whenever the receiver is `Some`, which
leaves the second option's absence for the delegate to unwrap by hand and for its
author to forget. `Zip` pairs the two values and `ZipWith` combines them, and
both answer `None` when either side is absent (`WM2023`). This applies to
`Option` alone — on a `Result`, `With(other).AndThen(…)` is the capture-free
spelling and is correct.

## Let the diagnostic find the call sites

`WM2017` discovers the member set from the binder that `With` returns rather
than from a fixed list, so it stays correct as the library grows and is the
authority on whether a given member can take bound state. Rewrite what it
reports rather than auditing call sites by hand.

Reading the binder is also why it reaches the `*Async` members. It used to ask
whether an overload of the same name took a `TState`, and none of them has one,
so a capturing asynchronous lambda went unreported.

`UseStateBindingCodeFix` does the rewrite, and it names the new delegate
parameter around whatever is already in scope rather than reusing the captured
name, which would shadow the enclosing local.

It declines rather than guesses in three cases, so expect a report with no
lightbulb behind it: a method group cannot grow the parameter the binder's
delegate needs, a capture whose name one of the lambdas already declares as a
parameter or a local would be shadowed by the rewrite, and a capture cannot name
a tuple member if it is called `Rest` or `ItemN` out of position. Rewrite those
by hand.

## The asynchronous surface

The binder carries an `*Async` form of every member it carries, and those forms
take an **asynchronous delegate**. Reach for
`With(state).MapAsync(static async (v, s) => …)` directly rather than awaiting
into a local to get at a synchronous overload.

The older form does not go that far. Its `*Async` members extend a `Task` or
`ValueTask` receiver but still take a synchronous delegate, so an asynchronous
delegate that needs state has only the binder. That gap is the strongest reason
to make `With` the default.
