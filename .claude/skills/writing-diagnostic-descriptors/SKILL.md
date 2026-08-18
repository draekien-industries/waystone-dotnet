---
name: writing-diagnostic-descriptors
description: Write a WM diagnostic descriptor for the Waystone.Monads analyzer — tier, id, title, message format, description and tags. Use when adding or changing a rule in Rules.cs, when deciding a rule's severity, category or default enablement, or when the user says "add an analyzer rule", "new WM rule", "write a diagnostic".
---

# Writing diagnostic descriptors

Every rule the analyzer reports is one `static readonly DiagnosticDescriptor`
field on `Rules`, built by one of three private factories — `Bug`, `Idiom`,
`Migration`. The factories own category, severity, enablement and the help
link; a descriptor supplies an id, a title, a message format, a description and
optionally custom tags. Do not call `Create` directly, and do not add a fourth
factory without settling the tier question below first — the tier *is* the
factory.

**This document is the standard.** Where a descriptor already in `Rules.cs`
differs from what follows, the descriptor is what a past change happened to
produce, not a convention to copy. Write the new rule the way this document
says and leave the old ones alone.

## Earn the rule first

Every rule ships inside the `Waystone.Monads` package, enabled, to every
consumer on upgrade. A consumer who wanted a new version of a library did not
ask to have their build broken, and `.editorconfig` is the only opt-out.

So before writing anything, answer: **can this rule fire on code that works
today?** If it can, it does not belong in WM1 at warning severity. The choices,
in order of preference:

- Narrow the rule until every hit is genuinely broken. Scoping to the cases
  that provably misbehave is better than a broad rule at a lower severity.
- Ship it as WM2 at `Info`, where it advises without failing a build.
- Ship it as WM3, disabled, if it fires across an entire codebase by design.

Lean toward the quieter tier when unsure, because the two directions are not
symmetric: lowering a rule's severity or disabling it later costs a consumer
nothing, while raising it breaks the build of everyone who was tolerating the
old level. Shipping a doubtful rule off and turning it on once it has proven
itself is the cheap order; shipping it loud and retreating is not.

A rule that duplicates one that already exists is worse than no rule: two
descriptors on one span report twice, and the second reads as a distinct
problem. If the situation is already covered, tighten the existing rule
instead. Where the overlap is deliberate — a declaration-site rule beside a
call-site one — say so in the docs page and in the XML doc on the field.

**Done when** the rule has a tier that its worst plausible false positive
survives.

## Tier

| Tier | Factory | Category | Severity | Default | Holds |
| --- | --- | --- | --- | --- | --- |
| `WM1xxx` | `Bug` | Reliability | `Warning` | on | Code that throws, silently does nothing, or means the opposite of what it reads as |
| `WM2xxx` | `Idiom` | Usage | `Info` | on | Code that works but says it worse than the library's own vocabulary does; obsoletions |
| `WM3xxx` | `Migration` | Design | `Info` | **off** | Rules that fire on code with no relationship to this library, useful only while migrating onto it |

`RulesTests` enforces every column of that table by id prefix, so a descriptor
in the wrong tier fails the build rather than shipping wrong. The categories are
the FxCop set that `RS1020` accepts; a new category has to come from that list.

Take the next free id within the tier by reading the highest one currently in
`Rules.cs`. Ids are permanent and are never reused, including for a rule that is
deleted. A consumer's `#pragma warning disable`, `.editorconfig` entry and
`[SuppressMessage]` all name the id, so reassigning one silently redirects their
suppression onto a rule they have never seen.

`Error` is not available to any tier. A rule that ships as an error breaks the
build of a consumer who only wanted a version bump, and no analyzer rule in a
library package has earned that.

A WM3 rule must live in its own analyzer class. `Microsoft.CodeAnalysis.Testing`
force-enables every diagnostic the analyzer under test supports, so a disabled
rule sharing a class with an enabled one fires in the other rule's tests.

## Voice

The three strings do three different jobs, and the commonest failure is writing
the same sentence into all of them.

**Title** — states the rule as an expectation of the code, the way the `CA`
corpus does. `"Do not …"` for a prohibition, `"Prefer X over Y"` for a
replacement, a bare imperative for a transformation. Sentence case, no trailing
period, and short enough to read in an error-list column — treat 60 characters
as the ceiling.

> Poor: `"Null assigned where an Option or Result is expected"` — describes a
> situation and leaves the reader to infer the rule.
>
> Strong: `"Do not assign null to an Option or Result"` — the reader knows what
> is being asked of them from the error list alone.

| Shape | Use for | Example |
| --- | --- | --- |
| `Do not …` | A construct that should not appear at all | `"Do not derive from Option or Result"` |
| `Prefer X over Y` | A working call with a better spelling | `"Prefer UnwrapOr or Match over Unwrap"` |
| Imperative | A mechanical transformation | `"Flatten a nested Option"` |

**Message format** — what is wrong *here*, and what to write instead. This is
the only string most readers ever see, so it must stand alone without the title.
Name the offending element through a placeholder rather than saying "this call";
the placeholder is what turns a statement about a pattern into a sentence about
the reader's own code. Quote every type and member name in `'single quotes'`.

> Poor: `"Avoid this pattern"` — true of anything, actionable for nothing.
>
> Poor: `"This call returns '{0}' and the value is unused"` — correct, but the
> reader has to find which call.
>
> Strong: `"'{0}' returns '{1}' and the value is unused, so a failure is
> silently ignored"` — names the member, the type, and the consequence.

