# Waystone.Monads.Schemas

Composable schema validation that *parses*: what comes out is a type the caller
could not have built without passing. The design lives in
[DRA-181](https://linear.app/draekien-industries/issue/DRA-181/design-waystonemonadsschema-composable-schema-validation);
this file carries only what a change here has to obey.

## The name is plural, and it has to be

Package, assembly, root namespace and directory are all `Waystone.Monads.Schemas`.
The plural is forced, not a preference.

A namespace and a type may share a name, but **the namespace wins lookup wherever
both are candidates**. With a singular namespace, any consumer whose own namespace
begins `Waystone.Monads.` resolves `Schema.Text` against the *namespace* and gets
`CS0234` — and that is every test project and every published doc sample in this
repository. Verified by scratch compile in both spellings. `Schema<TIn, TOut>` is
unaffected either way, since arity 2 excludes the namespace as a candidate; only
the bare `Schema` breaks, which is the one a reader types most.

The package id followed the namespace rather than the other way round, so nobody
has to hold two spellings of one word. The singular spelling appears nowhere.

## `Configure` lives on `SchemaConfig`, and that is the whole hierarchy rule

Three types, and the split is load-bearing:

- **`Schema<TIn, TOut>`** — public, abstract. Carries `Parse`, `ParseAsync` and the
  fluent surface. `Evaluate` is **`internal abstract`**.
- **`SchemaConfig<TIn, TOut>`** — public, abstract. Carries
  `protected abstract Configure` and seals `Evaluate` onto it. **The only public
  way into the hierarchy**; a consumer derives from this.
- The package's own nodes — primitives, combinators, decorators — derive from
  `Schema<TIn, TOut>` directly and override `Evaluate`.

**`Schema<TIn, TOut>` closes itself, with no marker member.** `Evaluate` is both
internal and abstract, so a type declared outside this assembly cannot satisfy
the contract and the compiler refuses the subclass — one `CS0534`, pinned by
`SchemaClosedHierarchyTests`. `Field` and `Field<T>` still need
`internal abstract void OnlyThisAssemblyMayDerive()`, because they have no other
internal abstract member to do the job.

**Do not put `Configure` back on `Schema<TIn, TOut>`.** It was there for one
layer, with an internal `SchemaNode<TIn, TOut>` sealing it to a
`NotSupportedException`, and that is a Liskov violation: half the hierarchy
inherited a member it could only refuse. The throw was unreachable, but only
because `Evaluate` was its single caller — an invariant held by convention across
a dozen files rather than by the type system, and one the layer 5 generator emits
code against. Splitting the type moves the guarantee into the compiler and deletes
the throw, `SchemaNode` and the test that covered it. The cost is one extra public
name in a base clause written once per schema class; the frequently written
`Schema<A, B>` field and parameter type is unchanged, which is why the *root*
keeps the short name.

## `FieldAccumulator` is public because generated code has to reach it

Evaluating a field is internal — `Field.Evaluate`, `Field<T>.EvaluateValue`,
`ParseContext` and `Outcome<T>` all are, deliberately, because they are what keeps
every schema's reporting behaviour identical. But the `Schema.Fields` ladder is
generated into the *consumer's* assembly, so it can see none of them.

`FieldAccumulator` is the one seam that crosses that line, and it is kept as narrow
as it can be: start one, take each field's value, refine with the ones that only
gate, ask whether anything was reported. Five members, no way to construct a
violation, no way to reach a `ParseContext`. **Anything the ladder needs in future
is added here, and adding it widens the published surface of the package
permanently** — so prefer teaching the generator to compose what already exists.

**It offers no success half, and that is the shape rather than an omission.** An
earlier draft took a `Func<TOut>` and called it only when nothing had been reported,
which allocated a delegate and a display class on *every* parse of every schema to
guard a branch the caller could take itself. `HasViolations` plus `Failed<TOut>()`
gives the caller the same guarantee — the constructor never sees a `default` a field
failed to produce — and the generated `Into` calls the caller's own delegate
directly.

`Take` returns the value rather than a success flag with an `out`. The flag was
never read: the generator runs every field regardless, then branches once on
`HasViolations`. A flag on the shipped surface would have implied a per-field
decision that nothing makes.

Note that it works at `ParseContext.Root`. A nested schema's paths are rebased by
`SchemaConfig.Evaluate` after `Configure` returns, so the accumulator never needs to
know where in a larger parse it sits.

## The `Transform` overloads are not ambiguous, and it is worth knowing why

`Transform(Func<TOut, TNext>)` and `Transform(Func<TOut, Result<TNext, Error>>)`
both apply to a factory returning `Result<Money, Error>`, and after inference both
reduce to the *same* parameter type. That looks like `CS0121`. It is not: C#
prefers the more specific parameter type, and `Result<TNext, Error>` is more
specific than a bare `TNext`, so the fallible overload wins and `TNext` binds to
`Money`. Verified by scratch compile, not by reading the specification. Do not
rename either overload to "fix" an ambiguity that does not exist.

Only the total overload guards a null return. `Ok<TOk, TErr>`'s own constructor
already rejects null, so the same guard on the fallible path was dead code and was
removed — a test asserting it threw `InvalidOperationException` failed with
`ArgumentNullException` from the monad, which is how it was caught.

## Decorators go through `DecoratorSchema`, and there are two tiers under it

`DecoratorSchema<TIn, TOut, TNext>` runs the inner schema and hands its `Outcome`
to `Decorate`, **sealing both `Evaluate` and `EvaluateAsync`**. That seal is the
point: a node written by hand has to override the asynchronous path too, and one
that forgets runs an asynchronous inner schema synchronously, which nothing in the
build notices.

Two tiers sit under it, and each exists because its subclasses were writing a stub:

- **`ContextSchema<TIn, TOut>`** — seals `Decorate` to the identity and makes
  `Adjust` abstract. For a node that changes the *context* rather than the
  outcome. `Named` and `Sensitive` are one method each.
- **`RewritingSchema<TIn, TOut>`** — owns the "short-circuit on no violations,
  rewrite each one, rebuild the outcome" loop and exposes a single `Rewrite`
  hook. `WithMessage` and `WithCode` are one method each.

**A stub override is the type telling you a tier is missing.** `Named` used to
carry a `Decorate` that returned its argument unchanged, and `Sensitive` was still
hand-rolling both paths — which contradicted this very file. Add the tier rather
than the stub.

Three nodes are outside the hierarchy and each has a reason. `Not` evaluates a
*second* schema, so a synchronous `Decorate` would run that one synchronously on
the asynchronous path. `When`/`Unless` may skip the inner schema entirely, so
there is no outcome to decorate. `All`/`Any` fold over several branches.

**A shared `CombinatorSchema` for `All` and `Any` was considered and rejected.**
Folding over branches needs the accumulator to survive an `await`, and a `ref`
local cannot cross one — so the base would have to allocate a mutable fold object
on every parse to remove thirty lines of straight-line code. `CompositeNodeAsyncPathTests`
guards the hazard instead, and costs nothing at runtime.

## `Outcome<T>` owns "same shape, new contents"

`WithViolations` keeps whether a value survived and swaps the violation list.
`WithValue` keeps the violations and swaps the value. Use them; do not re-derive
which of the three constructors applies.

Five sites used to spell that rule out by hand, two as
`Violations.Count == 0 ? Passed : Refined` and two as
`HasValue ? Refined : Failed` — the same rule seen from opposite sides. A sixth
node picking `Failed` where `Refined` was right silently stops the rest of a chain
reporting, and no test that did not specifically look for it would fail.

`WithViolations` requires a non-empty list, because `Refined` and `Failed` both
reject an empty one. Callers short-circuit on `Violations.Count == 0` first, which
they wanted to do anyway.

## The synchronous and asynchronous pair is guarded by a test, not by the type

`Evaluate` is `internal abstract`; `EvaluateAsync` is `internal virtual` with a
synchronous default. The default is right for a **leaf** — a rule with no inner
schema has nothing to await — and wrong for a node that holds one.

Inverting that (an abstract async member plus a `SyncSchema` tier) would force an
override onto every leaf and every test double, where the default is correct.
`CompositeNodeAsyncPathTests` reflects over the assembly instead: any concrete
`Schema<,>` holding a field of schema type must override `EvaluateAsync` somewhere
below the root, which deriving from `DecoratorSchema` satisfies. Add a wrapping
node in a later layer without an asynchronous path and that test names it.

## `When` and `Unless` are extension methods on purpose

Both can skip the schema they are called on, and skipping is only well typed when
the schema hands back what it was given — otherwise there is no `TOut` to return
for an input that was never parsed. So they sit on `Schema<T, T>` as extensions
and are simply absent from `Schema<string, EmailAddress>`, which is a
missing-method error rather than a runtime one. Do not "fix" this by moving them
onto the base with a throw; that is the mistake `SchemaConfig` exists to undo.

## `Any` nests its branch failures; do not flatten them

When every branch fails, `AnySchema` emits one violation at its own path plus each
branch's violations rebased under a numbered segment — `contact[0].email`. The
numbering is why `ParseContext.AtIndex` exists. Flattening onto the field's own
path would put a dozen irrelevant failures where a caller reading `ByPath()`
expects one, which is the most complained-of part of Zod's output.

**`Named` replaces a trailing name and appends after anything else.** Renaming an
index is never what a caller means: inside an `Any`, the innermost segment is a
branch number, so replacing it would turn `contact[1]` into `contact.byPhone` —
losing the branch and colliding with a real property of that name.
`ViolationPath.Rename` switches on `SegmentKind` to decide, which is a fact the
type holds rather than something recovered from rendered text.

`Any` allocates its rejection list before trying the first branch, so the
short-circuit path pays for one list. That is deliberate: the alternative is a
nullable local and a `!` on a state the type system cannot see is impossible,
which costs a partially covered branch to save one allocation on a path that is
not hot.

## RS0026 is suppressed for one file, and the reason is not stylistic

`CallerArgumentExpression` is what derives a violation path, and the compiler only
fills it in on an *optional* parameter. RS0026 forbids several overloads of one
name from carrying optional parameters. Every field constructor needs both, so the
two rules cannot both be satisfied.

The hazard RS0026 guards against — a later overload silently rebinding a call site
— cannot arise here, because the overloads are separated by mutually exclusive
generic constraints. `Option<TIn>`, a reference `TIn` and a nullable value `TIn`
are three shapes no single argument satisfies at once, so an ambiguity surfaces as
`CS0121` at the call site rather than as a silent rebind. The three-overload
pattern is `Option.FromNullable`'s, which already proves it compiles here.

**The suppression is an `.editorconfig` block scoped to `Schema.cs`, not a
`NoWarn` on the project.** Only that file needs it, and this package is still
growing — a project-wide `NoWarn` would silence a genuine overload hazard in a
file added later, and suppressions do not decay. Note that a severity glob does
reach a hand-written file; it is a *generated* document it cannot reach, which is
the trap `Waystone.Monads.SourceGenerators/AGENTS.md` records.

## Messages are rendered eagerly, and that bounds `.Sensitive()`

`Violation.Message` is a rendered string, fixed when the violation was created.
That is deliberate — DRA-181 settles it — and it has one consequence worth
knowing.

`.Sensitive()` propagates through `ParseContext`, so it reaches every rule the
schema is built from and every schema nested *beneath* it that this package
evaluates. It cannot reach a nested schema that overrides `Configure`, because
that schema renders its own messages before the outer context exists. Mark that
schema itself. The doc comment on `Sensitive` says so; do not quietly widen the
promise.

Paths do not have this problem: `SchemaConfig<TIn, TOut>.Evaluate` re-bases a
composed schema's violations under the parent's path through `ViolationPath.Nest`,
so `order.subject` comes out right even though `subject` was rendered first.

**`ViolationPath` holds segments and renders on demand; do not collapse it back to
a stored string.** An earlier version stored the rendered text and `Nest` decided
whether to insert a `.` by testing whether the child's text began with `[`. That is
correct only by coincidence — both bracketed segment kinds happen to start with one
— and a future segment kind that does not would silently glue two names together
with no separator, no compile error and no failing test. `Nest` is now array
concatenation and the separator is decided from `SegmentKind` at render time.

**`MessageTemplate.Render` is a single pass, and must stay one.** Substituting
token by token with `string.Replace` would re-substitute a rejected value that
happens to contain `{Code}` — and rejected values are exactly the untrusted input
this package exists to handle.

**`{Expected}` is not redacted by `.Sensitive()`, and must not be.** The other
tokens render something derived from the input; this one renders a bound the
schema's *author* wrote down. Redacting `Expected {Path} to be at least ***` costs
the reader the only actionable part of the sentence and protects nothing.

**A rule supplies `{Expected}` by constructing `CheckSchema` with a bound, not
through public `Check`.** `Check` has two overloads and no optional parameter;
adding one would trip RS0026, which this package already suppresses once and
should not suppress twice. `Rules.Add` is the in-assembly path, and it is also
where the `schema` null guard lives so every extension reports the same parameter
name.

**`WithMessage` renders `{Expected}` literally, on purpose.** It replaces the
messages of every rule on the chain at once, so there is no single bound left to
name. Documented on the member; do not "fix" it by threading the last bound
through, which would silently pick one rule out of several.

## The primitives are cached, and identity is the whole implementation

`Schema.Text`, `Bool`, `Id`, `Timestamp`, `Date` and the four under `Number` are
all `Schema.For<T>()`, which is `IdentitySchema<T>.Instance` — one instance per
type, for the process. They check nothing, and there is nothing for them to check:
the type system has established the type already, and `Required` and `Optional`
stop null before a schema sees it. `Schema.Enum<T>()` is the one exception and
caches through `DeclaredMembers<T>`, since it does carry a rule.

Reusing the instance is what makes `Schema.For<string>().ShouldBeSameAs(Schema.Text)`
true, and a test asserts it. Chaining allocates; reaching for a primitive does not.

**`Schema.Enum<T>()` is wrong for a `[Flags]` enumeration and says so.**
`Enum.IsDefined` asks for equality with a declared member, so `Read | Write` is
rejected unless it is itself declared. Handling flags properly means converting an
arbitrary enum to its underlying integer across all eight backing types, which is
more machinery than the case is worth; the doc comment points a flags user at
`Check` instead. If that changes, it is an additive fix, not a breaking one.

## The comparison rules are one family, and `Number` is the exception that proves it

`AtLeast`, `AtMost`, `GreaterThan` and `LessThan` are generic over
`IComparable<T>`, so one set covers both integers, both floating-point types, both
temporals, `TimeSpan`, `string` and any domain type a consumer brings — including
one produced by `Transform`, which is where they matter most. Do not add a
per-type copy.

`Positive` and `Negative` are four overloads apiece because they need a zero of
the value's own type, and `netstandard2.0` has no numeric constraint to get one
from. `INumber<T>` would collapse them, and cannot be used while that target
framework is in the list. `Before` and `After` are likewise aliases rather than
generic: they exist so the sentence reads the way a person says it about time.

**Inside `Schema.Number`, write `decimal` and `double`, not `Decimal` and
`Double`.** The two properties shadow the framework types in that scope. The
keywords are never shadowed, which is why the code reads normally; a future editor
spelling the type name instead gets a confusing error, and `global::System.Decimal`
is the escape hatch.

## The structures are the only schemas that iterate

`ListSchema` and `DictionarySchema` are the first nodes that run an inner schema
more than once, and the first producers of the indexed and keyed segments
`ViolationPath` has carried since layer 1. Everything before them evaluates one
value against one chain.

**The accumulation lives in `Entries<T>` and `Pairs<TKey, TValue>`, not in the
schemas.** Both nodes hand-roll the synchronous and asynchronous paths — as
`AllSchema` and `AnySchema` do, and for the same reason `DecoratorSchema` cannot
help here — so the "gather violations, keep the value only if every entry produced
one" rule would otherwise be written four times. It is written twice, in the
accumulators, and each schema's two paths differ only in the `await`.

**Both accumulators are classes, deliberately.** A struct would be the obvious
choice for something this small, and it does not work: the asynchronous path
mutates the accumulator across an `await`, and a `ref` local cannot cross one.
This is the same constraint that killed the shared `CombinatorSchema` above.

**A null entry is reported as `incomplete` at its own path and never reaches the
inner schema.** That is what lets every item schema assume its input is there, so
`Schema.List(Schema.Text.NotEmpty())` needs no null guard of its own. `Required`
and `Optional` do the same job one level up, and this is the same promise applied
per entry. Do not remove it in the name of consistency with `Parse`, which does
not guard its argument — a caller passing null to `Parse` made a programming
error, while a null *inside* a list is untrusted input arriving exactly as
expected from JSON.

**A dictionary reports a key failure and a value failure at the same path**, built
from the *incoming* key's text. That is the only spelling a caller can match
against what they sent, and splitting them into `rates["AUD"].key` would invent a
segment that does not exist in the input. The message distinguishes them.

**Two keys parsing to the same output key is a `Duplicate` and produces no
dictionary.** Keeping either one would silently drop data the caller sent. This is
the first producer of `ViolationCode.Duplicate`, and it honours that code's
promise to report against the later entry.

**`Pairs.Take` does `ContainsKey` then `Add`, and that stays until netstandard2.0
goes.** `Dictionary<TKey, TValue>.TryAdd` would make it one hash lookup instead of
two, and it does not exist on netstandard2.0. Wrapping it in
`#if NET8_0_OR_GREATER` buys one avoided lookup per entry at the cost of a second
code path through the only place duplicate keys are detected. Not worth it; when
the target framework list loses netstandard2.0, take `TryAdd` unguarded.

**`MinCount` and `MaxCount` are two overloads apiece and cannot be one.** Both
ways of writing the single rule fail, for different reasons, and it is worth
knowing which is which.

Writing it against the common base — `this Schema<TIn, IReadOnlyCollection<T>>` —
fails because `Schema<TIn, TOut>` is *invariant* in `TOut`. A receiver typed
`Schema<TIn, IReadOnlyList<T>>` is not a `Schema<TIn, IReadOnlyCollection<T>>`, so
the method is never a candidate.

Writing it generically over the collection — `this Schema<TIn, TColl>` with
`TColl : IReadOnlyCollection<TElement>` — compiles, and then cannot be called
fluently. `TElement` appears only in a constraint, never in a parameter, and C#
does not infer a type argument from a constraint. Every call site would have to
spell all three out, which is worse than the overload it replaced.

## The package multi-targets for exactly one type

`netstandard2.0;net8.0`, because `Schema.Date` needs `DateOnly` and PolySharp does
not polyfill it. `Date`, `Before(DateOnly)` and `After(DateOnly)` sit behind
`#if NET8_0_OR_GREATER`; nothing else in the package differs by target.

**The public API baseline is split three ways, and `Date` is only half the
reason.** `SchemaViolation`'s compiler-generated `<Clone>$` returns `Error` on
netstandard2.0 and `SchemaViolation` on net8.0 — a derived record gets a covariant
return only where the runtime supports one — so even without `Date` a single shared
baseline would be impossible. The root pair carries what both targets share; the
`netstandard2.0/` and `net8.0/` folders carry only what one alone has.
PublicApiAnalyzers *unions* every `AdditionalFile` of a given name, which is what
makes this work and was verified by experiment rather than assumed.

**Harvest baseline rows from the RS0016 build errors.** The message carries the
full row text, not just the symbol name, so
`dotnet build … | grep -oE "Symbol '[^']+' is not part.*TargetFramework=[a-z0-9.]+"`
gives you rows tagged by target framework, ready to `comm` into shared and
per-target sets. **`dotnet format analyzers --diagnostics RS0016` does nothing
here** — the code fix writes to an `AdditionalFile`, and `dotnet format` only
applies document changes. It exits reporting "Formatted 0 of 96 files" and looks
like a passing run.

## `Outcome<T>` has three constructors because a refinement is not a transform

- `Passed(value)` — no violations.
- `Refined(value, violations)` — the value survived, so the rest of its chain
  still runs. This is what makes "every failure is reported" true.
- `Failed(violations)` — no value, so nothing downstream of it can run.

`Failed` and `Refined` both reject an empty list, so "failed with nothing to say"
is unrepresentable rather than guarded against later. It is a class rather than a
struct for the same reason: `default(Outcome<T>)` would be exactly that state.
