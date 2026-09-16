# The Invulnerable Vagrant

Pumat Sol's magic item shop in Zadash, modelled as a domain-driven application so that
`Waystone.Monads` can be seen being consumed rather than demonstrated.

The design is
[docs/plans/2026-09-16-invulnerable-vagrant.md](../../docs/plans/2026-09-16-invulnerable-vagrant.md).
The vocabulary is [ubiquitous-language.yaml](ubiquitous-language.yaml).

## Running it

The projects arrive one bounded context at a time — see the design's Steps section for
which layer brings what. There is nothing to run until `Vagrant.Host` lands.

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
