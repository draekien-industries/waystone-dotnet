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
full. It also owns the split the descriptors get wrong most often: why a pattern
is a problem belongs in the `description` a consumer sees, while why the rule is
scoped as it is belongs in the XML doc on the field. Reasoning that lands in both
is a duplicate, and reasoning that lands only in the `description` is design
history in a tooltip.

**A new rule needs an `AnalyzerReleases.Unshipped.md` entry in the same change.**
RS2008 fails the build without one. Use severity `Disabled` in that table for a
rule that ships off.

The severity policy is checked two ways, and neither is redundant. An *enabled*
rule promoted to warning breaks the library's own build immediately, because
`src/**` is `TreatWarningsAsErrors` and the library trips its own idiom rules — but
it fails as twenty errors in unrelated files rather than as a statement about the
rule. A *disabled* rule at warning severity fires nothing and so builds clean;
`RulesTests.OnlyMisuseRulesShipAtWarningOrAbove` is what catches that one.

**A deprecation gets a code fix on `CS0618`, not a rule of its own.** The Migration
tier ships `Disabled`, so a rule there never fires; an enabled rule reports what the
compiler already reported, which is the `WM1002`-alongside-`WM2008` double-report.
Subclass `MonadCodeFix` with `FixableDiagnosticIds = ["CS0618"]` and bail unless the
symbol is one of ours, or the fix fires on a consumer's own obsolete API.

**The same reasoning reaches past deprecations: where the compiler already reports
a pattern, ship the fix and not the rule.** A rule was going to flag a `Map`
projection that may return null, until compiling the motivating example showed
`CS8714` already reports it — at *warning* rather than the `Info` the rule would
have shipped at, and from nullable flow analysis rather than an annotation, so it
stays quiet on `o => o.Customer ?? new()` and on a suppressed `o => o.Customer!`
where an annotation-keyed rule would have fired. The rule would have been a
strictly weaker duplicate. `UseAndThenWithFromNullableCodeFix` registers on
`CS8714` instead and contributes the one thing the compiler cannot know, which is
that this library spells the fix `AndThen` with `Option.FromNullable`. No rule id was
allocated for it, and the next free one has since gone to `TaskReturningAsyncStep` —
read the highest id out of `Rules.cs` rather than trusting a sentence here, because a
reused id silently redirects a consumer's existing suppression onto a rule they have
never seen. Check what the compiler already says before writing a descriptor; on a
constrained generic surface like this one it says more than you expect.

There is no worked example in the tree right now. `UseGeneratedErrorCodeCodeFix` was
the one, and it was deleted in the 7.0.0 stack along with the members it rewrote —
a `CS0618` fixer cannot outlive its own deprecation, because a removed member reports
`CS0117` or `CS1501` and the fixer has no symbol left to key on. Read it out of
history rather than reinventing the shape, and expect to delete the next one the same
way.

## The severity presets

`src/Waystone.Monads/build/` carries `recommended.globalconfig` and
`strict.globalconfig`, packed into the nupkg's `build/` folder beside a
`Waystone.Monads.props` that NuGet auto-imports. A consumer opts in with one
property, `WaystoneMonadsRuleset`, and gets nothing unless they set it — the shipped
defaults stay quiet, which is the whole reason the tiers exist as an opt-in rather
than as a change to `Rules.cs`. `Waystone.Monads.Shouldly` ships a parallel pair
reading the *same* property, so a posture is set once rather than per package.

**Add a rule and you add two preset rows.** `PresetTests` reads every descriptor out
of `Rules.cs` and fails when one has no entry, fails when an entry names an id no
descriptor declares, and fails when a tier's entries do not all carry that tier's
severity. It resolves them through Roslyn's own `AnalyzerConfigSet` rather than by
matching the text, so what it pins is the severity a compiler would apply.

**They are global configs, and that is not a style choice.** `WM2020` is reported
against `ErrorCodes.txt`, which has no syntax tree, so a path-matched `.editorconfig`
section cannot set its severity at all — an `.editorconfig` fragment would ship a
preset with one rule silently missing from it. Two consequences to keep: `global_level`
stays negative, because a tie with a consumer's own global config is resolved by
*unsetting* the option rather than by reporting a conflict; and a consumer's
path-matched `.editorconfig` beats the preset, which is the override route the docs
promise. `sample/Waystone.Monads.Analyzers.Sample` applies `strict` and holds the
seven `WM1` rules back down to warning in its `.editorconfig` — that is the only
executable statement of the precedence anywhere, and it is also what keeps a project
full of deliberate misuse building.

