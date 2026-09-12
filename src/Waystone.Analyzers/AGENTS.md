# Waystone.Analyzers

Rules that hold **this repository's own source** to invariants the compiler does
not check. Nothing here reaches a consumer.

## It is not a package, and that is the point

`IsPackable` is false and the assembly is absent from the `PackMonadAnalyzers`
allow-list in `src/Waystone.Monads/Waystone.Monads.csproj`, which names the three
analyzer assemblies that go into the nupkg one by one. Adding a project to `src/`
does not ship it; being on that list does.

The distinction is load-bearing rather than tidiness. `WA0001` tells you to call
`Option.NotNull`, which is `internal` — a consumer could not comply if they wanted
to. A rule nobody outside the assembly can satisfy must not leave it.

This mirrors [Waystone.SourceGenerators](../Waystone.SourceGenerators/AGENTS.md),
which is also generic, also build-time only, and also off that list. Read its
`AGENTS.md` before adding a second rule here — the two projects answer the same
question about what belongs in a repository-wide tool.

**A rule here may be an `Error`.** The prohibition in
`.claude/skills/writing-diagnostic-descriptors` is about a rule shipping inside
`Waystone.Monads` and breaking the build of someone who only wanted a version bump.
That reasoning does not reach a rule whose only subject is our own source, where
failing the build is the entire mechanism. The rest of that skill — the title,
message and description shapes, and the `RS1031`/`RS1032`/`RS1033` punctuation the
build enforces — applies unchanged.

## Opting a project in

Import the props. One line, and it is the whole contract:

```xml
<Import Project="..\Waystone.Analyzers\Waystone.Analyzers.props"/>
```

Only `Waystone.Monads` imports it today. Any project under `src/` may, and the
rules are written so that one which declares nothing for them to act on is silent
rather than noisy.

## The rules name no types

**A rule here must not mention `Option` or `Result`.** This project sits beside
the monads rather than inside them, and a rule that hardcoded their names would be
`Waystone.Monads.Analyzers` with the packaging left off.

`WA0001` reads its subject out of the compilation instead. A guard is any method
matching `GuardConvention` — static, named `NotNull` or `NotNullAsync`, taking a
value and a `string` naming whatever produced it, and returning that value's own
type with `Task` or `ValueTask` unwrapped. The types it guards are then whatever
those methods return, so writing a guard is what extends the rule, and a
compilation with no guards has no guarded types and is never reported.

**Recognition is by shape, not by an attribute, and the trade is deliberate.** An
attribute would survive a rename; a convention does not. What it buys is that
there is no marker type to compile into the importing project, no `Compile` item
in the props, and nothing to apply and forget to apply — the signature already
said everything the attribute would have.

**The rename hazard is closed by a test, not by the build.** `GuardConventionTests`
in `Waystone.Monads.Tests` asserts all four guards still match the convention.
Without it, renaming `Option.NotNull` turns `WA0001` off across the whole library
while every other test keeps passing. Any new guarded type needs its row there in
the same change.

## Release tracking

`AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` are maintained
even though nothing ships, because `RS2008` fails the build on an untracked id and
`EnforceExtendedAnalyzerRules` is on. The version headings are bookkeeping here
rather than a promise to anyone.

Ids are `WA` and are permanent. The other spaces in this repository are `WM`,
`WMS`, `WSG` and `WMSC`; do not reach into one of them for a rule that belongs
here.
