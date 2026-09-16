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

## Coverage

`codecov.yml` ignores `sample/**`. Tests here exist to demonstrate assertions on a monad,
not to reach a coverage target.
