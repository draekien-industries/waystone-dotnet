# The Invulnerable Vagrant

## Purpose

A domain-driven sample: Pumat Sol's magic item shop in Zadash, modelled to show how
`Waystone.Monads` is consumed in an application with real bounded contexts. It is run,
not published — no `#region` here is quoted onto a GitBook page.

**A package appears only where the domain needs it.** Do not reach for one to
demonstrate it.

## Ubiquitous Language

Invoke `ubiquitous-language` before introducing or using domain terminology, to query
or add to the project's dictionary.

The dictionary is [ubiquitous-language.yaml](ubiquitous-language.yaml). A type name
that is domain vocabulary appears there; an aggregate's identity type does not, because
identity is a general concept rather than a word anyone in the shop would use.

## Contexts do not reference each other

`Vagrant.Catalog`, `Vagrant.Appraisal`, `Vagrant.Ordering` and `Vagrant.Staffing` each
reference `Vagrant.SharedKernel` and nothing else in the sample. The boundary is the
project graph, so a translation between two contexts happens in `Vagrant.Host`.

Adding a `ProjectReference` between two contexts is not a shortcut; it deletes the
lesson.

## The same object has a different name in each context

The physical thing on the shelf is a `StockedItem` in Catalog, a `Specimen` in Appraisal
and a `LineItem` in Ordering. Do not unify them.

## Packages

Referenced: `Waystone.Monads`, `.SourceGenerators`, `.Schemas`, `.SystemTextJson`,
`.Extensions.DependencyInjection`, `.Extensions.Hosting`, `.Analyzers`, `.Shouldly`,
`Waystone.WideLogEvents` and the Serilog enrichers.

Deliberately absent, so do not add them: `.Linq`, `.FluentValidation`, `.NewtonsoftJson`,
`.Extensions.Logging`.

Requests are parsed by `.Schemas`; nothing else validates. `Option<T>` is serialized into
response bodies; `Result<T, E>` is unwrapped at the endpoint into a status code and never
appears on the wire.

A schema is the only route from a body to a domain type. `HandInSpecimenSchema` returns a
`Specimen`, not a validated DTO, so no method downstream accepts an unparsed request.

## An error catalog is internal, and the host sees it through `InternalsVisibleTo`

`CatalogError` and its siblings are `internal`. `Vagrant.Host` reads their generated
codes to choose a status code, and the context's own test project asserts on them; the
`InternalsVisibleTo` items are in the context's `.csproj`. Making one public puts codes
on the context's API surface for callers that have no status code to choose.

`Vagrant.Host` grants the same to `Vagrant.Host.Tests`, because top-level statements make
`Program` internal and `WebApplicationFactory<Program>` cannot see it otherwise.

## `strict` refuses every throwing unwrap

`Unwrap`, `Expect`, a discarded `InspectErr` and a bare `throw` in a method that does not
return a `Result` are all build errors here. The way through is to return the `Result` to
a caller that can act on it — `CatalogSeed` returns one, `ShopDatabase.OpenAsync` passes
it on, and `Program` turns it into a log line and an exit code.

A lambda that captures a local is WM2017. Take the stateful overload —
`Match(state, static (ok, s) => …)` — rather than suppressing it.

## An endpoint test class that claims clerks needs a shop per test

`SpecimenEndpointsTests`, `ClerkEndpointsTests` and `PurchaseEndpointsTests` each build
their own `ShopFixture` and implement `IAsyncDisposable`, rather than sharing one through
`IClassFixture`. A clerk stays engaged until the item is collected and there are four of
them, so a shared shop fails whichever test happens to run fifth. The other endpoint
classes take no clerk and share a fixture.

## A class fixture must not be a `WebApplicationFactory`

That type implements both `IDisposable` and `IAsyncDisposable`, and xUnit v3 fails a
class fixture implementing both with a `TestPipelineException` — reported as a *cleanup*
failure after every test has already passed, so `dotnet test` still says `Passed!` while
`pre-push` fails. `ShopFixture` wraps a private factory and implements
`IAsyncDisposable` alone.

## An aggregate carries a private parameterless constructor

EF Core cannot bind a complex property through a constructor parameter, so
`StockedItem(StockedItemId, string, PriceBand, uint)` alone fails at model build with
"No suitable constructor was found". The parameterless one is for rehydration and says
so; do not delete it as unused.

