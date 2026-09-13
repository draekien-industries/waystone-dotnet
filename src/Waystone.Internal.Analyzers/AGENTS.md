# Waystone.Internal.Analyzers

Rules that hold **this repository's own source** to invariants the compiler does
not check. Nothing here reaches a consumer.

Every analyzer in this repository that does not ship lives here. Three families —
`FactoryGuards/` holds `WA0001`, `AsyncSurface/` holds `WA0002` and `WA0003`,
`AwaitedReceivers/` holds `WA0004` — and the only thing they share is that a
consumer could not act on any of them. That is the whole membership test.

## Not a package

`IsPackable` is false and the assembly is absent from the `PackMonadAnalyzers`
allow-list in `src/Waystone.Monads/Waystone.Monads.csproj`, which names the three
analyzer assemblies that go into the nupkg one by one. Adding a project to `src/`
does not ship it; being on that list does.

The distinction is load-bearing rather than tidiness. `WA0001` tells you to call
`Option.NotNull`, which is `internal` — a consumer could not comply if they wanted
to. A rule nobody outside the assembly can satisfy must not leave it.

[Waystone.Internal.SourceGenerators](../Waystone.Internal.SourceGenerators/AGENTS.md)
is the other build-time-only project and is also off that list. An analyzer belongs
here; that project emits source and reports only on its own emission. `WA0002` and
`WA0003` lived there until 7.3.0 only because they were written alongside the
generator, and two of the three projects importing its props wanted nothing but the
analyzer.

**The line is what the rule reports *about*, not what it knows about.** `WA0004`
knows the generator's attributes, its lifting conditions and the set of parameter
attributes it carries, and still belongs here: its subject is a hand-written member
in this tree. `WSG0001` and `WSG0002` stay with the generator because their subject
is the generator failing to emit — a class it cannot add to, a member name it cannot
resolve. Ask which of the two a reader would go and edit.

**A rule here may be an `Error`.** The prohibition in
`.claude/skills/writing-diagnostic-descriptors` is about a rule shipping inside
`Waystone.Monads` and breaking the build of someone who only wanted a version bump;
that reasoning does not reach a rule whose only subject is our own source, where
failing the build is the entire mechanism. The rest of that skill — the title,
message and description shapes, and the `RS1031`/`RS1032`/`RS1033` punctuation the
build enforces — applies unchanged.

## Opting a project in

```xml
<Import Project="..\Waystone.Internal.Analyzers\Waystone.Internal.Analyzers.props"/>
```

That import is the whole contract. `Waystone.Monads`, `Waystone.Monads.Schemas` and
`Waystone.Monads.FluentValidation` import it. Any project under `src/` may: the rules
are written so that one which declares nothing for them to act on is silent rather
than noisy, which is what makes importing it cheap enough to be the default.

## The three families

**A rule here may name `Option` and `Result`, and one pair does.** Until 7.3.0 this
file carried the opposite instruction. It was never about the types; it was the only
available way to tell an internal analyzer project apart from the shipping one while
both were called `Waystone.*.Analyzers`. The `Internal` segment now carries that
distinction. **Do not reinstate the constraint.** The membership test is whether a
consumer could act on the rule, not whether it knows a type by name.

`WA0002` and `WA0003` name the monads outright, through `AsyncSurface/MonadTypes.cs`.
They report a public member or delegate parameter returning a `Task` of one of our
own monads, and there is no shape that generalises — the whole point is *which* types
are involved, because only this library's chains produce a shape a caller cannot
convert.

`WA0001` names nothing. A guard is any method matching `GuardConvention` — static,
named `NotNull` or `NotNullAsync`, taking a value and a `string` naming whatever
produced it, and returning that value's own type with `Task` or `ValueTask`
unwrapped. The types it guards are then whatever those methods return, so writing a
guard is what extends the rule, and a compilation with no guards has no guarded types
and is never reported.

**Recognition is by shape, not by an attribute, and the trade is deliberate.** An
attribute would survive a rename; a convention does not. What it buys is that there
is no marker type to compile into the importing project, no `Compile` item in the
props, and nothing to apply and forget to apply — the signature already said
everything the attribute would have.

`WA0004` names no monad, but holds a copy of a list that lives in another project:
the four caller-info attributes `Waystone.Internal.SourceGenerators`' `CallerInfo`
writes onto a generated parameter. **Nothing keeps the two in step.** The generator is
loaded as an analyzer and is on nobody's compile line, so there is no reference to
share the list through, and teaching one side to carry a fifth attribute without the
other makes this rule fail a build over an attribute that is in fact carried. Change
`AwaitedReceiverContract.Carried` and `CallerInfo` in the same commit.

**Read the marked class's members; do not walk up from the method.** A C# 14
`extension` block member is declared inside a compiler-generated container, so its
`ContainingType` is that container rather than the class carrying
`[GenerateAwaitedReceivers]`, and it does not report `IsExtensionMethod` either. A
`SymbolKind.Method` action over such a member therefore matches nothing and the rule
goes silent, which looks exactly like a clean tree. `WA0004` registers on
`SymbolKind.NamedType` and enumerates `GetMembers()`, which hands back the
compatibility static form the generator itself reads. The classic `static (this T)`
form works either way, so a test using only that shape passes against the broken
version.

**The rename hazard is closed by a test, not by the build.** `GuardConventionTests`
in `Waystone.Monads.Tests` asserts all four guards still match the convention.
Without it, renaming `Option.NotNull` turns `WA0001` off across the whole library
while every other test keeps passing. Any new guarded type needs its row there in the
same change.

## Release tracking

`AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` are maintained even
though nothing ships, because `RS2008` fails the build on an untracked id and
`EnforceExtendedAnalyzerRules` is on. The version headings are bookkeeping here rather
than a promise to anyone.

Ids are `WA` and are permanent. The other spaces in this repository are `WM`, `WMS`,
`WSG` and `WMSC`; do not reach into one of them for a rule that belongs here. The
space is flat — there is no tier scheme like the `WM1`/`WM2`/`WM3` one in
`Waystone.Monads.Analyzers`, because the tiers there encode a severity promise to a
consumer and every rule here is an `Error`. Take the next free number.

**`WA0002` and `WA0003` were `WSG0003` and `WSG0004` until 7.3.0**, when they moved
off the generator. Both old ids are retired and neither is ever reused, so a
suppression written against one fails loudly instead of silently redirecting onto a
rule its author never saw. The move is recorded as a `### Removed Rules` row in the
generator's own `AnalyzerReleases.Shipped.md` rather than by deleting the rows it
already shipped — the rules did exist under those ids, and the file is the record of
that. Renumbering was free here only because nothing ships and no `.editorconfig` in
the repository named either id; check both before assuming the same of a future move.
