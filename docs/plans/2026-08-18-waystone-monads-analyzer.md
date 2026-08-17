---
title: Waystone.Monads analyzer
date: 2026-08-18
status: done
---

## Goal

Ship a Roslyn analyzer that flags misuse of `Option<T>` and `Result<TOk, TErr>`, and
nudges consumers toward the library's idioms, delivered inside the existing
`Waystone.Monads` package so every consumer gets it without opting in.

The library has three properties that make an analyzer worth building rather than
merely nice to have:

- `Option<T>` and `Result<TOk, TErr>` are abstract **records** — reference types — so
  `Option<int> x = null!;` and `default(Option<int>)` compile, and in a consumer with
  nullable disabled they do not even warn.
- `new Some<T>(value)` **throws `InvalidOperationException`** when the value equals
  `default(T)`. `Option.Some(0)` is a guaranteed runtime crash that compiles cleanly
  today. `Ok`/`Err` have no equivalent check, so this asymmetry is Option-only.
- `Option<T>`'s implicit conversion maps `default(T)` to `None`, so `Option<int> x = 0;`
  silently produces `None` rather than `Some(0)`.

## Decisions

| Decision | Choice |
| --- | --- |
| Delivery | Packed into the existing `Waystone.Monads` nupkg as analyzer assets. No opt-out property; consumers suppress through `.editorconfig` as with any analyzer. |
| Severity | Warning for definite misuse, info for idiom. A consumer on `TreatWarningsAsErrors` only breaks where their code genuinely throws or silently misbehaves at runtime. |
| Unwrap rule | Unconditional, clippy-style. Every `Unwrap`/`Expect` call is flagged regardless of guards, matching clippy's `unwrap_used` and `expect_used`. No dataflow analysis. |
| Adoption nudges | Scoped variants on by default at info; broad variants shipped `isEnabledByDefault: false` for teams mid-migration. |
| Rule IDs | `WM` prefix. `WM1xxx` misuse, `WM2xxx` idiom, `WM3xxx` migration. |
| Roslyn target | Single `netstandard2.0` assembly against `Microsoft.CodeAnalysis.CSharp` 4.8.0, at `analyzers/dotnet/cs`. No multi-targeting — nothing in the catalogue needs a post-4.0 API, and a `roslyn4.x` folder can be added later without breaking any consumer. |

Deliberately **not** built: any rule inferring that an `Option` is carrying an error
rather than a value. `Option<Error>` is a legitimate design — "an error, or no error" —
and a rule fighting it would be wrong more often than right.

## Rules

### Tier 1 — misuse, warning

| ID | Flags | Fix |
| --- | --- | --- |
| WM1001 | `Option.Some(x)` where `x` is a constant equal to `default(T)` — `0`, `false`, `'\0'`, `default(T)`, and the well-known aliases `Guid.Empty`, `DateTime.MinValue`, `TimeSpan.Zero`. Throws `InvalidOperationException` at runtime. | `Option.None<T>()` |
| WM1002 | `null` (including `null!`) assigned, returned or passed where `Option<T>` or `Result<TOk, TErr>` is expected. | `Option.None<T>()` for Option; none for Result — Ok or Err is not inferable |
| WM1003 | `default(Option<T>)`, `default(Result<,>)`, or a `default` literal converted to either. Yields `null`, so the next member access throws. | `Option.None<T>()` |
| WM1004 | A constant equal to `default(T)` implicitly converted to `Option<T>` — `Option<int> x = 0;`. Silently produces `None`. | Make it explicit: `Option.None<T>()` |
| WM1005 | A maybe-null value passed to `Option.Some` per the compiler's nullable flow state. Throws when it is null at runtime. | `Option.FromNullable(x)` |
| WM1006 | A `Result`-returning call whose value is discarded as an expression statement. The failure is silently dropped — Rust marks `Result` `#[must_use]` for this reason. | None |

Excluded from WM1001 and WM1004: non-constant expressions. `Option.Some(count)` cannot be
judged without dataflow and is left alone.

### Tier 2 — idiom, info

