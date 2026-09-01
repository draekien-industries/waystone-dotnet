# Waystone.Net

## Purpose

A collection of small, independently published C# libraries reused across
draekien-industries projects. The packages share a repository and a version
number but are otherwise unrelated — there is no unifying thesis, and a change
in one package implies nothing about the others.

## Area guidance

This file carries only what applies everywhere. Read the file for the area you are
working in — each is loaded when you touch files under it.

| Area | Covers |
| --- | --- |
| [src/Waystone.Monads](src/Waystone.Monads/AGENTS.md) | The public API baseline, naming and breakage rules, the closed hierarchies |
| [src/Waystone.Monads.Schema](src/Waystone.Monads.Schema/AGENTS.md) | The plural namespace, which hierarchies are closed, eager message rendering |
| [src/Waystone.Monads.Analyzers](src/Waystone.Monads.Analyzers/AGENTS.md) | Rule severity policy, Roslyn version targeting, analyzer testing |
| [src/Waystone.Monads.Shouldly.Analyzers](src/Waystone.Monads.Shouldly.Analyzers/AGENTS.md) | Where the assertion-migration rules ship and why not in the core package |
| [src/Waystone.Monads.SourceGenerators](src/Waystone.Monads.SourceGenerators/AGENTS.md) | The shipped error code generator contract and emission |
| [src/Waystone.SourceGenerators](src/Waystone.SourceGenerators/AGENTS.md) | The awaited-receiver generator contract and emission |
| [sample](sample/AGENTS.md) | Which samples are published, and the snippet regions the GitBook pages quote |
| [test](test/AGENTS.md) | Running the framework matrix, the shared test configuration, shared mutable state |
| [.github](.github/AGENTS.md) | Workflow triggers, required checks, coverage gates |
| [docs](docs/AGENTS.md) | Where a document goes and how it is written |

## Setup

Run this once per clone, or neither hook below fires:

```
git config core.hooksPath .githooks
```

**In a git worktree, set it again with `--worktree`.** Some tooling writes an
*absolute* `core.hooksPath` into a new worktree's config, pointing at the main
checkout, so that checkout's hooks run against your worktree's files. Repair it with
`git config --worktree core.hooksPath .githooks`; a relative value resolves inside
whichever tree the hook is running in.

`commit-msg` rejects a subject that is not a conventional commit, since GitVersion
reads it and a misspelled type silently publishes the wrong version. `pre-commit`
rejects NUL bytes in text sources — git treats such a file as binary
and silently stops diffing it, and nothing in the build notices. `pre-push` runs
the full framework matrix, checks that no release-tracking rows are still
unshipped, and checks that no published page has drifted from the sample it quotes.
None is a substitute for a required check, since a clone without `core.hooksPath`
set has none of them.