**Do not read that sample as evidence that a codebase can adopt `strict` as
shipped.** It cannot, and neither can that project: nine of the rules `strict` moves
are overridden straight back down, so what the sample actually validates is the
`EditorConfigFiles` plumbing, the override precedence, and the `WM2` tier's raise from
suggestion to warning. A preset's effect on real code is not testable here, because
the only consumer in the tree is a fixture built to report.

**Do not change a shipped default to make a preset tidier.** The presets are additive
by construction; a default that moves needs a `### Changed Rules` row in
`AnalyzerReleases.Unshipped.md` and breaks the build of a consumer who only wanted a
version bump.

## Gotchas

**`WM2018` shares source with the generator.** `ErrorCodeFormat.cs` is a linked
`Compile` item from `Waystone.Monads.SourceGenerators`, not a project reference — the
two analyzer assemblies cannot reference each other, and the rule keys on the
*generated* code, so it has to resolve `[ErrorCodeCatalog(Format = ...)]` and
`[assembly: ErrorCodeFormat]` exactly as the generator does. A second copy of the
parser would let the rule and the generator disagree about what code an enum produces,
which is the one thing this rule cannot afford to be wrong about. Deriving the code
from the enum name instead is wrong in both directions once anyone sets a format:
`FlagsACollisionCausedByASharedFormat` and `IgnoresASharedNameWhenTheFormatsDiffer`
pin both.

**A diagnostic reported from a compilation end action cannot have a code fix.** It
lands in `AnalysisResult.CompilationDiagnostics` and is a *non-local* diagnostic even
when its location is an ordinary source span; Roslyn's code fix service will not offer
a fix for one, and `CodeFixTest` fails with "Code fix is attempting to provide a fix for
a non-local analyzer diagnostic" rather than letting you ship a fix nobody can reach.
`CodeFixTestBehaviors.SkipLocalDiagnosticCheck` silences the test and changes nothing
about the IDE, so do not reach for it. This is why `WM2019` reports from a symbol action
and `WM2020` — which cannot know an entry is stale until every enum has been seen — has
no fix at all. Both rules read the same two sets; the split is entirely about
fixability.

**A rule that reports from a compilation end action needs
`WellKnownDiagnosticTags.CompilationEnd`.** Roslyn reads the tag to decide when to run
the end action, so without it the rule can go quiet in the IDE while still firing on the
command line. `WM2018` and `WM2020` are the only two, and
`RulesTests.OnlyTheAggregatingRulesAreTaggedCompilationEnd` pins the pair, because
nothing in the build notices a missing tag.

**A path-based `.editorconfig` cannot set the severity of `WM2020`.** Roslyn resolves
`dotnet_diagnostic.X.severity` per syntax tree, and `WM2020` is reported against
`ErrorCodes.txt`, which has none — so a `[*]` section is simply not consulted and the
rule stays at its default however the section is written. Escalating it takes a global
analyzer config: `is_global = true` in a `.globalconfig`. `WM2019` is reported on the
enum member and does respond to `.editorconfig`, so the two rules need different
configuration to reach the same severity. The sample carries a `.globalconfig` that
does it, which is the only executable statement of this anywhere.

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
everywhere, so a rule that unwraps the inner call's type goes quiet on exactly the
style the library teaches. Read `IAwaitOperation.Type` instead when there is an
await.

**A `null` literal's `ConvertedType` is the monad in a comparison too.** A rule
keyed on it fires on `option == null` and `option is null` as readily as on an
assignment — this is how `WM1002` came to double-report alongside `WM2008`, with a
code fix producing `option is Option.None<int>()`, which does not compile.
`NullAndDefaultAnalyzer.IsNullTest` excludes the comparison and pattern positions.

## Tests

**The tests run on Roslyn 5.6 while the analyzer builds against 4.8.** That
mismatch is deliberate: it is the forward-compatibility case every consumer is in.
Both versions are pinned with `VersionOverride`, and the testing packages resolve
their own Roslyn floor to 1.0.1 unless a direct reference lifts it.

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
