# Waystone.Monads.Linq

Adds `Select`, `SelectMany` and `Where` to `Waystone.Monads`, so C# query
syntax works over `Option` and `Result`.

The core package deliberately does not ship these names. `Map`, `AndThen` and
`Filter` are the vocabulary it teaches, and a second spelling of each on the
core type would mean every reader learning both. Install this package if you
want query syntax; ignore it and nothing changes.

## Install

```
dotnet add package Waystone.Monads.Linq
```

Then import the namespace wherever you want the names in scope. The `using` is
the opt-in — without it, the operations are still only `Map`, `AndThen` and
`Filter`:

```csharp
using Waystone.Monads.Linq;
```

## Why query syntax

Rust's `?` operator early-returns on `None` or `Err`, so a multi-step chain
reads as straight-line code. C# has no `?`, and the method-syntax translation
nests one lambda per step. A query expression does not:

```csharp
Option<Quote> quote =
    from customer in FindCustomer(id)
    from address in customer.PostalAddress
    from rate in RateFor(address)
    select Price(customer, rate);
```

Every clause after the first is a `SelectMany`, and each short-circuits: the
first `None` ends the query and no later delegate runs.

The same works over a `Result`, where the first `Err` ends the query and is the
error that surfaces. Every clause must share one error type — a step that fails
differently has to be mapped onto it first.

## What maps to what

| Query clause | This package | Core member |
| --- | --- | --- |
| `select` | `Select` | `Map` |
| second and later `from` | `SelectMany` | `AndThen` |
| `where` | `Where` | `Filter` |

Each member is a one-line forward and adds no behaviour, so there is never a
reason to prefer one spelling for correctness — only for how the call site
reads.

## Two things this package does not have

**No `where` clause over a `Result`.** Discarding an ok value would have to
invent the error that replaces it, and a signature taking an error factory is
not the one query syntax looks for. Filter before entering the query.

**No `…Async` shapes.** A query expression cannot `await`, so an async LINQ
name would buy only method-syntax parity with `MapAsync`, which the core
package already provides under the name it actually teaches.
