# The public API baseline

Read before adding, changing or removing a public member, and whenever a build
reports RS0016 or RS0017.

Applies to every package under `src/` carrying a `PublicAPI.Shipped.txt`. List them:

```
find src -iname PublicAPI.Shipped.txt
```

`Microsoft.CodeAnalysis.PublicApiAnalyzers` runs against that file: an added member
fails RS0016, a removed one RS0017. Both are build errors, and they are what enforces
**deprecate; never remove** rather than review attention.

Let the analyzer's own code fix write the entries; do not hand-edit the format.

Move rows from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt` **before**
merging, filed under the version GitVersion will compute from the PR title. Merging
publishes, and there is no later release step that would move them, so a row left
unshipped is wrong from the moment the PR lands. `pre-push` fails on one.

**The baseline does not record generic constraints.** It records names, types and
nullability, so a `where` clause can be added or relaxed with RS0016/RS0017 silent. A
relaxation is source-compatible and not a break, but the baseline is the whole
argument that a refactor changed nothing and constraints are outside it. Diff the
`where` clauses by eye whenever a change touches a member that had any.

## Harvesting rows without an IDE

Each RS0016 reads ``Symbol '<row>' is not part of the declared public API``, and
`<row>` is the baseline line verbatim. Regex them out of `dotnet build`, merge into
`PublicAPI.Shipped.txt` and re-sort — the file is ordinal-sorted (`LC_ALL=C`) below
the `#nullable enable` header. A clean rebuild is the check that the format came out
right.

**`dotnet format analyzers --diagnostics RS0016` does nothing.** The code fix writes
to an `AdditionalFile`, and `dotnet format` only applies document changes. It exits
reporting "Formatted 0 of 96 files" and looks like a passing run.

## A change produces more rows than you wrote

One member in a C# 14 `extension` block produces **three** baseline entries: the
`extension<T>(Receiver)` container, the member, and the compiler's compatibility
`static Member<T>(this Receiver)` form. You author none of the extra ones, so expect
the baseline to grow by more lines than you wrote. The compat-static entry records the
receiver's nullability independently, so a block whose receiver is subtly wrong is
caught there even when the member entry matches.

**A core member addition can grow the async surface too.** Adding an overload to
`Option<T>` or `Result<TOk, TErr>` makes the awaited-receiver generator emit the
matching `…Async` shapes for every family already converted to
`[GenerateAwaitedReceivers]`, and those land in the baseline as well — nine members on
`Option<T>` produce 39 rows. Families still hand-written get nothing, so the async
surface goes asymmetric until they are converted. That is fine inside a stack that
lands as one release and wrong to ship on its own.

## `Waystone.Monads.Schemas` splits its baseline per target framework

`SchemaViolation`'s compiler-generated `<Clone>$` returns `Error` on netstandard2.0 and
`SchemaViolation` elsewhere — a derived record gets a covariant return only where the
runtime supports one — so a single shared baseline is impossible there, `Schema.Date`
aside. The root pair carries what every target shares; each per-target folder carries
only what that one alone has, and a framework added to the csproj needs its folder in
the same change. PublicApiAnalyzers *unions* every `AdditionalFile` of a given name,
which is what makes this work and was verified by experiment rather than assumed.

Harvest there with the target framework kept on each row:

```
dotnet build … | grep -oE "Symbol '[^']+' is not part.*TargetFramework=[a-z0-9.]+"
```

That gives rows tagged by target framework, ready to `comm` into shared and per-target
sets.
