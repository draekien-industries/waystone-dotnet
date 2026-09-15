---
title: Analyzer rules for composable monadic functions
date: 2026-09-15
status: active
---

## Goal

Ship three `WM2xxx` idiom rules that push a consumer's monad code toward functions that
can be named, tested and reused on their own: no pyramids of nested chain lambdas, no
`AndThen` doing `Map`'s job, and no side effect hidden inside a projection.

| Concept | Rule |
| --- | --- |
| Declarative style | `WM2025` — a chain call nested past a configurable depth |
| Function composition | `WM2026` — `AndThen` that only lifts its projection |
| Pure functions | `WM2027` — mutation of outside state inside a projecting lambda |

Two of the five concepts get no rule, on purpose:

- **Higher-order functions.** `IDE0200` already reports `o.Map(x => Net(x))` and offers
  `o.Map(Net)`, with the guards a rule of ours would have had to build — same parameter
  count and order, no side effects, explicit type arguments on a generic method, and
  only one applicable method in the group. Where the compiler already reports a
  pattern, ship the fix and not the rule.
- **Immutability.** Whether a projected value can be mutated at all is the type
  author's decision, and a record settles it. A rule reporting
  `o.Map(x => { x.Name = name; return x; })` on a deliberately mutable class argues with
  a choice already made.

The gap that leaves: mutation of a projecting lambda's *own parameter* goes unreported.
`WM2027` covers mutation of state from outside the lambda and stops there. That is the
consequence of dropping the immutability rule rather than an oversight — flip it by
widening `WM2027`'s target set, not by adding a rule.

Purity is the remaining concept C# cannot verify from a signature, and `WM2027` does not
try. It reports *observable* mutation at the call site and never claims a called method
is pure, so an opaque call such as `o.Map(x => _pricing.Recalculate(x))` stays silent.
That floor keeps the family inside the analyzer package: no `[Pure]` attribute to ship,
no `PublicAPI` entry, no new symbol that can only ever be deprecated.

## Decisions

| Decision | Choice |
| --- | --- |
| Ids | `WM2025`–`WM2027`, reallocated contiguously after the cut. Nothing has shipped, so no consumer suppression can be redirected. Read the highest id out of `Rules.cs` before implementing anyway. |
| Tier | Idiom. All three ship `Info`, per the policy that only `WM1xxx` ships at warning. |
| Purity basis | Observable mutation only. No `[Pure]` attribute is read, defined or shipped. |
| `WM2025` trigger | Nesting depth, not capture. The capture case is already `WM2017`'s. |
| `WM2025` threshold | Tunable, default 2. `dotnet_code_quality.WM2025.max_chain_depth`. |
| `WM2025` fix | None. The extracted method's name, parameters and location are not derivable from the source, and every IDE ships an extract-method refactoring. |
| `WM2026` fix | Yes — the rewrite to `Map` is determined. |
| `WM2027` fix | None. Moving an effect into `Inspect` changes when it runs. |
| Package | `Waystone.Monads.Analyzers`, shipped inside `Waystone.Monads` as every other `WM` rule is. No new package. |

Deliberately **not** built: a rule rewriting a nested chain into `Waystone.Monads.Linq`
query syntax. The analyzer resolves the library by metadata name and never references
it, so it cannot know whether `Waystone.Monads.Linq` is referenced; a fix emitting
`from … in …` against an absent package produces source that does not compile.

## Considered and cut

| Rule | Reason |
| --- | --- |
| Pass the method group — `o.Map(x => Net(x))` → `o.Map(Net)` | `IDE0200` reports it already, with better guards. |
| Fuse adjacent projections — `o.Map(Net).Map(Round)` | Saves one intermediate monad and costs the reader the two names that said what each step did. Argues against the rest of the family. |
| Return a new value rather than a mutated one | Immutability is the type's job; a record settles it. |

## The option key

No analyzer in this repository reads `AnalyzerConfigOptions` today. `WM2025` is the
first, so the reading is new machinery rather than a reuse.

```csharp
private const int DefaultMaxChainDepth = 2;

private static int MaxChainDepth(SyntaxNodeAnalysisContext context) =>
    context.Options.AnalyzerConfigOptionsProvider
           .GetOptions(context.Node.SyntaxTree)
           .TryGetValue("dotnet_code_quality.WM2025.max_chain_depth", out var raw)
 && int.TryParse(raw, out var parsed)
 && parsed > 0
        ? parsed
        : DefaultMaxChainDepth;
```

`dotnet_code_quality.` is the prefix Microsoft's own `CAxxxx` rules use for a
rule-scoped option, so a consumer who has configured one of those already knows the
shape. An unparseable or non-positive value falls back to the default in silence — an
analyzer that throws on a consumer's typo takes their build down, and there is no
diagnostic to report it through.

The presets carry the severity and nothing else.
`PresetTests.ThePresetSetsSeveritiesAndNothingElse` fails on any key that is not a
`dotnet_diagnostic.` severity, and the option's default is already what a preset would
have written:

```ini
# src/Waystone.Monads/build/strict.globalconfig
dotnet_diagnostic.WM2025.severity = warning
```

## Rules

### WM2025 — Extract a nested monad chain

Reports a chain call that sits `max_chain_depth` lambdas deep. Depth counts enclosing
*chain* lambdas and adds one for the call itself: a chain call at statement level is
depth 1, the same call inside one chain lambda is depth 2.

