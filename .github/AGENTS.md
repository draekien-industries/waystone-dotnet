# Workflows

## Constraints

**`release.yml` publishes to NuGet.org and tags a GitHub release**, on a push to
`main` matching its paths filter.

**The `main` ruleset requires `codecov/patch`, `Calculate Version` and `Build and
run tests`,** all three from `pull-request.yml`. Renaming one of those jobs leaves a
required check that can never report, which blocks every PR. Add jobs; do not
rename these.

**Checks live in git hooks where they can, to save runner minutes.** The framework
matrix and the release-tracking checks run in `.githooks/pre-push` rather than here.
Do not migrate them into a workflow without a reason that outweighs the cost.

## Gotchas

**Do not put a `branches` filter on `pull_request`.** GitHub's stacked-pull-request
documentation says no workflow changes are required; that is not what happens. With
`branches: [main]`, only the bottom PR of a stack runs build and test — every PR
above it targets its parent branch and is filtered out. The checks are required, so
a PR that never ran one can never satisfy it, and this blocks the whole stack rather
than failing visibly.

**`concurrency.group` must vary by PR.** `gh stack submit` pushes every branch at
once, and a group that does not vary puts all of them together, so
`cancel-in-progress` kills every run but the last to start. The group is keyed on
`github.event.pull_request.number`.

**`pull-request.yml` has no `paths` filter, and must not be given one.** A PR
touching only filtered-out paths sits `BLOCKED` forever: the workflow never runs,
so none of the three required checks reports, and there is nothing pending to wait
for. A check that never reports cannot be satisfied. `gh pr merge --admin` clears
one such PR, but not a stack — `gh stack merge` is the only supported way to land
one and it has no per-PR bypass, so a filtered-out PR at the bottom blocks every PR
above it.

**Skipping the jobs is not the alternative.** GitHub counts a skipped job as a
passing required check, so `Calculate Version` and `Build and run tests` would be
fine gated behind a `changes` job. `codecov/patch` would not: Codecov posts it
only after a coverage report is uploaded for the head commit, and no upload
happens if the job that runs `dotnet test` is skipped. The required check is
pinned to Codecov's integration id, so nothing else can post that context in its
place. Any scheme that skips the test job has to drop `codecov/patch` from the
ruleset, which gives up a real gate for runner minutes.

A filter also hides build failures outside it. `bench/Waystone.Monads.Benchmarks.csproj`
is in `Waystone.Net.slnx`, so `dotnet build` builds it; filter `bench/**` out and a
benchmark that stopped compiling reaches `main` without CI noticing.

`release.yml` keeps its filter, and there the `!**/*.md` ordering still matters.
The exclusion has to come *after* the positive patterns, because a later pattern
wins, and it applies inside `src` and `test` too, which is what keeps the area
`AGENTS.md` files from triggering a publish. `AnalyzerReleases.*.md` is then
re-included after it: RS2008 makes those files build-affecting. Do not remove
that filter to match `pull-request.yml` — without it a prose change under `src`
attempts a publish of a version that `docs:` and `chore:` subjects never bumped.

**`codecov/patch` counts every line the diff touches, not the lines that added
logic.** Stripping a nullable annotation across a dozen untested async overloads
puts all of their untested lines in the patch, so a change that added no behaviour
can fail it. It is a required check. Close the test gap rather than moving the
threshold; the gaps it reports are real ones, including the equality an incremental
generator pipeline caches through, which nothing else exercises.

**`codecov/project` is the one with a threshold, and only because it cannot be
satisfied otherwise.** It measures the whole repository, so `target: auto` with
codecov's default zero threshold fails on a decrease of a hundredth of a percent —
including on a PR whose entire content is a `.txt` baseline and two markdown files,
where there is no gap to close by definition. `codecov.yml` gives it 0.5% and
leaves `patch` strict. Do not relax `patch` on the same reasoning; the two are
measuring different things and only one of them is a required check.