Name the replacement wherever one exists, concretely enough to type. Where a
code fix exists, the message describes what that fix produces.

**Description** — written for a consumer who has just seen the diagnostic for
the first time and wants to know why the pattern is a problem. Nothing else
belongs here. It carries the part a reader cannot deduce from their own code:
why the compiler permits the mistake at all — that `Option` and `Result` are
records, so null is accepted where one is expected; that `Some` rejects
`default(T)`; that a `Result`'s two implicit conversions collapse when its type
arguments match.

Reasoning aimed at the *next author* rather than the consumer — why the rule is
scoped as narrowly as it is, which neighbouring rule it sits beside, what a
wider version would have broken — goes in an XML doc comment on the descriptor
field, which `AGENTS.md` permits as public API surface, and in the docs page.
A consumer reading a tooltip does not need the rule's design history.

A `Migration` description opens with `"Disabled by default. Enable it while …"`,
then says what it over-reports on. That sentence is the only warning a consumer
gets before turning the rule on.

**Done when** the title states an expectation, the message names the offending
element and a concrete replacement, and the description explains why the
compiler allowed it.

## The punctuation the build enforces

`Microsoft.CodeAnalysis.Analyzers` polices descriptor strings, these rules are
on by default at `Warning`, and `src/**` treats warnings as errors — so they
are build failures, not style preferences:

| Rule | Requires |
| --- | --- |
| `RS1031` | Title contains no period, no line return, no leading or trailing whitespace |
| `RS1032` | Message contains no line return and no surrounding whitespace, and is either a single sentence with **no** trailing period or multiple sentences **with** a trailing period |
| `RS1033` | Description is one or more sentences ending in punctuation, with no surrounding whitespace |

The line-return clause is the one that bites: write a long string as adjacent
literals joined by `+`, never as a verbatim or raw string literal spanning
lines, which embeds the newline and the source indentation into the message a
consumer reads.

`.editorconfig` additionally turns on `RS1015`, `RS1020` and `RS1028` for the
analyzer projects, so a missing help link or a category outside the accepted set
also fails the build. `RS1007` stays off deliberately: descriptor strings are
raw literals rather than `LocalizableResourceString`, so that they can be read
in `Rules.cs`. Do not introduce a `.resx` for one rule.

## Placeholders

`Diagnostic.Create` takes the message arguments positionally, and a count
mismatch is silent — a missing argument renders as a literal `{0}` in a
consumer's IDE, and no test catches it unless the expected message asserts the
substituted text. Write the analyzer's `ReportDiagnostic` call in the same
change as the descriptor.

Fill type and member placeholders through `Semantics.Display`, not
`ToDisplayString()` — it uses `MinimallyQualifiedFormat` and strips the nullable
annotation, so messages read `Option<int>` rather than a fully-qualified name
with a stray `?`.

## Custom tags

All three factories take `params string[] tags` and forward them to the
descriptor. Pass a tag whenever one describes the rule:

- `WellKnownDiagnosticTags.Unnecessary` when the fix *removes* code. This is
  what makes an IDE fade the redundant span rather than merely underline it, and
  it is the tag most WM2 rules with a code fix want.
- `WellKnownDiagnosticTags.CompilationEnd` when the diagnostic is reported from
  a compilation end action, or `RS1037` fails the build.

Do not pass a tag that does not apply. `Telemetry` and `Build` describe how a
host treats the diagnostic and say nothing true about a library rule, so a
descriptor with no applicable tag passes none. `RS1028` is enabled but does not
flag an empty tag list on a descriptor built through a helper, so this is on the
author rather than the build.

## Paired obligations

A descriptor alone does not ship. In the same change:

- [ ] Add the row to `AnalyzerReleases.Unshipped.md`, or `RS2008` fails the
      build. Use severity `Disabled` in that table for a WM3 rule. Move the row
      into `Shipped.md` under the version the PR title will compute *before*
      merging, not after — merging publishes.
- [ ] Record a change to a shipped rule in `Unshipped.md` too, never by editing
      the shipped table: a `### Changed Rules` row for a new severity or
      enablement, a `### Removed Rules` row for a rule no longer reported.
- [ ] Bump the tier count in `RulesTests.EveryTierIsPopulated`.
- [ ] Report the diagnostic from a `MonadAnalyzer` subclass.
      `EveryRuleIsSupportedByAnAnalyzer` fails on a descriptor no analyzer
      declares.
- [ ] Add the rule's section to the analyzer-rules page in the
      [docs repository](https://github.com/draekien-industries/docs), and link
      the two PRs. The help link is derived from the id, so a rule without a
      docs section ships a link to an anchor that does not exist — and the
      anchor has to keep working forever, since consumers reach it from the
      build output of versions long past.

Give the docs section a minimal violating example, the corrected form, and any
known false positive. A reader who followed the help link has already decided
the message was not enough.

## Gotchas

**The message is the contract the tests assert.** Analyzer tests match the
substituted message text, so editing a message format breaks every test that
expects it. That friction is intended, and it is not the only cost: consumers
grep for wording they have seen before. Reword a shipped message only when the
old one was wrong.

**A `Warning` that no code fix resolves is a trap.** If the message tells a
consumer to restructure their type, they have to do it by hand on an upgrade
they did not choose. Either supply the fix or drop to `Info`.

**Obsoletions are WM2, not WM1.** Calling deprecated API works; it just will not
work in the next major. The message names both the replacement and the version
that removes it, matching the obsoletion attribute's own message.
