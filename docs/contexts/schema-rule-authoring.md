# Adding a rule to `Waystone.Monads.Schemas`

Read before adding or changing a member under `Fields/`, `Extensions/` or the
`Schema.*` entry points in `src/Waystone.Monads.Schemas`.

## The primitives are cached, and identity is the whole implementation

`Schema.Text`, `Bool`, `Uuid`, `Timestamp`, `Date` and those under `Number` are all
`Schema.For<T>()`, which is `IdentitySchema<T>.Instance` — one instance per type, for the
process. They check nothing, and there is nothing for them to check: the type system has
established the type already, and `Required` and `Optional` stop null before a schema sees
it. `Schema.Enum<T>()` is the one exception and caches through `DeclaredMembers<T>`, since
it does carry a rule.

Reusing the instance is what makes `Schema.For<string>().ShouldBeSameAs(Schema.Text)` true,
and a test asserts it. Chaining allocates; reaching for a primitive does not.

**`Schema.Enum<T>()` is wrong for a `[Flags]` enumeration and says so.** `Enum.IsDefined`
asks for equality with a declared member, so `Read | Write` is rejected unless it is itself
declared. Handling flags properly means converting an arbitrary enum to its underlying
integer across all eight backing types; the doc comment points a flags user at `Check`
instead. If that changes, it is an additive fix, not a breaking one.

## The comparison rules are one family, with `Number` the exception

`AtLeast`, `AtMost`, `GreaterThan` and `LessThan` are generic over `IComparable<T>`, so one
set covers both integers, both floating-point types, both temporals, `TimeSpan`, `string`
and any domain type a consumer brings — including one produced by `Transform`. Do not add a
per-type copy.

`Positive` and `Negative` are four overloads apiece because they need a zero of the value's
own type, and `netstandard2.0` has no numeric constraint to get one from. `INumber<T>` would
collapse them, and cannot be used while that target framework is in the list. `Before`,
`After`, `OnOrBefore` and `OnOrAfter` are likewise aliases rather than generic: they exist
so the sentence reads the way a person says it about time.

**Inclusivity is spelled in the name, never passed as a flag.** `AtLeast` and `AtMost`
include the bound, `GreaterThan` and `LessThan` exclude it, and the temporal four mirror
that pair for pair. Do not add an `inclusive: true` argument: it says at the call site what
the name already says.

**`Between` exists even though `AtLeast(1).AtMost(10)` accepts the same values.** The chain
is two refinements, and refinements do not stop one another, so a value outside the range is
reported once per bound it missed. One mistake should read as one failure. `LengthBetween`
is the same argument for text, and both throw `ArgumentException` when the upper bound orders
below the lower one — a range no value can satisfy is a mistake in the schema, and it
surfaces when the schema is built rather than on the first parse that happens to reach it.

**Inside `Schema.Number`, write `decimal` and `double`, not `Decimal` and `Double`.** The two
properties shadow the framework types in that scope. The keywords are never shadowed; a
future editor spelling the type name instead gets a confusing error, and
`global::System.Decimal` is the escape hatch.

## The text rules avoid a regular expression wherever a scan will do

`Matches` carries a one-second timeout because a pattern's cost can explode on crafted
input. Every rule that reaches for `Regex` inherits that risk and the
`RegexMatchTimeoutException` that guards it, so a rule that can be written as a linear scan
is written as one.

**`Email` is a hand-written scan in `EmailAddress`, not a pattern and not `MailAddress`.**
`MailAddress.TryCreate` is .NET 5 and later, so the netstandard2.0 leg would need a
`try`/`catch` around the constructor — paying a thrown exception for every invalid address,
on a path where invalid is the expected case rather than the exceptional one.

**The accepted subset is a decision, and it is written down twice on purpose** — in the
rule's doc comment for a consumer, and in `EmailAddressTests` as cases. It rejects what RFC
5322 allows and nobody sends: quoted local parts, comments, bracketed IP literals, anything
outside ASCII. It accepts a single-label host, so `root@localhost` passes, because that is a
real address on an internal network and there would otherwise be no way to accept one.
Narrowing any of that is a behavioural change to a shipped rule, not a bug fix.

**`Url` requires the value to spell its own scheme, and that is a portability fix rather
than strictness.** `Uri.TryCreate(value, UriKind.Absolute, out _)` answers differently by
operating system: on Unix a bare path like `/quests/3` parses as an absolute `file:` URI,
and on Windows it does not. `IsAbsoluteUrl` therefore checks that the parsed scheme actually
appears at the front of the input. The framework matrix varies the framework, never the
platform, so a difference like this surfaces on CI's Linux run and never locally on Windows.

**`Url` has an unrestricted overload and a scheme-restricted one, and the restricted one is
the default in the docs.** An absolute URI includes `javascript:`, `data:` and `file:`, which
is how an open redirect and a script injection arrive. Passing no scheme accepts none rather
than widening to the unrestricted rule: an empty array is far more likely to be a bug than an
intention.

**`StartsWith`, `EndsWith` and `Contains` are literals.** Each replaces a `Matches` call
where a dot or a bracket in the argument would quietly mean something else. They take a
`StringComparison` rather than a `bool ignoreCase`, for the reason the bounds do.

**`OneOf` copies its array, as `Url` does with its schemes.** A `params` array is the
caller's, and a schema built once and parsed for the life of a process would otherwise follow
whatever the caller did to it afterwards.

**Neither predicate nests a lambda inside `Array.Exists`.** The inner delegate would capture
a parse-time local, so it is allocated on every parse rather than once when the schema is
built. `Holds` is a plain loop for that reason, and the same applies to any rule later added
over a set.

**`EmailAddress.IsLetterOrDigit` is hand-rolled, and `char.IsAsciiLetterOrDigit` is not the
fix.** That method is .NET 7 and later, PolySharp polyfills types and attributes rather than
BCL methods, and this package still targets netstandard2.0 — verified by compiling it there,
not assumed.

## RS0026 is suppressed for two files

`CallerArgumentExpression` is what derives a violation path, and the compiler only fills it
in on an *optional* parameter. RS0026 forbids several overloads of one name from carrying
optional parameters. Every field constructor needs both, so the two rules cannot both be
satisfied.

The hazard RS0026 guards against — a later overload silently rebinding a call site — cannot
arise here, because the overloads are separated by mutually exclusive generic constraints.
`Option<TIn>`, a reference `TIn` and a nullable value `TIn` are three shapes no single
argument satisfies at once, so an ambiguity surfaces as `CS0121` at the call site rather than
as a silent rebind. The three-overload pattern is `Option.FromNullable`'s, which already
proves it compiles here. The same argument covers `SchemaOfTInTOut.cs`, where each name has
two overloads separated by the type of their second parameter and no argument is both a
`ViolationCode` and an `ErrorCode`.

**The suppression is an `.editorconfig` block scoped to `Schema.cs` and `SchemaOfTInTOut.cs`,
not a `NoWarn` on the project.** A project-wide `NoWarn` would silence a genuine overload
hazard in a file added later, and suppressions do not decay. A severity glob does reach a
hand-written file; it is a *generated* document it cannot reach, which is the trap
[src/Waystone.Monads.SourceGenerators/AGENTS.md](../../src/Waystone.Monads.SourceGenerators/AGENTS.md)
records.
