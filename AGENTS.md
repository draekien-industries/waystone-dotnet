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
title becomes a version-determining subject — a `!` in a PR title ships a
major release. Use `!` only when something is actually being removed.

**Build a stack with `gh stack`, never by hand.** `gh stack init <bottom> … <top>`
takes the branches bottom to top, adopting ones that already exist and creating
the rest, and `gh stack submit` then pushes them, repoints each PR's base onto the
one below it, and registers the stack on GitHub. Do the same thing by passing
`--base` to `gh pr create` and you get the right base branches and *no stack*:
reviewers see unrelated PRs with no chain, `gh stack view` shows nothing, and
`gh stack merge` — the only supported way to land one — has nothing to merge.

`gh stack submit` opens an editor that an agent cannot drive, so pass `--auto`,
which is also what a non-interactive terminal falls back to. Note that `--auto`
creates *new* PRs as drafts unless you add `--open`.

Adopting a hand-built chain after the fact does work: `gh stack init` reports
"Found PRs for N of N branches" and `gh stack submit --auto` reports each one "up
to date" and links them without creating duplicates. That is a repair, not a
route — run `gh stack init` before opening the first PR.

**A stack contributes every one of its PR titles.** `gh stack merge` squashes each
PR in the stack separately, so an eleven-PR stack lands eleven commits and
GitVersion reads all eleven subjects. It applies one increment for the highest bump
among them, so ten `feat` commits give a single minor bump rather than ten. The trap
runs the other way: a `!` in *any* title in the stack takes the whole release major,
including a mid-stack PR nobody was thinking of as the release. Read the titles
together before merging, not one at a time as you open them.

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

**`CS0618` is no longer suppressed** in `Waystone.Monads.csproj`. It was, until
removing `FlatMap` in v6 left the library with no internal call sites of its own
obsoleted API. `src/**` builds with `TreatWarningsAsErrors`, so obsoleting a
member the library still calls now fails the build instead of passing silently.
Point those call sites at the replacement in the same change, or the deprecation
is not finished.

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

**An internal constructor does not close a `record` hierarchy.** Records get a
compiler-synthesized copy constructor, and CS8878 requires it to be `public` or
`protected` on an unsealed record — declaring it `private protected` does not
compile. `protected` reaches a derived type in another assembly, so an outside
record closes over the hole with `public Evil(Option<T> o) : base(o)`, which was
verified against the built package rather than reasoned about. What actually
closes both hierarchies is the `internal abstract OnlyThisAssemblyMayDerive` on
`Option<T>` and `Result<TOk, TErr>`: an outside type cannot override a member it
cannot see, so it fails CS0534 with no way out. `ClosedHierarchyTests` in the
analyzer test project holds the two regression cases, and it lives there because
`Waystone.Monads.Tests` has `InternalsVisibleTo` and would therefore compile a
derived type happily, proving nothing.

**A `branches` filter on `pull_request` stops mid-stack checks, whatever the docs
say.** GitHub's stacked-pull-request documentation states that workflows trigger as
if each PR in the stack targeted the base of the stack, and that no workflow changes
are required. That is not what happens. With `branches: [main]` on the trigger, only
the bottom PR of an eleven-PR stack ran build and test — the other ten target their
parent branch and were filtered out. The filter is gone from `pull-request.yml` and
should not come back. Since the checks are required by the `main` ruleset, a PR that
never ran one can never satisfy it, so this blocks the entire stack rather than
failing visibly.

**A stack cancels its own checks without a per-PR concurrency group.** `gh stack
submit` pushes every branch at once. A `concurrency.group` that does not vary by PR
puts all of them in one group, and `cancel-in-progress` then kills every run but the
last to start. The group is keyed on `github.event.pull_request.number` for that
reason.

**`codecov/patch` counts every line the diff touches, not the lines that added
logic.** Stripping a nullable annotation across a dozen untested async overloads
puts all of their untested lines in the patch, so a change that added no behaviour
at all fails the check. It is a required check, so it blocks the merge. Close the
test gap rather than moving the threshold — the gap it found in 5.4.0 was real.

**A PR that touches no filtered path cannot satisfy the required checks.** The
`main` ruleset requires `codecov/patch`, `Calculate Version` and `Build and run
tests`, and all three come from `pull-request.yml`, which has a `paths` filter. A
documentation-only PR matches nothing in it, so the workflow never runs, the three
checks never report, and the PR sits `BLOCKED` with nothing pending to wait for.
The ruleset grants `OrganizationAdmin` a standing bypass, so these land with
`gh pr merge --admin`. That is the accepted trade rather than an oversight —
widening the filter would run the five-framework matrix over prose. Do not wait on
checks for a docs-only PR; nothing is coming.

**Move the release-tracking row to `Shipped.md` before merging, not after.**
Merging publishes, and there is no separate release step that would move it later,
so a row left in `Unshipped.md` is wrong from the moment the PR lands. File it
under the version GitVersion will compute from the PR title.

## Documentation

Agent-facing documentation lives in `docs/`. Read [docs/AGENTS.md](docs/AGENTS.md) before reading or writing anything there.
