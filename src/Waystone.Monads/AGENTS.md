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

## A delegate that returns null

**Every delegate here whose return type is a monad guards it.** DRA-216 swept the
library and found the set is smaller than it looks: exactly two families,
`AndThen` and `OrElse`, in three shapes each — plain, state, async — across
`Some`/`None`, `Ok`/`Err` and both binders. All twenty guard. Wrap a synchronous
return in `Option.NotNull` or `Result.NotNull`, and a `ValueTask` in the `…Async`
twin, passing `nameof` the parameter so the exception names the delegate the
caller wrote.

**A new monad-returning delegate joins that set; there is no member exempt from
it.** The guard turns a factory returning null into an `ArgumentNullException`
naming the factory, instead of a `NullReferenceException` raised at whatever read
the monad next, with nothing pointing back at the cause. That distance is the
whole value, and it is why `OrElse` was brought in line rather than written down
as a deliberate exception — nothing separated it from `AndThen` except which one
got the guard first.

**A member that forwards inherits the guard and must keep forwarding.** The sync
binder overloads are one line — `Source.OrElse(_state, optionFactory)` — and
inlining the branch instead would compile, pass every behavioural test, and
silently drop the guard. `OptionBoundTests` and `ResultBoundTests` each carry one
test that exists only to pin the forward.

**Three categories are out of the sweep, each for its own reason.** `Try` and
`TryAsync` take a delegate returning a *value*, and already handle a null one
differently on each side by design: `Option.Try` yields `None`, `Result.Try`
yields an `Err` built by `FactoryReturnedNull`. The collection extensions take no
monad-returning delegate at all — their delegates are predicates, mappers and
value factories. The generated awaited receivers await and forward, so they
inherit whatever the core member does and must not guard a second time.

**Guarding costs a state machine on a pending async step and nothing on a
completed one.** See the conversion rules below before reading that as a
regression.

**`WA0001` fails the build on a guard left off, so none of the above is on you to
remember.** The rule lives in
[Waystone.Analyzers](../Waystone.Analyzers/AGENTS.md), never ships, and finds the
guards by shape rather than by name-checking `Option` — a static `NotNull` or
`NotNullAsync` taking a value and a `string`, returning that value's own type. Add
a guarded type by writing its guard in that shape, and add its row to
`GuardConventionTests`, which is the only thing that fails when a rename turns the
rule off instead of breaking the build.

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
`Option<T>` and `Result<TOk, TErr>` and overridden in both derived types, and
**not one of them is async**. There is nothing to forward to.

Check that claim rather than trusting this paragraph — it is the one thing here a
change can falsify, and nothing in the build asserts it:

```
grep -cE '^abstract .*\.[A-Za-z]+Async<TState' src/Waystone.Monads/PublicAPI.Shipped.txt
```

It reads `0`, and must. Drop the `Async` from the pattern and the same command
counts the sync members that *do* take a `TState`, which is the contrast the rule
rests on. This paragraph previously carried four exact declaration counts instead;
they had drifted from the source by the time anyone checked, so they are gone
rather than corrected — a count here is written once and read for years.

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
indirection. `AndThenAsync` and `OrElseAsync` did best of all: their delegates return
`ValueTask`, so the awaiting branch returned it straight through and neither branch
built a machine.

**DRA-216 took half of that back on purpose, and it is not a regression to undo.**
Both now wrap the delegate's task in `NotNullAsync`, which keeps the property for a
step whose task has already completed — the guard unwraps and rewraps it
synchronously — and pays one machine for a step that has not. That is the cost the
non-bound `Some.AndThenAsync` and `Ok.AndThenAsync` had always paid, on a branch
already awaiting a pending task, and it buys an `ArgumentNullException` naming the
factory in place of a `NullReferenceException` at whatever read the monad next.

The short-circuit branch is untouched in all four, so the property the rule above is
about — no machine on the branch that does not await — still holds everywhere.

**Count per overload, not per member.** `MatchAsync` and `MapOrElseAsync` are where
this bites: their both-asynchronous overloads await twice and keep `async`, while the
mixed ones DRA-211 added await once and drop it. So the two forms sit adjacent in the
file, spelled differently, and the difference is the rule applying — not drift. Read
the delegate types before concluding one of them is wrong.

**A one-await branch does not need the helper either — wrap the task.** `Awaited`
exists to move the `await` off the outer method, but `new ValueTask<T>(task)` removes
it altogether, which is the same trick `AndThenAsync` gets for free from a delegate
that already returns `ValueTask`. Measured on an incomplete task, 100k calls:

```
Awaited(task)             136.0 bytes per call
new ValueTask<int>(task)   16.0 bytes per call
```

The 120 bytes are the async state machine box. The two are otherwise
indistinguishable — same `IsCompleted`, same result, and the same exception timing
in both of the cases that get confused for each other:

| What fails | `Awaited` | wrapped |
| --- | --- | --- |
| Delegate throws *before* returning its task | at the call | at the call |
| Delegate returns an already-faulted task | at the await | at the await |

The eagerness the two `…ThrowsFromTheCall` tests pin is the first row, and neither
form changes it — the delegate is invoked before either wrapper sees anything. Do not
read those tests as a claim about the second row; a faulted task surfaces at the await
under both, and always did.

`ConfigureAwait(false)` inside `Awaited` is not a reason to keep it either. It governs
that method's own continuation, and the wrap has none; where the *caller* resumes is
decided by the caller's own await either way.

The mixed overloads DRA-211 added are written this way. **The roughly twenty older
binder members still call `Awaited` and were not converted here** — that is a uniform
change wanting its own benchmark run against `artifacts/dra-205/`, not a line in a
PR about binder shapes. Do not read the mixture as a decision that `Awaited` is
preferred anywhere; prefer the wrap in anything new.

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

**Those counts are stale and the section is under review.** `ResultBound.cs` reaches
the error through `UnwrapErr()` at sixteen sites now, not nine — DRA-211 added the
rest. DRA-216 also found the argument above answers consistency and coverage but
never safety: `UnwrapErr()` is a partial member used where the type test above it has
already proved totality, and nothing reports it if that stops being true. DRA-218
converts all sixteen to `Match`, which is total, and rewrites this section. Do not
carry the numbers forward, and do not reach for a `switch` expression — the compiler
cannot see the closed hierarchy and `CS8509` demands the uncoverable arm.

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
