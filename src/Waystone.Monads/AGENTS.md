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
The exception is a member that is *itself* obsolete: the compiler does not report
obsolete usage inside an obsolete context, so obsoleting a wrapper silences its
calls to what it wraps and leaves any `#pragma warning disable CS0618` around them
dead.

## The public API baseline

`Microsoft.CodeAnalysis.PublicApiAnalyzers` runs on this project against
`PublicAPI.Shipped.txt`: an added member fails RS0016, a removed one RS0017. Both
are build errors, and that is the point — it is what enforces **deprecate; never
remove** rather than review attention. Only this project is baselined.

Let the analyzer's own code fix write the entries; do not hand-edit the format.

Without an IDE, harvest them from the build rather than typing them. Each RS0016
reads ``Symbol '<row>' is not part of the declared public API``, and `<row>` is
the baseline line verbatim. Regex them out of `dotnet build`, merge into
`PublicAPI.Shipped.txt` and re-sort — the file is ordinal-sorted (`LC_ALL=C`)
below the `#nullable enable` header. A clean rebuild is the check that the
format came out right.

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

**A core member addition can grow the async surface too.** Adding an overload to
`Option<T>` or `Result<TOk, TErr>` makes the awaited-receiver generator emit the
matching `…Async` shapes for every family already converted to
`[GenerateAwaitedReceivers]`, and those land in the baseline as well — nine
members on `Option<T>` produced 39 rows. Families still hand-written get
nothing, so the async surface goes asymmetric until they are converted. That is
fine inside a stack that lands as one release and wrong to ship on its own.

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
Adding a public abstract member to either type means adding an override to that
test's probe record in the same change. The probe implements every public
abstract member so that exactly one CS0534 — `OnlyThisAssemblyMayDerive` — is
left; skip it and the test still fails, but for the wrong reason.

**There is one extension class per monad, and merging them was not stageable.**
`OptionExtensions` and `ResultExtensions` hold everything callable on the type
that is not declared on the type itself; `OptionsCollectionExtensions` and
`ResultsCollectionExtensions` are separate only because their receiver is an
`IEnumerable<T>`. DRA-111 merged the per-family classes in 7.0.0 and had to do it
atomically: two static classes that each declare the same extension member for the
same receiver make every reduced call site `CS0121` ambiguous, and `[Obsolete]`
does not remove a member from overload resolution — verified with two classes
declaring `extension(Box box) { int Doubled(); }` and one `box.Doubled()` call
site. So the old classes could not sit obsoleted beside the new one during a
transition, and deleting a public class is what **deprecate; never remove** forbids
outside a major.

Adding a family now means adding `[GenerateAwaitedMember(nameof(Option<>.Thing))]`
to that one class. Hand-write a member there only when its receiver is a
*particular* option or result — a nested one, a tuple, a value-type payload — or
when the shape awaits an argument as well as the receiver, which the generator
cannot reach.

**A hand-written member in those classes costs roughly three baseline rows and
gains an `…Async` pair you did not ask for.** The generator lifts hand-written
extension members onto both awaited receivers automatically — no attribute, and
`Waystone.SourceGenerators` has no exclude, ignore or skip concept, so there is
no opting out short of changing the generator. Measured on DRA-121: seven
members in `extension` blocks produced 42 rows, 26 of them the automatic async
shapes. The same seven as classic `static (this T)` extension methods in a
separate package produced 9.

That is the argument for a satellite package whenever a family is additive
vocabulary rather than core behaviour, and it is why the LINQ names ship in
`Waystone.Monads.Linq` instead of here. Weigh it before hand-writing a member:
the surface you are adding is not the surface you typed.

**`MonadOptionsScope.Dispose` restores only when it is the innermost live scope,
and reports rather than throws.** It compares `ScopedOptions.Value` against the
instance it installed, which is why the struct holds two fields rather than one —
a `readonly struct` cannot mark itself disposed, so identity is the only thing it
can check. Three cases fall out of that comparison and each is deliberate. The
live scope matches, so restore. `ScopedOptions.Value` is already this scope's
predecessor, so the restore has happened and a second `Dispose` returns silently,
which is what keeps an explicit `Dispose` inside a `using` from being reported as
misuse. Anything else — an outer scope disposed early, or a
`default(MonadOptionsScope)` — restores nothing and writes
`MonadDiagnostics.ScopeDisposedOutOfOrderEventName`.

Do not turn that event into a throw. `Dispose` runs from a `using`, so an
exception there displaces whichever one was already unwinding, and the misuse is
in the *caller's* disposal order rather than in anything the flow was doing. A
consumer who wants it fatal subscribes and throws from the subscriber.

The residue is real and is documented on `Dispose` rather than fixed: declining to
restore leaves the early-disposed scope's options in place until the live scope is
disposed, which then restores them as its own predecessor, so they outlive their
scope. `MonadOptionsScopeTests` pins that consequence, so a future change that
"tidies" it fails a test rather than silently changing what the doc comment
promises.
