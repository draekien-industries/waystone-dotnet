# Waystone.Monads.Shouldly

Shouldly assertions for `Option<T>` and `Result<TOk, TErr>`.

## Why

Checking a monad through its booleans throws the monad away before the assertion
runs, so a failure can only report the boolean:

```
option.IsSome.ShouldBeTrue();

option.IsSome
    should be
True
    but was
False
```

The same assertion here reports what the option was:

```
option.ShouldBeSome();

option
    should be Some
    but was
None
```

On a `Result` the difference is larger, because the error a failing test did not
expect is usually the whole explanation:

```
result.ShouldBeOk();

result
    should be Ok
    but was
Err("connection refused")
```

## Usage

The extensions live in the `Shouldly` namespace, so a test file that already has
`using Shouldly;` needs no new import.

The namespace is `Shouldly` rather than `Waystone.Monads.Shouldly` for a second
reason, and it is not a matter of taste: a nested `Waystone.Monads.Shouldly`
namespace shadows the global `Shouldly` for every file declared in
`namespace Waystone.Monads`, because C# resolves a using outward from the enclosing
namespace and stops at the first match. Under that layout `using Shouldly;` in this
repository's own tests would bind to the nested namespace and every plain Shouldly
assertion would stop compiling. Renaming the namespace to match the package id looks
like a tidy-up and silently breaks the callers most likely to use it.

```csharp
option.ShouldBeSome();
option.ShouldBeNone();
option.ShouldBeSomeValue(3);

result.ShouldBeOk();
result.ShouldBeErr();
result.ShouldBeOkValue(3);
result.ShouldBeErrValue("failed");
```

Every assertion takes an optional `customMessage`, printed under an
`Additional Info` heading rather than replacing the generated message.

### Returning the value

Every assertion but `ShouldBeNone` hands back what it unwrapped, so a check on the
state and a check on the contents are one statement instead of two:

```csharp
result.ShouldBeOk().Name.ShouldBe("waystone");
```

### Awaited receivers

Each assertion has an `*Async` form on `Task<Option<T>>`, `ValueTask<Option<T>>`
and the two `Result` equivalents, so an assertion on an async chain does not need
to be wrapped in parentheses to await it:

```csharp
// before
(await repository.FindAsync(id)).ShouldBeSomeValue(expected);

// after
await repository.FindAsync(id).ShouldBeSomeValueAsync(expected);
```

They return `ValueTask`, matching the rest of Waystone.Monads, and they forward the
caller's expression to the synchronous assertion — so the message is identical
apart from naming your receiver.

**The `Async` suffix is deliberate.** An overload sharing the synchronous name
would read identically to the correct synchronous call when the `await` is
missing — and a discarded `ValueTask` means the assertion never runs, so the test
passes without checking anything. The suffix makes that mistake visible.

## How the message is built

Following [Shouldly's extension guide](https://docs.shouldly.org/documentation/extending):
the classes carry `[ShouldlyMethods]`, `[DebuggerStepThrough]` and
`[EditorBrowsable(EditorBrowsableState.Never)]`, every assertion takes an optional
`customMessage`, and the receiver's source text arrives through
`[CallerArgumentExpression]` so the failure names your expression rather than a
parameter.

Two deliberate departures from that page, both forced by the Shouldly 4.3.0 this
package builds against:

- **The message text is built here rather than by `ActualShouldlyMessage` or
  `ExpectedActualShouldlyMessage`.** In 4.3.0 the first prints a literal `null`
  where a state assertion has no expected value, and the second ends `but was not`
  without printing the actual. Neither can render `but was Err("failed")`, which is
  the entire point of the package. The layout emitted here matches theirs.
- **`[ShouldlyMethods]` is kept.** The guide calls it unnecessary for targets with
  `[CallerArgumentExpression]`, but retained for `netstandard2.0` — which is this
  package's target framework.

`Option<T>` and `Result<TOk, TErr>` are records, so Shouldly's own object formatter
renders a `Some` as `Some { IsSome = True, IsNone = False }` — without its value.
That is why the description is formatted here as `Some(3)` and `Err("failed")`.

## Comparison of values

`ShouldBeSomeValue`, `ShouldBeOkValue` and `ShouldBeErrValue` report the wrong
*state* themselves, then hand a wrong *value* to Shouldly's own comparison. That
keeps its diff on strings and collections, which is better than anything this
package could restate.
