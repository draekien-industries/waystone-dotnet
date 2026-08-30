# Waystone.SourceGenerators

Emits the awaited receiver shapes — the `Task<Option<T>>` and
`ValueTask<Option<T>>` overloads that forward into a core member — for the
extension classes in `Waystone.Monads`.

## The generator contract

**There are two seeds, and knowing both is what makes a failed conversion
readable.** `Analyse` emits the union of:

* `FromReceiverMember` — the written list. A destination class carries
  `[GenerateAwaitedReceivers(typeof(Option<>))]` naming the receiver and one
  `[GenerateAwaitedMember(nameof(Option<>.Unwrap))]` per core member it wants.
* `FromExtensionBlocks` — every public extension member already in the destination
  class **whose receiver is not itself awaitable**, lifted onto both awaited
  receivers.

The written half used to be deliberate for a reason that no longer applies: which
extension class a core member's async shape belonged in was not derivable, since
the mapping was nearly `{Member}Extensions` but `Unwrap`, `UnwrapOr`,
`UnwrapOrDefault` and `Result.UnwrapErr` all lived in `UnwrapExtensions`. DRA-111
collapsed the per-family classes into `OptionExtensions` and `ResultExtensions`,
so there is now exactly one destination per monad and no mapping to get wrong.

The list stays anyway, because it still answers a second question the destination
never did: *which* core members get lifted. It remains the strongest available form
of "emit exactly today's set".

**A written list is only as good as the list, so a test reads the surface
instead.** Nothing here notices a core member left off every list — the generator
emits what it is told and says nothing about the rest, so the failure is a missing
overload rather than a diagnostic. Seven members reached 7.0.0 that way, and the
two causes need distinguishing because they suggest different fixes: `Option.Or`,
`Option.Xor` and `Result.Or` had no destination class, while `Option.Zip`,
`Result.And`, `Result.GetOk` and `Result.GetErr` each had one and were simply not
listed in it. `AwaitedReceiverCoverageTests` in `Waystone.Monads.Tests` now fails
when a public core member has no `…Async` reachable from either awaited receiver.
It asserts on the emitted surface rather than on the attributes, so it covers both
causes and a hand-written family equally.

The lifted half is what carries the shapes the core member does not have — an
async-delegate overload, say, which exists only as an extension. It is also the
half that surprises people, in both directions: a hand-written member on a
synchronous receiver silently gains two awaited overloads, and one on an awaited
receiver contributes nothing and will be deleted by the conversion. **Which
receiver a hand-written overload sits on is therefore part of the contract, not a
detail.**

Write the member with `nameof`, not a bare string. C# 14 takes an unbound generic
in `nameof`, so `nameof(Option<>.Unwrap)` compiles, matches the `typeof(Option<>)`
above it, and makes a rename fail the build instead of silently dropping a family
into `WSG0002`.

`GenerateAwaitedMember.Summary` overrides the synthesised summary per member, for
cases where the source member's own wording does not read well after the await
prefix.

**The generator writes only the `extension` block; the containing class must be
`partial`.** Generated shapes land in the same static class as the hand-written
ones so the baseline entries keep naming `OptionExtensions` rather than some new
class — a rename there would be a public API change. Two blocks of the same
receiver shape in the same partial class across two files merge without complaint,
so the generator can add to a class that already hand-writes one. `WSG0001` catches a
marked class that is not partial, because otherwise the failure is a CS0260
pointing at generated source.

**A new rule needs an `AnalyzerReleases.Unshipped.md` entry in the same change.**
RS2008 fails the build without one.

## Gotchas

**Members sharing a receiver shape do not necessarily share its constraints, and
`Key` has to say so.** `AwaitedReceiverWriter` groups members into one emitted
`extension` block per key, then takes the block's constraints from `group[0]`.
Key on the receiver type and parameter name alone and two members with the same
receiver but different constraints — `UnwrapOrNull` under `where T : struct`
beside `Map` under `where T : notnull` — land in one block that carries whichever
constraint happened to come first. Nothing fails in this repository: the generated
source compiles, and the break surfaces as `CS0453` in a *consumer*, at a call site
whose type argument the surviving constraint rejects. `Key` therefore includes the
rendered block constraints. This was latent until DRA-111 put both families in one
class; before that no marked class held two same-shape receivers.

