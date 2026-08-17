---
id: 0002
title: Target netstandard2.0 and backfill language features with PolySharp
status: accepted
date: 2026-08-17
deciders: [william-pei]
tags: [target-framework, compatibility, dependencies]
supersedes:
superseded-by:
---

# 0002 — Target netstandard2.0 and backfill language features with PolySharp

## Context

`Waystone.Monads` is consumed from codebases that include .NET Framework
applications which cannot be migrated. Those consumers are the reason the
library exists in a reusable package at all, so any target framework choice that
excludes them defeats the purpose.

At the same time the library leans heavily on recent C# — nullable reference
types, records, and C# 14 extension blocks among them. Several of those features
depend on attributes and types that the compiler expects to find in the
framework, and which netstandard2.0 does not provide.

## Decision

We will target `netstandard2.0` alone for the core library and use PolySharp to
generate the compiler-required types that the target framework lacks, rather
than multi-targeting current .NET versions.

## Consequences

The package reaches the widest possible set of consumers from a single build,
including the .NET Framework codebases that motivated it. The test matrix
exercises that reach directly: net472 and net481 sit alongside net8.0, net9.0,
and net10.0 on Windows.

Because there is one target, there is one code path. The core library contains
no `#if` blocks and no per-framework behaviour to hold in mind, so there is
nothing to test twice and no risk of the frameworks diverging in behaviour. This
single-target simplicity is a deliberate secondary benefit, not an accident of
the reach requirement.

The costs: the library cannot call APIs introduced after netstandard2.0 without
a shim, which is why `System.Threading.Tasks.Extensions` and `System.ValueTuple`
appear as dependencies. PolySharp becomes load-bearing — a compile-time
dependency the library cannot build without. And raising the target later is a
breaking change for the consumers who drove the choice, so `netstandard2.0` is
effectively fixed for the lifetime of the major version.

## Alternatives considered

**Multi-targeting current .NET plus netstandard2.0.** Would allow the modern
targets to use newer BCL APIs directly and drop the `System.*` shims on those
frameworks. Rejected because it reintroduces conditional compilation across the
whole library and creates the possibility of per-framework behaviour differences
in a library whose entire value is predictable control flow — the maintenance
and testing cost is paid on every change, while the benefit is a handful of
avoided shim packages.

**Targeting a modern .NET version only.** Rejected outright: it excludes the
.NET Framework consumers the library was extracted for.
