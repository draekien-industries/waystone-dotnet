# Waystone.Monads.Schemas.SourceGenerators

Emits the members a schema declared as a set of fields cannot write for itself: the
shared `Instance`, and the `Schema.Fields` ladder at every arity its `Configure`
body uses.

## Why this is its own project

`Waystone.Monads.SourceGenerators` is packed into `Waystone.Monads`, so anything
placed there loads into the compilation of every consumer of the monads package.
This generator has no attribute to key on — it triggers on *inheritance*, so its
predicate matches every class with a base list and its transform asks the compiler
for that class's symbol. Shipped with `Waystone.Monads`, that work would run over
every class in codebases that never installed the schema package, and
`WMSC0001`–`WMSC0005` would reach people with no `SchemaConfig` to get wrong.

`Waystone.Monads.Schemas.csproj` therefore carries its own `PackSchemaAnalyzers`
target rather than a condition added to `PackMonadAnalyzers`. **That target is
conditioned on `netstandard2.0`, and the condition is load-bearing**: the runtime
package multi-targets, `TargetsForTfmSpecificContentInPackage` runs once per
framework, and both runs would place the same file at the same package path.

## The id space is `WMSC`, not `WMS`

`WMS` is taken — `Waystone.Monads.Shouldly.Analyzers` ships `WMS2001` and
`WMS2002`. The numbers do not collide, but one prefix across two unrelated packages
means an `.editorconfig` entry for `dotnet_diagnostic.WMS*` silences both.
`RulesTests` pins the prefix so a new rule cannot drift to `WMS`.

The help link points at `source-generation/diagnostics`, the page that already
carries `WMG` and `WSG`. The anchor has to keep resolving forever — a consumer
reaches it from the build output of versions long past.

## What the generator decides, and why

**It resolves `SchemaConfig<,>` by metadata name and never references the runtime
package.** A generator that referenced its own runtime would load a second copy of
it into every consumer's compiler. There is no `WMG0004` equivalent here: the
trigger *is* deriving from `SchemaConfig`, so a compilation that cannot resolve it
has a class that does not compile and a generator that correctly matched nothing.

**An abstract schema is skipped in silence.** It has no shared instance to offer and
nothing about it is wrong; a diagnostic there would fire on every intermediate base
in the tree.

**`WMSC0001` is reported against the type missing the modifier, which is not always
the schema.** A nested schema needs every type containing it to be `partial` too,
and the declaration a reader has to edit is the outermost one that is not.

**A `private` parameterless constructor is enough.** The generated `Instance` sits
inside the schema, so `WMSC0002` asks whether a parameterless constructor *exists*,
not whether anyone else could reach it. `SchemaConfig` supplies a protected one, so
a derived schema has an implicit constructor until it declares a constructor of its
own — at which point the implicit one disappears with no diagnostic from the
compiler.

**`WMSC0003` covers every name the generator writes, not just `Instance`.** The other
two are the nested `Schema` and the `FieldSet` struct, checked only where a ladder is
being emitted — a schema that never calls `Schema.Fields` receives neither name and
may keep a member of either. Arity does not separate them: `CS0102` fires on a nested
`FieldSet<T1>` beside a property called `FieldSet`. `SchemaWriter` holds all three
names as constants so the guard and the emission cannot drift.

**The generator anchors on the first part carrying a base list, not the first part.**
A partial class reaches the pipeline once per part that names a base type, and
emitting from each would add the same hint name twice. Anchoring on
`DeclaringSyntaxReferences[0]` instead looks equivalent and is not: a part with no
base clause never reaches the transform, so a schema whose base clause is written
on a later part would generate nothing at all.

**The base-type walk compares the arity-bearing metadata name before the namespace.**
`MetadataName` is a string the symbol already holds; the namespace has to be
rendered. The order matters because this runs over every class with a base list in
the consumer's compilation, and almost none of them are schemas.

**The reopening declaration carries no accessibility and no constraints.** A partial
declaration may omit both, and repeating either only creates a second place they can
disagree. The type parameters *are* repeated, because they have to be.

## The ladder

**A generator cannot add an overload to a type in another assembly**, so
`Schema.Fields` cannot be widened where `Schema` is declared. The generator instead
nests `private sealed class Schema : global::Waystone.Monads.Schemas.Schema` inside
the consumer's own partial class and puts the overloads there. Static members are
inherited in C#, so `Schema.Text` and `Schema.Required` still resolve through it
unchanged, and the generator forwards nothing — a primitive added to the runtime
later needs no generator change.

**The arity is read syntactically, and it has to be.** `Schema.Fields` is the member
being generated, so it binds to nothing while the generator is deciding whether to
generate it. Everything else about the chain binds normally, which is why
`WMSC0005` can ask what a `Refine` argument actually yields.

**Reading it syntactically means the receiver is matched on its last name.** Both
`Schema.Fields(...)` and `OrderSchema.Schema.Fields(...)` reach the ladder; anything
else — an unqualified `Fields(...)`, `this.Fields(...)`, a receiver of another name —
does not, and gets `WMSC0007` saying so. **That rule fires only where the call binds
to nothing**, which is what keeps it off a consumer's own method named `Fields`. It
is the one warning here about code that does not compile: the compiler already
reports the missing member, and this adds the reason.