The snippet check needs a checkout of
[draekien-industries/docs](https://github.com/draekien-industries/docs), which it finds
without any path being written down — the candidates and their order are in
[tools/Waystone.DocSnippets/README.md](tools/Waystone.DocSnippets/README.md). Finding
none, it says so and lets the push through: a contributor without that clone is not
blocked, but nothing is guarding the pages either.

## The solution

**`Waystone.Net.slnx` is edited by hand.** It is XML, a project is one `<Project
Path="..." />` line inside the `<Folder>` it belongs to, and adding one is a
two-line diff you can read.

**Do not reach for `dotnet sln add`.** On the SDK pinned here it rewrites far more
than the solution: run once, it dropped nine projects from the solution and
stripped the `ProjectReference` elements out of eighteen `.csproj` files, which
turns into a few hundred `CS0246`s and looks nothing like a solution problem.

**Two projects carry `<Build Project="false" />`, and it is load-bearing.** Both
`Waystone.Monads.PreviousMajor*` samples compile against a *previous* major on
purpose, so the root build has to skip them while an IDE still opens them. See
[sample/Waystone.Monads.PreviousMajor.Sample/README.md](sample/Waystone.Monads.PreviousMajor.Sample/README.md).

## Versioning

**The commit type determines the published version.** GitVersion parses commit
subjects, so `feat` bumps minor, `fix` and `perf` bump patch, and a `!` before the
colon forces a major bump on *any* type. PRs are squash-merged, so the PR title
becomes a version-determining subject. Use `!` only when something is actually
being removed.

**One version covers every package.** A change to one package bumps and
republishes the rest. Packages cannot be versioned independently.

**Merging to `main` publishes to NuGet.org.** There is no staging gate and a
published version cannot be withdrawn, so "merge and see" is not available.

## Public API

**A companion package shadows the namespace of the library it companions.** A package
that extends a third-party library puts its types in that library's namespaces, not
under a parallel `Waystone.*` tree, so a consumer reaches them from a `using` they
already have. `Waystone.Monads.Shouldly` ships in `Shouldly`,
`Waystone.Monads.Extensions.DependencyInjection` in
`Microsoft.Extensions.DependencyInjection`, and `Waystone.Monads.FluentValidation` in
`FluentValidation`, `FluentValidation.Extensions` and `FluentValidation.Configs`.
Mirror the library's own segments where it has them, and keep our segment names
(`Configs`, `Extensions`) beneath its root where it does not. The package and
assembly names keep the `Waystone.` prefix — only the namespaces shadow.

This does not apply to a package that extends `Waystone.Monads` itself rather than a
third party: `Waystone.Monads.Linq` and `Waystone.Monads.Extensions.Logging` keep their
own namespaces, because the namespace they would shadow is already ours.

**Deprecate; never remove.** Public API is obsoleted with a message naming both its
replacement and the version that removes it, and removed only in the next major.
Deleting public API outright is not an option, however small the change looks.

**Deprecations are tracked in GitBook, not here.** The published documentation
carries the Deprecations page and is the source of truth for what is going away and
when. Do not reintroduce a `BREAKING_CHANGES.md` — a second list drifts from the
first.

**A public API change needs a paired documentation PR** in
[draekien-industries/docs](https://github.com/draekien-industries/docs), linked to
this one so a reviewer sees both at once. This applies when you add, change,
obsolete or remove a public type or member, and when you change the behaviour or
default of one that is already documented. Read that repository's `AGENTS.md`
first — a new page there needs a `SUMMARY.md` entry in the same commit or GitBook
never shows it.

**Merge this repository's PR first, then the documentation PR.** Merging here
publishes to NuGet; merging there syncs GitBook. In that order the documentation
describes a version consumers can already install. If a code PR is closed without
merging, close its documentation PR too — a docs PR left open against abandoned API
is worse than none, because someone will eventually merge it.

Changes that leave the public surface untouched need no documentation PR: internal
refactors, tests, CI, and build configuration.

**Comment only public API surface.** XML doc comments on public members; no
explanatory comments inside method bodies. `CS1591` is suppressed, so the build
will not tell you when public docs are missing.

**A doc-only change must diff clean twice.** `git diff --stat -- '*PublicAPI*'` is
empty because the baseline records no comments, and `git diff -U0 | grep -v '///'`
prints nothing because nothing but comments moved. Either one printing something
means code leaked into the change.

**`Write` strips the BOM; several sources here have one.** Use `Edit` on an existing
file. A stripped BOM shows up as `-\xef\xbb\xbfnamespace` in the diff and nothing in
the build notices.

**Write and audit those comments through the
`engineering-skills:with-doc-comments` skill.** Nothing in the build checks that
a doc comment says anything, so the default outcome is a slot-filled restatement
of the signature. It applies to new comments and to existing ones alike. The two
failures it catches most often here are an overload whose summary is copied from
its sibling — doc generators index the first sentence alone, so the pair becomes
indistinguishable — and a `<param>` that repeats the parameter's own type.
Diagnostic descriptors are the exception and go through
`writing-diagnostic-descriptors` instead; see
[Waystone.Monads.Analyzers](src/Waystone.Monads.Analyzers/AGENTS.md).

## Branches and stacks

**Work flows `feature/*` → `main`.** There is no `develop` branch and there will
not be one, despite the GitFlow configuration in `GitVersion.yml`.

**Build a stack with `gh stack`, never by hand.** `gh stack init <bottom> … <top>`
takes the branches bottom to top, adopting ones that already exist and creating the
rest; `gh stack submit` then pushes them, repoints each PR's base onto the one
below, and registers the stack. Passing `--base` to `gh pr create` instead gives
you the right base branches and *no stack*: reviewers see unrelated PRs with no
chain, `gh stack view` shows nothing, and `gh stack merge` — the only supported way
to land one — has nothing to merge. Run `gh stack init` before opening the first
PR; adopting a hand-built chain afterwards works but is a repair, not a route.

`gh stack submit` opens an editor an agent cannot drive, so pass `--auto`. Note
that `--auto` creates *new* PRs as drafts unless you add `--open`.

**`--auto` titles a *new* PR from its branch name, not its commit subject.** A layer
submitted that way arrives titled `feature/dra 113 doc sweep long tail`, which is not
a conventional commit, so GitVersion reads no increment from it. Run
`gh pr edit <n> --title` straight after submitting. Existing PRs keep their titles.

**Run `gh stack add <branch>` before committing the next PR's work, not after.**
It creates the branch and checks it out. Commit first and the work lands on the
branch below, which then takes a `git branch`, a `reset --hard` and a
cherry-pick to unpick.

**A stack contributes every one of its PR titles.** `gh stack merge` squashes each
PR separately, so an eleven-PR stack lands eleven commits and GitVersion reads all
eleven subjects. It applies one increment for the highest bump among them, so ten
`feat` commits give a single minor bump. The trap runs the other way: a `!` in
*any* title takes the whole release major, including a mid-stack PR nobody was
thinking of as the release. Read the titles together before merging, not one at a
time as you open them.

## Documentation

Agent-facing documentation lives in `docs/`. Read [docs/AGENTS.md](docs/AGENTS.md)
before reading or writing anything there.

**A published C# code block is quoted from `sample/`, never typed into the page.**
`tools/Waystone.DocSnippets` lifts named `#region` blocks out of the sample projects
and writes them into GitBook, and `pre-push` fails on a page that has drifted. So a
documentation change that adds or edits C# is a change *here* first, in a project that
compiles, and only then a change to the page.

**Editing a page that still holds a hand-written C# block converts that block.** The
space is moving across one page at a time, as each is next edited, rather than in a
sweep — so the conversion lands in front of a reviewer already reading that page.
Read [sample/AGENTS.md](sample/AGENTS.md) for the region naming rules and the steps;
they are easy to get subtly wrong and the tool ignores a bad region name in silence.
