# Documentation

Four directories, four purposes. Put a document where its purpose says it goes; do not create new top-level directories here.

| Directory | Holds | Lifetime |
| --- | --- | --- |
| `adr/` | Architectural decision records | Permanent — amended in place for updates, superseded only for a reversal |
| `references/` | Material pulled in from outside: API docs, `llms.txt` indexes, specs, vendor guides | Refreshed when the source changes |
| `explorations/` | Investigations into options — the evidence gathered and what it favours, not a commitment to act | Kept as a record of what was already looked at |
| `plans/` | Proposed work, written before it is done | Marked `done` once executed |

Search `explorations/` and `plans/` for the topic before starting work, and `adr/` for a decision that constrains the approach. Read [adr/AGENTS.md](adr/AGENTS.md) before reading or writing an ADR — the bar is high, and most decisions do not clear it.

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
