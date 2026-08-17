---
id: 0003
title: Extend monad options through an internal satellite registry
status: accepted
date: 2026-08-17
deciders: [william-pei]
tags: [configuration, package-boundaries, monads]
supersedes:
superseded-by:
---

# 0003 — Extend monad options through an internal satellite registry

## Context

`MonadOptions` configures the monads core: the exception logger, the error code
factory, and the fallback error code and message. `Waystone.Monads.FluentValidation`
adds its own settings — the validation error code and a fallback validation
message — which previously lived in a second, independent singleton.

Introducing `MonadOptions.BeginScope`, which overrides configuration for the
current asynchronous flow, exposed the problem. A scope that covered only the
core settings would leave the validation settings reading from their own global
singleton, so the one construct a caller reaches for to isolate configuration
would silently fail to isolate half of it.

The obvious fix — have `MonadOptions` know about the validation settings
directly — is not available. `Waystone.Monads` cannot reference
`Waystone.Monads.FluentValidation`; the dependency runs the other way, and
inverting it would put FluentValidation in front of every consumer of the core.

## Decision

We will let satellite packages attach their own options objects to a
`MonadOptions` instance through an internal registry. Satellites implement an
internal `IMonadOptionsSatellite` with a `Clone` method, `MonadOptions` holds
them in a lazily allocated dictionary keyed by type, and entering a scope clones
each attached satellite along with the core settings. A satellite resolves its
own options from a given `MonadOptions` instance rather than from a singleton of
its own.

## Consequences

One `using` covers every package. A caller writes
`MonadOptions.BeginScope(options => options.UseValidationErrorCode(...))` and the
validation settings honour it, with no second scope to keep in step. Satellite
extension methods now resolve from their `MonadOptions` receiver instead of
discarding it, which is what makes this work.

New satellites cost nothing in the core: a package implements the interface and
attaches itself, and `MonadOptions` needs no knowledge of it.

The cost lands on the hottest read in the library. Resolving options now consults
an `AsyncLocal` and, for satellites, a dictionary. This is mitigated rather than
eliminated — a volatile one-way latch keeps the pre-scope path on the singleton
until a scope is actually opened, the satellite dictionary is allocated only when
a satellite registers, and a `TryGetValue` fast path avoids allocating a closure
on every cache hit.

We accept that the registry is closed to third parties. `IMonadOptionsSatellite`
and `MonadOptions.Create` are internal, so only packages in this repository can
add satellites or construct an options instance. Consumers configure; they do not
construct. Opening either would make the extension surface public API and bind us
to the clone-on-entry contract permanently.

## Alternatives considered

**A project reference from the core to the FluentValidation package.** The
direct, simple version. Rejected because it inverts the existing dependency and
would force FluentValidation on every consumer of the core, including those who
only want `Option` and `Result`.

**A separate `BeginScope` per package, nested by the caller.** Each package keeps
its own singleton and its own scope construct. Rejected because it pushes the
composition problem onto the caller: two `using` statements that must be opened
in the right order and torn down together, with a silent half-scoped state when
someone forgets the second one. The failure mode is invisible at the call site,
which is the specific thing the scope exists to prevent.

**Passing options explicitly through the monad APIs.** Would remove the ambient
state entirely and with it the whole class of problem. Rejected as a change to
every public signature in the library, for a configuration surface that callers
overwhelmingly want to set once.
