---
title: The Invulnerable Vagrant DDD sample
date: 2026-09-16
status: active
---

# The Invulnerable Vagrant

A domain-driven sample shop — Pumat Sol's magic item shop in Zadash — that exercises
the Waystone packages against the working tree and shows how they are consumed in an
application with real bounded contexts. Tracked as
[DRA-232](https://linear.app/draekien-industries/issue/DRA-232).

Vocabulary is fixed in
[sample/InvulnerableVagrant/ubiquitous-language.yaml](../../sample/InvulnerableVagrant/ubiquitous-language.yaml).
A term used here without a definition there is a mistake in one of the two.

## Goal

A reader clones the repository, runs one command, and sees `Option<T>` and
`Result<T, Error>` used the way we intend them to be used, in a domain substantial
enough that the usage is not obviously contrived.

**No package appears to demonstrate itself.** Each one is present because the shop
needs it.

## Scope

Five projects carrying domain code, one host, one test project per context.

| Project | Owns |
| --- | --- |
| `Vagrant.SharedKernel` | `Coin`, `PatronId` |
| `Vagrant.Catalog` | what the shop holds and what it asks for it |
| `Vagrant.Appraisal` | what an unidentified item turns out to be |
| `Vagrant.Ordering` | coin moving in either direction |
| `Vagrant.Staffing` | which clerk is free |
| `Vagrant.Host` | HTTP, persistence wiring, translation between contexts |

Each context project references `Vagrant.SharedKernel` and nothing else in the sample.
Translation between two contexts happens in `Vagrant.Host`.

**No context project references EF Core.** A context declares its repository interface;
`Vagrant.Host` implements it. The contexts are persistence-ignorant, so their tests need
no database, run in milliseconds, and cannot reach for a `DbContext` by accident.

Every project here targets `net10.0`, which is what ASP.NET Core 10 and EF Core 10
support. CI's test step was moved to `net10.0` to match; the framework matrix still runs
in `pre-push`.

## Key data structures

### Vagrant.SharedKernel

```csharp
readonly record struct Coin
{
    static Coin FromGold(uint gold);
    static Coin From(uint platinum, uint gold, uint silver, uint copper);
    static Coin operator +(Coin left, Coin right);
    static Coin operator *(Coin coin, uint quantity);
    Option<Coin> Less(Coin other);
    bool IsAtLeast(Coin other);
    string ToString();                      // "45gp 3sp"
}

readonly record struct PatronId(Guid Value);
```

**`Coin` holds a count of copper in a `ulong`, and every component is unsigned.** A
negative price is a compile error rather than an `ArgumentOutOfRangeException`, so the
type needs no guards and no caller has a rejection to handle. `+` and `*` are `checked`,
so the one remaining way to reach a wrong amount — a sum wrapping past `ulong.MaxValue` —
throws instead of producing a small number.

There is no subtraction operator. `Less` returns `Option<Coin>` — `None` when the
subtrahend exceeds the balance — so no caller can build a debt by accident.

This is the rule the rest of the sample follows. A quantity, a count on hand or an
amount of coin is unsigned wherever it appears.

`Patron` is vocabulary, not a type. Each context that needs a patron holds a
`PatronId` and its own view of what it needs to know about them.

### Vagrant.Catalog

```csharp
sealed class StockedItem                                    // aggregate root
{
    static StockedItem Stock(
        StockedItemId id, string name, PriceBand band, uint onHand);

    StockedItemId Id { get; }
    string Name { get; }
    PriceBand Band { get; }
    uint OnHand { get; }

    Result<Withdrawal, Error> Withdraw(uint quantity);
}

readonly record struct PriceBand           // invariant: FloorPrice <= AskingPrice
{
    static Result<PriceBand, Error> Between(Coin askingPrice, Coin floorPrice);
    Coin AskingPrice { get; }
    Coin FloorPrice { get; }
    bool Admits(Coin offer);
}

readonly record struct Withdrawal(StockedItemId Item, uint Quantity, PriceBand Band);

interface IStockLedger
{
    Task<Option<StockedItem>> FindAsync(StockedItemId id, CancellationToken ct);
    Task<IReadOnlyList<StockedItem>> OnDisplayAsync(CancellationToken ct);
    Task<Result<Withdrawal, Error>> WithdrawAsync(
        StockedItemId id, uint quantity, CancellationToken ct);
}
```

`PriceBand` carries both prices together because neither is meaningful alone and the
pair has an invariant. A `StockedItem` whose floor exceeds its asking price cannot be
constructed.

`WithdrawAsync` is one call. It finds the item, takes the quantity, persists, and
returns what was taken — the caller never sees a reservation it has to commit.

### Vagrant.Appraisal

```csharp
sealed class Specimen                                       // aggregate root
{
    static Specimen HandedIn(SpecimenId id, PatronId patron, string description, Aura aura);

    SpecimenId Id { get; }
    PatronId HandedInBy { get; }
    Aura Aura { get; }
    Option<Enchantment> Enchantment { get; }                // None until identified

    void Identify(ArcanaCheck check);
}

readonly record struct Aura(uint Obscurity);
readonly record struct ArcanaCheck(int Total);
readonly record struct Enchantment(string Name, string Effect);

interface ISpecimenShelf
{
    Task<Option<Specimen>> FindAsync(SpecimenId id, CancellationToken ct);
    Task AddAsync(Specimen specimen, CancellationToken ct);
    Task<Option<Specimen>> IdentifyAsync(
        SpecimenId id, ArcanaCheck check, CancellationToken ct);
}
```

`Identify` changes the specimen and returns nothing; `Enchantment` reads it and changes
nothing. A check that falls short of the aura's obscurity leaves `Enchantment` as
`None`, which is an outcome rather than a failure — Pumat looked and could not tell.

Identifying an already-identified specimen does nothing. Once the enchantment is known
it does not become unknown, and a second look cannot contradict the first, so there is
no "already identified" failure to report. Appraisal is the one context with no error
family at all, which is the point of it: absence answers every question it is asked.

### Vagrant.Ordering

```csharp
sealed class Purchase                                       // aggregate root
{
    static Purchase Open(PurchaseId id, PatronId patron, LineItems items);

    PurchaseId Id { get; }
    Coin Total { get; }
    Result<AgreedPrice, Error> Agree(StockedItemId item, Offer offer);
    Result<Receipt, Error> Settle(Coin tendered);
}

sealed class Buyback                                        // aggregate root
{
    static Buyback Open(BuybackId id, PatronId patron, string description, Coin offered);
    Result<Receipt, Error> Settle(Coin fromTill);
}

readonly record struct LineItems                            // invariant: at least one
{
    static Result<LineItems, Error> Of(IEnumerable<LineItem> items);
    IReadOnlyList<LineItem> All { get; }
}

readonly record struct LineItem(StockedItemId Item, uint Quantity, PriceBand Band);
readonly record struct Offer(Coin Named);
readonly record struct AgreedPrice(Coin Settled);
readonly record struct Receipt(ReceiptId Id, PatronId Patron, Coin Moved, DateTimeOffset At);

interface IPurchaseBook
{
    Task AddAsync(Purchase purchase, CancellationToken ct);
    Task<Result<AgreedPrice, Error>> AgreeAsync(
        PurchaseId id, StockedItemId item, Offer offer, CancellationToken ct);
    Task<Result<Receipt, Error>> SettleAsync(
        PurchaseId id, Coin tendered, CancellationToken ct);
}

interface IBuybackBook
{
    Task AddAsync(Buyback buyback, CancellationToken ct);
    Task<Result<Receipt, Error>> SettleAsync(
        BuybackId id, Coin fromTill, CancellationToken ct);
}
```

`LineItems` cannot be empty, so `Open` has no empty-purchase failure to report and no
caller has to remember to check. `LineItems.Of` reports `NoLineItems` at the one place an
empty set can be presented.

`Total` is the sum, over `LineItems.All`, of each line's `AgreedPrice` where `Agree` has
been called for it and `AskingPrice * Quantity` where it has not. A purchase that nobody
haggled over totals what the shop asked.

`Purchase` and `Buyback` both produce a `Receipt` and are not unified. They share a
shape, not a concept: one takes coin from the till and one puts coin in it, and the
invariants differ accordingly.

### Vagrant.Staffing

```csharp
sealed class Clerk                                          // aggregate root
{
    ClerkId Id { get; }
    string Name { get; }
    Option<Assignment> Engagement { get; }

    Result<Assignment, Error> Take(Errand errand);
    void Release();
}

readonly record struct ErrandSubject(Guid Value);
readonly record struct Errand(ErrandSubject Subject);
readonly record struct Assignment(ClerkId Clerk, Errand Errand, DateTimeOffset Since);

interface IClerkRoster
{
    Task<Result<Assignment, Error>> ClaimAsync(Errand errand, CancellationToken ct);
    Task ReleaseAsync(Assignment assignment, CancellationToken ct);
}
```

`ClaimAsync` is one call: it finds a free clerk, gives them the errand, saves, and on a
lost race tries the next free clerk. The caller learns that no clerk was free, never
that a particular row version was stale.

`Clerk` is the aggregate root rather than a roster holding all clerks, so two patrons
claiming different clerks do not contend. A roster aggregate would serialise every
claim on one row and the concurrency would be an artefact of the model.

`ErrandSubject` wraps a `Guid` that Staffing does not interpret. Staffing cannot name a
`SpecimenId` without referencing Appraisal, and a bare `Guid` would let any identifier
in the sample be handed to a clerk as any other. The host translates, and Staffing knows
only that an errand is about something.

## Interface at the boundary

```
GET  /items                     -> 200 [StockedItemResponse]
GET  /items/{id}                -> 200 StockedItemResponse | 404
POST /specimens                 -> 201 SpecimenResponse
POST /specimens/{id}/identify   -> 200 SpecimenResponse | 404 | 503
POST /orders                    -> 201 PurchaseResponse | 400 | 404 | 409
POST /orders/{id}/offers        -> 200 PurchaseResponse | 400 | 404 | 409
POST /orders/{id}/settlement    -> 200 ReceiptResponse | 402 | 404 | 409
POST /buybacks                  -> 201 BuybackResponse | 400
POST /buybacks/{id}/settlement  -> 200 ReceiptResponse | 402 | 404 | 409
```

`POST /orders` withdraws from Catalog before opening the Purchase, so it fails the way
Catalog fails: 404 for an item that is not stocked, 409 for `NotEnoughOnHand` when too few
are on hand.

```csharp
internal sealed record SpecimenResponse(
    Guid Id, string Description, Option<Enchantment> Enchantment);

internal sealed record PurchaseResponse(
    Guid Id, IReadOnlyList<LineItemResponse> Lines, string Total);

internal sealed record LineItemResponse(
    Guid Item, uint Quantity, string AskingPrice, Option<string> AgreedPrice);
```

`Option<T>` is serialized into response bodies by `AddMonadConverters()`. `Result<T, E>`
is unwrapped at the endpoint into a status code and a ProblemDetails carrying the
generated error code, and never appears on the wire.

`LineItemResponse.AgreedPrice` is the second `Option<T>` on the wire and the one that
makes the case: a line nobody haggled over has no agreed price, and that is different
from an agreed price equal to the asking price.

Request bodies are parsed by `Waystone.Monads.Schemas` into the domain type. There is no
second validation step and no domain method that accepts an unparsed body.

## Option or Result

| Return | When | Example |
| --- | --- | --- |
| `Option<T>` | absence is an answer the caller can act on without being told why | `IStockLedger.FindAsync`, `Specimen.Enchantment`, `Coin.Less` |
| `Result<T, Error>` | the caller owes a patron a reason | `Purchase.Settle`, `IClerkRoster.ClaimAsync`, `PriceBand.Between` |

The test is whether a reason exists that someone outside the system needs to hear. "No
item with that id" needs none; "you are eighty gold short" does.

## Error taxonomy

Codes are generated by `Waystone.Monads.SourceGenerators` from one `[ErrorCodeCatalog]`
enum per context, each setting its own `Format`.

```csharp
[ErrorCodeCatalog(Format = "vagrant.ordering.{member:kebab}")]
internal enum OrderingError
{
    NoLineItems,
    OfferBelowFloor,
    InsufficientCoin,
    PurchaseAlreadySettled,
    BuybackAlreadySettled,
    TillCannotCover,
}
```

The format is literal text with `{enum}` and `{member}` placeholders, so a code is
derived from the member's own name. A sequence number is not something the generator can
produce, and inventing one by hand would mean maintaining a lookup table between a
number and a meaning that the member name already carries.

| Code | Meaning |
| --- | --- |
| `vagrant.catalog.floor-above-asking` | asking price is below the floor price |
| `vagrant.catalog.not-enough-on-hand` | not enough on hand to withdraw |
| `vagrant.ordering.no-line-items` | a purchase must carry at least one line item |
| `vagrant.ordering.offer-below-floor` | offer falls below what the shop will take |
| `vagrant.ordering.insufficient-coin` | tendered coin falls short of the total |
| `vagrant.ordering.purchase-already-settled` | purchase has already settled |
| `vagrant.ordering.buyback-already-settled` | buyback has already settled |
| `vagrant.ordering.till-cannot-cover` | the till cannot cover the buyback |
| `vagrant.staffing.no-clerk-free` | no clerk is free |
| `vagrant.staffing.clerk-already-engaged` | clerk is already engaged |

The enums are named `CatalogError`, `OrderingError` and `StaffingError` rather than
`Catalog`, `Ordering` and `Staffing`. An enum sharing its name with its namespace loses
lookup to the namespace, which is the same collision `Waystone.Monads.Schemas` is plural
to avoid.

Every code above is produced by a signature in this document, and every `Result`-returning
signature can produce at least one of them. A code with no signature behind it is a
promise the domain cannot keep.

| Status | Codes |
| --- | --- |
| 400 | a `SchemaViolation`, or `NoLineItems` |
| 402 | `InsufficientCoin`, `TillCannotCover` |
| 404 | a `None` from a repository lookup — no code, because no reason is owed |
| 409 | `NotEnoughOnHand`, `OfferBelowFloor`, `PurchaseAlreadySettled`, `BuybackAlreadySettled`, `ClerkAlreadyEngaged` |
| 503 | `NoClerkFree` |

`FloorAboveAsking` reaches no status code. `PriceBand.Between` is called when the shop
stocks an item, which happens in seeding, so the failure is a startup failure rather than
a response.

Appraisal has no codes. Every question it answers is answered by `Option<Enchantment>`,
and nothing a patron does to a `Specimen` can fail.

## Internal decomposition

- **`Vagrant.Host/Endpoints`** — one file per resource, mapping `Result` to a status code.
  `POST /specimens/{id}/identify` is the one endpoint that spans two contexts: it claims a
  clerk through `IClerkRoster.ClaimAsync`, calls `ISpecimenShelf.IdentifyAsync`, and
  releases the clerk through `ReleaseAsync`. `NoClerkFree` becomes a 503 and the
  identification never starts; a `None` from `IdentifyAsync` becomes a 404 and the clerk
  is released either way.
- **`Vagrant.Host/Contracts`** — request schemas and response records
- **`Vagrant.Host/Translation`** — turns a Catalog `Withdrawal` into an Ordering
  `LineItem` and an Appraisal `SpecimenId` into a Staffing `ErrandSubject`, the one
  place two contexts' vocabularies meet
- **`Vagrant.Host/Infrastructure`** — four `DbContext`s over one SQLite file, the `Coin`
  converter, the seeding path, and every repository implementation. This is where
  `DbUpdateConcurrencyException` is converted and stopped; no context project sees one.

## Rejected alternatives

**A `Transaction` base type over `Purchase` and `Buyback`** — rejected: they share a
receipt and nothing else. Unifying them now would fix the wrong axis before either
aggregate's invariants are known.

**`IClerkRoster.NextFreeAsync` followed by `CommitAsync`** — rejected: splits one
question into two calls in execution order, leaves the caller holding a stale clerk, and
makes the retry-on-conflict the caller's problem.

**`Waystone.Monads.Linq` query syntax over the endpoint chains** — rejected by decision.
The binder chain is what most consumers will write, and keeping it flat also keeps the
sample under the nesting-depth rule.

**A shared `Item` type across contexts** — rejected: the physical object is a
`StockedItem`, a `Specimen` and a `LineItem` depending on who is holding it, and each
context needs different facts about it.

## Steps

1. This document, and the vocabulary dictionary — DRA-233
2. `Vagrant.SharedKernel`, `Vagrant.Host` skeleton, SQLite wiring, `codecov.yml` — DRA-234
3. `Vagrant.Catalog` — DRA-235
4. `Vagrant.Appraisal` — DRA-236
5. `Vagrant.Ordering` — DRA-237
6. `Vagrant.Staffing` — DRA-238

## Finished when

- `dotnet run --project sample/InvulnerableVagrant/Vagrant.Host` serves every endpoint
  above against a seeded SQLite file
- no context project references another context project
- every `Vagrant.*` project builds analyzer-clean with no suppressions
- two concurrent claims on the shop's clerks produce one `Assignment` per clerk and no
  `DbUpdateConcurrencyException` outside a repository

## Refinement record

### Round 1 — 7 findings, 7 applied

**Applied**

- *Define Errors Out of Existence* — `Coin.FromGold` and `Coin.From` took `int`
  components with the non-negative invariant asserted only in prose. Now stated as a
  guard at construction, with the reason the check belongs there rather than in a
  `Result`.
- *Fail Fast* — `void Identify` could not report an already-identified error, which the taxonomy
  named. Applied by removing the error rather than by widening the return: identifying
  an already-identified specimen is now a no-op, so the failure does not exist to
  report. Appraisal ends with no error family.
- *Fail Fast* — `Buyback.Settle` returned `Result<Receipt, Error>` with no `OrderingError`
  code reachable from it. Added `BuybackAlreadySettled`, buyback has already settled.
- *Minimize Complexity* — `StockedItem.Stock` returned `Result<StockedItem, Error>`
  with no failure left to report, since `PriceBand.Between` already rejects an inverted
  band. Now returns `StockedItem`.
- *Names as Documentation* — `PriceBand.Ask` and `PriceBand.Least` matched no entry in
  the dictionary. Renamed `AskingPrice` and `FloorPrice`, which are the defined terms.
- *Names as Documentation* — `Purchase.Consider` read as evaluation without commitment
  while in fact committing the agreed price. Renamed `Agree`.
- *Names as Documentation* — `IStockLedger.OnTheFloorAsync` collided with `Floor`, an
  alias of `FloorPrice` in the same context. Renamed `OnDisplayAsync` rather than the
  proposed `AvailableAsync`, which names no domain concept.

**Rejected**

- None.

### Round 2 — 5 findings, 5 applied

**Applied**

- *Deep Modules* — `ISpecimenShelf` made the caller sequence `FindAsync`, `Identify`
  and `SaveAsync`, which is the one repository in the draft that did not follow the
  shape the draft argues for. Replaced with `IdentifyAsync`, one call.
- *Define Errors Out of Existence* — `Errand` carried a bare `Guid Subject`, so any
  identifier in the sample could be handed to a clerk as any other. Applied with
  `ErrandSubject` rather than the proposed `SpecimenId`, which would have given
  `Vagrant.Staffing` a reference to `Vagrant.Appraisal` and broken the context
  boundary the sample exists to demonstrate. `ErrandKind` dropped with it — one kind
  of errand needs no discriminator.
- *Deep Modules* — `IPurchaseBook` could not persist a newly opened `Purchase`, and its
  `FindAsync` served no endpoint in the boundary table. Replaced with `AddAsync`.
- *Principle of Least Astonishment* — `Buyback` had no repository while every other
  aggregate root had exactly one. Added `IBuybackBook`.
- *Principle of Least Astonishment* — `BuybackAlreadySettled` had no status code to travel on,
  because `POST /buybacks` listed no 409. Added.

**Rejected**

- None.

### Round 3 — 4 findings, 4 applied

**Applied**

- *Principle of Least Astonishment* — `POST /specimens/{id}/identify` listed a 503 for a
  clerk that no described call ever claimed, and had no 404 for a specimen that is not
  there. The endpoint now states the two-context orchestration, and the 404 is listed.
- *Minimize Complexity* — `POST /orders` created and settled a purchase in one call, so
  `Purchase.Agree` had no moment to happen in and `OfferBelowFloor` and `PurchaseAlreadySettled` were
  unreachable. Split into open, offer and settlement. This is the finding that mattered
  most: the haggling decision taken during alignment had no path through the boundary at
  all, and two rounds had not noticed.
- *Principle of Least Astonishment* — `POST /buybacks` listed a 400 and a 402 that
  nothing could produce. Split the same way, and `TillCannotCover` added for a till that
  cannot cover the offer.
- *Minimize Complexity* — `Coin` had no multiplication, so `Purchase.Total` was
  unimplementable for a line with a quantity above one, and `AgreedPrice` was never
  connected to what is owed. Added `operator *` and stated how `Total` is computed.

**Rejected**

- None.

### Rounds

Three rounds at medium effort, 16 findings, 16 applied, none rejected. Two findings had
their proposed replacement changed during adjudication and both are noted above. No round
returned clean, so the budget stopped the loop rather than a clean pass — a fourth round
would likely still find something.