```csharp
// depth 2 — reports at the default
FindUser(id).AndThen(user => FindAccount(user.AccountId).Map(a => a.Label));

// depth 1 — silent
FindUser(id).AndThen(FindAccountFor).Map(a => a.Label);
```

Report the *shallowest* node at or over the threshold and skip its descendants. A
three-deep chain has two qualifying nodes and is one problem; reporting both puts two
squiggles on one expression and makes the count look like the severity.

Only a lambda passed to a monad member counts toward depth. A chain inside a
`List<T>.Select` lambda, or inside any other delegate-taking method, is depth 1.

Skip nodes inside a query expression. `Waystone.Monads.Linq` desugars `from … from …`
into chained `SelectMany` calls whose lambdas the author never wrote, and reporting
those blames generated nesting. Test with `ITranslatedQueryOperation` present in the
ancestry.

### WM2026 — An AndThen that only lifts is a Map

The C# reading of clippy's `bind_instead_of_map`.

```csharp
o.AndThen(x => Option.Some(Net(x)));                 // reports → o.Map(x => Net(x))
r.AndThen(x => Result.Ok<decimal, Error>(Net(x)));   // reports → r.Map(x => Net(x))
o.AndThen(x => x.Total > 0                           // silent — branches on the value
    ? Option.Some(x)
    : Option.None<Order>());
```

The rewrite is behaviour-preserving. `Option.Some(null!)` throws `ArgumentNullException`
from `Some`'s constructor and `Map` throws `ArgumentNullException` from
`Option.SomeOrThrow`; only the message differs, and `Map`'s names the delegate. Note
that `Some` guards **null alone** — `Option.Some(0)` is `Some(0)`, not a throw — so the
two forms do not diverge on a value type's default either.

Boundary with `WM2005`: that rule reports `Map(…).Flatten()` and points at `AndThen`.
This one points the other way over a different shape. They cannot both fire on one call.

The fix leaves a lambda — `o.Map(x => Net(x))` — which `IDE0200` then reduces to
`o.Map(Net)` where the conversion is safe. Two fixes rather than one, and the second is
already in the box.

### WM2027 — Do not mutate inside a projection

Reports an assignment, compound assignment, `++`/`--`, or a call to a known-mutating
collection member, targeting state from outside the lambda, inside a lambda passed to a
*projecting or filtering* member — `Map`, `AndThen`, `Filter`, `MapOr`, `MapOrElse`,
`UnwrapOrElse` and their `*Async` siblings.

```csharp
int seen = 0;
o.Map(x => { seen++; return x.Total; });     // reports
o.Filter(x => { _audit.Add(x); return x.IsOpen; });  // reports

o.Inspect(x => _log.Saw(x));                 // silent — the member built for effects
o.Map(x => { var t = x.Total; t += 1; return t; });   // silent — local to the lambda
o.Map(x => { x.Name = name; return x; });    // silent — the parameter is the type's own business
```

`Filter` is the worst case and worth naming in the description: the predicate runs only
on `Some`, so the effect is conditional on a state the reader is not looking at.

No code fix. `Inspect` runs on a different schedule than the projection it is lifted out
of, so the rewrite is not determined.

## Shared obligations

Each of the three carries all of these, and none is checked end to end by the build:

- A row in `AnalyzerReleases.Unshipped.md`, or `RS2008` fails the build.
- Two preset rows: `suggestion` in `recommended.globalconfig`, `warning` in
  `strict.globalconfig`. `PresetTests` fails on a descriptor with no entry and on an
  entry at the wrong tier's severity. Read
  [contexts/analyzer-severity-presets.md](../contexts/analyzer-severity-presets.md)
  first.
- A descriptor written through the `writing-diagnostic-descriptors` skill. Why the
  pattern is a problem goes in the `description`; why the rule is scoped as it is goes
  in the XML doc on the field.
- Rejection on type identity as the *first* clause of any `OperationKind.Invocation`
  action. All three run on every call site in every consumer's compilation. Nothing
  fails if the cheap reject moves behind the expensive one, and no test catches it.
- `MonadSymbols.IsMonadInvocation` for receiver identification. `IsExtensionMethod` is
  not reliable here — the library's extensions are C# 14 `extension` blocks.

Every descriptor's `HelpLinkUri` points at
`https://draekien-industries.wpei.me/analyzers/idioms`, so the family owes a
documentation PR in `draekien-industries/docs` even though it adds no public API. That
PR merges *after* this repository's, and a new page needs its `SUMMARY.md` entry in the
same commit.

## Steps

One PR per rule, stacked bottom to top with `gh stack`.

| Layer | Issue | Rule |
| --- | --- | --- |
| 1 | DRA-222 | `WM2025` nested chain |
| 2 | DRA-224 | `WM2026` lifting `AndThen` |
| 3 | DRA-225 | `WM2027` mutation in a projection |

Parent: DRA-221. Each issue carries its own fire and silent cases. Each layer's codecov
patch check is the one most likely to fail — cover the silent cases as well as the
reporting ones.

## Done when

All three rules are shipped in `AnalyzerReleases.Shipped.md` under a release heading,
both presets carry six new rows, the documentation PR for the idioms page is merged,
and this plan is `status: done`.