| ID | Flags | Fix |
| --- | --- | --- |
| WM2001 | `Unwrap`, `UnwrapAsync`, `UnwrapErr`, `UnwrapErrAsync`. | `UnwrapOr`, `UnwrapOrElse`, `UnwrapOrDefault`, `Match` |
| WM2002 | `Expect`, `ExpectAsync`, `ExpectErr`, `ExpectErrAsync`. Separate from WM2001 so a team can permit `Expect` — where the message documents an invariant — while forbidding `Unwrap`. | as WM2001 |
| WM2003 | A `throw` inside a member returning `Result<,>`, `Task<Result<,>>` or `ValueTask<Result<,>>`. Excludes rethrows, argument-validation exceptions (`ArgumentNullException`, `ArgumentException`, `ArgumentOutOfRangeException` — a contract violation is a panic, not a domain failure), and throws inside a lambda passed to `Result.Try`/`Option.Try`, where throwing is the point. | `return Result.Err<TOk>(Error.FromException(...))` when `TErr` is `Error`; none otherwise |
| WM2004 | An `IsSome`/`IsOk` guard whose body unwraps the same instance. | `Match` or `Inspect` |
| WM2005 | `Map(...).Flatten()`. | `FlatMap(...)` |
| WM2006 | `IsSome && predicate` and `IsNone \|\| predicate`. | `IsSomeAnd(predicate)`, `IsNoneOr(predicate)` |
| WM2007 | `UnwrapOr(default)` / `UnwrapOr(default(T))`. | `UnwrapOrDefault()` |
| WM2008 | An `Option` or `Result` compared to `null`. | `IsNone` / `IsErr` |
| WM2009 | `Option<Option<T>>` as a declared type. | None — the fix crosses the signature |
| WM2010 | `Result<T, T>` — identical type arguments make both implicit conversions ambiguous, so the operators become unusable. | None |
| WM2011 | `Some<T>`, `None<T>`, `Ok<,>` or `Err<,>` used as a declared type rather than the base. | Widen to `Option<T>` / `Result<,>` |
| WM2012 | A member returning `T?` in a type that already exposes at least one `Option`- or `Result`-returning member. | Change the return type to `Option<T>` and wrap returns with `Option.FromNullable` |
| WM2013 | An `Option`-returning call whose value is discarded. | None |

### Tier 3 — migration, disabled by default

| ID | Flags |
| --- | --- |
| WM3001 | Any member returning a nullable type — suggest `Option<T>`. |
| WM3002 | Any `throw` statement — suggest `Result<TOk, Error>`. |

Shipped with `isEnabledByDefault: false`, raised through
`dotnet_diagnostic.WM3001.severity = suggestion`. These fire across a consumer's whole
project rather than their monad usage, which is why they are dormant by default.

## Steps

Three PRs, each with its own paired documentation PR. Phasing keeps the first PR about
the delivery mechanism, where the risk actually is.

### Phase 1 — scaffolding and tier 1

1. `src/Waystone.Monads.Analyzers/` targeting `netstandard2.0`, with
   `EnforceExtendedAnalyzerRules` (RS1038) and `IsPackable=false` — it ships inside the
   Monads package, not as its own.
2. Reference `Microsoft.CodeAnalysis.CSharp` 4.8.0 and `Microsoft.CodeAnalysis.Analyzers`,
   both `PrivateAssets=all`. Add versions to `Directory.Packages.props`.
3. Add `AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` (RS2008 fails the
   build without them, and `TreatWarningsAsErrors` is on for `src/**`).
4. Pack the analyzer into the Monads nupkg at `analyzers/dotnet/cs`, and have
   `Waystone.Monads` consume its own analyzer via `OutputItemType=Analyzer` with
   `ReferenceOutputAssembly=false`.
