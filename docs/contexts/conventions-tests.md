# `Waystone.Conventions.Tests`

Read before adding a test to `test/Waystone.Conventions.Tests` or editing its `.csproj`.

**It holds rules about the tree, not about behaviour.** A test belongs there when its
subject is what the build produced — a file the compiler wrote, a naming rule spanning
projects, a layout that has to hold — and belongs in a type's own test project when it
exercises that type. It targets `net8.0` alone, because a property of the tree answers the
same on every runtime and the matrix would prove it five times.

## The build-ordering references are what make the guard real

It carries `ProjectReference` items to everything under `src/` with
`ReferenceOutputAssembly="false"`, which is build ordering rather than a dependency —
nothing there is on its compile line. Reading build output without it means reading whatever
the last build left: a targeted `dotnet test` never rebuilds the library, so the guard passes
against a doc comment that has just reintroduced the very leak it exists to catch.

Do not add `SkipGetTargetFrameworkProperties`; it forces `net8.0` onto each reference instead
of letting them negotiate, and the `netstandard2.0` analyzers then fail `NETSDK1005` for a
target they never had.

## The documentation scan

`PackagedDocumentationTests` fails the build on a `WA` or `WSG` id in a shipped XML doc
comment. Members under `Waystone.Internal.*` are exempt: the awaited-receiver attributes
document a contract for this repository's own authors, and a consumer cannot reach them.

**That exemption reads a namespace, not an accessibility, and the difference is pinned
rather than ignored.** The two attributes are `internal`, but the scan skips them because of
where their doc id says they live — put a genuinely public type under `Waystone.Internal.*`
and the proxy diverges from the invariant it stands in for. Reading the real accessibility
means resolving each doc id back to a symbol, which needs the assembly loaded, and a net8.0
test host cannot load all of them.
`TheInternalNamespaceExemptionCoversOnlyTheGeneratorAttributes` pins the exempted set to
those two members instead, so a third arriving under that namespace fails and has to be
confirmed internal by hand.

`EveryPackableProjectHasDocumentationToScan` fails when a project yields no XML, so a scan
that found nothing cannot pass as a scan that found no problem — `src/Directory.Build.props`
sets `GenerateDocumentationFile` for everything under `src/`, so a project with no XML was
not built rather than undocumented. And a project counts as packable unless its `.csproj`
text carries `<IsPackable>false</IsPackable>` spelled exactly that way — check the opt-outs
still match with `grep -rl '<IsPackable>false</IsPackable>' src/*/*.csproj`. A project that
escaped the match would be scanned and required to have XML, which errs toward checking too
much rather than too little.

**One source comment is not one shipped line.** The awaited-receiver generator lifts a
comment onto every generated member it produces, so five source comments ship as thirteen
citations, and a single `MapAsync` remark ships three. Read the count off the built `.xml`,
never off the source.
