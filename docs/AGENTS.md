# Documentation

Put a document where its purpose says it goes; do not create new top-level directories here.

| Directory | Holds | Lifetime |
| --- | --- | --- |
| `contexts/` | Standing guidance an `AGENTS.md` links to instead of carrying | Permanent — edited with the code it describes |
| `adr/` | Architectural decision records | Permanent — amended in place for updates, superseded only for a reversal |
| `references/` | Material pulled in from outside: API docs, `llms.txt` indexes, specs, vendor guides | Refreshed when the source changes |
| `explorations/` | Investigations into options — the evidence gathered and what it favours, not a commitment to act | Kept as a record of what was already looked at |
| `plans/` | Proposed work, written before it is done | Marked `done` once executed |

Search `explorations/` and `plans/` for the topic before starting work, and `adr/` for a decision that constrains the approach. Read [adr/AGENTS.md](adr/AGENTS.md) before reading or writing an ADR — the bar is high, and most decisions do not clear it.

## Contexts

A context holds guidance that is true forever and needed for one task. An `AGENTS.md` is
read on every turn in its area; a context is read only when the task calls for it, so
material that fails the "would every agent working here need this?" test goes here and
the `AGENTS.md` keeps a link naming the trigger.

Filename describes the task, not the area: `binder-async-conversion.md`, not
`monads-notes.md`. No frontmatter and no status — a context is current or it is edited.

| File | Read before |
| --- | --- |
| `public-api-baseline.md` | Adding, changing or removing a public member; any RS0016 or RS0017 |
| `binder-async-conversion.md` | Editing an async member of `OptionBound` or `ResultBound` |
| `awaited-receiver-conversion.md` | Applying `[GenerateAwaitedReceivers]` to an extension family |
| `analyzer-severity-presets.md` | Adding a `WM` rule; editing `src/Waystone.Monads/build/` |
| `wmsc-severity.md` | Adding a `WMSC` rule |
| `schema-rule-authoring.md` | Adding or changing a `Schema.*` rule |
| `schema-violation-reporting.md` | Editing `Violations/` or `Internal/Reporting/` |
| `schema-structures.md` | Editing `Internal/Structures/`, `MinCount` or `MaxCount` |
| `conventions-tests.md` | Adding to `Waystone.Conventions.Tests` or editing its `.csproj` |
| `assertion-analyzer-sweep.md` | Migrating a test project's assertions to the Shouldly forms |

**Every link to a context states the trigger**, because a link with no trigger is a
context nobody opens. Several areas may link the same file where the task spans them —
`public-api-baseline.md` is linked from two — but the file states each rule once, and an
`AGENTS.md` that restates one has duplicated it rather than summarised it.

## Explorations and plans

Filename `YYYY-MM-DD-short-title.md`. Frontmatter:

```yaml
---
title: Short title
date: YYYY-MM-DD
status: active # active | superseded | done
supersedes: # optional, filename of the document this replaces
---
```

An exploration records the question asked, the options examined, the evidence for each, and what the evidence favours. It may end without a recommendation — say so rather than manufacturing one.

A plan records the goal, the steps, and how to tell the work is finished, linked to the exploration or ADR it follows from.

## References

Filename describes the source: `stripe-api.md`, `react-router-llms.txt`. Record where the material came from and when it was captured at the top of the file, so a reader can tell how stale it is.

## Status discipline

A stale `active` document is worse than a missing one — an agent will act on it. Set `done` when the work ships and `superseded` when a later document replaces it, naming the replacement in both, in the same change that finishes the work. Never delete.
