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

**A new rule needs an `AnalyzerReleases.Unshipped.md` entry in the same change.**
RS2008 fails the build without one. Use severity `Disabled` in that table for a
rule that ships off.

The severity policy is checked two ways, and neither is redundant. An *enabled*
rule promoted to warning breaks the library's own build immediately, because
`src/**` is `TreatWarningsAsErrors` and the library trips its own idiom rules — but
it fails as twenty errors in unrelated files rather than as a statement about the
rule. A *disabled* rule at warning severity fires nothing and so builds clean;
`RulesTests.OnlyMisuseRulesShipAtWarningOrAbove` is what catches that one.

## Gotchas

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
