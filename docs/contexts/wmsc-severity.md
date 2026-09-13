# Choosing a severity for a `WMSC` rule

Read before adding a rule to `src/Waystone.Monads.Schemas.SourceGenerators`. Write the
descriptor's strings through the `writing-diagnostic-descriptors` skill.

`Create` builds an error; `Advice` builds a warning; `Suggestion` builds an information
diagnostic. **The line is whether the code has a reading that is correct**, not whether the
schema generates.

`WMSC0001`–`WMSC0004` describe a schema that cannot be built, so failing the build is the
whole point — the alternative is a missing member reported against a generated file the
author cannot open. `WMSC0006` describes one that builds perfectly and throws the moment
anyone runs it, which is no better; a field set only ever runs the synchronous path, so an
asynchronous rule reached from `Configure` never does its job under any input.

`WMSC0005` is the case to reason from: gating on a value deliberately not kept, such as a
confirmation field that must be well-formed and never stored. An error there would leave
that author nothing but the id in an `.editorconfig`.
`RulesTests.OnlyTheRulesWithACorrectReadingAreWarnings` holds the current set.

`WMSC0009` is the only rule here reporting on code with nothing wrong with it: both
spellings of a named schema are the same cached object, and one is merely easier to find
the rules for. A warning would put a line in the build log of a consumer who wrote correct
code on an upgrade they did not choose, so it suggests — an IDE offers it and a build never
mentions it.

**An analyzer ships in this assembly as well as the generator, and `WMSC0009` is why.** The
generator only ever sees a `Configure` body, and a schema is as likely to sit in a shared
static field, which is the shape the documentation recommends. The assembly is packed to
`analyzers/dotnet/cs`, which Roslyn loads analyzers and generators from alike.

**That analyzer skips the assembly it is compiling when that assembly declares `Schema.For`
itself.** The named schemas *are* `For<T>()` initialisers, so without the guard the rule
reports on every one of the definitions it is recommending — and this project's `.props` is
imported into `Waystone.Monads.Schemas.csproj`, so it runs there. No unit test covers it,
because the harness compiles a subject assembly that is never the runtime one; verify it by
raising the rule to an error and building both the package and a consumer.

`RulesTests` spells out which ids warn and which suggest rather than deriving either, so
promoting a rule has to be a deliberate edit in two places.
