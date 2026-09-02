# Waystone.Monads.Schemas.SourceGenerators

Emits the members a schema declared as a set of fields cannot write for itself: the
shared `Instance`, and the `Schema.Fields` ladder at every arity its `Configure`
body uses.

## Why this is its own project

`Waystone.Monads.SourceGenerators` is packed into `Waystone.Monads`, so anything
placed there loads into the compilation of every consumer of the monads package.
This generator has no attribute to key on — the design triggers on *inheritance*,
so its predicate matches every class with a base list and its transform asks the
compiler for that class's symbol. Shipped with `Waystone.Monads`, that work would
run over every class in codebases that never installed the schema package, and
`WMSC0001`–`WMSC0005` would reach people with no `SchemaConfig` to get wrong.

`Waystone.Monads.Schemas.csproj` therefore carries its own `PackSchemaAnalyzers`
target rather than a condition added to `PackMonadAnalyzers`. **That target is
conditioned on `netstandard2.0`, and the condition is load-bearing**: the runtime
package multi-targets, `TargetsForTfmSpecificContentInPackage` runs once per
framework, and both runs would place the same file at the same package path.

## The id space is `WMSC`, not `WMS`

`WMS` was the design's choice and is already taken — `Waystone.Monads.Shouldly.Analyzers`
ships `WMS2001` and `WMS2002`. The numbers would not have collided, but one prefix
across two unrelated packages means an `.editorconfig` entry for
`dotnet_diagnostic.WMS*` silences both. `RulesTests` pins the prefix so the next
rule cannot drift back.

The help link points at `source-generation/diagnostics`, the page that already
carries `WMG` and `WSG`. These are source-generation diagnostics like those, and
the anchor has to keep resolving forever — a consumer reaches it from the build
output of versions long past.

## What the generator decides, and why

**It resolves `SchemaConfig<,>` by metadata name and never references the runtime
package.** A generator that referenced its own runtime would load a second copy of
it into every consumer's compiler. There is no `WMG0004` equivalent here: the
trigger *is* deriving from `SchemaConfig`, so a compilation that cannot resolve it
has a class that does not compile and a generator that correctly matched nothing.

**An abstract schema is skipped in silence.** It exists to be derived from, has no
shared instance to offer, and nothing about it is wrong. A diagnostic there would
fire on every intermediate base in the tree.

**`WMSC0001` is reported against the type missing the modifier, which is not always
the schema.** A nested schema needs every type containing it to be `partial` too,
and the declaration a reader has to edit is the outermost one that is not.

**A `private` parameterless constructor is enough.** The generated `Instance` sits
inside the schema, so `WMSC0002` asks whether a parameterless constructor *exists*,
not whether anyone else could reach it. Note that `SchemaConfig` supplies a
protected one, so a derived schema has an implicit constructor until it declares a
constructor of its own — at which point the implicit one disappears with no
diagnostic from the compiler.

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

**Reading it syntactically means the receiver is matched as the bare identifier
`Schema` and nothing else.** A consumer who writes `using Schema = Something;`, or
who has a local or field of that name in scope, gets no ladder and no `WMSC` message
— only the compiler's own error against a member that was never generated. Left
alone deliberately: the alias is a name collision the consumer created inside a type
whose whole vocabulary is `Schema.`, and a rule that tried to detect it would have to
re-implement alias resolution for the one member that cannot bind.

**The ladder type is `FieldSet<T1..Tn>`, never `Fields`.** A member named `Fields`
would hide a namespace-level `Fields<,>` in type-name lookup. Only the method is
called `Fields`.

**`FieldAccumulator` is the seam and the only one.** Evaluating a field is internal
to the runtime, and generated code compiles in the consumer's assembly, so every
generated `Into` goes through that public type. Widening the ladder therefore starts
in `Waystone.Monads.Schemas`, not here.

**The emitted `Into` branches on `HasViolations` itself rather than handing the
accumulator a delegate.** Generated code runs in production on every parse, so the
closure that a `Complete(Func<TOut>)` seam would allocate each time is worth the two
extra emitted lines.

**There is no arity cap.** Generation scales, so a cap would be a policy rather than
a limit, and the policy is wrong: flat thirty-field objects arrive from external
APIs, and failing legitimate code is worse than emitting a wide type.

## The emitted constraints depend on the consumer's language version

`where T1 : notnull` is C# 8. Before that the word parses as a missing type and the
generated file is a build error in someone else's project. Omitting it there costs
nothing, because a compiler that cannot spell the constraint does not check
nullability either.

**Read that from `ParseOptionsProvider`, never from `CompilationProvider`.** The
compilation changes on every keystroke and combining with it would defeat the cache
for every schema in the solution. Parse options change when the project file does.

This is why the transform builds a `SchemaModel` of plain values rather than source:
the writer has to run after the language version is known. Keeping symbols out of
the pipeline is the second reason and the more important one — a cached symbol
compares by reference, so the cache never hits, and it roots the compilation it came
from.

## Emission constraints

All of these are cheap to break and expensive to notice.

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
diagnostic. `GeneratedInstanceTests` and `GeneratedLadderTests` are the other half
and are not optional: this project loads the generator as an analyzer, so the schemas
at the bottom of those files are compiled against emitted source rather than written
source. **A snapshot proves text; only those files prove the text compiles, binds,
infers and runs.** The ladder is where that matters most — generic inference through
`Schema.Fields(...).Refine(...).Into(...)` is the thing most likely to be subtly
wrong, and no amount of string comparison would notice.

`RunOnCSharp73` is the only case that exercises the constraint decision, because
every other test compiles at the latest version where both spellings are legal.

## Duplication that was considered and kept

Two analyzer assemblies cannot share a runtime assembly without shipping it, so
some scaffolding here has a twin in `Waystone.Monads.SourceGenerators`. Both were
looked at and both were left alone; do not "fix" either without a new reason.

* **`DiagnosticInfo` and `EquatableArray`** are near-copies of that project's, and
  converged on it once the ladder needed a third message argument. They *could* be
  shared with a `<Compile Include="..\..." Link="..."/>` item —
  `Waystone.Monads.Analyzers.csproj` links `ErrorCodeFormat.cs` that way — but that
  precedent is a hundred lines of parsing with real logic to get wrong, and this is
  a record and a thirty-line struct. Linking would couple two generators' builds and
  put a `Waystone.Monads.SourceGenerators.ErrorCodes` using into files that have
  nothing to do with error codes.
* **`SchemaWriter.Writer`** is byte-for-byte the same fourteen-line `StringBuilder`
  wrapper for the same reason.

## Severity is not uniform, and the split is the point

`Create` builds an error; `Advice` builds a warning. The line is whether the schema
generates at all.

`WMSC0001`–`WMSC0004` describe a schema that cannot be built, so failing the build
is the whole point — the alternative is a missing member reported against a
generated file the author cannot open. `WMSC0005` describes one that generates and
runs and is probably not what its author meant, and there is a correct reading of
the same code: gating on a value deliberately not kept, such as a confirmation field
that must be well-formed and is never stored. An error there would leave that author
nothing but the id in an `.editorconfig`.

`RulesTests` spells out which ids warn rather than deriving it, so promoting a rule
has to be a deliberate edit in two places.
