# Reusing a chain

A chain becomes reusable by being a step: one parameter in, one monad out, so
`AndThen` takes it as a method group. The first section below is what that takes
for an async chain, and each one after it is a shape that breaks reuse, with what
to reach for instead.

## An async chain is a step

An async step is `T → ValueTask<Result<U, Error>>`, and so is an async chain, so
a chain composes exactly as a single step does:

```csharp
// Charge is a chain of two steps and a step itself, both at once.
private static ValueTask<Result<Quote, Error>> Charge(Order order) =>
    FetchAsync(order).AndThenAsync(Price);

internal ValueTask<Result<Invoice, Error>> Bill(int id) =>
    FindAsync(id)
       .AndThenAsync(Charge)
       .AndThenAsync(Validated)
       .MapAsync(Render);
```

**Declare an async step `ValueTask`, not `Task`.** This is the one thing to get
right. `AndThenAsync` and `OrElseAsync` take `ValueTask`-returning delegates, so
a chain — which returns `ValueTask` — is a step, and a `Task`-returning method
group handed to either is not.

The compiler reports that as `CS0411`, "the type arguments cannot be inferred
from the usage", against the call site and naming neither `ValueTask` nor the
parameter — so it reads like a generics problem rather than the one-word fix it
is. `WM2022` is the rule that says what `CS0411` does not, naming both the step
and the type it must return. Change the method's own return type to `ValueTask`,
which is the better declaration for a step regardless. Where the method is
genuinely someone else's, `AndThenAsync(async o => await Foreign(o))` binds, and
that wrap is what `WM2022`'s code fix applies — it cannot edit a signature it
does not own.

**Only the step-shaped parameters take `ValueTask`.** `AndThenAsync` and
`OrElseAsync` are the chain operators, and they are the whole list. A delegate
returning an arbitrary type takes `Task`, because foreign code produces that
shape — so `MapAsync(client.GetStringAsync)` binds, and `MapAsync`, `FilterAsync`
and the rest all want `Task`.

The gap that leaves, stated plainly: a `ValueTask`-returning expression cannot
feed one of those boundary parameters.
`FilterAsync(o => o.IsSomeAndAsync(p))` fails, and the workaround is
`async o => await o.IsSomeAndAsync(p)`. Feeding a predicate from an exit member
is a much rarer shape than chaining.

`.AsTask()` does not make a chain composable and is not the fix for `CS0411`.
It is the conversion for a foreign API that demands a `Task`, and nothing else.

A **synchronous** chain stays reusable inside an async one, because the `*Async`
members take a synchronous delegate too: `Validated` above — a plain
`Order → Result<Order, Error>` chain — drops into the async chain unchanged.
Where a run of steps genuinely repeats and none of them awaits, extracting it as
a sync chain is the one extraction that survives.

## A variation point belongs in a field, not the signature

A chain that differs by caller in a single step invites passing that step in.
Doing so adds a second parameter, and the chain stops being a step:

```csharp
// Poor — Bill is no longer a method group, so nothing can chain onto it
Result<Invoice, Error> Bill(Order order, Func<Order, Result<Quote, Error>> price)

// Good — the variation is held, and the chain keeps its shape
internal Billing(Func<Order, Result<Quote, Error>> price) => _price = price;
internal Result<Invoice, Error> Bill(Order order) =>
    Validated(order).AndThen(Price).Map(Render);
```

The variation is then chosen once where the type is constructed rather than
restated at every call, and the chain composes exactly as a fixed one does.

Resist the further step of holding a collection of `Func<T, Result<U, E>>`
values and composing them into a chain before applying it. C# has no
composition operator, so what comes out is a nested `Func` expression that reads
worse than the `AndThen` chain it replaced, loses the method names that made the
chain legible, and allocates a delegate per step. The chain is already the
composition, and a named method is already the reusable unit — there is nothing
for a combinator library to add here.

## A chain applies over a collection unchanged

```csharp
Result<IReadOnlyList<Invoice>, Error> billed = orders.Select(Bill).Collect();
```

Gather with `Collect` or `Partition` at the **call site**, not inside the chain.
The two differ in what they do with failures after the first, and which one is
right is the caller's question — a chain that gathers its own results has
answered it for every caller and stopped being a step besides.

## Test the steps, then the seams

Each step is a small function returning a monad, so test it directly: one case
per failure it can produce, one for success. The chain then needs far fewer
tests than it has steps — one per short-circuit point, asserting that the error
arriving at the caller is the one the failing step produced, **unchanged**.

That last assertion is the one worth writing. It is what catches a `MapErr` that
collapsed two distinct failures onto one code, or a step reordered so a cheaper
check no longer runs first, and no step-level test can see either.
