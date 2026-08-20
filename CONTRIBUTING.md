# Contributing

Thanks for taking an interest. Waystone.Net is a collection of small C# class
libraries published to NuGet.org, so the bar for changes to public API is higher
than the size of this repository might suggest — see [Releasing](#releasing)
below.

## Getting set up

1. Clone the repository, e.g. `gh repo clone draekien-industries/waystone-dotnet`
2. Open `Waystone.Net.sln` in your IDE of choice. The SDK version is pinned in
   `global.json`.
3. `dotnet restore` — package versions are managed centrally in
   `Directory.Packages.props`, so do not add a `Version` attribute to a
   `PackageReference`.

## Making a change

1. Branch off `main`: `git checkout -b feature/my-cool-new-feature`. There is no
   `develop` branch.
2. Code up a storm.
3. Add tests. `Waystone.Monads.Tests` uses xUnit v3 with Shouldly. Unit tests
   sit at the top level, mirroring the namespace they cover. Reqnroll
   behaviour tests live under `Specs/`, with a `Features/` and `Steps/` pair
   per area.

   The generated `.feature.cs` files are committed, so build after editing a
   `.feature` and commit both.

   Features carry an `@option` or `@result` tag, which Reqnroll maps to an
   xUnit trait. To run one kind on its own:

   ```sh
   dotnet test --filter "Category=option|Category=result"    # behaviour only
   dotnet test --filter "Category!=option&Category!=result"  # unit only
   ```

   Scenarios that differ only in a value or a variant belong in a
   `Scenario Outline` with an `Examples` table, not in copied scenarios.
   Shared scenario state goes through the injected `SpecContext` rather than
   Reqnroll's `ScenarioContext`. Note that Reqnroll binds steps across the
   whole assembly, so a `Steps` folder scopes nothing — step text has to be
   unique repository-wide.
4. If you are adding or changing an analyzer rule, see
   [Analyzer rules](#analyzer-rules) below.
5. Run the tests — all target frameworks, not just the default:

   ```sh
   dotnet test
   ```

   **This matters.** CI runs `dotnet test --framework net8.0` only, while the
   Monads tests target net8.0, net9.0, net10.0 and, on Windows, net472 and
   net481. A break on any framework other than net8.0 will pass CI and reach
   NuGet. Running the full matrix locally is the only place that gets caught.

6. Commit and push, then open a Pull Request.

## Analyzer rules

`Waystone.Monads.Analyzers` ships **inside** the `Waystone.Monads` package, so a
new rule reaches every consumer on their next upgrade. There is no opt-out beyond
`.editorconfig`.

Severity follows the tier, and the tier is not a matter of taste:

| Tier | Severity | Admits |
| --- | --- | --- |
| `WM1xxx` | Warning | Code that throws or silently misbehaves at runtime |
| `WM2xxx` | Info | Idiom — correct code that reads better another way |
| `WM3xxx` | Disabled | Migration aids that fire across a whole codebase |

**Nothing that fires on working code may ship at warning.** A consumer building
with `TreatWarningsAsErrors` gets a broken build from a version bump they did not
ask for, and the rule gets suppressed wholesale rather than read.

To add one:

1. Add the descriptor to `Rules.cs` through `Bug`, `Idiom` or `Migration`.
2. Add a row to `AnalyzerReleases.Unshipped.md` — RS2008 fails the build without
   it, and `src/**` treats warnings as errors. Use `Disabled` for a `WM3xxx` rule.

   Before merging, move the row into `AnalyzerReleases.Shipped.md` under the
   version the PR will publish, and leave `Unshipped.md` holding only its header.
   Merging publishes, so a row left in `Unshipped.md` is wrong the moment the PR
   lands, and this repository has no separate release step that would move it
   later. The version is whatever GitVersion computes from the PR title — a `feat`
   on top of `v5.2.0` publishes `5.3.0`.
3. Implement it on an existing analyzer, or a new one deriving from
   `MonadAnalyzer`. Resolve library types through the injected `MonadSymbols`;
   never add a project reference to `Waystone.Monads`.
4. Write three tests: it fires, it does not fire on the nearest legitimate shape,
   and — if it has a fix — the fix produces the expected source.
5. Add the misuse to `sample/Waystone.Monads.Analyzers.Sample` so the rule shows
   up in build output too.

Messages must satisfy RS1032: one sentence with no trailing period, or several
with one.

The analyzer builds against Roslyn 4.8 for reach, while its tests run on 5.6 —
the forward-compatibility case consumers are actually in. Do not assume the two
agree about extension methods; see the gotchas in [AGENTS.md](AGENTS.md).

## Commit messages

Use [Conventional Commits](https://www.conventionalcommits.org/). This is not a
style preference — GitVersion parses commit subjects to compute the published
version:

| Subject | Effect |
| --- | --- |
| `feat: ...` | Minor bump |
| `fix: ...` or `perf: ...` | Patch bump |
| Any type with `!` before the colon | **Major bump** |
| Anything else (`docs`, `chore`, `test`, `refactor`, ...) | No bump |

```sh
git commit -m 'feat(monads): add my cool new feature'
```

PRs are squash-merged, so **the PR title becomes the commit subject that
determines the version.** Give it the same care as a commit message, and reserve
`!` for changes that actually remove something.

## Changing public API

Any change to the public surface needs a matching PR in
[draekien-industries/docs](https://github.com/draekien-industries/docs). That
covers new types and members, changed behaviour, obsoletions and removals. Link
the two PRs to each other.

**Merge this repository's PR first, then the docs PR.** Merging here publishes to
NuGet; merging there syncs GitBook. That order means the documentation always
describes a version you can install. The other order documents API that does not
exist yet.

If your code PR is closed without merging, close the docs PR too.

### Deprecating

Public API is deprecated, never deleted:

1. Mark the old member `[Obsolete]` with a message naming its replacement and
   the version that removes it, so it reads the same in the IDE as it does in the
   documentation:

   ```csharp
   [Obsolete(
       "Use TryAsync instead. This overload will be removed in v6 of Waystone.Monads.")]
   ```

2. Add it to the Deprecations page in the [published documentation](https://draekien-industries.wpei.me/),
   in the same change. Record the version that deprecated it and the version that
   removes it, grouped under the owning package. The documentation is the source
   of truth for what is deprecated — this repository does not track it separately.
3. Remove it in the next major release, not before. When that major is cut, delete
   the members, move their entries into the release notes, and clear the
   Deprecations page for the next cycle.

`CS0618` is not suppressed in `Waystone.Monads.csproj` and `src/**` builds with
warnings as errors, so obsoleting a member the library still calls fails the
build. Point those call sites at the replacement in the same change.

Public members need XML doc comments. `CS1591` is suppressed, so nothing will
tell you when they are missing. Comments elsewhere are unwelcome — if a method
body needs explaining, it usually needs simplifying.

## Releasing

Merging to `main` publishes. `release.yml` calculates the version, packs every
package, pushes to NuGet.org, and tags a GitHub release — on any push to `main`
touching `src/**`. There is no staging gate and a published version cannot be
withdrawn.

All packages share one version number, so a change to one republishes all of
them. See [docs/adr/0001-publish-every-package-from-one-shared-version.md](docs/adr/0001-publish-every-package-from-one-shared-version.md)
for why.

## Working with an AI agent

[AGENTS.md](AGENTS.md) carries the conventions, constraints and traps of this
repository in the form agents read. If you change how this repository works,
change it there too. Agent-facing documentation — decision records,
explorations, plans — lives in [docs/](docs/AGENTS.md).
