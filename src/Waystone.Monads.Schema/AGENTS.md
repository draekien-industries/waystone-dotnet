# Waystone.Monads.Schema

Composable schema validation that *parses*: what comes out is a type the caller
could not have built without passing. The design lives in
[DRA-181](https://linear.app/draekien-industries/issue/DRA-181/design-waystonemonadsschema-composable-schema-validation);
this file carries only what a change here has to obey.

## The namespace is plural and the package is not

Package and assembly `Waystone.Monads.Schema`. Namespace
`Waystone.Monads.Schemas`. The mismatch is forced, not a preference.

A namespace and a type may share a name, but **the namespace wins lookup wherever
both are candidates**. With a singular namespace, any consumer whose own namespace
begins `Waystone.Monads.` resolves `Schema.Text` against the *namespace* and gets
`CS0234` — and that is every test project and every published doc sample in this
repository. Verified by scratch compile in both spellings. `Schema<TIn, TOut>` is
unaffected either way, since arity 2 excludes the namespace as a candidate; only
the bare `Schema` breaks, which is the one a reader types most.

## `Schema<TIn, TOut>` is open; `Field` is closed

DRA-181's first draft said both were closed with
`internal abstract void OnlyThisAssemblyMayDerive()`. That cannot hold for
`Schema<TIn, TOut>`: its own worked example has the consumer writing
`class OrderSchema : Schema<OrderDto, Order>`, and a consumer in another assembly
cannot override an internal member.

The resolution is that **`Evaluate` carries the guarantee instead of the marker**.
`Schema<TIn, TOut>` has a `protected` constructor and a `protected abstract
Configure`, so a consumer derives and shapes the parse; `Evaluate` and
`EvaluateAsync` are `internal`, so no outside type can change how violations
accumulate. `Field` and `Field<T>` keep the marker, because a consumer never
derives from them.

`Configure` is **abstract, not virtual-with-a-throw**, so a consumer who forgets
it gets a compile error rather than a runtime one. Internal nodes — primitives,
combinators, decorators — derive from `SchemaNode<TIn, TOut>`, which seals
`Configure` with the single unreachable throw in the package.
`SchemaClosedHierarchyTests` in the *analyzer* test project pins all of this,
including the positive case that an outside composed schema still compiles.

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

Paths do not have this problem: `Schema<TIn, TOut>.Evaluate` re-bases a composed
schema's violations under the parent's path through `ViolationPath.Nest`, so
`order.subject` comes out right even though `subject` was rendered first.

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

## `Outcome<T>` has three constructors because a refinement is not a transform

- `Passed(value)` — no violations.
- `Refined(value, violations)` — the value survived, so the rest of its chain
  still runs. This is what makes "every failure is reported" true.
- `Failed(violations)` — no value, so nothing downstream of it can run.

`Failed` and `Refined` both reject an empty list, so "failed with nothing to say"
is unrepresentable rather than guarded against later. It is a class rather than a
struct for the same reason: `default(Outcome<T>)` would be exactly that state.
