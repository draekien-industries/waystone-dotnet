# Waystone.Monads.Analyzers

Also covers `Waystone.Monads.Analyzers.CodeFixes`. Both are `IsPackable=false` and
ship inside the `Waystone.Monads` package.

## Constraints

**Every consumer gets these rules on upgrade with no opt-out beyond
`.editorconfig`.** That is why `WM1xxx` are the only rules allowed to ship at
warning severity — a rule that fires on working code breaks a build somebody did
not ask to change. Anything else ships `Disabled` or as a suggestion.

**Target Roslyn 4.8, and never reference `Waystone.Monads`.** The library's types
are resolved by metadata name through `MonadSymbols.TryCreate`, and the analyzer
goes silent when they are absent. A project reference would make the library's own
consumption of its analyzer a build cycle.

**Write and change descriptors through the `writing-diagnostic-descriptors`
skill**, including an edit to a shipped rule's strings. It owns the tier-to-factory
mapping, the id allocation, the voice of each of the three strings, and the
paired obligations further down this file — none of which the build checks in
full. It also owns the split descriptors get wrong most often: why a pattern
is a problem belongs in the `description` a consumer sees, while why the rule is
scoped as it is belongs in the XML doc on the field. Reasoning that lands in both
is a duplicate.

**A new rule needs an `AnalyzerReleases.Unshipped.md` entry in the same change.**
RS2008 fails the build without one. Use severity `Disabled` in that table for a
rule that ships off.

The severity policy is checked two ways. An *enabled* rule promoted to warning
breaks the library's own build immediately, because `src/**` is
`TreatWarningsAsErrors` and the library trips its own idiom rules — but it fails as
twenty errors in unrelated files rather than as a statement about the rule. A
*disabled* rule at warning severity fires nothing and so builds clean;
`RulesTests.OnlyMisuseRulesShipAtWarningOrAbove` catches that one.

**A deprecation gets a code fix on `CS0618`, not a rule of its own.** The Migration
tier ships `Disabled`, so a rule there never fires; an enabled rule reports what the
compiler already reports — the `WM1002`-alongside-`WM2008` double-report below.
Subclass `MonadCodeFix` with `FixableDiagnosticIds = ["CS0618"]` and bail unless the
symbol is one of ours, or the fix fires on a consumer's own obsolete API.

**Where the compiler already reports a pattern, ship the fix and not the rule.**
`CS8714` already reports a `Map` projection that may return null — at *warning*
rather than the `Info` a rule of ours would ship at, and from nullable flow analysis
rather than an annotation, so it stays quiet on `o => o.Customer ?? new()` and on a
suppressed `o => o.Customer!` where an annotation-keyed rule fires.
`UseAndThenWithFromNullableCodeFix` registers on `CS8714` instead and contributes what
the compiler cannot know: this library spells the fix `AndThen` with
`Option.FromNullable`. It holds no rule id of its own — `TaskReturningAsyncStep` holds
the one it would have taken. Read the highest id out of `Rules.cs` rather than
trusting a sentence here, because a reused id silently redirects a consumer's existing
suppression onto a rule they have never seen. Check what the compiler already reports
before writing a descriptor.

No worked example is in the tree. A `CS0618` fixer cannot outlive its own
deprecation, because a removed member reports `CS0117` or `CS1501` and the fixer has no
symbol left to key on, so each one goes with the members it rewrote. Read
`UseGeneratedErrorCodeCodeFix` out of git history rather than reinventing the shape,
and expect to delete the next one the same way.

## The severity presets

`src/Waystone.Monads/build/` carries `recommended.globalconfig` and
`strict.globalconfig`, packed into the nupkg and opted into with one property,
`WaystoneMonadsRuleset`. The shipped defaults stay quiet; the tiers are an opt-in rather
than a change to `Rules.cs`. **Add a rule and you add two preset rows** — `PresetTests`
fails on a descriptor with no entry, and on an entry carrying the wrong tier's severity.

Read [docs/contexts/analyzer-severity-presets.md](../../docs/contexts/analyzer-severity-presets.md)
before editing anything under `build/`: why they must be global configs rather than
`.editorconfig` fragments, what `sample/Waystone.Monads.Analyzers.Sample` does and does
not prove, and why a shipped default must not move to tidy a preset.

## Gotchas

**`WM2018` shares source with the generator.** `ErrorCodeFormat.cs` is a linked
`Compile` item from `Waystone.Monads.SourceGenerators`, not a project reference — the
two analyzer assemblies cannot reference each other, and the rule keys on the
*generated* code, so it has to resolve `[ErrorCodeCatalog(Format = ...)]` and
`[assembly: ErrorCodeFormat]` exactly as the generator does. A second copy of the
parser would let the rule and the generator disagree about what code an enum produces.
Deriving the code from the enum name instead is wrong in both directions once anyone
sets a format: `FlagsACollisionCausedByASharedFormat` and
`IgnoresASharedNameWhenTheFormatsDiffer` pin both.

