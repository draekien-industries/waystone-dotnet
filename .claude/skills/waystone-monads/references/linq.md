# Query syntax

`Waystone.Monads.Linq` adds `Select`, `SelectMany` and `Where`, so C# query
syntax works over `Option` and `Result`. The names are one-line forwards and add
no behaviour:

| Query clause | Member | Core member |
| --- | --- | --- |
| `select` | `Select` | `Map` |
| second and later `from` | `SelectMany` | `AndThen` |
| `where` | `Where` | `Filter` |

The core package deliberately ships none of these names, so a chain written
without this package has one spelling per operation and a reader learns one
vocabulary. Adding the package is an opt-in, and so is the `using` — import
`Waystone.Monads.Linq` or the names do not appear.

## Reach for it where the steps need each other

A chain of `AndThen` over method groups is the default and stays the default.
Query syntax earns its place on the one shape that chain handles badly: a later
step needing a value an earlier step produced. Method syntax carries that in a
tuple or a state argument; a query expression keeps every earlier name in scope.

```csharp
Option<Quote> quote =
    from customer in FindCustomer(id)
    from address in customer.PostalAddress
    from rate in RateFor(address)
    select Price(customer, rate);
```

Every clause after the first is a `SelectMany`, so the first `None` or `Err` ends
the query and no later delegate runs — the same short-circuit `AndThen` gives.
Over a `Result`, every clause must share one error type; convert a step from
another taxonomy with `MapErr` before it enters the query.

## Two shapes it does not have

**No `where` over a `Result`.** Discarding an ok value would have to invent the
error that replaces it, and a clause taking an error factory is not what query
syntax looks for. Filter before entering the query.

**No `*Async` names.** A query expression cannot `await`, so there is nothing to
add over `MapAsync` and `AndThenAsync`. An async chain stays method syntax.
