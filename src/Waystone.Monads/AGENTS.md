# Waystone.Monads

The `Option<T>` and `Result<TOk, TErr>` library, and the most widely consumed
package here.

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

`Microsoft.CodeAnalysis.PublicApiAnalyzers` runs against `PublicAPI.Shipped.txt` here
and in every other package carrying one: an added member fails RS0016, a removed one
RS0017. Both are build errors, and they are what enforces **deprecate; never remove**
rather than review attention. Let the analyzer's own code fix write the entries; do
not hand-edit the format.

Read [docs/contexts/public-api-baseline.md](../../docs/contexts/public-api-baseline.md)
before adding, changing or removing a public member: harvesting rows without an IDE,
when to move them out of `PublicAPI.Unshipped.txt`, why a change produces more rows than
you wrote, and what the baseline does not record.

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
attributes applied and read the RS0016/RS0017 pair, which names the exact drift. The
procedure is in
[docs/contexts/awaited-receiver-conversion.md](../../docs/contexts/awaited-receiver-conversion.md).

## A delegate that returns null

**Every delegate here whose return type is a monad guards it.** Two families,
`AndThen` and `OrElse`, in three shapes each — plain, state, async — across
`Some`/`None`, `Ok`/`Err` and both binders. Wrap a synchronous return in
`Option.NotNull` or `Result.NotNull`, and a `ValueTask` in the `…Async` twin,
passing `nameof` the parameter so the exception names the delegate the caller
wrote.

**A new monad-returning delegate joins that set; there is no member exempt from
it.** The guard turns a factory returning null into an `ArgumentNullException`
naming the factory, instead of a `NullReferenceException` raised at whatever read
the monad next, with nothing pointing back at the cause.

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
completed one.** Read
[docs/contexts/binder-async-conversion.md](../../docs/contexts/binder-async-conversion.md)
before treating that as a regression.

**`WA0001` fails the build on a guard left off.** The rule lives in
[Waystone.Internal.Analyzers](../Waystone.Internal.Analyzers/AGENTS.md), never
ships, and finds the guards by shape rather than by name-checking `Option` — a
static `NotNull` or `NotNullAsync` taking a value and a `string`, returning that
value's own type. Add a guarded type by writing its guard in that shape, and add
its row to `GuardConventionTests`, which is the only thing that fails when a
rename turns the rule off instead of breaking the build.

## The async delegate split

**A delegate producing another monad takes a `ValueTask`; every other delegate
takes a `Task`.** A chain *step* is the first kind — `AndThenAsync` and
`OrElseAsync` are the only two — so an existing async chain composes into one by
name. Everything else is a transform, so an ordinary `async` method group
converts to it. Check the claim rather than trusting it:

```
grep -cE 'Func<.*ValueTask<' src/Waystone.Monads/Options/OptionOfT.cs
```

Every hit is an `AndThenAsync` or an `OrElseAsync`.

**`WA0002` and `WA0003` fail the build on a member that breaks it.** Both live in
[Waystone.Internal.Analyzers](../Waystone.Internal.Analyzers/AGENTS.md) and never
ship — which is exactly why **neither may be named in an XML doc comment.** Those
comments land in `Waystone.Monads.xml` and in GitBook, where the id names a rule
the reader cannot run, look up or suppress. State the constraint and name the
error a caller actually sees: `CS0411` where the member is generic, `CS0407` on
the non-generic
`Option<T>.OrElseAsync`. Name the compiler's code, never ours.

## Gotchas

**An internal constructor does not close a `record` hierarchy.** Records get a
compiler-synthesized copy constructor, and CS8878 requires it to be `public` or
`protected` on an unsealed record — `private protected` does not compile.
`protected` reaches a derived type in another assembly, so an outside record
derives anyway with `public Evil(Option<T> o) : base(o)`. What actually
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

