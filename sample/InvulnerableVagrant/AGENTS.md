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

## A nested monad chain is extracted into a named method

WM2025 rejects an `Option.Match` written inside a `Result.Match` delegate.
`SpecimenEndpoints.ReadAsync` is the extracted method, passed as a method group to the
stateful `Match`. This is separate from WM2017: WM2017 is about capture, WM2025 about
nesting, and a `static` lambda satisfies only the first.

## Coverage

`codecov.yml` ignores `sample/**`. Tests here exist to demonstrate assertions on a monad,
not to reach a coverage target.
