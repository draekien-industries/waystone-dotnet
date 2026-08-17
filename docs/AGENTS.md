# Documentation

Four directories, four purposes. Put a document where its purpose says it goes; do not create new top-level directories here.

| Directory | Holds | Lifetime |
| --- | --- | --- |
| `adr/` | Architectural decision records | Permanent — an ADR is superseded, never edited into a new decision |
| `references/` | Material pulled in from outside: API docs, `llms.txt` indexes, specs, vendor guides | Refreshed when the source changes |
| `explorations/` | Investigations into options and ideas — the evidence gathered and what it favours, not a commitment to act | Kept as a record of what was already looked at |
| `plans/` | Proposed work, written before it is done | Marked `done` once executed |

Read [adr/AGENTS.md](adr/AGENTS.md) before reading or writing an ADR. It carries the bar for what warrants one — the bar is high, and most decisions do not clear it.

## Before starting work

Search `explorations/` and `plans/` for the topic first. Someone may have already investigated it, and repeating an exploration wastes the record of the last one. Check `adr/` for a decision that constrains the approach.

## Explorations

Filename: `YYYY-MM-DD-short-topic.md`. Frontmatter:

```yaml
---
title: Short topic
date: YYYY-MM-DD
status: active # active | superseded | done
supersedes: # optional, filename of an earlier exploration
---
```

Record the question asked, the options examined, the evidence for each, and what the evidence favours. An exploration may end without a recommendation — say so rather than manufacturing one. When a later exploration replaces one, set the old file's status to `superseded` and name the replacement.

## Plans

Filename: `YYYY-MM-DD-short-title.md`. Frontmatter:

```yaml
---
title: Short title
date: YYYY-MM-DD
status: active # active | superseded | done
supersedes: # optional, filename of an earlier plan
---
```

State the goal, the steps, and how to tell the work is finished. Link the exploration or ADR it follows from, if any. Set status to `done` when the work ships; do not delete the file. When a later plan replaces one, set the old file's status to `superseded` and name the replacement.

## References

Filename describes the source: `stripe-api.md`, `react-router-llms.txt`. Record where the material came from and when it was captured at the top of the file, so a reader can tell how stale it is.

## Status discipline

A stale `active` document is worse than a missing one — an agent will act on it. When you finish or supersede work, update the frontmatter of the document that drove it in the same change.
