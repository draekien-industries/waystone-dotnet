# Waystone.Monads.SourceGenerators

Emits the error code members of an enum a consumer marked with
`[ErrorCodeCatalog]` — the code strings, the `ErrorCode` fields, the `Error`
factories and the three extensions that map a value to each.

## Why this is not Waystone.SourceGenerators

**This assembly ships; that one does not.** `Waystone.SourceGenerators` is
`IsPackable=false` *and* absent from `PackMonadAnalyzers`, so it only ever runs on
this repository's own compilations. This project is in that pack target, so it lands
in `analyzers/dotnet/cs` and runs in every consumer's build. Adding the awaited
receivers generator to the pack target instead would have shipped
`[GenerateAwaitedReceivers]` to consumers as a side effect.

The two consequences of shipping: the generator must stay silent on a compilation
with no attributed enum — `ForAttributeWithMetadataName` gives that for free — and
every consumer gets it on upgrade with no opt-out beyond not applying the attribute.

## The generator contract

**The attribute is hand-written public API in `Waystone.Monads`, not emitted.**
`ErrorCodeCatalogAttribute` lives next to `ErrorCode` so it is discoverable in
IntelliSense and in the published API reference without knowing a generator exists.
That puts it under deprecate-never-remove, and it needs a `PublicAPI` entry like any
other public type. The generator still must not reference `Waystone.Monads`: it
resolves the attribute by metadata name through `ForAttributeWithMetadataName`, and
`ErrorCode` and `Error` through `compilation.GetTypeByMetadataName`, reporting
`WMG0004` rather than emitting source that cannot compile.

**The scheme is declarative and evaluated at compile time.** `ErrorCodeFormat` parses
`{enum}` and `{member}` with an optional `kebab`, `snake`, `lower` or `upper` casing,
and the default is `{enum}.{member}` so an enum that sets nothing gets exactly what
`ErrorCodeFactory.FromEnum` produces. Precedence is the enum's `Format`, then the
assembly's `[ErrorCodeFormat]`, then that default.

This exists because the alternative does not work: a generator cannot execute a
factory. `ErrorCodeFactory.FromEnum` is arbitrary C# that runs later, and the compiler
has no facility to invoke user code and read the result back. So a consumer who
installs a custom factory through `MonadOptions.UseErrorCodeFactory` changes the
runtime string and not the generated one, which is why the factory is being obsoleted
in favour of the format — see DRA-112.

**`ErrorCodeFormat.cs` is compiled into `Waystone.Monads.Analyzers` as well**, as a
linked `Compile` item rather than a project reference, because `WM2018` keys on the
generated code and has to resolve the format identically. Two copies of the parser
would let the rule and the generator disagree about what code an enum produces, which
is exactly the bug the rule exists to catch. The two assemblies cannot reference each
other, so shared source is the only mechanism.

**`ApplyToUndeclared` folds everything that does not depend on the member into a
literal**, so the `default:` arm is a concatenation of constants around one
`ToString()`. The member's own casing is dropped there rather than emitted as a runtime
call: an undeclared value renders as digits, and all four casings are the identity on
digits. `EveryCasingIsTheIdentityOnDigits` is what makes that safe to rely on.

**The nesting is what makes member names safe.** `Names`, `Codes`
and `Errors` each hold one member per enum member, named verbatim, so an enum member
called `NotFoundCode` cannot collide with the generated name for `NotFound`. The
price is `WMG0003`, and only that: a member named after one of the three *nested
classes* produces a member with its enclosing type's name, which is CS0542. A member
named after one of the three *extensions* is fine — the extensions are on the outer
class and the members on the nested ones, so the two never share a container.
`AcceptsAMemberNamedAfterAnExtension` pins that down, because the asymmetry looks
like an oversight otherwise.

**The three extensions must sit on the outer class**, because C# forbids extension
methods in a nested static class. Their `default:` arm is not optional either — a value
cast from an arbitrary integer is a legal enum value, so the switch has to be
exhaustive.

**No generated member ever consults the configured `ErrorCodeFactory`, including on
that fallback path.** The arm builds the string itself rather than calling
`ErrorCode.FromEnum`. It used to call it, which was wrong in a way no test could see:
under the default factory the two are identical, so a declared member returning the
baked constant while an undeclared value returned the factory's string looked correct
until someone installed a custom factory, at which point one method disagreed with
itself. `ACustomFactoryChangesNothingTheGeneratedMembersReturn` installs a factory that
prefixes every code and asserts the generated members do not move.

**A new rule needs an `AnalyzerReleases.Unshipped.md` entry in the same change.**
RS2008 fails the build without one. `WMG` is its own id space with its own release
files; do not number into `WSG`, which belongs to the other generator assembly.

## Gotchas

+**The emitted source is marked as generated code, deliberately.** Hint names end
+`.g.cs` and the file opens with a `// <auto-generated/>` header, so Roslyn (and any
+consumer's analyzers) skip it — a consumer with `TreatWarningsAsErrors` and a style
+rule like a namespace-declaration preference would otherwise fail its own build on
+code it never wrote. `[GeneratedCode]` is deliberately still absent: that specific
+attribute is what coverlet's default `ExcludeByAttribute` keys on, and dropping the
+generated members from the coverage denominator would make `GeneratedErrorCodeTests`
+stop counting. `EmitsAHintNameThatIsTreatedAsGeneratedCode` holds the new line.

**The emitted source targets C# 7.3, not the repository's language version.** A
`switch` statement rather than a switch expression, `new T(...)` rather than a
target-typed `new`, a block namespace, and no `#nullable` directive. The generator
runs in the *consumer's* compilation, and the default `LangVersion` for a `net472`
project is still 7.3 — a switch expression there is a compile error in a file the
consumer did not write and cannot edit.

**Generated doc comments use `<c>` and never `<see cref="..." />`.** An unresolved
cref is CS1574, which is an error in any consumer with
`TreatWarningsAsErrors`. Docs are emitted at all — rather than suppressed — because
CS1591 is on by default in a consumer with a documentation file, and the generated
surface is public.

**The catalog name is the enum's name plus `Catalog`, and nothing is trimmed off
it.** An earlier version deduplicated a trailing `Error` or `ErrorCode`, which gave
`OrderError` and `OrderErrorCode` one class name and a CS0101 the generator never
reported. Do not reintroduce trimming: the hint name is keyed on the *enum* name, so a
collision here does not throw a duplicate-hint-name exception and surfaces only as a
confusing error in the consumer.

**`StringBuilder.AppendLine` writes CRLF on Windows.** `ErrorCodeCatalogWriter`
appends `'\n'` directly and never calls `AppendLine`, so the emitted source does not
vary by build platform. The snapshot test normalises its expected literal too, since
git may check the test file out either way.

## Testing

`test/Waystone.Monads.SourceGenerators.Tests` runs the generator two ways.
`Verify.Run` drives it over a synthetic compilation and asserts the emitted text and
the diagnostics. `GeneratedErrorCodeTests` is the stronger one: the test project
imports `Waystone.Monads.SourceGenerators.props`, so it declares a real
`[ErrorCodeCatalog]` enum and calls the generated members, which proves the emitted
source compiles and agrees with `ErrorCode.FromEnum` at runtime rather than by
inspection.