**A diagnostic reported from a compilation end action cannot have a code fix.** It
lands in `AnalysisResult.CompilationDiagnostics` and is a *non-local* diagnostic even
when its location is an ordinary source span; Roslyn's code fix service will not offer
a fix for one, and `CodeFixTest` fails with "Code fix is attempting to provide a fix for
a non-local analyzer diagnostic". `CodeFixTestBehaviors.SkipLocalDiagnosticCheck`
silences the test and changes nothing about the IDE, so do not reach for it. This is
why `WM2019` reports from a symbol action and `WM2020` — which cannot know an entry is
stale until every enum has been seen — has no fix at all. Both rules read the same two
sets; the split is entirely about fixability.

**A rule registered on `OperationKind.Invocation` must reject on type identity
before anything else.** That action runs on every method call in every file a
consumer compiles, so the first clause of the gate is the one that decides the
rule's cost, and almost every call it sees belongs to somebody else. Collapsing
`WM2017`'s "is this ours" check into its "can this be rewritten" check leaves
`TakesState` — an `ImmutableArray` scan of the method's type parameters — running
on every call in the compilation, because the cheap reject has moved behind it.
Nothing fails, and no test can catch it; the order in
`StateOverloadAnalyzer.Analyze` is load-bearing and reads as arbitrary.

**A rule that reports from a compilation end action needs
`WellKnownDiagnosticTags.CompilationEnd`.** Roslyn reads the tag to decide when to run
the end action, so without it the rule can go quiet in the IDE while still firing on the
command line. `WM2018` and `WM2020` are the only two, and
`RulesTests.OnlyTheAggregatingRulesAreTaggedCompilationEnd` pins the pair, because
nothing in the build notices a missing tag.

**A path-based `.editorconfig` cannot set the severity of `WM2020`.** Roslyn resolves
`dotnet_diagnostic.X.severity` per syntax tree, and `WM2020` is reported against
`ErrorCodes.txt`, which has none — so a `[*]` section is not consulted and the rule
stays at its default however the section is written. Escalating it takes a global
analyzer config: `is_global = true` in a `.globalconfig`. `WM2019` is reported on the
enum member and does respond to `.editorconfig`, so the two rules need different
configuration to reach the same severity. The sample carries a `.globalconfig` that
does it.

**`WM2018` is the only rule that aggregates across declarations.** Every other
analyzer decides from one node or one symbol; `ErrorCodeReuseAnalyzer` has to see two
enums at once, so it collects into a `ConcurrentBag` under `RegisterSymbolAction` and
reports from `RegisterCompilationEndAction`. `Initialize` calls
`EnableConcurrentExecution`, so the collection must be thread-safe and the *order*
must not matter — it reports on the second member in ordinal order by display string
rather than on whichever symbol arrived second, or the same source would blame a
different declaration between runs.

**`IsExtensionMethod` is not a reliable test.** The library's extensions are C# 14
`extension` blocks, and the compiler emits a compatibility static method that
older Roslyn sees as a classic extension. A rule keyed on `IsExtensionMethod`
passes its tests and then misses real call sites on a modern consumer's compiler.
Identify the receiver instead — `MonadSymbols.IsMonadInvocation` falls back to the
type of the expression before the dot.

**`UnwrapAwaitable` does not see through `ConfigureAwait`.** It knows `Task<T>` and
`ValueTask<T>` only, and this library awaits with `.ConfigureAwait(false)`
everywhere, so a rule that unwraps the inner call's type goes quiet on the style the
library teaches. Read `IAwaitOperation.Type` instead when there is an await.

**A `null` literal's `ConvertedType` is the monad in a comparison too.** A rule
keyed on it fires on `option == null` and `option is null` as readily as on an
assignment — a rule keyed on it double-reports `WM1002` alongside `WM2008`, with a
code fix producing `option is Option.None<int>()`, which does not compile.
`NullAndDefaultAnalyzer.IsNullTest` excludes the comparison and pattern positions.

## Tests

**The tests run on Roslyn 5.6 while the analyzer builds against 4.8.** The mismatch
is the forward-compatibility case every consumer is in. Both versions are pinned with
`VersionOverride`, and the testing packages resolve their own Roslyn floor to 1.0.1
unless a direct reference lifts it.

**`Microsoft.CodeAnalysis.Testing` force-enables every diagnostic the analyzer
under test supports**, so `isEnabledByDefault: false` cannot be observed through it
and a disabled rule fires in tests that do not expect it. Assert the default on the
descriptor instead — `RulesTests` does — and keep a disabled-by-default rule in its
own analyzer class so it does not pollute another rule's tests.

**`Verify.CompilerCodeFixAsync` covers a fix registered on a compiler diagnostic**,
using `EmptyDiagnosticAnalyzer` and `DiagnosticResult.CompilerWarning("CS0618")`
against a `{|#0:...|}` span.

**No generator runs in these tests.** A source that references generated catalog
members has to declare them by hand in the shape `ErrorCodeCatalogWriter` emits, or
the fixed code will not compile. That leaves the emitted shape asserted in
`Waystone.Monads.SourceGenerators.Tests` and assumed here, so a change to the
nesting has to be carried across by hand.
