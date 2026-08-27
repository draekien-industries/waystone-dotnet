# Tests

## Running them

`dotnet test` with no `--framework` runs every target framework. The `pre-push`
hook runs exactly that, because CI pins `--framework net8.0` for coverage
collection and would let a net472, net481, net9.0 or net10.0 break through.

## Shared configuration

**`test/Directory.Build.props` owns the framework matrix and the warning policy**,
the way `src/Directory.Build.props` does for the shipped projects. A test project
declares neither, and adding `Nullable`, `IsPackable`, `OutputType`, `LangVersion`
or `ImplicitUsings` to one is duplication rather than intent.

`OutputType` and `LangVersion` are load-bearing there, not tidiness. xunit.v3
fails the build outright unless the project is an executable, and net472 and
net481 default to C# 7.3, so a test project that set neither would either not
build or compile against a language a decade older than the tests are written in.

**Warnings are errors across `test/`.** The only warnings left in the tree are
codeless MSBuild ones from `Microsoft.Extensions.Diagnostics.Testing`, which says
it does not support net472 or net481. A warning with no code cannot be promoted
to an error, so those survive; anything with a code will fail the build.

**One project overrides the matrix, and says why in its own file.** Keep that
shape: an exception belongs next to the code it applies to, not as a condition in
the shared props. `Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore.Tests`
drops net472 and net481, because its subject depends on ASP.NET Core and ships
net8.0 and net10.0 only. It keeps net9.0, which resolves the net8.0 asset.

**Both analyzer harnesses select `ReferenceAssemblies` from the framework the
test host is running on**, through `Verify.Target`. Do not pin a version there.
Pinned, all five frameworks compiled the identical net8.0 source, so the matrix
proved one thing five times — and in `Waystone.Monads.Shouldly.Analyzers.Tests`
it was worse than useless: that harness hands the compilation the Shouldly
assembly the *host* has loaded, so a net9.0 host mixed a net8.0 compilation with
a Shouldly built against `System.Runtime` 9.0.0.0 and failed 44 tests on
`CS1705` rather than on anything the analyzer did.

**On net472 and net481 those harnesses add `System.Threading.Tasks.Extensions`
explicitly.** `ValueTask<T>` is not part of .NET Framework, so a test with a
`ValueTask` receiver otherwise fails on `CS0012` naming an assembly its source
never mentions. Keep the version in step with the one `Waystone.Monads`
references, so the compilation sees what a consumer on that framework would.

## Gotchas

**The Reqnroll specs are gone; their coverage is not.** 186 scenarios across 21
`.feature` files were translated into xUnit in DRA-135, not deleted — mostly into
`Options/Extensions` and `Results/Extensions` files that did not exist before.
Do not reintroduce a spec layer for a family that now has a test class; add the
case to the class.

One branch fell out of that translation and had to be replaced deliberately:
`ExceptionHandledLogger.Attach` rejects a listener whose name is not the monad
listener's, and the only thing reaching that branch was Reqnroll creating a
`DiagnosticListener` of its own. `MonadLoggingOptionsTests` now creates a foreign
listener on purpose. Incidental coverage from a test framework is worth checking
for whenever one leaves.

**`MonadOptions.Global` is process-wide.** Prefer `MonadOptions.BeginScope`, which
confines the override to the current asynchronous flow and needs no coordination
with anything. A test that genuinely needs the global — because it is testing
publication itself, or reads a fallback the global supplies — must carry
`[Collection(GlobalMonadOptionsCollection.Name)]`, which serialises it against
every other class that does. The collection is not optional bookkeeping: it is the
only thing stopping these classes from racing, and `ErrorCodeTests` and
`ErrorTests` are in it because they assert on the *default* fallbacks, which a
parallel class configuring the global would change under them.

Inside that collection, call `MonadOptions.Reset()` from the constructor rather
than capturing and restoring by hand. Reset swaps in the same default snapshot the
type built at start-up, so it covers a setting added later; a hand-written restore
of each scalar does not. `MonadOptionsResetIsolationTests` and its `…PairTests`
sibling exist as a pair for that reason — each resets on entry, so neither sees the
other's configuration whichever order the runner picks, which is the property no
single class can demonstrate. Do not merge them.

Reset clears the calling flow's open scope and nothing else. It cannot clear
another flow's `AsyncLocal`, and it deliberately leaves the `_scopingHasBeenUsed`
latch set, so a suite that has opened one scope keeps paying for the scoped read
path for the rest of the run.

**A half-extracted reference-assembly cache fails every analyzer test at once.**
`Microsoft.CodeAnalysis.Testing` unpacks into `%TEMP%\test-packages\`, and an
interrupted run leaves the `.nupkg` there with no nuspec beside it. Every test
then throws `PackagingException: The package is missing the required nuspec
file`, which reads like a broken test rather than a broken download. Delete the
offending package directory and it re-downloads.

**`ClosedHierarchyTests` lives in the analyzer test project, not here.**
`Waystone.Monads.Tests` has `InternalsVisibleTo`, so it would compile an
out-of-assembly derived type happily and prove nothing.

**`Waystone.Monads.Tests` imports the assertion analyzers, so WMS2001 and WMS2002
report on it.** They are `Info`, so nothing fails; MSBuild does not log them
either, which is why the build looks silent. To see them, run the fix:

```
dotnet format analyzers test/Waystone.Monads.Tests/Waystone.Monads.Tests.csproj \
  --diagnostics WMS2001 WMS2002 --severity info
```

**Run that until it stops changing files — one pass is not enough.** WMS2001
rewrites `(await x).IsSome.ShouldBeTrue()` into `(await x).ShouldBeSome()`, which
is then WMS2002's input, and the batch fixer only lands non-overlapping fixes per
pass. Sweeping this project took three passes to reach a fixed point.

**Always scope it with `--diagnostics`.** Without it, `dotnet format` applies
every fix available at `Info` across the project, and the sweep disappears into
several hundred unrelated edits.
