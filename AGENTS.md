# Waystone.Net

## Purpose

A collection of small, independently published C# libraries reused across
draekien-industries projects. The packages share a repository and a version
number but are otherwise unrelated — there is no unifying thesis, and a change
in one package implies nothing about the others.

## Conventions

**The commit type determines the published version.** GitVersion parses commit
subjects, so `feat` bumps minor, `fix` and `perf` bump patch, and a `!` before
the colon forces a major bump on *any* type. PRs are squash-merged, so the PR
title becomes the version-determining subject — a `!` in a PR title ships a
major release. Use `!` only when something is actually being removed.

**Deprecate; never remove.** Public API is obsoleted with a message naming both
its replacement and the version that removes it, and removed only in the next
major. Deleting public API outright is not an option, however small the change
looks.

**Deprecations are tracked in GitBook, not here.** The published documentation
carries the Deprecations page, and it is the source of truth for what is going
away and when. Do not reintroduce a `BREAKING_CHANGES.md` — a second list drifts
from the first. The documentation source lives in the
[draekien-industries/docs](https://github.com/draekien-industries/docs)
repository.

**A public API change needs a paired documentation PR.** Open a PR in
[draekien-industries/docs](https://github.com/draekien-industries/docs) covering
the change, and link the two PRs to each other so a reviewer can see both at once.
This applies when you add, change, obsolete or remove a public type or member, and
when you change the behaviour or the default of one that is already documented.

**Merge this repository's PR first, then the documentation PR.** Merging here
publishes to NuGet; merging there syncs GitBook. In that order the documentation
describes a version that consumers can already install. Reverse the order and the
published documentation describes API that does not exist yet.

If a code PR is closed without merging, close its documentation PR too. A docs PR
left open against abandoned API is worse than no docs PR — someone will eventually
merge it.

Changes that leave the public surface untouched need no documentation PR: internal
refactors, tests, CI, and build configuration. Write the documentation the way that
repository asks — read its `AGENTS.md` first, since new pages there need a
`SUMMARY.md` entry in the same commit or GitBook never shows them.

**Comment only public API surface.** XML doc comments on public members; no
explanatory comments inside method bodies. `CS1591` is suppressed, so the build
will not tell you when public docs are missing.

**Work flows `feature/*` → `main`.** There is no `develop` branch and there will
not be one, despite the GitFlow configuration in `GitVersion.yml`.

## Constraints

**Merging to `main` publishes to NuGet.org.** `release.yml` packs and pushes on
every push to `main` under `src/**`, then tags a GitHub release. There is no
staging gate and a published version cannot be withdrawn, so "merge and see"
is not available.

**`netstandard2.0` cannot be raised** for `Waystone.Monads`; PolySharp supplies
the newer language features. Consumers on older frameworks depend on it, which
is why net472 and net481 sit in the test matrix.

**One version covers every package.** GitVersion computes a single number
applied to all of them, so a change to one package bumps and republishes the
rest. Packages cannot be versioned independently.

**The analyzer ships inside the `Waystone.Monads` package.**
`Waystone.Monads.Analyzers` and `Waystone.Monads.Analyzers.CodeFixes` are
`IsPackable=false` and are packed into `analyzers/dotnet/cs` of the Monads nupkg
by the `PackMonadAnalyzers` target. Every consumer therefore gets the rules on
upgrade with no opt-out beyond `.editorconfig`, which is why `WM1xxx` are the only
rules allowed to ship at warning severity — a rule that fires on working code
breaks a build somebody did not ask to change.

**The analyzer targets Roslyn 4.8 and must not reference `Waystone.Monads`.** It
resolves the library's types by metadata name through `MonadSymbols.TryCreate` and
goes silent when they are absent. A project reference would make the library's own
consumption of its analyzer a build cycle.

## Gotchas

**CI runs one target framework.** Both workflows call
`dotnet test --framework net8.0`, while the Monads tests target five frameworks
on Windows. A break on net472, net481, or net10.0 passes CI untouched — run the
full matrix locally before opening a PR.

**`MonadOptions.Global` is a process-wide mutable singleton.** A test that
mutates it and then asserts on it will flake against tests in other xUnit
collections running in parallel; this produced a roughly 1-in-3 failure rate
before it was diagnosed. Use `MonadOptions.BeginScope` in tests so the override
is confined to the current asynchronous flow.

**`CS0618` is suppressed** in `Waystone.Monads.csproj`, so calling your own
obsoleted API raises no warning in this repository. The build will not find call
sites for you after a deprecation — search for them.

**Reqnroll binds step definitions across the whole test assembly.** The
`Specs/Options/Steps` and `Specs/Results/Steps` folders scope nothing, so step
text has to be unique across the project and a step class binds happily from the
wrong folder. Two classes sat in the wrong area for a long time because nothing
complains. When a step is not found, the folder is never the reason.

**A step that switches on a string argument needs a `default` that throws.**
Without one, an unmatched value runs no assertion and the scenario passes. This
hid three no-op assertions in the Result specs.

**The library's extensions are C# 14 `extension` blocks, so `IsExtensionMethod`
is not a reliable test in an analyzer.** The compiler emits a compatibility static
method that older Roslyn sees as a classic extension, so a rule keyed on
`IsExtensionMethod` passes its tests and then misses real call sites on a modern
consumer's compiler. Identify the receiver instead — `MonadSymbols.IsMonadInvocation`
falls back to the type of the expression before the dot.

**The analyzer tests run on Roslyn 5.6 while the analyzer builds against 4.8.**
That mismatch is deliberate: it is the forward-compatibility case every consumer
is in. Both versions are pinned in the test project with `VersionOverride`, and
the testing packages resolve their own Roslyn floor to 1.0.1 unless a direct
reference lifts it.

**`Microsoft.CodeAnalysis.Testing` force-enables every diagnostic the analyzer
under test supports**, so `isEnabledByDefault: false` cannot be observed through
it and a disabled rule fires in tests that do not expect it. Assert the default on
the descriptor instead — `RulesTests` does — and keep a disabled-by-default rule in
its own analyzer class so it does not pollute another rule's tests.

**A new rule needs an `AnalyzerReleases.Unshipped.md` entry in the same change.**
RS2008 fails the build without one, and `src/**` builds with
`TreatWarningsAsErrors`. Use severity `Disabled` in that table for a rule that
ships off.

**A `null` literal's `ConvertedType` is the monad in a comparison too.** A rule
keyed on it fires on `option == null` and `option is null` as readily as on an
assignment, so `WM1002` double-reported alongside `WM2008` at warning severity and
its code fix produced `option is Option.None<int>()`, which does not compile.
`NullAndDefaultAnalyzer.IsNullTest` excludes the comparison and pattern positions.

**`UnwrapAwaitable` does not see through `ConfigureAwait`.** It knows `Task<T>` and
`ValueTask<T>` only, and this library awaits with `.ConfigureAwait(false)`
everywhere, so a rule that unwraps the inner call's type goes quiet on exactly the
style the library teaches. Read `IAwaitOperation.Type` instead when there is an
await.

**Move the release-tracking row to `Shipped.md` before merging, not after.**
Merging publishes, and there is no separate release step that would move it later,
so a row left in `Unshipped.md` is wrong from the moment the PR lands. File it
under the version GitVersion will compute from the PR title.

## Documentation

Agent-facing documentation lives in `docs/`. Read [docs/AGENTS.md](docs/AGENTS.md) before reading or writing anything there.
