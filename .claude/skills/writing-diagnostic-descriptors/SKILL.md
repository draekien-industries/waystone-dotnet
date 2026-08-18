---
name: writing-diagnostic-descriptors
description: Write a WM diagnostic descriptor for the Waystone.Monads analyzer — tier, id, title, message format and description. Use when adding or changing a rule in Rules.cs, when deciding a rule's severity, category or default enablement, or when the user says "add an analyzer rule", "new WM rule", "write a diagnostic".
---

# Writing diagnostic descriptors

Every rule the analyzer reports is one `static readonly DiagnosticDescriptor`
field on `Rules`, built by one of three private factories — `Bug`, `Idiom`,
`Migration`. The factories own category, severity, enablement and the help
link; a descriptor supplies only an id, a title, a message format and a
description. Do not call `Create` directly, and do not add a fourth factory
without settling the tier question below first — the tier *is* the factory.

Read `src/Waystone.Monads.Analyzers/Rules.cs` before writing. It is the whole
convention, and matching the rules already there beats following this document
where the two disagree.

## Earn the rule first

Every rule ships inside the `Waystone.Monads` package, enabled, to every
consumer on upgrade. A consumer who wanted a new version of a library did not
ask to have their build broken, and `.editorconfig` is the only opt-out.

So before writing anything, answer: **can this rule fire on code that works
today?** If it can, it does not belong in WM1 at warning severity. The
choices, in order of preference:

- Narrow the rule until every hit is genuinely broken. Scoping to the cases
  that provably misbehave is better than a broad rule at a lower severity.
- Ship it as WM2 at `Info`, where it advises without failing a build.
- Ship it as WM3, disabled, if it fires across an entire codebase by design.

A rule that duplicates one that already exists is worse than no rule: two
descriptors on one span report twice, and the second reads as a distinct
problem. If the situation is already covered, tighten the existing rule
instead. Where the overlap is deliberate — a declaration-site rule beside a
call-site one — say so in the description and name the rules it sits beside.

**Done when** the rule has a tier that its worst plausible false positive
survives.

## Tier

| Tier | Factory | Category | Severity | Default | Holds |
| --- | --- | --- | --- | --- | --- |
| `WM1xxx` | `Bug` | Reliability | `Warning` | on | Code that throws, silently does nothing, or means the opposite of what it reads as |
| `WM2xxx` | `Idiom` | Usage | `Info` | on | Code that works but says it worse than the library's own vocabulary does; obsoletions |
| `WM3xxx` | `Migration` | Design | `Info` | **off** | Rules that fire on code with no relationship to this library, useful only while migrating onto it |

`RulesTests` enforces every column of that table by id prefix, so a descriptor
in the wrong tier fails the build rather than shipping wrong. Take the next
free id within the tier by reading the highest one currently in `Rules.cs`;
ids are never reused, even for a rule that is deleted.

A WM3 rule must live in its own analyzer class. `Microsoft.CodeAnalysis.Testing`
force-enables every diagnostic the analyzer under test supports, so a disabled
rule sharing a class with an enabled one fires in the other rule's tests.

## Voice

The three strings do three different jobs, and the commonest failure is
writing the same sentence into all of them.

**Title** — names the situation as a fact, in sentence case, with no trailing
period. Never an instruction, never a scold; an IDE renders it as a column
heading, not as advice.

> Poor: `"Don't use Unwrap"` — an instruction, and it names the fix instead of
> the problem.
>
> Strong: `"Unwrap throws on the failure case"` — the reader learns why before
> they have opened anything.

**Message format** — what is wrong *here*, and what to write instead. This is
the only string most readers ever see, so it must stand alone without the
title. Quote every type and member name in `'single quotes'`. One sentence
takes no trailing period; two or more are each terminated.

> Poor: `"Avoid this pattern"` — true of anything, actionable for nothing.
>
> Strong: `"'{0}' throws when there is no value. Prefer 'UnwrapOr',
> 'UnwrapOrElse', 'UnwrapOrDefault' or 'Match'."`

Name the replacement wherever one exists, concretely enough to type. Where a
code fix exists, the message describes what that fix produces.

**Description** — why the compiler permits the mistake at all, which is the
part a reader cannot deduce from their own code. This field carries the design
reasoning: that `Option` and `Result` are records, so null is accepted where
one is expected; that `Some` rejects `default(T)`; that a `Result`'s two
implicit conversions collapse when its type arguments match. It is also where
a deliberate scope limit and the rule's relationship to its neighbours belong,
so the next author does not widen it back out.

A `Migration` description opens with `"Disabled by default. Enable it while
…"`, then says what it over-reports on. That sentence is the only warning a
consumer gets before turning the rule on.

**Done when** the title reads as a fact, the message names a concrete
replacement, and the description explains something the message does not.

## Placeholders

`Diagnostic.Create` takes the message arguments positionally, and a count
mismatch is silent — a missing argument renders as a literal `{0}` in a
consumer's IDE, and no test catches it unless the expected message asserts the
substituted text. Keep the argument count small, and write the analyzer's
`ReportDiagnostic` call in the same change as the descriptor.

Fill type and member placeholders through `Semantics.Display`, not
`ToDisplayString()`. It uses `MinimallyQualifiedFormat` and strips the nullable
annotation, so messages read `Option<int>` rather than a fully-qualified name
with a stray `?`.

## Paired obligations

A descriptor alone does not ship. In the same change:

- [ ] Add the row to `AnalyzerReleases.Unshipped.md`. `RS2008` fails the build
      without it, and `src/**` treats warnings as errors. Use severity
      `Disabled` in that table for a WM3 rule. Move the row into `Shipped.md`
      under the version the PR title will compute *before* merging, not after —
      merging publishes. Changing a shipped rule's severity or default
      enablement is a `### Changed Rules` row in `Unshipped.md`, not an edit to
      the shipped table.
- [ ] Bump the tier count in `RulesTests.EveryTierIsPopulated`.
- [ ] Report the diagnostic from a `MonadAnalyzer` subclass.
      `EveryRuleIsSupportedByAnAnalyzer` fails on a descriptor no analyzer
      declares.
- [ ] Add the rule's section to the analyzer-rules page in the
      [docs repository](https://github.com/draekien-industries/docs), and link
      the two PRs. The help link is derived from the id, so a rule without a
      docs section ships a link to an anchor that does not exist.

## Gotchas

**The message is the contract the tests assert.** Analyzer tests match the
substituted message text, so editing a message format breaks every test that
expects it. That friction is intended — it is also why a vague message is cheap
to write and expensive to improve later.

**A `Warning` that no code fix resolves is a trap.** If the message tells a
consumer to restructure their type, they have to do it by hand on an upgrade
they did not choose. Either supply the fix or drop to `Info`.

**Obsoletions are WM2, not WM1.** Calling deprecated API works; it just will
not work in the next major. The message names both the replacement and the
version that removes it, matching the obsoletion attribute's own message.

**Do not localize.** Descriptors use raw strings, not a `.resx`. Adding
localizable resources for one rule leaves two conventions in one file.
