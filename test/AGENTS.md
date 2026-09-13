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

**A project overriding the matrix says why in its own file.** An exception belongs
next to the code it applies to, not as a condition in the shared props.
`Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore.Tests` drops net472 and
net481, because its subject depends on ASP.NET Core; it keeps net9.0, which
resolves the net8.0 asset. `grep -l TargetFramework test/*/*.csproj` lists them.

**Both analyzer harnesses select `ReferenceAssemblies` from the framework the
test host is running on**, through `Verify.Target`. Do not pin a version there.
Pinned, all five frameworks compile the identical net8.0 source, so the matrix
proves one thing five times — and in `Waystone.Monads.Shouldly.Analyzers.Tests`
it is worse than useless: that harness hands the compilation the Shouldly
assembly the *host* has loaded, so a net9.0 host mixes a net8.0 compilation with
a Shouldly built against `System.Runtime` 9.0.0.0 and fails 44 tests on
`CS1705` rather than on anything the analyzer did.

**On net472 and net481 those harnesses add `System.Threading.Tasks.Extensions`
explicitly.** `ValueTask<T>` is not part of .NET Framework, so a test with a
`ValueTask` receiver otherwise fails on `CS0012` naming an assembly its source
never mentions. Keep the version in step with the one `Waystone.Monads`
references, so the compilation sees what a consumer on that framework would.

**`Waystone.Internal.SourceGenerators.Tests` picks its subject's references a third
way — the test host's own loaded assemblies — so what a subject can name is decided
by the framework running the tests.** That is why the matrix is worth running here;
a subject using a type .NET Framework lacks fails on net472 and net481 alone.
`CallerArgumentExpressionPolyfill.cs` declares
`CallerArgumentExpression` publicly under `#if NETFRAMEWORK` for that reason: the
only declaration otherwise reachable is PolySharp's `internal` one inside
`Waystone.Monads`, which reports as `CS0122` rather than as a missing type. Declare
such a fill in the *test assembly*, never in the subject source — `Verify.Preamble`
opens with a file-scoped namespace, so a subject cannot open a second namespace at
all, and on the three frameworks that already have the real type a duplicate is a
`CS0436`.

## Conventions

**`Waystone.Conventions.Tests` holds rules about the tree, not about behaviour.**
A test belongs there when its subject is what the build produced — a file the
compiler wrote, a naming rule spanning projects, a layout that has to hold — and
belongs in a type's own test project when it exercises that type. Read
[docs/contexts/conventions-tests.md](../docs/contexts/conventions-tests.md) before adding
a test there or editing its `.csproj`.

**`WA` and `WSG` ids must not appear in a shipped XML doc comment**, and
`PackagedDocumentationTests` fails the build on one. Both spaces belong to
analyzers that never leave this repository, so a consumer who sees the id in a
tooltip cannot run the rule, look it up, or suppress it, and the help link derived
from it points at an anchor that does not exist. State the constraint and name the
compiler error the caller actually sees instead. Members under
`Waystone.Internal.*` are exempt: the awaited-receiver attributes document a
contract for this repository's own authors, and a consumer cannot reach them.

## Gotchas

**There is no spec layer, and the coverage is in xUnit.** Do not reintroduce a
spec layer for a family that has a test class; add the case to the class.

One branch has no incidental cover and is reached deliberately: the subscription
rejects a listener whose name is not the monad listener's, and nothing but a test
framework creating a `DiagnosticListener` of its own would otherwise reach it.
`MonadLoggingOptionsTests` creates a foreign listener on purpose. Incidental
coverage from a test framework is worth checking for whenever one leaves. The
branch lives in `MonadDiagnosticEvent` rather than `ExceptionHandledLogger` and is
covered on both sides — keep the logging test anyway, since it is the only one
proving the filter reaches a consumer of the helper rather than only the helper.

**`MonadOptions.Global` is process-wide.** Prefer `MonadOptions.BeginScope`, which
confines the override to the current asynchronous flow and needs no coordination
with anything. A test that genuinely needs the global — because it is testing
publication itself, or reads a fallback the global supplies — must carry
`[Collection(GlobalMonadOptionsCollection.Name)]`, which serialises it against
every other class that does. The collection is the only thing stopping these classes
from racing, and `ErrorCodeTests` and `ErrorTests` are in it because they assert on
the *default* fallbacks, which a parallel class configuring the global would change
under them.

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
either, which is why the build looks silent. The `dotnet format` invocation that
surfaces and fixes them is in
[docs/contexts/assertion-analyzer-sweep.md](../docs/contexts/assertion-analyzer-sweep.md).
