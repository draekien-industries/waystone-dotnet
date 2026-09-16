# The Invulnerable Vagrant

Pumat Sol's magic item shop in Zadash, modelled as a domain-driven application so that
`Waystone.Monads` can be seen being consumed rather than demonstrated.

The design is
[docs/plans/2026-09-16-invulnerable-vagrant.md](../../docs/plans/2026-09-16-invulnerable-vagrant.md).
The vocabulary is [ubiquitous-language.yaml](ubiquitous-language.yaml).

## Running it

```
dotnet run --project sample/InvulnerableVagrant/Vagrant.Host
```

Then open `http://localhost:5000/scalar/v1`, which is every route below with its
schemas, tags and refusal codes — read off the running shop rather than written by hand.
The document it renders is at `http://localhost:5000/openapi/v1.json`. Both are mapped in
every environment, because plain `dotnet run` is Production and a reader following this
page would otherwise be answered a 404.

The reference is where `Option<T>` is worth looking at twice. `enchantment`, `holding`
and `agreedPrice` are each documented as their payload or `null` — the shape the
converters actually write — rather than as the monad holding them:

```jsonc
"enchantment": {
  "anyOf": [{ "$ref": "#/components/schemas/Enchantment" }, { "type": "null" }],
  "description": "What it does, once the shop has read it."
}
```

`OptionSchemaTransformer` is what puts it there. Without it the exporter finds no
properties on a converted type and publishes an empty `OptionOfEnchantment` schema, which
describes neither the enchantment nor the null.

```
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
`invulnerable-vagrant-catalog.db`, `invulnerable-vagrant-appraisal.db`,
`invulnerable-vagrant-ordering.db` and `invulnerable-vagrant-staffing.db`. Delete them
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

## Buying something

```
curl -X POST http://localhost:5000/purchases -H 'Content-Type: application/json'   -d '{"patron":"0199aa00-0000-7000-8000-000000000001",
       "items":[{"item":"<potion of healing>","quantity":3},
                {"item":"<driftglobe>","quantity":1}]}'
{"id":"01a0a8bd-e844-...","patron":"0199aa00-...",
 "lines":[{"item":"...","quantity":3,"askingPrice":"5pp","agreedPrice":null,"due":"15pp"},
          {"item":"...","quantity":1,"askingPrice":"75pp","agreedPrice":null,"due":"75pp"}],
 "total":"90pp"}

curl -X POST http://localhost:5000/purchases/01a0a8bd-e844-.../offer   -H 'Content-Type: application/json' -d '{"item":"<potion of healing>","offer":{"gold":100}}'
{"status":409,"detail":"10pp offered for 3, and the shop will not go below 13pp 5gp",
 "code":"vagrant.ordering.offer_below_floor"}

curl -X POST http://localhost:5000/purchases/01a0a8bd-e844-.../offer   -H 'Content-Type: application/json' -d '{"item":"<potion of healing>","offer":{"gold":135}}'
{"item":"...","agreedPrice":"13pp 5gp"}

curl -X POST http://localhost:5000/purchases/01a0a8bd-e844-.../settle   -H 'Content-Type: application/json' -d '{"tendered":{"gold":800}}'
{"status":402,"detail":"80pp tendered against a total of 88pp 5gp",
 "code":"vagrant.ordering.insufficient_coin"}

curl -X POST http://localhost:5000/purchases/01a0a8bd-e844-.../settle   -H 'Content-Type: application/json' -d '{"tendered":{"platinum":88,"gold":5}}'
{"id":"01a0a8bd-eab6-...","patron":"0199aa00-...","moved":"88pp 5gp",
 "at":"2026-09-16T05:43:33.8148923+00:00"}
```

Every route is named for what the patron is doing — `offer`, `settle` — and no route
accepts a field describing the outcome. There is no `PATCH /purchases/{id}` taking
`{"settled":true}` or `{"agreedPrice":400}`, because the shop decides both. The body of
`POST /purchases` names shelf labels and quantities and no prices at all; what each line
costs is read off the Catalog withdrawal.

The two `agreedPrice` fields on the read-back are the strongest case in the sample for
putting an `Option<T>` on the wire. One line was haggled over and one was not, and
`null` against `"13pp 5gp"` is that difference. A nullable decimal would render a line
agreed at its asking price identically to a line nobody touched.

`at` comes from a `TimeProvider`. `Purchase.Settle` takes one rather than reading
`DateTimeOffset.UtcNow`, the host registers `TimeProvider.System`, and the domain tests
pass a `FakeTimeProvider` — so what goes on a receipt is something a test can assert.

## Selling something back

```
curl -X POST http://localhost:5000/buybacks -H 'Content-Type: application/json'   -d '{"patron":"0199aa00-0000-7000-8000-000000000001",
       "description":"a wand nobody can place","offered":{"gold":200}}'
{"id":"01a0a8be-17b2-...","patron":"0199aa00-...",
 "description":"a wand nobody can place","offered":"20pp"}

