# Waystone.Internal.Analyzers

Rules that hold **this repository's own source** to invariants the compiler does
not check. Nothing here reaches a consumer.

Every analyzer in this repository that does not ship lives here, in a directory
named for its subject — `FactoryGuards/`, `AsyncSurface/`, `AwaitedReceivers/`. The
only thing they share is that a consumer could not act on any of them. That is the
whole membership test.

## Not a package

`IsPackable` is false and the assembly is absent from the `PackMonadAnalyzers`
allow-list in `src/Waystone.Monads/Waystone.Monads.csproj`, which names the analyzer
assemblies that go into the nupkg one by one. Adding a project to `src/` does not
ship it; being on that list does.

`WA0001` tells you to call `Option.NotNull`, which is `internal` — a consumer could
not comply if they wanted to. A rule nobody outside the assembly can satisfy must not
leave it.

[Waystone.Internal.SourceGenerators](../Waystone.Internal.SourceGenerators/AGENTS.md)
is the other build-time-only project and is also off that list. An analyzer belongs
here; that project emits source and reports only on its own emission.

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

That import is the whole contract. Any project under `src/` may take it: the rules
are written so that one which declares nothing for them to act on is silent rather
than noisy, so importing it is the default.

## The rule families

**A rule here may name `Option` and `Result`, and one pair does.** The membership
test is whether a consumer could act on the rule, not whether it knows a type by
name; the `Internal` segment is what tells this project apart from the shipping one.

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

**Recognition is by shape, not by an attribute.** An attribute would survive a
rename; a convention does not. What it buys is that there is no marker type to
compile into the importing project, no `Compile` item in the props, and nothing to
apply and forget to apply.

`WA0004` names no monad, but holds a copy of a list that lives in another project:
the caller-info attributes `Waystone.Internal.SourceGenerators`' `CallerInfo` writes
onto a generated parameter. **Nothing keeps the two in step.** The generator is
loaded as an analyzer and is on nobody's compile line, so there is no reference to
share the list through, and teaching one side to carry an attribute the other does
not makes this rule fail a build over an attribute that is in fact carried. Change
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
in `Waystone.Monads.Tests` asserts every guard still matches the convention.
Without it, renaming `Option.NotNull` turns `WA0001` off across the whole library
while every other test keeps passing. Any new guarded type needs its row there in the
same change.

## Release tracking

`AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` are maintained even
though nothing ships, because `RS2008` fails the build on an untracked id and
`EnforceExtendedAnalyzerRules` is on. The version headings are bookkeeping here rather
than a promise to anyone.

Ids are `WA` and are permanent. Do not reach into another space for a rule that
belongs here; read the spaces in use out of the tree with
`grep -rohE '"(WM|WMS|WSG|WMSC|WMG|WA)[0-9]+"' src --include=Rules.cs | sed 's/[0-9].*//' | sort -u`. The
space is flat — there is no tier scheme like the `WM1`/`WM2`/`WM3` one in
`Waystone.Monads.Analyzers`, because the tiers there encode a severity promise to a
consumer and every rule here is an `Error`. Take the next free number.

**`WSG0003` and `WSG0004` are retired and are never reused**, so a suppression
written against one fails loudly instead of silently redirecting onto a rule its
author never saw. A retired id keeps its `### Removed Rules` row in the release file
that shipped it; do not delete the rows instead.

**Renumbering a rule is free only where nothing ships it and no `.editorconfig` in
the repository names the id.** Check both before moving one.
