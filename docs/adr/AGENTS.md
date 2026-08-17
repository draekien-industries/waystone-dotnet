# Architectural decision records

An ADR records why a decision was made, for a reader who arrives years later with no access to the people who made it.

## The bar

Offer an ADR only when **all three** are true:

1. **Hard to reverse** — changing your mind later carries a meaningful cost.
2. **Surprising without context** — a future reader will ask "why did they do it this way?"
3. **The result of a real trade-off** — there were genuine alternatives, and one was chosen for specific reasons.

If a decision is easy to reverse, skip it — it will just be reversed. If it is not surprising, nobody will wonder why. If there was no real alternative, there is nothing to record beyond "we did the obvious thing."

Offer ADRs sparingly. A directory of ADRs for routine choices trains readers to ignore all of them.

### What qualifies

- **Architectural shape** — "we use a monorepo"; "the write model is event-sourced, the read model projects into Postgres".
- **Integration patterns between contexts** — "Ordering and Billing communicate via domain events, not synchronous HTTP".
- **Technology choices that carry lock-in** — database, message bus, auth provider, deployment target. Not every library; only the ones that would take a quarter to swap out.
- **Boundary and scope decisions** — "Customer data is owned by the Customer context; other contexts reference it by ID only". The explicit no-s matter as much as the yes-s.
- **Deliberate deviations from the obvious path** — "manual SQL instead of an ORM, because X". Anything a reasonable reader would assume the opposite of. These stop the next engineer from "fixing" something intentional.
- **Constraints not visible in the code** — "we cannot use AWS, for compliance reasons"; "responses must be under 200 ms, per the partner API contract".
- **Rejected alternatives where the rejection is non-obvious** — if GraphQL was considered and REST chosen for subtle reasons, record it, or someone proposes GraphQL again in six months.

## Frontmatter

Every ADR carries this frontmatter, with each field on its own line so the directory stays searchable by plain text tools.

```yaml
---
id: 0007
title: Communicate between Ordering and Billing via domain events
status: accepted # proposed | accepted | superseded | deprecated
date: 2026-01-30
deciders: [ada-lovelace, grace-hopper]
tags: [integration, messaging]
supersedes: 0003-synchronous-billing-calls
superseded-by:
---
```

- `id` — four digits, zero-padded, one higher than the highest existing id.
- `status` — exactly one of the four listed values.
- `tags` — lowercase, hyphenated, flat list. Reuse an existing tag before coining a new one.
- `supersedes` / `superseded-by` — the other ADR's filename without extension, or empty.

Filename: `<id>-<kebab-case-title>.md`, matching the `id` and `title` fields.

## Searching

Fields are line-anchored, so search them directly with ripgrep (or any equivalent):

| Question | Search |
| --- | --- |
| What decisions are in force? | `rg -l '^status: accepted' docs/adr` |
| What has been decided about X? | `rg -il '^(title\|tags):.*x' docs/adr` |
| List every ADR by title | `rg -N '^title: ' docs/adr` |
| What replaced this ADR? | `rg -N '^superseded-by: ' docs/adr/0003-*.md` |

Filter to `status: accepted` before acting on an ADR. A `proposed` or `superseded` record is not a constraint.

## Changing a decision

Never edit a decision into an existing ADR. Write a new one, set the old one's `status` to `superseded` and its `superseded-by` to the new filename, and set the new one's `supersedes` to the old filename. The history is the point.

## Template

````markdown
---
id: NNNN
title: <Decision, stated as an action>
status: proposed
date: YYYY-MM-DD
deciders: []
tags: []
supersedes:
superseded-by:
---

# NNNN — <Decision, stated as an action>

## Context

What forces created this decision — constraints, requirements, the state of the system. Written so a reader with no memory of the moment can reconstruct it. No solution language.

## Decision

What was decided, in the active voice: "we will ...". One paragraph.

## Consequences

What becomes easier, what becomes harder, and what the team accepts as a cost. Include the consequences that were unwelcome but accepted — those are the ones a future reader needs.

## Alternatives considered

For each real alternative: what it was, and the specific reason it was rejected. "Not a good fit" is not a reason. If an alternative was never seriously on the table, leave it out.
````