## One SQLite file per bounded context

`ShopDatabase.For(connectionString, "catalog")` derives each context's file from the one
configured name. `EnsureCreated` builds a schema only when the database does not exist,
so on a shared file the first context creates its tables and every later one finds a
database already there and creates nothing — surfacing as
`SQLite Error 1: 'no such table: Specimens'` on the first request, not at startup.

A new context adds a file. `ShopFixture` deletes by glob for that reason; a named list
leaves the newest behind.

## A request body carries `Option<T>`, never a nullable

WM3001 rejects a DTO of `Guid?`/`string?`/`int?` under `strict`. Declare each field as
`Option<T>` with a `= Option.None<T>()` initializer rather than a positional record
parameter: `Option<T>` is a record class, so an omitted JSON property would otherwise
arrive as `null` rather than `None`. `Schema.Required` takes the `Option<TIn>` directly.

## An aggregate that needs the time takes a `TimeProvider`

`Purchase.Settle(Coin, TimeProvider)` and `Buyback.Settle(Coin, TimeProvider)`. Neither
the moment nor `DateTimeOffset.UtcNow` appears anywhere else: `Program` registers
`TimeProvider.System`, the books hold it, and the domain tests pass a
`FakeTimeProvider`. Do not add a `DateTimeOffset` parameter to a repository method — a
caller that can name the hour can write any hour it likes on a receipt.

## A route is named for the intent, never for the row it changes

`POST /purchases/{id}/offer` and `POST /purchases/{id}/settle`, not a `PATCH
/purchases/{id}` taking `{"settled": true}` or `{"agreedPrice": 400}`. Both of those are
outcomes the shop decides; a body that could set them would let a client write what it
wants to be true.

The same rule shapes the bodies. `POST /purchases` names shelf labels and quantities and
carries no prices, because the prices are the Catalog's. `POST /buybacks/{id}/settle`
carries no body at all, because what the shop offered and what its till holds are both
the shop's to know.

## A schema produces the domain type when the body holds one

`OpenBuybackSchema` returns a `Buyback`, so nothing downstream accepts an unparsed body.
`OpenPurchaseSchema` returns `OpenPurchase` instead — a purchase is opened on prices that
are not in the body, so a `Purchase` is not something the parse can honestly produce. Do
not add a request type between a schema and an aggregate the body could have built.

## EF maps public properties and nothing else

`Purchase.Settled`, `LineItem.Haggled` and `Specimen.Identified` are internal, so
`Property(p => p.Settled)` is named explicitly in the context's `OnModelCreating`. Leave
it out and the column is silently absent: a settled purchase reads back as open, and
nothing in the build or at startup says so.

## Stock comes off the shelf all at once

`IStockLedger.WithdrawAsync` takes every `Wanted` line in one call. A patron refused their
fourth item must find the first three still on the shelf, and one call per line cannot
promise that. `StockLedger` relies on `SaveChangesAsync` running once, after every line
has succeeded — the tracked changes of a failed set are discarded with the scope.

## A purchase carries one line per thing

`LineItems.Of` refuses two lines naming the same subject, and `PurchaseLines` is keyed on
`(PurchaseId, Subject)` because of it. `Purchase.Agree` finds a line by subject; two
matching lines would haggle over one and leave the other at the asking price.

## A lost race is caught at the repository, never surfaced

`Clerk.Engaged` is an EF concurrency token, so a claim is `UPDATE ... WHERE Id = @id AND
Engaged = 0`. `ClerkRoster.ClaimAsync` catches the `DbUpdateConcurrencyException` that
raises, reloads the entry and tries the next free clerk. No caller sees that type, and
`IClerkRoster` names no version, row or token — the only outcomes are an `Assignment` and
`NoClerkFree`.

`ClerkRosterTests` drives that with a real `DbUpdateConcurrencyException`: a
`SaveChangesInterceptor` on the losing context runs the winning claim to completion at
the moment the loser is about to write. Deterministic, no threads, and a real refusal
from SQLite. Do not replace it with a mocked repository or a simulated conflict.

## EF cannot bind a complex property to a constructor parameter

`Assignment(ClerkId, Errand, DateTimeOffset)` is a positional record, so leaving `Errand`
as a nested complex property fails at model build with "No suitable constructor was found
for the type 'Clerk.Holding#Assignment'". `ErrandConverter` makes it a scalar and it
binds. The same rule is why each aggregate carries a private parameterless constructor.