A consumer who writes `using Schema = Something;` is outside all of it: the call
matches by name, so a ladder is generated and nothing is reported, and the alias
sends it somewhere else. Do not try to detect it: that means re-implementing alias
resolution for the one member that cannot bind.

**`WMSC0008` re-runs the runtime's own path derivation at build time.** A field's
path comes from `CallerArgumentExpression`, and `PathName.From` keeps whatever
follows the last dot — so a method call, an indexer or a literal leaves its
punctuation in a path that reaches logs and API responses. `FieldNames` applies the
same rule to the same text and names `.Named(...)` as the fix. The two derivations
have to stay in step and cannot be shared, because the generator does not reference
the runtime; a test asserts the derived path in the message rather than only the id,
so a change to one shows up as a failure rather than as drift.

**The ladder type is `FieldSet<T1..Tn>`, never `Fields`.** A member named `Fields`
would hide a namespace-level `Fields<,>` in type-name lookup. Only the method is
called `Fields`.

**`FieldAccumulator` is the seam and the only one.** Evaluating a field is internal
to the runtime and generated code compiles in the consumer's assembly, so every
generated `Into` goes through that public type. Widening the ladder starts in
`Waystone.Monads.Schemas`, not here.

**The emitted `Into` branches on `HasViolations` itself rather than handing the
accumulator a delegate.** Generated code runs on every parse, so avoiding the
closure a `Complete(Func<TOut>)` seam would allocate is worth two extra emitted
lines.

**There is no arity cap.** Flat thirty-field objects arrive from external APIs, and
failing legitimate code is worse than emitting a wide type.

## The emitted constraints depend on the consumer's language version

`where T1 : notnull` is C# 8. Before that the word parses as a missing type and the
generated file is a build error in someone else's project. Omitting it there costs
nothing, because a compiler that cannot spell the constraint does not check
nullability either.

**Read that from `ParseOptionsProvider`, never from `CompilationProvider`.** The
compilation changes on every keystroke and combining with it would defeat the cache
for every schema in the solution. Parse options change when the project file does.

The transform builds a `SchemaModel` of plain values rather than source, because the
writer has to run after the language version is known, and because a cached symbol
compares by reference — the cache never hits, and it roots the compilation it came
from.

## Emission constraints

* **Emitted source targets C# 7.3**, not this repository's language version. It
  compiles in the consumer's project, and a `net472` project still defaults to 7.3.
  A static auto-property with an initializer is C# 6 and safe; a switch expression
  or a target-typed `new` is not.
* **Doc comments use `<c>`, never `<see cref>`.** An unresolved cref is `CS1574`,
  an error under a consumer's `TreatWarningsAsErrors`.
* **Hint names end `.g.cs` with an `// <auto-generated/>` header, and
  `[GeneratedCode]` stays absent** — coverlet's default `ExcludeByAttribute` keys on
  it and would drop the generated members from the coverage denominator.
* **Append `'\n'` directly; never `StringBuilder.AppendLine`**, which writes CRLF on
  Windows and makes the emitted source vary by build platform.
* A hint name spells a generic type's arity with an underscore. The metadata name
  uses a backtick, which is not a character to put in a file name.

## Testing

`SchemaGeneratorTests` and `LadderGeneratorTests` pin the emitted text and each
diagnostic. `GeneratedInstanceTests` and `GeneratedLadderTests` are not optional:
this project loads the generator as an analyzer, so the schemas at the bottom of
those files are compiled against emitted source rather than written source. **A
snapshot proves text; only those files prove the text compiles, binds, infers and
runs.** Generic inference through `Schema.Fields(...).Refine(...).Into(...)` is what
a string comparison would not notice going wrong.

`RunOnCSharp73` is the only case that exercises the constraint decision, because
every other test compiles at the latest version where both spellings are legal.

## Duplication that is deliberate

Two analyzer assemblies cannot share a runtime assembly without shipping it, so
some scaffolding here has a twin in `Waystone.Monads.SourceGenerators`. Do not
"fix" either without a new reason.

* **`DiagnosticInfo` and `EquatableArray`** are near-copies of that project's. They
  *could* be shared with a `<Compile Include="..\..." Link="..."/>` item —
  `Waystone.Monads.Analyzers.csproj` links `ErrorCodeFormat.cs` that way — but that
  precedent is a hundred lines of parsing with real logic to get wrong, and this is
  a record and a thirty-line struct. Linking would couple two generators' builds and
  put a `Waystone.Monads.SourceGenerators.ErrorCodes` using into files that have
  nothing to do with error codes.
* **`SchemaWriter.Writer`** is byte-for-byte the same fourteen-line `StringBuilder`
  wrapper for the same reason.

## Severity is not uniform

`Create` builds an error; `Advice` builds a warning; `Suggestion` builds an information
diagnostic. **The line is whether the code has a reading that is correct**, not whether
the schema generates. `RulesTests` spells out which ids warn and which suggest rather
than deriving either, so promoting a rule has to be a deliberate edit in two places.

Read [docs/contexts/wmsc-severity.md](../../docs/contexts/wmsc-severity.md) before adding
a rule: which side of that line each shipped id fell on, and why an analyzer ships in this
assembly alongside the generator.