5. Resolve library types by metadata name — `Waystone.Monads.Options.Option`1` and
   friends — and bail when absent. The analyzer must not reference `Waystone.Monads`, or
   step 4 becomes a project cycle.
6. `test/Waystone.Monads.Analyzers.Tests/` targeting `net8.0` only. Analyzer tests have no
   reason to run the five-framework matrix, and CI's solution-wide
   `dotnet test --framework net8.0` picks the project up with no workflow change.
7. Implement WM1001–WM1006 with their fixes.

### Phase 2 — tier 2

Implement WM2001–WM2013. Move tier 1 entries from `Unshipped` to `Shipped` as part of the
release that carries them.

### Phase 3 — tier 3

Implement WM3001–WM3002, disabled by default, and document the opt-in.

## Done when

- Every rule has a test asserting it fires, a test asserting it does not fire on the
  legitimate shape closest to it, and — where a fix exists — a fix test.
- `dotnet test` passes on all five target frameworks. The Monads test project is
  unaffected, but the library now builds under its own analyzer, so a tier 1 hit in
  `src/` breaks the build. That is intended.
- `dotnet pack -c Release` produces a `Waystone.Monads` nupkg containing
  `analyzers/dotnet/cs/Waystone.Monads.Analyzers.dll`, verified by unzipping it.
- A scratch consumer project referencing the packed nupkg from a local feed shows the
  diagnostics, including one on a nullable-disabled project, and shows nothing on the
  tier 3 rules until they are enabled.
- The documentation PR carries the rule catalogue as a page with a `SUMMARY.md` entry, and
  merges after this repository's PR.

## Risks

**The first PR changes the contents of a published package.** Every consumer gets
diagnostics on their next upgrade. Tier 1 is warning-severity, so anyone on
`TreatWarningsAsErrors` breaks where a rule fires — which is the intent, since those rules
mark code that throws or silently misbehaves, but it is still a build break arriving from a
version bump. The PR title must be `feat` so the minor bump signals a change worth reading.

**The analyzer test harness may not sit cleanly on xunit v3.** The
`Microsoft.CodeAnalysis.*.Testing.XUnit` packages target xunit v2, while this repository
uses `xunit.v3`. The framework-agnostic `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing`
package with `DefaultVerifier` should avoid the dependency, but this needs proving in step 6
before the rules are written. If it does not work, the fallback is hand-rolled verification
over `CSharpCompilation` — more code, no new dependency.

**WM1005 depends on the compiler's nullable flow state**, so it goes quiet in a consumer
with nullable disabled. That is the population most exposed to the bug. Accepted: the
alternative is a nullability analysis of our own.

**WM2012's fix changes a signature**, which cascades to callers the fix cannot see. It is
offered as an IDE action at info severity, never applied in bulk.

## Outcome

Shipped in one PR off `feature/monads-analyzer`. All 21 rules are implemented across 10
analyzer classes, with 135 tests in `test/Waystone.Monads.Analyzers.Tests`.

The harness risk did not materialise: `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` with
`DefaultVerifier` runs on xunit v3 unchanged, so no hand-rolled verification was needed. Two
things about it are worth knowing and are recorded in `AGENTS.md`: it force-enables every
supported diagnostic, so `isEnabledByDefault: false` is unobservable through it, and the
testing packages resolve their own Roslyn floor to 1.0.1 unless a direct reference lifts it.

Deviations from the plan:

- **Fixes ship for 11 rules, not for every rule the table names one against.** WM1002 has no
  fix on `Result` (`Ok` or `Err` is not inferable, as planned), and WM1006, WM2003, WM2004,
  WM2006, WM2009, WM2010, WM2012, WM2013, WM3001 and WM3002 report without one — each either
  changes a signature and cascades to callers the fix cannot see, or has no single correct
  rewrite.
- **WM2006 is narrower than described.** It requires the unwrap to be on the same instance as
  the state check in the right operand of the `&&`, rather than anywhere in the expression.
- **WM3001 sits in its own analyzer class**, `NullableReturnAnalyzer`, because the test
  harness force-enables it and it otherwise fired inside WM2012's tests.
- **The library's own source produces one WM2005 hit** — `FlatMap` is defined as
  `Map(...).Flatten()`, which is exactly the shape the rule flags. It is info severity and
  unavoidable, so it stands.
- **`null` in an argument position now takes its nullability from the parameter**, not from an
  enclosing member's return type. Without that, `string? Caller() => Take(null!);` was
  suppressed by the `string?` return annotation.