curl -X POST http://localhost:5000/buybacks/01a0a8be-17b2-.../settle
{"id":"01a0a8be-18a8-...","patron":"0199aa00-...","moved":"20pp",
 "at":"2026-09-16T05:43:45.5765283+00:00"}

curl -X POST http://localhost:5000/buybacks/01a0a8be-17b2-.../settle
{"status":409,"detail":"buyback 01a0a8be-17b2-... has already been settled",
 "code":"vagrant.ordering.buyback_already_settled"}
```

That settle call carries no body, and the absence is the point. The shop already knows
what it offered, and what its till holds is `ShopTill`'s answer rather than the request's
— a patron cannot be asked how much money the shop has. So there is nothing left for the
body to say, and something offered more than the till holds is a refusal with a reason:

```
{"status":402,"detail":"300pp offered against a till holding 200pp",
 "code":"vagrant.ordering.till_cannot_cover"}
```

## Four clerks and no more

```
curl http://localhost:5000/clerks
[{"id":"01a0a8e8-60d6-...","name":"Pumat Prime","holding":null},
 {"id":"01a0a8e8-60f9-...","name":"Pumat Sol","holding":null},
 {"id":"01a0a8e8-60fa-70a1-...","name":"Pumat Sol","holding":null},
 {"id":"01a0a8e8-60fa-7c43-...","name":"Pumat Sol","holding":null}]
```

Three of them answer to the same name. Pumat Sol keeps three simulacra to serve the
counter, and the wiki records no number or nickname telling them apart — so `Clerk.Name`
is not unique and `ClerkId` is what distinguishes them. A model that keyed on the name
would collapse three clerks into one.

Reading an item takes one of them, and they hold it until the patron comes back:

```
curl -X POST http://localhost:5000/specimens/01a0a8e8-901a-.../identify   -H 'Content-Type: application/json' -d '{"check":15}'
{"id":"01a0a8e8-901a-...","description":"a grey cloak, well worn",
 "enchantment":{"name":"Cloak of Elvenkind","effect":"you are harder to see"}}

curl http://localhost:5000/clerks
[{"id":"01a0a8e8-60d6-...","name":"Pumat Prime","holding":"01a0a8e8-901a-..."},
 {"id":"01a0a8e8-60f9-...","name":"Pumat Sol","holding":"01a0a8e8-9089-..."},
 {"id":"01a0a8e8-60fa-70a1-...","name":"Pumat Sol","holding":"01a0a8e8-90f5-..."},
 {"id":"01a0a8e8-60fa-7c43-...","name":"Pumat Sol","holding":"01a0a8e8-9161-..."}]

curl -i -X POST http://localhost:5000/specimens/01a0a8e8-91cf-.../identify   -H 'Content-Type: application/json' -d '{"check":15}'
HTTP/1.1 503 Service Unavailable
{"status":503,"detail":"every clerk is holding something, and 01a0a8e8-91cf-... is waiting",
 "code":"vagrant.staffing.no_clerk_free"}

curl -X POST http://localhost:5000/specimens/01a0a8e8-901a-.../collect
{"id":"01a0a8e8-901a-...","description":"a grey cloak, well worn",
 "enchantment":{"name":"Cloak of Elvenkind","effect":"you are harder to see"}}

curl -o /dev/null -w '%{http_code}\n' -X POST http://localhost:5000/specimens/01a0a8e8-91cf-.../identify   -H 'Content-Type: application/json' -d '{"check":15}'
200
```

`holding` is an `Option<Guid>`, so a free clerk is a `null` rather than a zero GUID — the
same reason `enchantment` is one.

The 503 is what the whole context exists to produce. `Clerk.Engaged` is an EF concurrency
token, so a claim is `UPDATE Clerks SET Engaged = 1 WHERE Id = @id AND Engaged = 0`: two
requests reading the same free clerk both issue it, one matches a row and the other
matches none. `ClerkRoster` catches the `DbUpdateConcurrencyException` that raises,
reloads, and tries the next free clerk. A caller is told that no clerk was free — never
that a row it had never heard of was stale.

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
| Pumat Prime | Critical Role — the original, as against his simulacra |
| Patron, Clerk, Counter, Errand, ErrandSubject, Assignment | ours |
| StockedItem, AskingPrice, FloorPrice, PriceBand, Withdrawal, Wanted | ours |
| Specimen | ours |
| Purchase, LineItem, Offer, AgreedPrice, Buyback, Receipt | ours |

Aggregate identity types — `StockedItemId`, `SpecimenId`, `PurchaseId` and the rest —
are deliberately absent from the dictionary. Identity is a general concept, not a word
anyone in the shop would use.
