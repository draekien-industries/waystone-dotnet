# How a schema violation is named, pathed and rendered

Read before editing anything under `Violations/` or `Internal/Reporting/` in
`src/Waystone.Monads.Schemas`, or any member that names a violation or writes its message.

## `Any` nests its branch failures; do not flatten them

When every branch fails, `AnySchema` emits one violation at its own path plus each branch's
violations rebased under a numbered segment — `contact{0}.email`. The numbering is why
`ParseContext.AtBranch` exists, and the braces are what keep a branch apart from a list
position. Flattening onto the field's own path would put a dozen irrelevant failures where a
caller reading `ByPath()` expects one.

**`Named` replaces a trailing name and appends after anything else.** Renaming an index is
never what a caller means: inside an `Any`, the innermost segment is a branch number, so
replacing it would turn `contact{1}` into `contact.byPhone` — losing the branch and colliding
with a real property of that name. `ViolationPath.Rename` switches on `PathSegmentKind` to
decide, which is a fact the type holds rather than something recovered from rendered text.

## There are two `Named` members, and the field one is the one to reach for

`Schema.Named` renames through the parse context; `FieldExtensions.Named` rebuilds the field
with a different segment.

**A schema is shared and a field is not.** A schema declared as a static field is reused by
every field of its shape, so `.Named("patron")` baked into one silently renames all of them,
and nothing reports it — the parse succeeds and the paths are simply wrong. A field is built
inside `Configure`, once per parse, so the same call cannot leak. Inside a field set, use the
field one.

**`Schema.Named` stays, because a schema is not always reached through a field.** A branch of
`Schema.Any` and a schema handed straight to `Parse` both need a name and have no field to
hang it on.

**It is implemented as `Field<T>.WithName`, not as a wrapper.** A wrapping field would append
its own segment on top of the one the inner field already appends, giving
`patronEmail.patron`. Each field type instead rebuilds itself with the new name.

**On `ExtendField` it nests rather than replaces.** An extension reports at the subject's own
path precisely so a cross-field rule is not filed under one of the fields it spans, so it has
no segment of its own to overwrite. Naming one gives those violations somewhere to live;
leaving it unnamed keeps them at the root, which is the default.

`Any` allocates its rejection list before trying the first branch, so the short-circuit path
pays for one list. The alternative is a nullable local and a `!` on a state the type system
cannot see is impossible, which costs a partially covered branch to save one allocation on a
path that is not hot.

## `.Sensitive()` reaches a nested `Configure`

`Violation.Message` reads as a rendered string, but the template and the raw received and
expected values stay on the violation, private. That is what lets a schema learn it is
sensitive *after* a nested one already reported: `SchemaConfig` re-creates the nested
violations under `_isSensitive || context.IsSensitive` and they render again with `***`.

So marking the outermost schema is enough, and marking an inner one as well changes nothing.
The doc comment on `Sensitive` says so.

**The template and the raw values stay private, and must.** Exposing either would hand back
the value the redaction exists to withhold — a caller could read `Received` off a violation
whose `Message` says `***`. A caller who wants to re-render has to be given a schema, not a
violation.

Paths do not have this problem: `SchemaConfig<TIn, TOut>.Evaluate` re-bases a composed
schema's violations under the parent's path through `ViolationPath.Nest`, so `order.subject`
comes out right even though `subject` was rendered first.

**`ViolationPath` holds segments and renders on demand; do not collapse it to a stored
string.** Storing the rendered text makes `Nest` decide whether to insert a `.` by testing
whether the child's text begins with `[`. That is correct only by coincidence — both
bracketed segment kinds happen to start with one — and a future segment kind that does not
would silently glue two names together with no separator, no compile error and no failing
test. `Nest` is array concatenation and the separator is decided from `PathSegmentKind` at
render time.

**`MessageTemplate.Render` is a single pass, and must stay one.** Substituting token by token
with `string.Replace` would re-substitute a rejected value that happens to contain `{Code}` —
and rejected values are exactly the untrusted input this package exists to handle.

## The message tokens

**Neither `{Expected}` nor `{Predicate}` is redacted by `.Sensitive()`, and neither must
be.** `{Path}` and `{Received}` render something derived from the input; these two render a
bound and a lambda the schema's *author* wrote down. Redacting
`Expected {Path} to be at least ***` costs the reader the only actionable part of the
sentence and protects nothing.

**A rule supplies `{Expected}` by constructing `CheckSchema` with a bound, not through public
`Check`.** `Check` takes no bound, so `{Expected}` renders literally in a message a caller
writes. `Rules.Add` is the in-assembly path, and it is also where the `schema` null guard
lives so every extension reports the same parameter name.

**`{Predicate}` is what public `Check` fills instead.** A `CallerArgumentExpression` parameter
captures the predicate's source text, so a caller's own rule describes itself without the
condition being written out twice. That parameter is part of why RS0026 is suppressed for
`SchemaOfTInTOut.cs`; see
[schema-rule-authoring.md](schema-rule-authoring.md).

**A forwarding overload has to pass the captured text on by hand.**
`Check(predicate, ViolationCode, …)` calls its `ErrorCode` sibling, and left to the compiler
that inner call captures its own argument — so every violation would read `predicate` rather
than the caller's lambda. The tests assert the rendered text through both overloads, which is
the only thing that catches this.

**`WithMessage` renders `{Expected}` and `{Predicate}` literally, on purpose.** It replaces
the messages of every rule on the chain at once, so there is no single bound or predicate
left to name. Documented on the member; do not "fix" it by threading the last one through,
which would silently pick one rule out of several.
