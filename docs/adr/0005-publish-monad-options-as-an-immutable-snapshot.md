---
id: 0005
title: Publish monad options as an immutable snapshot
status: accepted
date: 2026-08-28
deciders: [william-pei]
tags: [configuration, package-boundaries, monads]
supersedes: 0003-extend-monad-options-through-an-internal-satellite-registry
superseded-by:
---

# 0005 — Publish monad options as an immutable snapshot

## Context

ADR 0003 gave satellite packages a way to attach their own settings to
`MonadOptions`: an internal `IMonadOptionsSatellite` with a `Clone` method, a
dictionary keyed by type, and a clone of every attached satellite on scope entry.
That solved the problem it was written against — a scope that isolated the core
settings and left a satellite's reading from its own global — and it has held for
a year of satellites we wrote ourselves.

Two things then arrived that it did not anticipate.

The first was configuration through a dependency-injection container. A container
builds a configured object during registration and hands it over later, but
`MonadOptions.Configure` was `configure.Invoke(Global)` — it mutated a singleton
in place, and `Global` was a `Lazy<MonadOptions>` over a private constructor.
There was no way to say "here is a finished instance, use this one".

The second was the accumulated cost of in-place mutation, which the same line
explains in full. There was no way to return the options to their initial state,
so a test that configured the global could not undo it — `test/AGENTS.md`
documented the resulting cross-contamination as a hazard and told test authors to
use `BeginScope` instead. And because a reader could observe the singleton
part-way through a `Configure` call, a `Try` running concurrently with start-up
could see a new fallback error code beside an old fallback message. Nothing in
the API claimed configuration was atomic, and it was not.

`Clone` was the other liability. Its contract — return a detached copy, or a
scope leaks state into its parent — could not be enforced, and an implementation
that returned `this` would have broken scoping silently. That was acceptable
while every satellite was ours. Making the interface public, which is what a
third-party satellite would have required, would have made it a supported
contract for code we had not seen, permanently.

## Decision

We will make `MonadOptions` immutable and publish it by atomic swap. A separate
`MonadOptionsBuilder` carries the mutable settings and the four `Use*` methods;
`Configure` seeds a builder from the options in effect, applies the caller's
action and swaps the result into a static field under a lock; `BeginScope` does
the same into an `AsyncLocal`. Installing a pre-built snapshot and resetting to
the defaults are then the same one-line overwrite of that field, differing only in
where the snapshot came from — neither needs a mechanism of its own. `Configure`
remains distinct from those two: it reads the current snapshot before it writes, so
it accumulates, where `Install` and `Reset` replace. Satellites become immutable too, are stored in an
array indexed by a process-wide slot rather than a dictionary keyed by type, and
are carried into a new snapshot by reference — a satellite is rebuilt only when
the caller reconfigures it, through a builder the satellite's own package
constructs from its own immutable options. `IMonadOptionsSatellite` and its
`Clone` method are deleted.

## Consequences

**Configuration is atomic, and this is now a property rather than a hope.** A
snapshot is published whole, so a reader sees either every old setting or every
new one. The invariant is testable without racing the scheduler: hold a reference
to `Global`, call `Configure`, and assert the held instance is unchanged.

**`Reset` cannot drift.** It swaps in the same default snapshot the type built at
start-up, so a setting added in a later version is covered by it without anyone
remembering to. The hand-written alternative — restore each scalar — is wrong on
the second change after it ships. `test/AGENTS.md` no longer has to tell a test
author that the global is untouchable; it tells them to join the collection that
serialises the classes which touch it and to reset on entry.

**Atomic publication does not make the global safe to share, and the test suite
still coordinates around it.** This is the second unwelcome consequence, and it is
worth stating plainly because the first one invites the wrong conclusion.
Immutability fixed torn reads; it did not stop two parallel test classes calling
`Configure` from overwriting each other, because there is still one field. A test
that only needs different options uses `BeginScope`, which is per-flow and needs no
coordination at all — that is the isolated-instance answer, and it already existed.
A test that is exercising *publication itself* has no such escape and must be
serialised, which is what the shared xUnit collection does. Nothing enforces the
tag at compile time, so that guard is documentation-shaped rather than structural.