**There is one extension class per monad, and moving members between such classes
is not stageable.**
`OptionExtensions` and `ResultExtensions` hold everything callable on the type
that is not declared on the type itself; `OptionsCollectionExtensions` and
`ResultsCollectionExtensions` are separate only because their receiver is an
`IEnumerable<T>`. Any such move has to be atomic: two static classes that each
declare the same extension member for the same receiver make every reduced call
site `CS0121` ambiguous, and `[Obsolete]` does not remove a member from overload
resolution — two classes declaring `extension(Box box) { int Doubled(); }` and one
`box.Doubled()` call site demonstrate it. So an old class cannot sit obsoleted
beside a new one during a transition, and deleting a public class is what
**deprecate; never remove** forbids outside a major.

Adding a family now means adding `[GenerateAwaitedMember(nameof(Option<>.Thing))]`
to that one class. Hand-write a member there only when its receiver is a
*particular* option or result — a nested one, a tuple, a value-type payload — or
when the shape awaits an argument as well as the receiver, which the generator
cannot reach.

**A hand-written member in those classes costs roughly three baseline rows and
gains an `…Async` pair you did not ask for.** The generator lifts hand-written
extension members onto both awaited receivers automatically, with no opt-in.
Seven members in `extension` blocks produce 42 rows, 26 of them the automatic
async shapes. The same seven as classic `static (this T)` extension methods in a
separate package produce 9 — but the saving comes from the *package*, not the
syntax. `FromExtensionBlocks` filters on `IsExtensionMethod`, which a classic
method satisfies too, so rewriting a member in classic form to dodge the lift does
not work: it drops the `extension<…>` container row and keeps both `…Async` pairs.

**Decline the pair with `[ExcludeFromAwaitedReceivers]` when the awaited shape is
meaningless.** A member returning a builder or a state binder is the case it
exists for — awaited, it hands back a task of something the caller cannot chain,
and the pair costs two public members that **deprecate; never remove** then locks
until the next major. `Option.With` and `Result.With` carry it. Do not reach for
it to save rows on a member whose awaited form a caller would actually use.

A family that is additive vocabulary rather than core behaviour belongs in a
satellite package, which is why the LINQ names ship in `Waystone.Monads.Linq`
instead of here. Weigh that before hand-writing a member.

**The state binder's async members do not forward, and must not be made to.**
`Option<T>.Bound<TState>` and `Result<TOk, TErr>.Bound<TState>` forward their
*sync* members to state overloads on the monad — `Source.Map(_state, map)` — and
their *async* members to nothing, matching on the case themselves. Those state
overloads are abstract on `Option<T>` and `Result<TOk, TErr>` and overridden in
both derived types, and **not one of them is async**. There is nothing to forward
to.

Check that claim rather than trusting it; nothing in the build asserts it:

```
grep -cE '^abstract .*\.[A-Za-z]+Async<TState' src/Waystone.Monads/PublicAPI.Shipped.txt
```

It reads `0`, and must. Drop the `Async` from the pattern and the same command
counts the sync members that *do* take a `TState`, which is the contrast the rule
rests on. Write the command, never the count: a count here is written once, read
for years, and drifts unnoticed.

**Writing or editing an async binder member is its own procedure.** Read
[docs/contexts/binder-async-conversion.md](../../docs/contexts/binder-async-conversion.md)
first: why a short-circuit branch carries no `async` keyword, the count-the-awaits rule
for which members convert, why `new ValueTask<T>(task)` beats the `Awaited` helper, what
conversion does to exception timing, why the bodies match `Some`/`Ok` and reach the other
side through `UnwrapErr()`, and why async state overloads on the monads are not the fix.

**`MonadOptionsScope.Dispose` restores only when it is the innermost live scope,
and reports rather than throws.** It compares `ScopedOptions.Value` against the
instance it installed, which is why the struct holds two fields rather than one —
a `readonly struct` cannot mark itself disposed, so identity is the only thing it
can check. Each of the three cases is deliberate. The live scope matches, so
restore. `ScopedOptions.Value` is already this scope's
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
