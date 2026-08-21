# Waystone.SourceGenerators

Emits the awaited receiver shapes — the `Task<Option<T>>` and
`ValueTask<Option<T>>` overloads that forward into a core member — for the
extension classes in `Waystone.Monads`.

## The generator contract

**The emitted set is a written list, not a walk of the receiver's surface.** A
destination class carries `[GenerateAwaitedReceivers(typeof(Option<>))]` naming the
receiver and one `[GenerateAwaitedMember(nameof(Option<>.Unwrap))]` per member it
wants. That is deliberate: which extension class a core member's async shape
belongs in is not derivable — the mapping is nearly `{Member}Extensions`, but
`UnwrapOr` and `UnwrapOrDefault` live in `UnwrapExtensions`, and `Or`/`Xor` have no
class at all. A written list is the strongest available form of "emit exactly
today's set".

Write the member with `nameof`, not a bare string. C# 14 takes an unbound generic
in `nameof`, so `nameof(Option<>.Unwrap)` compiles, matches the `typeof(Option<>)`
above it, and makes a rename fail the build instead of silently dropping a family
into `WSG0002`.

`GenerateAwaitedMember.Summary` overrides the synthesised summary per member, for
cases where the source member's own wording does not read well after the await
prefix.

**The generator writes only the `extension` block; the containing class must be
`partial`.** Generated shapes land in the same static class as the hand-written
ones so the baseline entries keep naming `MapExtensions` rather than some new class
— a rename there would be a public API change. Two blocks of the same receiver
shape in the same partial class across two files merge without complaint, so the
generator can add to a class that already hand-writes one. `WSG0001` catches a
marked class that is not partial, because otherwise the failure is a CS0260
pointing at generated source.

**A new rule needs an `AnalyzerReleases.Unshipped.md` entry in the same change.**
RS2008 fails the build without one.

## Gotchas

**The generated receivers are deliberately not marked as generated code.** Hint
names end `.AwaitedReceivers.cs`, not `.g.cs`, and the members carry no
`[GeneratedCode]`. Both omissions are load-bearing: Roslyn suppresses analyzers on
source it considers generated, and that would take RS0016/RS0017 with it — which
are the entire proof that the generated surface matches the hand-written one it
replaced. Verified by marking a class and watching RS0016 fire on the emitted file.
A side effect is that coverlet's default `ExcludeByAttribute` never matches, so the
generated members stay in the coverage denominator and the specs covering them keep
counting.

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
