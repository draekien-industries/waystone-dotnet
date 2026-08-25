# Reusing a chain

A chain becomes reusable by being a step: one parameter in, one monad out, so
`AndThen` takes it as a method group. Each section below is a shape that breaks
that, and what to reach for instead.

## An async chain cannot be a step

The async surface is deliberately asymmetric: an `*Async` member **returns**
`ValueTask`, and the delegate it **accepts** returns `Task`. An async chain
therefore ends in the one shape the next `*Async` member has no overload for,
which makes it **terminal** — correct and complete as a chain, and unusable as a
step.

```csharp
// This chain is complete and correct, and Charge can never be a step.
private static ValueTask<Result<Quote, Error>> Charge(Order order) =>
    FetchAsync(order).AndThenAsync(Price);
```

Handing `Charge` to `AndThenAsync` fails as `CS0411` — "the type arguments
cannot be inferred from the usage" — reported against the call site and naming
neither `ValueTask` nor the step, so it reads like a generics problem in the
chain rather than the design constraint it is.

Two conversions can force it into a step's shape, and it is worth knowing what
each really costs, because the intuition is wrong in both directions.
`.AsTask()` is free when the chain suspends — a suspended chain is already
`Task`-backed, so `AsTask` hands that same instance back — and costs one small
allocation when the chain completes synchronously. Declaring the chain
`async Task<…>` and awaiting it is **never cheaper and is worse when it
suspends**, because the extra state machine sits on top of the one the chain
already built.

So converting is a tax on exactly the synchronous-completion path `ValueTask` was
chosen to keep free, and the `async` wrapper is the more expensive way to pay it.
Neither is a catastrophe; the reason to avoid both is that they bury the real
signal — the unit of reuse was picked wrong. Reuse **async steps**, not async
chains. An async step is `T → Task<Result<U, Error>>`, which is what an I/O
method returns anyway, and `AndThenAsync` takes it directly:

```csharp
private static Task<Result<Reservation, Error>> ReserveAsync(Order order) => …

internal ValueTask<Result<Invoice, Error>> Bill(int id) =>
    FetchAsync(id)
       .AndThenAsync(ReserveAsync)
       .AndThenAsync(Validated)
       .MapAsync(Render);
```

Extract the steps, let each call site build its own flat chain, and the
duplication that remains is one `AndThenAsync` line per site — cheaper than the
allocation and clearer than the indirection.

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
