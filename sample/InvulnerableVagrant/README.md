# The Invulnerable Vagrant

Pumat Sol's magic item shop in Zadash, modelled as a domain-driven application so that
`Waystone.Monads` can be seen being consumed rather than demonstrated.

The design is
[docs/plans/2026-09-16-invulnerable-vagrant.md](../../docs/plans/2026-09-16-invulnerable-vagrant.md).
The vocabulary is [ubiquitous-language.yaml](ubiquitous-language.yaml).

## Running it

```
dotnet run --project sample/InvulnerableVagrant/Vagrant.Host

curl http://localhost:5000/
{"shop":"The Invulnerable Vagrant","city":"Zadash"}

curl http://localhost:5000/items
[{"id":"01a0a847-fc0d-717e-a441-bd5414595330","name":"Bag of Holding","askingPrice":"400pp","onHand":1}, ...]

curl -i http://localhost:5000/items/00000000-0000-0000-0000-000000000000
HTTP/1.1 404 Not Found

curl -X POST http://localhost:5000/specimens -H 'Content-Type: application/json'   -d '{"patron":"0199aa00-0000-7000-8000-000000000001",
       "description":"a grey cloak, well worn","obscurity":15,
       "enchantmentName":"Cloak of Elvenkind","enchantmentEffect":"you are harder to see"}'
{"id":"01a0a884-447b-78fe-aa46-744f66e5bec6","description":"a grey cloak, well worn","enchantment":null}

curl -X POST http://localhost:5000/specimens/01a0a884-447b-78fe-aa46-744f66e5bec6/identify   -H 'Content-Type: application/json' -d '{"check":14}'
{"id":"01a0a884-...","description":"a grey cloak, well worn","enchantment":null}

curl -X POST http://localhost:5000/specimens/01a0a884-447b-78fe-aa46-744f66e5bec6/identify   -H 'Content-Type: application/json' -d '{"check":15}'
{"id":"01a0a884-...","description":"a grey cloak, well worn",
 "enchantment":{"name":"Cloak of Elvenkind","effect":"you are harder to see"}}
```

Each bounded context gets its own SQLite file beside the host, gitignored — currently
`invulnerable-vagrant-catalog.db` and `invulnerable-vagrant-appraisal.db`. Delete them
and run again to start from a freshly stocked shop.

That 404 is the sample's first argument in one line. `IStockLedger.FindAsync` returns
`Option<StockedItem>`, the endpoint calls `Match`, and nothing anywhere throws or
invents an error code — because an identifier the shop has never held needs no
explanation beyond its absence.

The two identify calls are the second argument, and the stronger one. A specimen nobody
has read is a 200 with `"enchantment":null`, and so is one the shop looked at and could
not place. Neither is a 404 and neither carries an error code, because Pumat holds the
item in both cases and has nothing to tell you about it yet. Appraisal declares no error
codes at all for that reason.

A body the shop cannot read as a request is a different answer:

```
curl -X POST http://localhost:5000/specimens -H 'Content-Type: application/json'   -d '{"patron":"0199aa00-0000-7000-8000-000000000001","description":"a grey cloak",
       "obscurity":-1,"enchantmentName":"x","enchantmentEffect":"y"}'
{"status":400,"errors":{"obscurity":["Expected obscurity to be at least 0, but got -1."]}}
```

`Aura.Obscurity` is a `uint` and JSON has one kind of number. `HandInSpecimenSchema` is
where the two meet, so the domain needs no guard against a negative it can no longer be
given.

The bounded contexts arrive one at a time — see the design's Steps section for which
layer brings what.

## How it is put together

`Vagrant.SharedKernel` and each bounded context reference no persistence library at all.
`Vagrant.Host` is the only project that knows EF Core exists: a context declares its
repository interface and the host implements it.

Two things follow. A context's tests need no database, and a context cannot reach for a
`DbContext` by accident — the reference is not there to reach for.

A context also gets its own file rather than a shared one, so the boundary holds in SQL
as well as in the project graph. That began as a constraint rather than a preference:
`EnsureCreated` builds a schema only when the database does not exist, so on one shared
file the first context creates its tables and every later one finds a database already
there and creates nothing.

## Where the names come from

The shop is someone else's invention. This table says which words are theirs and which
are ours, so nobody goes looking for a canonical definition of a term we made up.

| Term | Source |
| --- | --- |
| Pumat Sol, the Invulnerable Vagrant, Zadash | Critical Role |
| Simulacrum | Critical Role, and a D&D spell |
| Identification | D&D — Pumat identifies items for a fee |
| Coin, Platinum, Gold, Silver, Copper, Denomination | D&D currency |
| Aura | D&D — what *detect magic* reveals |
| ArcanaCheck | D&D — the Arcana skill |
| Enchantment | ours. In D&D, Enchantment is a school of magic about charming people, not the property a magic item carries. We use it in the loose sense, and it is the one term here most likely to mislead a reader who knows the game. |
| Patron, Clerk, Counter, Errand, Assignment | ours |
| StockedItem, AskingPrice, FloorPrice, PriceBand, Withdrawal | ours |
| Specimen | ours |
| Purchase, LineItem, Offer, AgreedPrice, Buyback, Receipt | ours |

Aggregate identity types — `StockedItemId`, `SpecimenId`, `PurchaseId` and the rest —
are deliberately absent from the dictionary. Identity is a general concept, not a word
anyone in the shop would use.
