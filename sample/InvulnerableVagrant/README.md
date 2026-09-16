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
```

The shop's SQLite file is created beside the host on startup and is gitignored. Delete
it and run again to start from a freshly stocked shop.

The bounded contexts arrive one at a time — see the design's Steps section for which
layer brings what. Until Catalog lands there is nothing to buy.

## How it is put together

`Vagrant.SharedKernel` and each bounded context reference no persistence library at all.
`Vagrant.Host` is the only project that knows EF Core exists: a context declares its
repository interface and the host implements it.

Two things follow. A context's tests need no database, and a context cannot reach for a
`DbContext` by accident — the reference is not there to reach for.

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