## A clerk is claimed only once the shop knows it holds the item

`SpecimenEndpoints.ReadAsync` asks the shelf before the roster. Claiming first would need
a compensating release when the item turns out not to be there, which is a second rule
about the same clerk and a second thing to get wrong.

`ClerkRoster.ClaimAsync` answers an errand a clerk already holds with that clerk rather
than a second one. Four clerks would otherwise run out on the fourth question about one
cloak.

## A nested monad chain is extracted into a named method

WM2025 rejects an `Option.Match` written inside a `Result.Match` delegate.
`SpecimenEndpoints.ReadAsync` is the extracted method, passed as a method group to the
stateful `Match`. This is separate from WM2017: WM2017 is about capture, WM2025 about
nesting, and a `static` lambda satisfies only the first.

## The document is generated, and the reference is mapped in every environment

`GET /openapi/v1.json` and `GET /scalar/v1`, both mapped unconditionally in `Program.cs`.
Plain `dotnet run` is Production, so an `IsDevelopment` check answers a 404 to the reader
the README sends there.

`GenerateDocumentationFile` is set in the `.csproj` for the document, not for a published
`.xml`. The generator reads its descriptions out of the compilation's doc comments, so with
the property off every `<summary>` in `Contracts` is dropped from the document in silence
while a referenced package's summaries still arrive — which makes the gap look like a
transformer bug rather than a missing property.

`Microsoft.AspNetCore.OpenApi` is referenced directly although `Scalar.AspNetCore.Microsoft`
brings it. That package asks for `10.0.0`, which resolves `Microsoft.OpenApi` 2.0.0 and
its GHSA-v5pm-xwqc-g5wc advisory; `TreatWarningsAsErrors` is on here, so the `NU1903` that
raises fails the restore. Deleting the reference as redundant breaks the build.
`sample/WideEvents.AspNetCore.Sample` carries the same reference for the same advisory,
where it silences a warning rather than an error — the central `PackageVersion` reaches
neither project on its own, because `CentralPackageTransitivePinningEnabled` is unset.

## An endpoint declares the responses it produces

Every handler returns `IResult`, so nothing about a response is inferred. A route without
`.Produces<T>(...)` and a `.ProducesProblem(...)` per refusal appears in the document as a
path with a bare 200 and no schema, and the build says nothing.

A 404 is `.ProducesProblem`, not `.Produces`. `UseStatusCodePages` fills in a bodiless
`Results.NotFound()` with a problem document, so the plain form describes a response the
shop does not send.

**The context's tag goes on a route group, never on a route.** Each `Map*Endpoints` class
opens with `routes.MapGroup(string.Empty).WithTags("...")` and maps every route off that,
so a route added later inherits the tag instead of needing one nothing in the build would
miss. The prefix is empty because each route names its whole path. The tag has to match a
name `ShopDocumentTransformer` declares, or the reference shows a group with no
description.

A 400 is declared once, as `.ProducesValidationProblem()`, on a route whose body a schema
parses. Adding `.ProducesProblem(400)` beside it leaves two declarations of one status and
the document keeps one of them.

## An `Option` is inlined into the document as its payload or null

`OptionSchemaTransformer` rewrites it to `anyOf: [payload, null]`, and its `ReferenceId`
returns none for an option so the schema is inlined rather than registered. Both halves
matter: without the transformer a converted type has no properties for the exporter to
read and the field is published as an empty schema, and without the inlining the document
grows an `OptionOfGuid` component naming a CLR type no client ever sees.

Do not null the schema's description there. The doc-comment generator runs after this
transformer, so the description on the inlined schema is the field's own summary.

`RefusalSchemaTransformer` adds `code` to `ProblemDetails` for the same reason:
`Refusal.From` writes it into `Extensions`, which the serializer flattens into the body
and the exporter does not describe.

## A string literal here is ASCII

No file in this sample carries a byte order mark, so the compiler reads a source file as
Windows-1252 and an em dash inside a string literal reaches the wire as three bytes of
mojibake. Comments and doc comments are safe, since neither reaches the output.

## Coverage

`codecov.yml` ignores `sample/**`. Tests here exist to demonstrate assertions on a monad,
not to reach a coverage target.
