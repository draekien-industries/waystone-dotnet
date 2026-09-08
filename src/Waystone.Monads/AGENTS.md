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
extension members onto both awaited receivers automatically, with no opt-in.
Measured on DRA-121: seven members in `extension` blocks produced 42 rows, 26 of
them the automatic async shapes. The same seven as classic `static (this T)`
extension methods in a separate package produced 9 — but the saving came from the
*package*, not the syntax. `FromExtensionBlocks` filters on `IsExtensionMethod`,
which a classic method satisfies too, so rewriting a member in classic form to
dodge the lift does not work. Measured again in DRA-200: it drops the
`extension<…>` container row and keeps both `…Async` pairs.

**Decline the pair with `[ExcludeFromAwaitedReceivers]` when the awaited shape is
meaningless.** A member returning a builder or a state binder is the case it
exists for — awaited, it hands back a task of something the caller cannot chain,
and the pair costs two public members that **deprecate; never remove** then locks
until the next major. `Option.With` and `Result.With` carry it. Do not reach for
it to save rows on a member whose awaited form a caller would actually use.

That is the argument for a satellite package whenever a family is additive
vocabulary rather than core behaviour, and it is why the LINQ names ship in
`Waystone.Monads.Linq` instead of here. Weigh it before hand-writing a member:
the surface you are adding is not the surface you typed.

**The state binder's async members do not forward, and must not be made to.**
`Option<T>.Bound<TState>` and `Result<TOk, TErr>.Bound<TState>` forward their
*sync* members to state overloads on the monad — `Source.Map(_state, map)` — and
their *async* members to nothing, matching on the case themselves. The asymmetry
reads as an oversight and is not: those state overloads are abstract on
`Option<T>` and `Result<TOk, TErr>` and overridden in both derived types, 57 and
59 declarations on the bases with 43 and 45 in each derived type, and **not one of
them is async**. There is nothing to forward to.

Do not close the gap by adding async state overloads to the monads. Each one costs
three declarations — abstract plus two overrides — so the 27 the binder needs is
81, and the abstract ones land in the baseline where **deprecate; never remove**
locks them until the next major. They would also be 27 more of exactly the surface
`With` exists to replace.

**A binder member with a short-circuit branch carries no `async` keyword, and that
is deliberate.** Per-case overrides on the monad drop `async` altogether on the
trivial branch — `None<T>.IsSomeAndAsync` is `=> new ValueTask<bool>(false)` — because
virtual dispatch has already chosen the case. The binder has no such dispatch: it
tests `Source is Some<T>` inside one method. Written `async`, that method builds a
state machine even on the branch that never awaits, and DRA-201 measured the cost at
**1.76x a closure** on `IsSomeAndAsync` over a `None`, the cheapest member in the set.

DRA-205 removed it without touching the monads. The outer method is not `async` and
returns a completed `ValueTask` on the short-circuit branch; the await moves into a
private `async` helper reached only from the branch that awaits:

```csharp
public ValueTask<bool> IsSomeAndAsync(Func<T, TState, Task<bool>> predicate) =>
    Source is Some<T> some
        ? Awaited(predicate(some.Value, _state))
        : new ValueTask<bool>(false);

private static async ValueTask<bool> Awaited(Task<bool> task) =>
    await task.ConfigureAwait(false);
```

**The rule for which members convert is mechanical: count the awaits.** One await
means a branch returns without awaiting, and it converts. Two awaits means both
branches build a machine anyway, so converting buys nothing and only adds an
indirection — `MatchAsync` and `MapOrElseAsync` on both binders stay `async` for that
reason, and the file reads as inconsistent until you know this. `AndThenAsync` and
`OrElseAsync` do best of all: their delegates return `ValueTask`, so the awaiting
branch returns it straight through and neither branch builds a machine.

**It costs no public API.** `async` is not part of a signature, so the conversion
moved zero baseline rows. Verify with `git diff --stat -- '*PublicAPI*'` rather than
assuming.

**It does change when exceptions surface, which is the part to be careful with.**
A non-`async` method reads `Source` and invokes the delegate eagerly, so a `default`
binder and a delegate that throws before returning its `Task` both throw *at the
call* rather than yielding a faulted task. `ADefaultBoundThrowsFromTheCallRatherThan
FromTheAwait` and `ADelegateThrowingBeforeItsTaskThrowsFromTheCall` pin it in both
async test classes, asserting with the synchronous `Should.Throw` and no `await` —
which is what makes them fail against the `async` form rather than passing either
way.

The conversion moved every case it touched, and moved most of them past the closure
rather than merely level with it. `artifacts/dra-205/` against `artifacts/dra-201/`:
`MapAsync` on a `None` went 1.11x to **0.73x**, `MapAsync` on an `Err` 1.08x to
**0.56x**, `AndThenAsync` on a `Some` 0.98x to **0.57x** — that last one being the
member that now builds no machine on either branch. Six of the eight categories are
faster than the closure, at zero allocation where the closure pays 88 bytes.

Two figures are deliberately still above 1.0 and neither is a defect.
`IsSomeAndAsync` over a `None` sits at **1.10x**: the residue of testing the case at
all, on the cheapest member in the set, and the floor for anything short of adding
async state overloads to the monads. `MatchAsync` sits at 1.04x because it awaits on
both branches and was never converted. Read an allocation claim off the predicate
categories rather than off `MapAsync`, whose non-zero figure is the new result
instance and not a state machine.

**Match the success case, never the failure case.** The async bodies read
`Source is Some<T>` and `Source is Ok<TOk, TErr>`, and reach the other side through
`UnwrapErr()`. Matching `Err<TOk, TErr>` directly and reading `err.Value` looks
tidier on the two members that need only the error, and is wrong to reach for: the
other seven need *both* values, so one pattern cannot serve them, and nothing in this
library matches `Err<TOk, TErr>` or `None<T>` anywhere. Two members in the new idiom
and seven in the old is worse than nine consistent ones. `UnwrapErr()` also avoids
the alternative — a cast, or a `_ => throw` arm that is a line coverage can never
reach.

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

Two residues are real and are documented on `Dispose` rather than fixed. Declining
to restore leaves the early-disposed scope's options in place until the live scope
is disposed, which then restores them as its own predecessor, so they outlive their
scope. And a scope that has already declined declines again on every further
`Dispose`, writing the event each time, because the struct cannot record that it
reported — so the "safe to call more than once" guarantee covers the restoring path
only. `MonadOptionsScopeTests` pins both, so a future change that "tidies" either
fails a test rather than silently changing what the doc comment promises.
