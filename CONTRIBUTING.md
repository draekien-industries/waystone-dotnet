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
3. Add tests. `Waystone.Monads.Tests` uses xUnit v3 with Shouldly, and covers
   behaviour-style scenarios with Reqnroll `.feature` files. The generated
   `.feature.cs` files are committed, so build after editing a `.feature` and
   commit both.
4. Run the tests — all target frameworks, not just the default:

   ```sh
   dotnet test
   ```

   **This matters.** CI runs `dotnet test --framework net8.0` only, while the
   Monads tests target net8.0, net9.0, net10.0 and, on Windows, net472 and
   net481. A break on any framework other than net8.0 will pass CI and reach
   NuGet. Running the full matrix locally is the only place that gets caught.

5. Commit and push, then open a Pull Request.

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

Note that `CS0618` is suppressed in `Waystone.Monads.csproj`, so the build will
not flag call sites of your own obsoleted API. Search for them yourself.

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
