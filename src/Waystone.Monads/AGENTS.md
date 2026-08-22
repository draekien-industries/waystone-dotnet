# Waystone.Monads

The `Option<T>` and `Result<TOk, TErr>` library. The most widely consumed package
here, and the only one with a public API baseline.

## Constraints

**`netstandard2.0` cannot be raised.** PolySharp supplies the newer language
features. Consumers on older frameworks depend on it, which is why net472 and
net481 sit in the test matrix.

**The analyzer ships inside this package.** `Waystone.Monads.Analyzers` and
`Waystone.Monads.Analyzers.CodeFixes` are `IsPackable=false` and are packed into
`analyzers/dotnet/cs` by the `PackMonadAnalyzers` target. Every consumer gets the
rules on upgrade with no opt-out beyond `.editorconfig`.

**`CS0618` is not suppressed**, and `src/**` builds with
`TreatWarningsAsErrors`. Obsoleting a member the library still calls fails the
build. Point those call sites at the replacement in the same change, or the
deprecation is not finished.

## The public API baseline

`Microsoft.CodeAnalysis.PublicApiAnalyzers` runs on this project against
`PublicAPI.Shipped.txt`: an added member fails RS0016, a removed one RS0017. Both
are build errors, and that is the point — it is what enforces **deprecate; never
remove** rather than review attention. Only this project is baselined.

Let the analyzer's own code fix write the entries; do not hand-edit the format.

Move rows from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt` **before**
merging, filed under the version GitVersion will compute from the PR title. Merging
publishes, and there is no later release step that would move them, so a row left
unshipped is wrong from the moment the PR lands. `pre-push` fails on one.

One member in a C# 14 `extension` block produces **three** baseline entries: the
`extension<T>(Receiver)` container, the member, and the compiler's compatibility
`static Member<T>(this Receiver)` form. You author none of the extra ones, so
expect the baseline to grow by more lines than you wrote. That makes it a stricter
check than it looks: the compat-static entry records the receiver's nullability
independently, so a block whose receiver is subtly wrong is caught there even when
the member entry matches.

**The baseline does not record generic constraints.** It records names, types and
nullability, so a `where` clause can be added or relaxed with RS0016/RS0017
silent. A relaxation is source-compatible and therefore not a break, but it is a
hole in the instrument — and the instrument is the whole argument that a
refactor changed nothing. Diff the `where` clauses by eye whenever a change
touches a member that had any.

## Naming

**A parameter rename is source-breaking; a type parameter rename is not.** Named
arguments work in reduced extension syntax, so `option.OkOrAsync(err: e)` compiles
today and a rename would stop it — and a parameter name cannot be obsoleted, so
there is no deprecate-then-remove path. Those wait for a major. No call site can
name a type parameter, so renaming one breaks nothing and lands as an ordinary
`refactor:`. Do not lump the two together.

Type parameter names carry roles, not spellings: `TOut` for a mapped output,
`TOther` for another input element, `T1`/`T2` for tuple positions. `Zip` takes
`TOther` and `Unzip` keeps `T1`/`T2` for that reason.

**Extension names drift from the core members they forward to, and that is what
limits how much of `Extensions` can be generated.** A generated shape takes its
parameter and type parameter names from the core member, so a family only converts
with an untouched baseline when the two already agree — and they frequently do
not. Do not estimate which families are convertible: build the family with the
attributes applied and read the RS0016/RS0017 pair, which names the exact drift.
See [Waystone.SourceGenerators](../Waystone.SourceGenerators/AGENTS.md).

## Gotchas

**An internal constructor does not close a `record` hierarchy.** Records get a
compiler-synthesized copy constructor, and CS8878 requires it to be `public` or
`protected` on an unsealed record — `private protected` does not compile.
`protected` reaches a derived type in another assembly, so an outside record
closes over the hole with `public Evil(Option<T> o) : base(o)`. What actually
closes both hierarchies is the `internal abstract OnlyThisAssemblyMayDerive` on
`Option<T>` and `Result<TOk, TErr>`: an outside type cannot override a member it
cannot see, so it fails CS0534 with no way out. The regression cases live in
`ClosedHierarchyTests` in the *analyzer* test project, because
`Waystone.Monads.Tests` has `InternalsVisibleTo` and would compile a derived type
happily, proving nothing.

**Merging the per-family extension classes into one cannot be staged.** Two static
classes that each declare the same extension member for the same receiver make
every reduced call site `CS0121` ambiguous, and `[Obsolete]` does not remove a
member from overload resolution — verified with two classes declaring
`extension(Box box) { int Doubled(); }` and one `box.Doubled()` call site. So the
old classes cannot sit obsoleted beside the new one during a transition. The move
would have to be atomic, and deleting a public class is what **deprecate; never
remove** forbids outside a major.