**The generated receivers are deliberately not marked as generated code.** Hint
names end `.AwaitedReceivers.cs`, not `.g.cs`, and the members carry no
`[GeneratedCode]`. Both omissions are load-bearing: Roslyn suppresses analyzers on
source it considers generated, and that would take RS0016/RS0017 with it — which
are the entire proof that the generated surface matches the hand-written one it
replaced. Verified by marking a class and watching RS0016 fire on the emitted file.
A side effect is that coverlet's default `ExcludeByAttribute` never matches, so the
generated members stay in the coverage denominator and the specs covering them keep
counting. `DoesNotMarkTheEmittedSourceAsGeneratedCode` holds the line. Note the
scope: the attribute file from `RegisterPostInitializationOutput` *is* `.g.cs`
legitimately, since it is not part of the surface RS0016/RS0017 prove. Only the
receiver files must stay `.AwaitedReceivers.cs`.

**A forwarding call must not name the type arguments of a member read from an
`extension` block.** The generator sees that member as the compatibility static
form, whose type parameter list is the block's followed by the member's — so
`MapOrNull<TOut>` arrives as `MapOrNull<T, TOut>`, and writing the member's own
`<TOut>` onto the reduced call is the wrong arity. The compiler reports that as
`CS1061`, "no accessible extension method", which reads like a missing `using` and
sends you looking in the wrong place. `AwaitedReceiverWriter.CallTypeArguments`
leaves those to inference and writes them out only for a real instance method. A
test double needs a *generic* member inside an extension block to cover this; one
with no type parameters of its own renders identically either way.

**A C# 14 extension member's doc comment is an `<inheritdoc>` to an unspeakable
type.** `GetDocumentationCommentXml()` on the compatibility static form returns a
cref naming the compiler's synthesized extension container, whose name contains an
unstable hash. Emitting it verbatim puts that hash in generated source.
`DocComments.Load` follows the cref with
`DocumentationCommentId.GetFirstSymbolForDeclarationId` to reach the real text. The
compat-static form is also how a Roslyn-4.8-built generator sees extension members
at all, since `ExtensionBlockDeclarationSyntax` does not exist in its reference
assembly.

**`StringBuilder.AppendLine` writes CRLF on Windows, so emitted source would vary
by build platform.** `AwaitedReceiverWriter` normalises the line endings before
returning, and the snapshot test normalises its expected literal too, since git may
check the test file out either way.

## Converting a family

Read the baseline, do not estimate. Apply the attributes, build, and read the
RS0016/RS0017 pair: it names the exact parameter and type parameter drift between
the hand-written extension and the core member it forwards to. A family converts
with an untouched baseline only when the two already agree. See
[Waystone.Monads](../Waystone.Monads/AGENTS.md) for what each kind of drift costs.

**Drift is not the only blocker.** DRA-108 tried all eight remaining families and
landed one — `Result.Match`. Six were parameter renames, which DRA-110 owns.
`Option.Match` was neither, and what it turned out to be is the generalisable
lesson: **when a conversion appears to remove overloads, check which receiver the
hand-written ones sit on before reaching for the generator.**

Converting `Option.MatchExtensions` removed six overloads — the three
async-delegate shapes on each of the `Task` and `ValueTask` receivers — because
every one of its hand-written overloads was on an awaited receiver, and
`FromExtensionBlocks` skips those. `Result.Match` lost nothing from the identical
attempt because its async-delegate shapes sit on the synchronous
`Result<TOk, TErr>` receiver, so they were lifted. Both core types declare the same
four-overload `Match` set, so the core surface was never the difference.

DRA-130 fixed it by adding the three synchronous-receiver overloads Option was
missing and then converting, which measured 0 RS0017 and 38 RS0016. Note what that
means for the shape of the problem: an apparent removal was a *missing addition*,
and the family was the only Option family with no synchronous-receiver block at
all. Reach for that check first.

The lead recorded here before DRA-130 — that Option's lost overloads all involved a
parameterless `Func<Task<TOut>>` branch where Result's took the contained value —
was a coincidence of which overloads Option happened to ship. It is called out
because it is the kind of pattern that reads like a cause and costs a day.

`OkOrElseExtensions` is the other trap, and it is downstream of the same thing. It
forwards through `optionTask.MatchAsync(...)` with a value-returning `async` lambda
in the `None` branch. Convert without the synchronous overloads and resolution
falls to the generated `MatchAsync(Action<T>, Action)` shape, so the lambda becomes
a void-returning conversion and fails as `CS8030` in a file with nothing wrong in
it. With the synchronous overloads present it resolves correctly and needs no edit,
so treat a `CS8030` there as a symptom of the missing block rather than a call site
to fix.

Run the experiment on the whole set at once rather than one family at a time:
one build reports every family's verdict, and the count of RS0017 rows per class
is the verdict. Do it before the core members change, so a failed conversion
does not also hide fresh RS0016 rows.
