---
id: 0001
title: Publish every package from one shared version
status: accepted
date: 2026-08-17
deciders: [william-pei]
tags: [release, versioning, repository-layout]
supersedes:
superseded-by:
---

# 0001 — Publish every package from one shared version

## Context

The repository began as a single library, `Waystone.Monads`, with a single
GitVersion configuration computing a single version number. It now holds five
published packages — the monads core, its FluentValidation satellite, wide log
events, and two Serilog enrichers — which are functionally unrelated to each
other and change at different rates.

GitVersion still computes one number from the commit history of the whole
repository, and `release.yml` packs and pushes every package with that number.
A change confined to one package therefore bumps and republishes the other four.
By the time this pattern became visibly awkward, all five packages had published
history under the shared numbering.

## Decision

We will continue to publish every package in this repository from one shared,
GitVersion-computed version number, rather than versioning each package
independently.

## Consequences

Releases stay simple: one version calculation, one pack step, one push, and a
single GitHub release per merge to `main`. There is no per-package tagging
scheme, no path-filtered release matrix, and no per-package changelog to keep
straight.

The costs are accepted rather than mitigated. Consumers see version bumps in
packages that did not change, so a version number carries no information about
whether a given package actually moved. Release notes describe the repository,
not the package a consumer installed. A breaking change in any one package
forces a major bump across all of them — the deprecation policy in `AGENTS.md`
exists partly to keep that from happening often.

This decision is recorded as inherited rather than reasoned. It was the path of
least resistance when there was one package, and it is now load-bearing: the
alternative would require renumbering already-published history. That honesty
matters more here than a retrofitted justification, because it tells a future
reader the decision can be revisited on its merits rather than defended.

## Alternatives considered

**Independent per-package versioning.** Each package tagged and released on its
own cadence, so a version number means something about that package. Rejected
because the five packages already have published history under the shared
numbering, and splitting now would mean either renumbering that history or
carrying an awkward discontinuity in every package's version sequence. The
ongoing cost of republishing unchanged packages was judged smaller than the
one-time cost of that break.
