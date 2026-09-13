# Waystone.Monads.Schemas

Composable schema validation that *parses*: what comes out is a type the caller
could not have built without passing. The design lives in
[DRA-181](https://linear.app/draekien-industries/issue/DRA-181/design-waystonemonadsschema-composable-schema-validation).

## The name is plural

Package, assembly, root namespace and directory are all `Waystone.Monads.Schemas`.

A namespace and a type may share a name, but **the namespace wins lookup wherever
both are candidates**. With a singular namespace, any consumer whose own namespace
begins `Waystone.Monads.` resolves `Schema.Text` against the *namespace* and gets
`CS0234` — and that is every test project and every published doc sample in this
repository. Verified by scratch compile in both spellings. `Schema<TIn, TOut>` is
unaffected either way, since arity 2 excludes the namespace as a candidate; only
the bare `Schema` breaks.

The singular spelling appears nowhere.

## The public surface is flat; the machinery is not

Every public type is in the root namespace `Waystone.Monads.Schemas`, whatever
folder its file sits in. `Fields/`, `Extensions/` and `Violations/` are navigation
only, and moving a type between them is not an API change.

The generator emits the literal text
`global::Waystone.Monads.Schemas.Schema`, so the entry point cannot move without
the generator moving with it. An extension method is only found through a `using`,
so putting `TextSchemaExtensions` in a sub-namespace would make `.NotEmpty()` stop
resolving until a consumer added a second one. And `PublicAPI.Shipped.txt` records
fully-qualified names, so any move after the package ships is a major bump.

**Everything under `Internal/` takes a sub-namespace.**
`Internal.Combinators` decorate and compose, `Internal.Structures` iterate,
`Internal.Fields` are what `Schema.Required` and its siblings return, and
`Internal.Reporting` is the plumbing a violation is built from. A type appearing
in the root namespace is a claim that a consumer is meant to reach it.

**The `Internal.*` namespaces are wired in with `global using`, in `Internal/InternalUsings.cs`
and its counterpart in the test project.**

## `Configure` lives on `SchemaConfig`, and that is the whole hierarchy rule

Three types:

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

**Do not put `Configure` back on `Schema<TIn, TOut>`.** Sealing it to a
`NotSupportedException` on an internal `SchemaNode<TIn, TOut>` is a Liskov
violation: half the hierarchy inherits a member it can only refuse. The throw is
unreachable only while `Evaluate` is its single caller — an invariant held by
convention across a dozen files rather than by the type system, and one the
generator emits code against.

## `FieldAccumulator` is public because generated code has to reach it

Evaluating a field is internal — `Field.Evaluate`, `Field<T>.EvaluateValue`,
`ParseContext` and `Outcome<T>` all are, deliberately, because they are what keeps
every schema's reporting behaviour identical. But the `Schema.Fields` ladder is
generated into the *consumer's* assembly, so it can see none of them.

`FieldAccumulator` is the one seam that crosses that line: five members, no way to
construct a violation, no way to reach a `ParseContext`. **Anything the ladder
needs in future is added here, and adding it widens the published surface of the
package permanently** — so prefer teaching the generator to compose what already
exists.

**It offers no success half.** A `Func<TOut>` success half would allocate a
delegate and a display class on *every* parse of every schema, to guard a branch
the caller can take itself. `HasViolations` plus `Failed<TOut>()` gives the caller
the same guarantee — the constructor never sees a `default` a field failed to
produce.

`Take` returns the value rather than a success flag with an `out`. Nothing would
read the flag: the generator runs every field regardless, then branches once on
`HasViolations`.

It works at `ParseContext.Root`; a nested schema's paths are rebased by
`SchemaConfig.Evaluate` after `Configure` returns.

## The `Transform` overloads are not ambiguous

`Transform(Func<TOut, TNext>)` and `Transform(Func<TOut, Result<TNext, Error>>)`
both apply to a factory returning `Result<Money, Error>`, and after inference both
reduce to the *same* parameter type. That looks like `CS0121`. It is not: C#
prefers the more specific parameter type, and `Result<TNext, Error>` is more
specific than a bare `TNext`, so the fallible overload wins and `TNext` binds to
`Money`. Verified by scratch compile, not by reading the specification. Do not
rename either overload to "fix" an ambiguity that does not exist.

**Only the total overload guards a null return, and it reports rather than throws.**
`MapSchema` turns a null conversion into a `Malformed` violation at the value's own
path. `Waystone.Monads` throws on a null `Option` projection and that stays true;
this package reports instead.

`Ok<TOk, TErr>`'s own constructor already rejects null, so the fallible path
carries no guard of its own and must not grow one: it would be dead code, and a
null there throws `ArgumentNullException` from the monad rather than the
`InvalidOperationException` a duplicated guard would raise.

## Decorators go through `DecoratorSchema`, and there are two tiers under it

`DecoratorSchema<TIn, TOut, TNext>` runs the inner schema and hands its `Outcome`
to `Decorate`, **sealing both `Evaluate` and `EvaluateAsync`**. A node written by
hand has to override the asynchronous path too, and one that forgets runs an
asynchronous inner schema synchronously, which nothing in the build notices.

Two tiers sit under it:

- **`ContextSchema<TIn, TOut>`** — seals `Decorate` to the identity and makes
  `Adjust` abstract. For a node that changes the *context* rather than the
  outcome. `Named` and `Sensitive` are one method each.
- **`RewritingSchema<TIn, TOut>`** — owns the "short-circuit on no violations,
  rewrite each one, rebuild the outcome" loop and exposes a single `Rewrite`
  hook. `WithMessage` and `WithCode` are one method each.

**A stub override means a tier is missing.** A `Decorate` that returns its
argument unchanged, or a node hand-rolling both paths, is a tier that has not been
written. Add the tier rather than the stub.

Four nodes are outside the hierarchy and each has a reason. `Not` evaluates a
*second* schema, so a synchronous `Decorate` would run that one synchronously on
the asynchronous path. `When`/`Unless` may skip the inner schema entirely, so
there is no outcome to decorate. `All`/`Any` fold over several branches.
`AsyncCheckSchema` has a rule that must be awaited, and `Decorate` returns a value
rather than a task.

**Do not give `AsyncCheckSchema` an asynchronous `Decorate` hook.** It means a
second abstract member on a base every decorator inherits, so every existing node
grows an override or a default that reintroduces the exact seal `DecoratorSchema`
exists to enforce — all to share about twenty lines with a single subclass.
Revisit it when a second node needs to await.

**Do not add a shared `CombinatorSchema` for `All` and `Any`.** Folding over
branches needs the accumulator to survive an `await`, and a `ref` local cannot
cross one — so the base would have to allocate a mutable fold object on every
parse to remove thirty lines of straight-line code. `CompositeNodeAsyncPathTests`
guards the hazard instead.

## `Outcome<T>` owns "same shape, new contents"

**`Outcome<T>` is not a `Result` and must not become one.** A `Result` has two
states; `Outcome<T>` has three, and the third one is the entire design. `Refined`
carries a value *and* violations, which is what lets a failed refinement leave the
value intact so the rest of that chain keeps running and keeps reporting. Collapse
it to `Result<T, SchemaViolation>` and the first violation ends the parse — the
gather-everything promise goes with it, and no existing test would fail loudly
enough to say so.

**Do not rebuild its internals on `Option<T>`.** `HasValue`, the
`Value` throw and the `default!` are `Option<T>` hand-rolled, and swapping them
would delete both. It would also allocate one `Option<T>` per outcome, and an
outcome is built per field, per list entry and per dictionary pair — five hundred
extra allocations on a five-hundred-item list.

`WithViolations` keeps whether a value survived and swaps the violation list.
`WithValue` keeps the violations and swaps the value. Use them; do not re-derive
which of the three constructors applies.

A node picking `Failed` where `Refined` was right silently stops the rest of a
chain reporting, and no test that did not specifically look for it would fail.

`WithViolations` requires a non-empty list, because `Refined` and `Failed` both
reject an empty one. Callers short-circuit on `Violations.Count == 0` first.

## The synchronous and asynchronous pair is guarded by a test, not by the type

`Evaluate` is `internal abstract`; `EvaluateAsync` is `internal virtual` with a
synchronous default. The default is right for a **leaf** — a rule with no inner
schema has nothing to await — and wrong for a node that holds one.

Inverting that (an abstract async member plus a `SyncSchema` tier) would force an
override onto every leaf and every test double, where the default is correct.
`CompositeNodeAsyncPathTests` reflects over the assembly instead: any concrete
`Schema<,>` holding a field of schema type must override `EvaluateAsync` somewhere
below the root, which deriving from `DecoratorSchema` satisfies. Add a wrapping
node without an asynchronous path and that test names it.

## Asynchrony reaches composition, and deliberately stops before `SchemaConfig`

`CheckAsync` is the only rule that has to go somewhere to decide, and
`AsyncCheckSchema` is the only node whose `Evaluate` throws instead of returning.
There is no `AsyncSchema<TIn, TOut>`: a second hierarchy would double every type
and every generator path to turn one misuse into a compile error, and the misuse is
already reported at build time by `WMSC0006` wherever a generator can see it.

**`SchemaConfig` is synchronous and stays that way.** `Configure` returns a value
rather than a task, so a field set only ever runs `Evaluate` — even under
`ParseAsync`. An asynchronous rule reached from a `Configure` body therefore never
runs, whatever the caller does, which is why `WMSC0006` is an error rather than
advice. A schema that needs one is composed instead, and parsed with `ParseAsync`.

**`Parse` throws rather than blocking.** Blocking would deadlock a caller on a
synchronisation context and quietly work everywhere else. The throw depends on the
input: a rule after a failed conversion, or under an absent `Schema.Optional`, is
never reached — so a schema can pass one call and throw on the next.

Widening this means adding an asynchronous path to `FieldAccumulator`, a
`ConfigureAsync` to `SchemaConfig` and an `IntoAsync` to every generated
`FieldSet`. That is deferred, not ruled out.

## `When` and `Unless` are extension methods on purpose

Both can skip the schema they are called on, and skipping is only well typed when
the schema hands back what it was given — otherwise there is no `TOut` to return
for an input that was never parsed. So they sit on `Schema<T, T>` as extensions
and are simply absent from `Schema<string, EmailAddress>`, which is a
missing-method error rather than a runtime one. Do not "fix" this by moving them
onto the base with a throw; that is the mistake `SchemaConfig` exists to undo.

## Violations, paths and messages

A violation carries its template and raw values privately and renders on demand, which
is what lets `.Sensitive()` redact a nested schema's violations after they were already
reported. `ViolationPath` holds typed segments and renders on demand for the same kind of
reason.

Read [docs/contexts/schema-violation-reporting.md](../../docs/contexts/schema-violation-reporting.md)
before editing anything under `Violations/` or `Internal/Reporting/`, or any member that
names a violation or writes its message: why `Any` nests branch failures rather than
flattening them, which of the two `Named` members to reach for, what `.Sensitive()` does
and does not redact, and how `{Expected}` and `{Predicate}` get filled.

## The rules

Read [docs/contexts/schema-rule-authoring.md](../../docs/contexts/schema-rule-authoring.md)
before adding or changing a member under `Fields/`, `Extensions/` or the `Schema.*` entry
points: why the primitives are cached identity schemas, why the comparison rules are one
generic family with `Number` the exception, why the text rules avoid a regular expression
wherever a scan will do, and why RS0026 is suppressed for two files rather than the
project.

## The structures are the only schemas that iterate

`ListSchema` and `DictionarySchema` are the only nodes that run an inner schema more than
once, and the only callers of `ParseContext.AtIndex` and `ParseContext.AtKey` — so they
are the only producers of the indexed and keyed segments `ViolationPath` carries.

Read [docs/contexts/schema-structures.md](../../docs/contexts/schema-structures.md) before
editing anything under `Internal/Structures/` or the `MinCount`/`MaxCount` extensions:
where the accumulation lives, how the report cap's two halves differ, why a null entry
never reaches the inner schema, and why those two extensions are two overloads apiece.

## The package multi-targets only for types a target framework adds

A framework joins `<TargetFrameworks>` when the BCL type a rule needs is not
polyfillable — `Schema.Date` needs `DateOnly`, which PolySharp does not supply.
Every such member sits behind an `#if`, and nothing else in the package differs by
target. Read the current list off the csproj and the members off the guards:

```
grep -n 'TargetFrameworks' src/Waystone.Monads.Schemas/Waystone.Monads.Schemas.csproj
grep -rn '#if NET' src/Waystone.Monads.Schemas --include=*.cs
```

**The public API baseline is split per target, and a framework added to the csproj needs
its folder in the same change.**
[docs/contexts/public-api-baseline.md](../../docs/contexts/public-api-baseline.md) has why
the split is unavoidable and how to harvest rows tagged by target framework.

## `Outcome<T>` has three constructors because a refinement is not a transform

- `Passed(value)` — no violations.
- `Refined(value, violations)` — the value survived, so the rest of its chain
  still runs. This is what makes "every failure is reported" true.
- `Failed(violations)` — no value, so nothing downstream of it can run.

`Failed` and `Refined` both reject an empty list, so "failed with nothing to say"
is unrepresentable rather than guarded against later. It is a class rather than a
struct for the same reason: `default(Outcome<T>)` would be exactly that state.
