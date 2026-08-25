# State overloads

Nearly every delegate-taking member of `Option` and `Result` has a sibling that
takes a **state** argument and hands it to the delegate. The point is
allocation: a lambda that captures a local or a parameter forces a display class
on every call, while a delegate that closes over nothing is cached by the
compiler into a static field.

```csharp
// Poor — captures multiplier, allocating per call
option.Map(value => value * multiplier);

// Good
option.Map(multiplier, static (value, m) => value * m);
```

## Always mark the lambda `static`

A lambda that merely happens not to capture measures the same as a `static` one,
because the compiler caches both. `static` is what stops the next edit from
silently reaching for an outer variable and bringing the allocation back with no
warning. The compiler rejects any capture inside it.

Write `static` in every state overload, without exception — it costs nothing and
it is the only part of this that is enforced.

## Where the state argument goes

State is the delegate's extra parameter, and the shape follows the branch:

- Members that hand the delegate a value pass `(value, state)` — `Map`,
  `Filter`, `AndThen`, `IsSomeAnd`, `Inspect`, and their `Result` equivalents,
  where the error-side delegates receive `(error, state)`.
- Branches with no value to give pass state alone. On `Option` that is `Match`'s
  `onNone`, and all of `UnwrapOrElse`, `OrElse` and `OkOrElse`.
- `MapOrElse` threads the **same** state through both of its delegates.

Pack a tuple when more than one value would be captured.

## What to rewrite and what to leave

`Match` repays the rewrite most: its two branches share one display class but
need a delegate each, so a capturing `Match` is the most expensive call in the
library.

Leave a lambda that captures only `this`. That allocates a delegate rather than
a display class — a smaller cost, and rewriting every ordinary instance-method
call site would drown the signal. `WM2017` excludes it deliberately.

`ZipWith` and `Reduce` have no state overload and never will: their delegates
already receive every operand the call involves, so there is nothing left to
capture. Do not go hunting for one.

## Let the diagnostic find the call sites

`WM2017` discovers the overload set from the type rather than from a fixed list,
so it stays correct as the library grows and is the authority on whether a given
member has a state sibling. Rewrite what it reports rather than auditing call
sites by hand.

It ships no code fix, because the natural rewrite reuses the captured name as
the new delegate parameter, which shadows the enclosing local — so the rewrite
is manual, and renaming the parameter is part of it.

The async surface is converted only in part, so where an `*Async` state overload
does not exist, await first and call the synchronous state overload rather than
reintroducing a closure.
