# Architectural decision records

An ADR records why a decision was made, for a reader who arrives years later with no access to the people who made it. Write one from [TEMPLATE.md](TEMPLATE.md).

## The bar

All three must hold. **Hard to reverse** — an easy reversal will just be reversed. **Surprising without context** — nobody wonders about the unsurprising. **The result of a real trade-off** — with no alternative there is nothing to record beyond "we did the obvious thing".

Qualifying ground: architectural shape, integration patterns between contexts, technology choices carrying lock-in, boundary and ownership decisions including the explicit no-s, deliberate deviations from the obvious path, constraints invisible in the code, and rejected alternatives whose rejection is non-obvious.

Offer ADRs sparingly. A directory of ADRs for routine choices trains readers to ignore all of them.

## Frontmatter

Each field on its own line, so the directory stays searchable by plain text tools.

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

`id` is four digits, zero-padded, one higher than the highest existing. `tags` are lowercase and hyphenated; reuse one before coining a new one. `supersedes` and `superseded-by` hold the other ADR's filename without extension, or nothing. Filename is `<id>-<kebab-case-title>.md`, matching the `id` and `title` fields.

## Searching

Fields are line-anchored, so search them directly: `rg -l '^status: accepted' docs/adr` lists what is in force, and `rg -il '^(title|tags):.*x' docs/adr` finds what was decided about X. Filter to `status: accepted` before acting on an ADR — a `proposed` or `superseded` record is not a constraint.

## Changing a decision

Amend the existing ADR when new information updates or refines the decision without reversing it: edit the body in place and add a dated entry to its Amendments section recording what changed and why. Write a new ADR only when the decision is reversed or replaced outright — set the old one's `status` to `superseded` and its `superseded-by` to the new filename, and the new one's `supersedes` to the old filename.
