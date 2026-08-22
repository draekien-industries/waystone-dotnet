# Workflows

## Constraints

**`release.yml` publishes to NuGet.org on every push to `main` under `src`,** then
tags a GitHub release. There is no staging gate and a published version cannot be
withdrawn.

**The `main` ruleset requires `codecov/patch`, `Calculate Version` and `Build and
run tests`,** all three from `pull-request.yml`. Renaming one of those jobs leaves a
required check that can never report, which blocks every PR. Add jobs; do not
rename these.

**Runner minutes are the reason checks live in git hooks where they can.** The
framework matrix and the release-tracking checks run in `.githooks/pre-push`
rather than here. Do not migrate them into a workflow without a reason that
outweighs the cost.

## Gotchas

**Do not put a `branches` filter on `pull_request`.** GitHub's stacked-pull-request
documentation says workflows trigger as if each PR targeted the base of the stack
and that no workflow changes are required. That is not what happens: with
`branches: [main]`, only the bottom PR of an eleven-PR stack ran build and test —
the other ten target their parent branch and were filtered out. Since the checks
are required, a PR that never ran one can never satisfy it, so this blocks the
whole stack rather than failing visibly.

**`concurrency.group` must vary by PR.** `gh stack submit` pushes every branch at
once, and a group that does not vary puts all of them together, so
`cancel-in-progress` kills every run but the last to start. The group is keyed on
`github.event.pull_request.number`.

**The `paths` filter means a markdown-only PR cannot satisfy the required
checks.** It matches nothing in the filter, the workflow never runs, the three
checks never report, and the PR sits `BLOCKED` with nothing pending to wait for.
Those land with `gh pr merge --admin` under the ruleset's `OrganizationAdmin`
bypass. That is the accepted trade — widening the filter would run the build over
prose. Do not wait on checks for a markdown-only PR; nothing is coming.

`!**/*.md` does the excluding, and it has to be *after* the positive patterns,
because a later pattern wins. It applies inside `src` and `test` too, which is what
keeps the area `AGENTS.md` files from triggering a build. `AnalyzerReleases.*.md`
is then re-included after it: RS2008 makes those files build-affecting, so a change
touching only one of them must still run CI. `release.yml` carries the same pair,
or a prose change under `src` would attempt a publish of a version that `docs:` and
`chore:` subjects never bumped.

**`codecov/patch` counts every line the diff touches, not the lines that added
logic.** Stripping a nullable annotation across a dozen untested async overloads
puts all of their untested lines in the patch, so a change that added no behaviour
can fail it. It is a required check. Close the test gap rather than moving the
threshold — the gap it found in 5.4.0 was real, and so was the one it found in the
source generator, which sat at 91% because nothing exercised the equality the
incremental pipeline caches through.

**`codecov/project` is the one with a threshold, and only because it cannot be
satisfied otherwise.** It measures the whole repository, so `target: auto` with
codecov's default zero threshold fails on a decrease of a hundredth of a percent —
including on a PR whose entire content is a `.txt` baseline and two markdown files,
where there is no gap to close by definition. `codecov.yml` gives it 0.5% and
leaves `patch` strict. Do not relax `patch` on the same reasoning; the two are
measuring different things and only one of them is a required check.
