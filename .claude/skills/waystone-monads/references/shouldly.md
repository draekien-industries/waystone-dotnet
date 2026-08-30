# Asserting on a monad

`Waystone.Monads.Shouldly` supplies the assertions. Reach for them over the
`Unwrap` a test would otherwise be allowed — a failing `Unwrap` throws before the
assertion runs, and a boolean assertion throws the monad away before it can be
reported.

```csharp
// Poor — reports that True was expected, and says nothing about what was found
result.IsOk.ShouldBeTrue();

// Good — reports Err("connection refused")
result.ShouldBeOk();
```

The assertions live in the `Shouldly` namespace, not a `Waystone` one, so a test
file that already has `using Shouldly;` needs no new import.

| Assertion | Asserts |
| --- | --- |
| `ShouldBeSome` / `ShouldBeNone` | The option's case |
| `ShouldBeSomeValue(expected)` | The case, then the contents |
| `ShouldBeOk` / `ShouldBeErr` | The result's case |
| `ShouldBeOkValue(expected)` / `ShouldBeErrValue(expected)` | The case, then the contents |

Every assertion but `ShouldBeNone` returns what it unwrapped, so a check on the
case and a check on the contents are one statement:

```csharp
result.ShouldBeOk().Name.ShouldBe("waystone");
```

Each takes an optional `customMessage`, printed under an `Additional Info`
heading rather than replacing the generated one.

## Assert on the task, not on a parenthesised await

Member access binds tighter than `await`, so asserting on an async chain's result
forces the await into parentheses. Each assertion is declared on the `Task` and
`ValueTask` receivers too, which removes them:

```csharp
// Poor
(await repository.FindAsync(id)).ShouldBeSomeValue(expected);

// Good
await repository.FindAsync(id).ShouldBeSomeValueAsync(expected);
```

**The `Async` suffix is deliberate, and dropping the `await` is the failure it
guards.** An overload sharing the synchronous name would read identically to a
correct synchronous call when the `await` is missing — and a discarded
`ValueTask` means the assertion never runs, so the test passes having checked
nothing.

## The rules that find these

Two diagnostics ship with the package under the `WMS` prefix, both `Info`, both
with a code fix. `WMS2001` reports an assertion made on `IsSome`/`IsOk` or on the
result of `Unwrap`; `WMS2002` reports the parenthesised await. `WMS2001` overlaps
`WM2001` on the `Unwrap` shape on purpose — applying its fix resolves both,
because the rewrite is what removes the `Unwrap`.

`ShouldBeOfType<Some<T>>()` is excluded from `WMS2001` rather than handled, since
such a site is usually testing the closed hierarchy itself.