**There is no clone contract left to get wrong.** An immutable satellite is safe
to share by reference, so scope entry copies references. The interface is gone,
which settles the question DRA-124 was opened to answer: it does not need
answering, because there is nothing to make public.

**The registry stays closed to third parties, and now costs nothing to keep
closed.** `ISatelliteBuilder` and the slot allocator are internal, reached by the
first-party satellite packages through `InternalsVisibleTo` exactly as
FluentValidation already was. ADR 0003 accepted that closure as a cost of the
clone contract. The closure survives; the cost it was paying for does not.

**We accept a hard break in 7.0.0 rather than a deprecation.** This is the
unwelcome one. `Configure` and `BeginScope` change their parameter type, and the
four `Use*` methods move off `MonadOptions` onto the builder, as do the public
fluent methods on both satellite packages. The repository's rule is *deprecate;
never remove* — but an obsolete mutator on an immutable type has nothing to
mutate, so the only honest implementation of the deprecated form would throw at
run time. A consumer would get a warning saying their working code now fails,
which is worse than a compile error and worse than a clean break. So the old
members are removed outright in the major and the migration is carried by the
consolidated code fix provider instead.

**Call sites in the library did not move.** The roughly two dozen reads of
`MonadOptions.Current` across `Option`, `Result`, `Error` and `ErrorCode` are
untouched, and every scalar kept its name — which is what kept a change to the
configuration model out of the monads themselves.

**A lambda-shaped call site did not move either.** Because `Configure` and
`BeginScope` take an action whose parameter type is inferred, existing source of
the form `BeginScope(o => o.UseFallbackErrorCode("x"))` compiles against the new
type unchanged. Of the 1,019 tests in the suite at the time of the change, two
files needed an edit. A consumer who wrote the parameter type explicitly, or who
held a `MonadOptions` to configure it later, is the one who has work to do.

**Scoping is still a one-way latch, and still the thing to avoid.** The volatile
`_scopingHasBeenUsed` flag is unchanged: the first scope in a process moves every
later read onto a path that consults an `AsyncLocal`, and nothing moves it back.
`Reset` deliberately does not clear it, because clearing it while another flow
held a live scope would make reads there skip that scope.

## Alternatives considered

**Keep the mutable singleton and add `Install` and `Reset` to it.** The smallest
change, and the one the dependency-injection issue originally proposed. Rejected
because both members would be bolted onto the shape that caused the problem:
`Install` would have to reconcile a pre-built instance against a singleton other
code may already have mutated, `Reset` would be a hand-written restore that drifts,
and torn reads would remain. It also spends the major on a design we would rebuild
in the next one.

**Make `IMonadOptionsSatellite` public.** What a third-party satellite needs under
ADR 0003, and additive rather than breaking. Rejected because it makes the
unenforceable `Clone` contract permanent public API to buy an extension point
nobody has asked for, and because it is unmakeable — a public interface cannot be
re-internalised without another major. The premise was also checked and found
false for the case that raised it: the dependency-injection package writes only to
settings that already exist, so it needs no satellite of its own.

**A public interface with an internal registry.** Third parties implement and
attach; only we can read one back. Rejected as worse than either end — a satellite
you can attach but not read is not useful to the party attaching it.

**Freeze a mutable `MonadOptions` in place with a flag, instead of splitting off a
builder.** Keeps one type and one set of `Use*` methods, so the public surface
barely changes. Rejected because it moves a compile-time error to run time: the
`Use*` methods would still be callable on a published snapshot and would have to
throw, which is the same trap as the deprecation path above, permanently rather
than for one major.

**Keep the type-keyed dictionary and make the satellites immutable.** Would have
deleted `Clone` and left the storage alone, which is the smaller diff. Rejected on
the shape of the carry-forward step rather than on a measurement: once satellites
are immutable and carried by reference, copying the container is all that scope
entry does, and that is one `Array.Copy` against a dictionary allocation and a
rehash per entry. Both were not built and compared — the array was chosen because
it makes the carry-forward a single call, and the read an index rather than a hash
lookup, for a container whose keys are assigned by us and are dense by
construction.
