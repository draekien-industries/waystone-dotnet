# Waystone.Internal.SourceGenerators

Emits the awaited receiver shapes — the `Task<Option<T>>` and
`ValueTask<Option<T>>` overloads that forward into a core member — for the
extension classes in `Waystone.Monads`.

## The generator contract

**There are two seeds.** `Analyse` emits the union of:

* `FromReceiverMember` — the written list. A destination class carries
  `[GenerateAwaitedReceivers(typeof(Option<>))]` naming the receiver and one
  `[GenerateAwaitedMember(nameof(Option<>.Unwrap))]` per core member it wants.
* `FromExtensionBlocks` — every public extension member already in the destination
  class **whose receiver is not itself awaitable**, lifted onto both awaited
  receivers, unless it carries `[ExcludeFromAwaitedReceivers]`.

The written list is not a mapping: there is exactly one destination class per monad,
`OptionExtensions` and `ResultExtensions`. It answers *which* core members get
lifted.

**Nothing here notices a core member left off every list** — the generator emits
what it is told, so the failure is a missing overload rather than a diagnostic.
Two causes produce it and they need different fixes: a core member whose family has
no destination class, and one whose family has a destination class that does not
name it. `AwaitedReceiverCoverageTests` in `Waystone.Monads.Tests`
fails when a public core member has no `…Async` reachable from either awaited
receiver. It asserts on the emitted surface rather than on the attributes, so it
covers both causes and a hand-written family equally.

The lifted half carries the shapes the core member does not have — an
async-delegate overload, which exists only as an extension. A hand-written member
on a synchronous receiver silently gains two awaited overloads; one on an awaited
receiver contributes nothing and will be deleted by the conversion. **Which
receiver a hand-written overload sits on is part of the contract.**

**`[ExcludeFromAwaitedReceivers]` on the member is the only way to decline the
lift.** It exists for `Option.With` and `Result.With`, whose awaited
form returns a task of a state binder — a shape nobody can chain. The seed filters
on `IsExtensionMethod`, so a classic `static (this T)` method is lifted exactly
like an `extension` block member and rewriting one in classic form does **not**
dodge it. The attribute is read only inside
`FromExtensionBlocks` and is inert elsewhere, including on a member named by
`GenerateAwaitedMember` — that list is opt-in, so leave the name off it instead.

Write the member with `nameof`, not a bare string. C# 14 takes an unbound generic
in `nameof`, so `nameof(Option<>.Unwrap)` compiles, matches the `typeof(Option<>)`
above it, and makes a rename fail the build instead of silently dropping a family
into `WSG0002`.

`GenerateAwaitedMember.Summary` overrides the synthesised summary per member, where
the source member's own wording does not read well after the await prefix.

**Four parameter attributes reach the generated member and every other one is
dropped.** `CallerInfo` carries `[CallerMemberName]`, `[CallerFilePath]`,
`[CallerLineNumber]` and `[CallerArgumentExpression]`, because the compiler fills
those at the outer call site and the forward hands the value straight on. Nothing
else has a meaning that survives being forwarded —
`WA0004` in
[Waystone.Internal.Analyzers](../Waystone.Internal.Analyzers/AGENTS.md) is what
stops a dropped one reaching a build, and the two lists of four have to stay in
step.

`CallerArgumentExpression` needs more than a copy. Its target is a parameter name,
and the receiver is the one parameter a generated member renames, so
`nameof(option)` on the source becomes `"optionTask"` on the generated member while
a target naming any other parameter is written through unchanged. The target is
emitted as a string literal rather than a `nameof` deliberately: a name matching
nothing then reports the compiler's own "will have no effect" warning — the same
diagnostic the source member already gets — instead of a `CS0103` against generated
source, which is the harder of the two to act on.

**`ReceiverParameterName` pins the awaited receiver's name, for keeping a name
rather than choosing one.** The default is the source receiver's name with `Task`
appended — `option` becomes `optionTask` — and that name is in the public API
baseline, where renaming it is source-breaking for a caller naming it in static
invocation syntax. A class whose awaited shapes were hand-written before they were
generated has to pin whatever they were already called, or the conversion that
changes nothing else still moves every baseline row. `Waystone.Monads.Shouldly`
pins `actual` for exactly that. A new class has nothing to preserve: leave it unset.

Pinning the source receiver's own name works: the local holding the awaited value
is renamed to `awaited` instead of colliding with the parameter it is awaiting.
`CallerInfo` re-points a `CallerArgumentExpression` at whatever the pin decided.

A dropped `[CallerArgumentExpression]` is silent in every channel. The generated
assertion still compiles, still ships, and merely stops naming the caller's
expression in its failure message; the public API baseline does not record parameter
attributes, so RS0016/RS0017 say nothing either.

**The generator writes only the `extension` block; the containing class must be
`partial`.** Generated shapes land in the same static class as the hand-written
ones, so the baseline entries keep naming `OptionExtensions`; a rename there would
be a public API change. Two blocks of the same receiver shape in the same partial
class across two files merge without complaint, so the generator can add to a class
that already hand-writes one. `WSG0001` catches a marked class that is not partial,
because otherwise the failure is a CS0260 pointing at generated source.

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
rendered block constraints.

**The generated receivers are deliberately not marked as generated code.** Hint
names end `.AwaitedReceivers.cs`, not `.g.cs`, and the members carry no
`[GeneratedCode]`. Both omissions are load-bearing: Roslyn suppresses analyzers on
source it considers generated, and that would take RS0016/RS0017 with it — which
are the entire proof that the generated surface matches the hand-written one it
replaced. A side effect is that coverlet's default `ExcludeByAttribute` never
matches, so the generated members stay in the coverage denominator.
`DoesNotMarkTheEmittedSourceAsGeneratedCode` holds the line. The attribute file from
`RegisterPostInitializationOutput` *is* `.g.cs` legitimately, since it is not part
of the surface RS0016/RS0017 prove. Only the receiver files must stay
`.AwaitedReceivers.cs`.

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

**A polyfilled attribute is an error type here, and reading it off the symbol gets
nothing.** `[CallerArgumentExpression]` in a `netstandard2.0` project resolves
through PolySharp, whose declaration is another generator's output and therefore
invisible to this one — the nearest candidate left is the `internal` copy inside
`Waystone.Monads`, which is not accessible, so `AttributeClass` comes back with the
right name and `TypeKind.Error`. An error type has no bound constructor, so
`ConstructorArguments` is *empty* rather than wrong, and a reader that trusts it
silently emits no attribute at all. `CallerInfo.Target` falls back to the
application syntax for that reason.

The name still matches because Roslyn keeps it on the error symbol; only the
arguments are missing. The package compiles either way and only the assertion
failure text changes, so nothing but reading the emitted attribute catches it.

**`StringBuilder.AppendLine` writes CRLF on Windows, so emitted source would vary
by build platform.** `AwaitedReceiverWriter` normalises the line endings before
returning, and the snapshot test normalises its expected literal too, since git may
check the test file out either way.

## Converting a family

Read the baseline, do not estimate. Applying the attributes and reading the RS0016/RS0017
pair is the only reliable verdict, and an apparent removal is usually a missing addition.
The procedure, the two traps it has, and what each kind of drift costs are in
[docs/contexts/awaited-receiver-conversion.md](../../docs/contexts/awaited-receiver-conversion.md).
