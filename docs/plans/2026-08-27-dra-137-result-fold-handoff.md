---
title: Fold the Result async members onto the core records
date: 2026-08-27
status: active
---

Handoff for the remainder of
[DRA-137](https://linear.app/draekien-industries/issue/DRA-137). The Option half is
built and merged into the stack as PR #157; this plan covers the Result half, which
has not been started. It also records
[DRA-138](https://linear.app/draekien-industries/issue/DRA-138), built alongside it,
and the follow-up issues neither one closes.

## Where the work stands

Stack **#142**, eighteen PRs, **nothing merged**, release **7.0.0**. Merging to
`main` publishes to NuGet with no staging gate, so nothing here is recoverable
after the fact.

The current branch is `feature/dra-138-throw-on-a-null-projection` with a clean
working tree. The top two layers are new and both have all eight checks green:

| PR | Branch | Title |
| --- | --- | --- |
| [#158](https://github.com/draekien-industries/waystone-dotnet/pull/158) | `feature/dra-138-throw-on-a-null-projection` | `fix!: throw when an Option projection returns null` |
| [#157](https://github.com/draekien-industries/waystone-dotnet/pull/157) | `feature/dra-137-fold-option-sync-receiver-members` | `refactor!: fold the Option async members onto the core records` |
| [#156](https://github.com/draekien-industries/waystone-dotnet/pull/156) | `feature/dra-136-close-the-awaited-receiver-holes` | `feat: close the awaited-receiver holes and pin core-member coverage` |

Below those, #155 down to #140 in stack order: DRA-119, DRA-135, DRA-134 ×2,
DRA-90, DRA-133, DRA-110 ×4, DRA-115 ×2, DRA-130, DRA-129, DRA-128.

Read the titles together before merging. `gh stack merge` squashes each PR
separately, so all eighteen subjects reach GitVersion and a single `!` anywhere
takes the whole release major. Several already carry one, so 7.0.0 is settled —
but check rather than assume.

## Goal

Move the twenty async members that still sit in `Results/Extensions/` on a
synchronous `Result<TOk, TErr>` receiver onto `Result<TOk, TErr>`, `Ok<TOk, TErr>`
and `Err<TOk, TErr>`, mirroring what #157 did for `Option`. The awaited surface must
not change: each emptied class becomes an attribute-only manifest naming both the
synchronous member and its async sibling, and the generator lifts both onto a `Task`
and a `ValueTask` receiver.

## What moves

Twenty members across fourteen files, all with an
`extension<TOk, TErr>(Result<TOk, TErr> result)` receiver:

| File | Members |
| --- | --- |
| `AndThenExtensions.cs` | `AndThenAsync` |
| `InspectExtensions.cs` | `InspectAsync` |
| `InspectErrExtensions.cs` | `InspectErrAsync` |
| `IsErrAndExtensions.cs` | `IsErrAndAsync` |
| `IsOkAndExtensions.cs` | `IsOkAndAsync` |
| `MapExtensions.cs` | `MapAsync` |
| `MapErrExtensions.cs` | `MapErrAsync` |
| `MapOrExtensions.cs` | `MapOrAsync` |
| `MapOrDefaultExtensions.cs` | `MapOrDefaultAsync` |
| `MapOrElseExtensions.cs` | `MapOrElseAsync` ×3 |
| `MapOrNullExtensions.cs` | `MapOrNull`, `MapOrNullAsync` |
| `MatchExtensions.cs` | `MatchAsync` ×4 |
| `OrElseExtensions.cs` | `OrElseAsync` |
| `UnwrapOrElseExtensions.cs` | `UnwrapOrElseAsync` |

## What stays an extension, and why

Three cannot fold, for two structural reasons:

- **Nested receiver.** `Flatten` takes `Result<Result<TOk, TErr>, TErr>` and
  `Transpose` takes `Result<Option<TOk>, TErr>`. Neither is a member of
  `Result<TOk, TErr>`.
- **A constraint on the monad's own type parameter.** `UnwrapOrNull` is
  `extension<TOk, TErr>(Result<TOk, TErr> result) where TOk : struct`. An abstract
  member cannot narrow `TOk` for one case.

A constraint on the *method's* own type parameter is fine —
`MapOrNull<TOut> where TOut : struct` moved without trouble on `Option`.

## The trap: eleven of the twenty ship undocumented

`Map`, `MapErr`, `MapOr`, `MapOrElse` ×3, `Match` ×4, `OrElse` and `UnwrapOrElse`
have **no XML doc comments at all**. They are public API; `CS1591` is suppressed, so
nothing in the build ever said so. Moving them is the right moment to write those
docs, and it makes this layer materially larger than the Option one.

Write them through the `engineering-skills:with-doc-comments` skill. The failure to
watch for here is the four `MatchAsync` overloads and the three `MapOrElseAsync`
overloads: doc generators index the first sentence alone, so summaries copied
between siblings make the set indistinguishable. Distinguish them by which delegate
is asynchronous, the way #157 distinguished the two `ZipWithAsync` receiver shapes.

## Steps

1. `gh stack add feature/dra-137-fold-result-sync-receiver-members` **before**
   committing anything. Committing first lands the work on #158's branch and costs a
   `git branch`, a `reset --hard` and a cherry-pick to unpick.
2. Add twenty abstract declarations to `ResultOfTOkTErr.cs`, each **interleaved
   after its synchronous sibling**, not appended. Same grouping rule as
   `OptionOfT.cs`: same-name overloads stay contiguous and the async member follows
   the sync family.
3. Override all twenty in `Ok.cs` and `Err.cs`, interleaved the same way. Follow
   `ResultOfTOkTErr.cs`'s order in all three files.
4. Reduce the fourteen classes to attribute-only manifests naming both members:

   ```csharp
   [GenerateAwaitedReceivers(typeof(Result<,>))]
   [GenerateAwaitedMember(nameof(Result<,>.Map))]
   [GenerateAwaitedMember(nameof(Result<,>.MapAsync))]
   public static partial class MapExtensions;
   ```

5. Add the twenty members to the `Either<TOk, TErr>` probe in
   `ClosedHierarchyTests.AnOutsideAssemblyCannotDeriveFromResult`, and add
   `using System.Threading.Tasks;` to that test's source string. The probe
   implements everything a derived type *can* so that exactly one `CS0534` —
   `OnlyThisAssemblyMayDerive` — is left. Skip this and the test fails for the wrong
   reason, reporting twenty-one diagnostics instead of one.
6. Harvest `PublicAPI.Shipped.txt` from the build. Do not hand-write rows.
7. `dotnet test` with no `--framework`. All five target frameworks, 38 runs.
8. `simplify` inline, then commit, then `gh stack submit --auto --open`, then
   `gh pr edit <n> --title` — `--auto` titles a *new* PR from its branch name, which
   is not a conventional commit and gives GitVersion no increment.

## How to tell it is finished

- `dotnet build` clean, and `dotnet test` green across all 38 runs.
- `AwaitedReceiverCoverageTests` passes for `Result<,>` without editing the test.
  It reads the emitted surface, so it is the check that the manifests actually name
  everything.
- `git diff --stat -- '*PublicAPI*'` shows growth only. An RS0017 means a member was
  renamed or lost, not moved.
- The `where` clauses diff clean by eye. The baseline does not record generic
  constraints, so RS0016/RS0017 stay silent on a changed constraint — and the
  baseline being clean is the whole argument that the refactor changed nothing.

## Behaviour to preserve

Unlike the Option fold, this one has no null-rule decision to make: `Ok.Map` is
`Result.Ok<TOut, TErr>(map(Value))` and `Ok`'s constructor already throws
`ArgumentNullException` on a null value. `Result` was strict all along. That
asymmetry is what settled DRA-138 — see below.

Each moved member should drop the guard it opens with. They all begin by
re-deriving a branch virtual dispatch already performs, in the shape
`if (result.IsErr) { ... }` followed by
`result.Expect("Expected Ok but found Err.")` on a path that cannot be reached.
On `Ok<TOk, TErr>` and `Err<TOk, TErr>` the case is known, so the guard and the
re-extraction both go.

## Context from this session that is not in the code

### DRA-138 and the null-projection rule

`Option` and `Result` disagreed on what a null projection meant. `Some.Map` used
`Option.NoneIfNull` and returned `None`; `Ok.Map` threw. #158 makes `Option` match
`Result`.

The reasoning, so it is not relitigated: every projection is constrained
`where TOut : notnull`, so a null return is a broken contract rather than data, and
collapsing it to `None` made the bug indistinguishable from legitimate absence. The
library already has the explicit spelling for a projection that may genuinely yield
nothing — `AndThen` with `Option.FromNullable` — so `NoneIfNull` inside `Map`
duplicated it implicitly and blurred mapping into binding.

Rust was checked and does not settle it: `Option::map` wraps unconditionally because
`U` cannot be null there, and `zip_with` is still nightly-only under feature
`option_zip`. Read literally it leans away from `NoneIfNull`, which is the same
direction we went, but for a reason that does not transfer.

`Option.Try` and `Option.FromNullable` stay lenient. They are the two explicitly
named opt-ins to "null means absence", and `Try` additionally sits inside a `catch`
governed by `MonadOptions.Current.Catches`, so making it strict would leave its
result depending on configuration. `NoneIfNull` now has exactly one caller — `Try`.

The rule is documented once in the remarks on `Option<T>`, with an `<exception>`
element on each member that enforces it. State it the same way if `Result` ever
needs it spelled out; do not repeat the paragraph per member.

### Two generator rules worth not rediscovering

- `AwaitedReceiverWriter.EmitMember` keeps the emitted name when the source member
  already ends in `Async`, so `MapAsync` lifts to `MapAsync`, not `MapAsyncAsync`.
  `AwaitedReceiverCoverageTests.AwaitedName` restates that rule deliberately rather
  than reading the emitted name back, so that the two disagreeing is a failure.
- The generator lifts a member onto an awaited receiver **without touching its
  parameters**, so it can never reach a shape where a second argument is itself
  awaited. That is why `Option`'s `ZipWithExtensions` keeps two hand-written
  overloads. Nothing in the Result set has that shape, so all fourteen classes
  should end up attribute-only.

## Follow-ups this work does not close

File these rather than folding them in:

- **`AndThenAsync` does not guard against a factory returning a null option.** The
  synchronous `AndThen` overloads were fixed in #158 — they were
  `Map(optionFactory).Flatten()`, which strict `Map` would have made throw naming
  `map`, a parameter the caller never wrote. `AndThenAsync` returns the factory's
  `ValueTask` directly, so guarding it means awaiting the result, which puts a state
  machine on the success path of the primary chaining operator. Deliberately left
  open; the cost is worth weighing on its own.
- **A WM2 `Info` rule** flagging a `Map` whose projection is nullable-annotated,
  pointing the caller at `AndThen` plus `Option.FromNullable`. Needs a descriptor, an
  `AnalyzerReleases.Unshipped.md` row, a docs section and tests. Write it through the
  `writing-diagnostic-descriptors` skill.
- **`Transpose` has no awaited receivers on either monad** — zero async rows. Its
  receiver is nested, so it needs hand-written blocks like `Flatten` and `Unzip`.
- **DRA-127 must carry docs for all of this** in one consolidated PR: DRA-136's
  seven new members, DRA-137's moved surface on both monads including class names and
  `cref`s in published examples, and DRA-138's behaviour change. Do not open a second
  docs PR. Merge this repository's PRs first, then the documentation PR.

## Smaller things noticed and not acted on

- `.editorconfig` line 4 has a typo: `indent_stype`.
- No `AGENTS.md` row for `src/Waystone.Monads.Shouldly`.
- `Verify.cs` is duplicated across the two analyzer test projects, including the
  `"4.6.3"` literal copied from `Directory.Packages.props`.
- Unused `using System.Linq;` in `DeclaredTypeAnalyzer.cs`.
- `src/Waystone.Monads/README.md` says the async-factory `Try` overloads "will be
  removed in v6"; they were removed *in* 6.0.0.
- Pre-existing and unrelated: `NU1903` for `Microsoft.OpenApi` 2.0.0, and
  `CS8619`/`CS8714`/`WM1005` in `sample/Waystone.Monads.Analyzers.Sample/Idioms.cs`.
- `docs/plans/2026-08-26-native-observability-for-waystone-monads.md` is still
  `active`.
- DRA-110, 115, 128, 129, 130 and 135 are still In Progress in Linear though their
  PRs are green.

## House rules that bit during this work

- **`Write` strips the BOM and several sources here have one** — most of
  `Results/Extensions/` does. Use `Edit`, or write with `utf-8` and preserve the
  leading BOM explicitly. A stripped BOM shows up as `-\xef\xbb\xbfnamespace` in the
  diff and nothing in the build notices.
- **The `commit-msg` hook rejects a subject of 72 characters or more.**
- **A bulk script is fine for a mechanical reorder, but scope it.** A doc-rewrap
  script run over `Options/Extensions/*.cs` reached five files that were not part of
  the change and had to be reverted.
