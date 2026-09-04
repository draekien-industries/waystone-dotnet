# Parsing with a schema

`Waystone.Monads.Schemas` is a separate package, and it **parses rather than
validates**. A validator checks an object you already built; a schema builds the
object, and what comes out is a type the caller could not have constructed
without passing. Holding the result *is* the proof.

```csharp
Result<Quest, SchemaViolation> quest = QuestSchema.Instance.Parse(posting);
```

`SchemaViolation` derives from `Error`, so it is the `TErr` of an ordinary
`Result` — `Match`, `AndThen` and `MapErr` all work on a parse unchanged, and a
schema needs no adapter to sit in a chain.

**One parse reports every failure at once.** Three bad fields give three
violations, so a caller fixes their payload once instead of three times. A
failed `Transform` is the single exception: it produces no value, so the rules
after it *on that chain* cannot run. Its siblings are unaffected and still
report.

Reach for it at the boundary — a request body, a queue message, a file row. Skip
it inside the domain, where the types already say what is true.

## Declare it as a field set

The canonical shape is a `partial` class deriving `SchemaConfig<TIn, TOut>`,
with the rules pulled out into shared static schemas above it:

```csharp
public static class Guild
{
    public static readonly Schema<string, string> Email =
        Schema.Text.Trim().Email();
}

public partial class QuestSchema : SchemaConfig<QuestDto, Quest>
{
    protected override Result<Quest, SchemaViolation> Configure(QuestDto subject) =>
        Schema.Fields(
                   Schema.Required(subject.Title, Schema.Text.Trim().NotEmpty()),
                   Schema.Required(subject.PatronEmail, Guild.Email).Named("patron"),
                   Schema.Optional(subject.PartySize, Schema.Number.Int32.Positive()))
              .Into((title, patron, party) => new Quest(title, patron, party));
}
```

A source generator writes `Instance`, and writes `Schema.Fields` at exactly the
arity used — which is why a wrong-sized `Into` lambda is a build error rather
than a surprise. Three things make a first attempt fail to compile, and each has
a diagnostic naming it: the class and every type containing it must be `partial`
(`WMSC0001`), it needs a constructor callable with no arguments (`WMSC0002`),
and it must not declare a member named `Instance`, `Schema` or `FieldSet`
(`WMSC0003`).

`Schema.Optional` yields `Option<TOut>`, not `TOut?`, so an absent value never
reaches a rule and never reaches the constructor.

**A field's path is read from the expression passed to it**, through
`CallerArgumentExpression`. `subject.Title` gives `title`; anything else — a
method call, an indexer, a literal — keeps its punctuation, and that text
reaches logs and API responses. `WMSC0008` warns; `.Named("total")` fixes it.

**Set the name on the field, not on the schema.** A schema is shared, so a name
baked into one renames every field of its shape and nothing reports it. A field
is built per parse and cannot leak.

## Reach for the named schema

Every primitive below is `Schema.For<T>()` with rules already hung off it, and
`For<T>()` is cached per type — the named spelling and the bare one are the
*same object*. Prefer the named one anyway, because it is where the rules for
that type are listed. `WMSC0009` suggests it.

| Type | Schema | Rules to chain |
| --- | --- | --- |
| `string` | `Schema.Text` | `Trim`, `NotEmpty`, `Length`, `LengthBetween`, `MinLength`, `MaxLength`, `Matches`, `Email`, `Url`, `OneOf`, `StartsWith`, `EndsWith`, `Contains` |
| `int`, `long`, `decimal`, `double` | `Schema.Number.Int32` and siblings | `Between`, `AtLeast`, `AtMost`, `GreaterThan`, `LessThan`, `Positive`, `Negative` |
| `Guid` | `Schema.Uuid` | `NotEmpty`, `IsVersion4`, `IsVersion7` |
| `bool` | `Schema.Bool` | `IsTrue`, `IsFalse` |
| `DateTimeOffset` | `Schema.Timestamp` | `After`, `Before`, `OnOrAfter`, `OnOrBefore` |
| `DateOnly` | `Schema.Date` | The same four. Not on `netstandard2.0` |
| An enumeration | `Schema.Enum<T>()` | Nothing — it checks membership itself |
| Anything else | `Schema.For<T>()` | `Check` and `Transform`, which is where your own rules start |

`Schema.List(item)` and `Schema.Dictionary(key, value)` are the structures, both
taking `MinCount` and `MaxCount`. Every entry is parsed independently, so a bad
item at index 3 does not hide a bad one at index 7 — both are reported, with
paths like `objectives[1]` and `rates["AUD"]`. One structure reports at most 64
problems and then emits a `Truncated` violation; check for that code before
telling a caller their list has exactly 64 problems.

`AtLeast`, `AtMost` and `Between` include their bounds; `GreaterThan`,
`LessThan`, `Before` and `After` exclude them; `Positive` and `Negative` exclude
zero. **Prefer one rule where one says the same thing** — `Between(1, 6)` and
`AtLeast(1).AtMost(6)` accept exactly the same parties and report a bad one
exactly once either way. The first is simply shorter to read.

Two rules carry the security of the system that installs them, and both are easy
to under-specify:

- **Give `Matches` a timeout.** It takes a `Regex` rather than a pattern string
  precisely so the choice is in front of you. The pattern is yours; the value is
  a stranger's, and an expression with no ceiling runs for as long as a crafted
  input takes to defeat it.
- **Restrict `Url`'s scheme.** An absolute URL includes `javascript:`, `data:`
  and `file:`. Write `Url("https")` wherever the value will be followed or
  rendered, which is nearly always. An empty scheme list accepts nothing at all.

## Gate without producing a value

Some fields must be *checked* and have nothing to contribute to the object being
built. Those yield `Field<Checked>` — `Checked` is a struct carrying no data,
standing in for the value a gating rule would have produced. They go to
`Refine`, not into the `Into` lambda, and take no slot in it.

| Reach for | When |
| --- | --- |
| `Schema.Forbidden(subject.LegacyId, "Do not send {Path}.")` | The field must be **absent** |
| `Schema.Extend(subject, Chronology)` | A rule spans several fields — it runs over the whole subject and reports at the subject's path |
| `field.AsChecked()` | The field is allowed and checked, and its value has nowhere to go |

```csharp
Schema.Fields(
           Schema.Required(subject.Email, Guild.Email),
           Schema.Required(subject.Name, Schema.Text.NotEmpty()))
      .Refine(
           Schema.Required(subject.ConfirmEmail, Guild.Email).AsChecked(),
           Schema.Forbidden(subject.LegacyId, "Do not send {Path}."))
      .Into((email, name) => new Party(email, name));
```

`AsChecked` drops the value and keeps everything else — the rules still run, and
a failure is still reported at that field's own path. Reach for it where a
caller has to send a field correctly but the parsed type has no place for it: a
confirmation address, or part of a wire contract another system reads.

**`Refine` accepts a value-producing field and silently discards its value**,
which is what `WMSC0005` warns about. Two wrong ways to answer that warning, and
one right one:

```csharp
// Poor — the value is checked and thrown away, and WMSC0005 says so
.Refine(Schema.Required(subject.ConfirmEmail, Guild.Email))

// Poor — the discard is positional, so reordering the fields silently rebinds
// every parameter whose type still lines up
.Into((email, name, _) => new Party(email, name))

// Good — says the discard was meant, and leaves WMSC0005 working on the
// fields where it was not
.Refine(Schema.Required(subject.ConfirmEmail, Guild.Email).AsChecked())
```

A gating schema that builds nothing at all ends in `Checked()` rather than
`Into`, and is declared `SchemaConfig<ConsentDto, Checked>`.

## Compose

Every member below returns a new schema; nothing mutates the receiver, so a
result that is not captured or chained does nothing at all.

| Member | Adds |
| --- | --- |
| `Check(predicate, code, message)` | A rule. The value survives a failure, so every later rule still runs |
| `Transform(convert)` | A new produced type. A failure stops that chain — there is no value to carry |
| `Not(schema, message)` | The inverse of a schema. A message is required, since negation has none to borrow |
| `When(predicate)` / `Unless(predicate)` | A condition, on `Schema<T, T>` only |
| `All(branches)` / `Any(branches)` | Every branch, or the first that accepts |
| `WithMessage(text)` | One message replacing *every* violation the chain produced, not only the last |
| `WithCode(code)` | A domain code to branch on |
| `Sensitive()` | Redaction of `{Received}` here and everywhere nested inside |

**Mark the outermost schema `Sensitive` and stop.** Everything nested inside is
redacted too, including a nested schema that reported before the outer one ran.
The value cannot be read back afterwards — a `Violation` exposes its path, its
code and its rendered message, and nothing else.

`Trim` is a transform, so **it must come first**. `Schema.Text.Trim().NotEmpty()`
rejects a string of spaces; `Schema.Text.NotEmpty().Trim()` accepts it and hands
the constructor an empty string.

## Write your own rules

Everything the package ships is composition, and so is everything you add. There
is **no rule interface to implement and no schema to subclass**:
`Schema<TIn, TOut>` closes itself with an internal abstract member, so a subclass
declared outside the package is a `CS0534` the compiler refuses.
`SchemaConfig<TIn, TOut>` is the only public way into the hierarchy, and it is
for a field set rather than for a rule.

A rule is therefore an **extension method that returns `schema.Check(...)`**:

```csharp
public static class QuestRules
{
    public static Schema<TIn, int> Even<TIn>(this Schema<TIn, int> schema)
        where TIn : notnull =>
        schema.Check(
            static value => value % 2 == 0,
            ViolationCode.Mismatched,
            "Expected {Path} to be even, but got {Received}.");
}
```

It composes with everything already there —
`Schema.Number.Int32.Positive().Even()` — because it takes a schema and gives
one back. Where the codebase is on C# 14, an extension member block spells the
same thing and groups a family of rules under one receiver:

```csharp
public static class QuestRules
{
    extension<TIn>(Schema<TIn, int> schema) where TIn : notnull
    {
        public Schema<TIn, int> Even() =>
            schema.Check(
                static value => value % 2 == 0,
                ViolationCode.Mismatched,
                "Expected {Path} to be even, but got {Received}.");
    }
}
```

Either spelling is fine, and both are found the same way — through a `using` for
the namespace the static class sits in.

**Do not copy the package's own rule source literally.** Every rule it ships
calls an internal `Rules.Add`, which a consumer's assembly cannot see, so a rule
written that way fails to compile with nothing explaining why. `Check` is the
public equivalent and takes the same three things.

### Choose the code before the message

`Check` takes either a `ViolationCode` — a deliberately small vocabulary — or
any `ErrorCode` of your own. Reach for the enum wherever a
domain code would only restate the check.

| Code | Means |
| --- | --- |
| `Incomplete` | A required value was absent; nothing arrived |
| `Malformed` | A value arrived and could not be read as the expected shape |
| `NotAllowed` | A value arrived where the schema permits none |
| `OutOfRange` | A value fell outside an *ordered* bound — length, count, magnitude, date |
| `Mismatched` | A value failed an *unordered* check — a pattern, a permitted set, a version |
| `Duplicate` | Two entries collided on uniqueness. Reported at the later one |
| `Conflicting` | Two individually valid values contradict each other — what a cross-field rule reports |
| `Truncated` | The *report* stopped gathering. Describes the report, never the input |

A domain code goes through the `ErrorCode` overload —
`schema.Check(predicate, OrderCatalog.Codes.LineCountExceeded, message)` — and
groups through `SchemaViolation.ByCode()` beside the built-in kinds.

### The message template has five tokens, and one you cannot fill

`{Path}`, `{Received}`, `{Predicate}`, `{Expected}` and `{Code}`. **Anything else
renders literally**, so a misspelled `{Exepcted}` reaches a caller as those exact
characters — no exception, no warning, nothing in the build.

`{Expected}` is the one to avoid. It renders a bound, and the public `Check` has
nowhere to put one, so it renders literally in any message you write.
**Interpolate the bound yourself, or reach for `{Predicate}`:**

```csharp
// Poor — the caller is told "Expected sku to be at least {Expected}."
schema.Check(
    value => value >= floor,
    ViolationCode.OutOfRange,
    "Expected {Path} to be at least {Expected}.");

// Good — the bound is interpolated, so it survives into the message
schema.Check(
    value => value >= floor,
    ViolationCode.OutOfRange,
    $"Expected {{Path}} to be at least {floor}.");
```

`{Path}` still has to survive the interpolation, hence the doubled braces.

`{Predicate}` needs no argument at all. The compiler captures the predicate's
source text through `CallerArgumentExpression` and that text is what renders, so
a rule describes itself without the condition being written out twice:

```csharp
// "Expected sku to satisfy value => value >= floor."
schema.Check(
    value => value >= floor,
    ViolationCode.OutOfRange,
    "Expected {Path} to satisfy {Predicate}.");
```

Pass a fourth argument to override that text where the lambda reads badly
mid-sentence — `"at least the floor price"` rather than the source.

`{Received}` also needs no argument: it renders the rejected value, or `***` under
`Sensitive()`. Neither `{Expected}` nor `{Predicate}` is ever redacted, because
both come from what the rule's author wrote rather than from anything that
arrived.

### What the predicate must obey

- **It must not throw.** Nothing catches it, so an exception aborts the whole
  parse and every sibling field's report with it. Guard inside the predicate and
  return `false`.
- **It must have no side effects.** A schema may run it on the synchronous or
  the asynchronous path, depending on how the caller parsed.
- **Mark it `static` where it can be.** A schema is usually a shared static
  field, so a capturing lambda allocates on every parse of every input. A bound
  the rule took as a parameter has to be captured; nothing else should be.
- **Do not capture the clock.** A static schema capturing `DateTimeOffset.UtcNow`
  freezes that instant at process start. Take a clock the rule can call instead.

### Write it as a transform when it produces a value

`Check` refines: the value comes through unchanged and the chain keeps running
even on failure. `Transform` narrows to a new type and stops that chain when it
fails. Use `Transform` for a factory that can refuse:

```csharp
Schema<string, EmailAddress> Email =
    Schema.Text.Trim().Transform(EmailAddress.Create);
```

The overload taking `Func<TOut, Result<TNext, Error>>` is the one to reach for
whenever a conversion can fail, because it keeps the factory's own code and
message. The total overload reports a `null` return as a `Malformed` violation
rather than throwing — so a mistake there fails the parse instead of the process
— but it cannot say *why* the conversion refused.

## Asynchrony stops before the field set

`CheckAsync` is the rule that has to go somewhere to decide, and it takes the
parse's cancellation token. It runs only after everything before it accepted, so
a round trip is never spent on input that was already going to fail.

**`SchemaConfig.Configure` returns a value rather than a task, so a field set
only ever runs the synchronous path — even under `ParseAsync`.** An asynchronous
rule reached from a `Configure` body throws `InvalidOperationException`, and
nothing in the type system says so, because `CheckAsync` returns the same type a
synchronous rule does. `WMSC0006` is an error for exactly that reason.

Compose the asynchronous rule *around* the generated schema instead:

```csharp
public static ValueTask<Result<Quest, SchemaViolation>> ParseAgainstTheBoard(
    QuestDto posting,
    IQuestBoard board,
    CancellationToken cancellationToken) =>
    QuestSchema.Instance
               .CheckAsync(
                    (quest, token) => board.TitleIsFree(quest.Title, token),
                    ViolationCode.Duplicate,
                    "That quest is already on the board.")
               .ParseAsync(posting, cancellationToken);
```

The field set stays synchronous, and the round trip happens once, after every
cheap rule has had its say.

`Parse` throws rather than blocking on an asynchronous schema — and whether it
reaches one depends on the input, so a schema can pass one call and throw on the
next.

## Traps

- **`Schema.Uuid` accepts `Guid.Empty`.** An omitted `Guid` deserialises to
  `Guid.Empty` rather than to null, so `Required` alone does not catch it. Chain
  `NotEmpty`. Both version rules already reject it, so adding `NotEmpty` beside
  one of them says nothing new.
- **`IsVersion7` is on .NET 9 and later only**, which is where
  `Guid.CreateVersion7()` arrived. Versions 4 and 7 are the only ones .NET
  creates, so they are the only ones with rules. Both read the version digits and
  nothing else, so a value with those digits set passes however it was made.
- **`Schema.Enum<T>()` is wrong for `[Flags]`** unless the exact combination is a
  declared member, and zero passes only where a member declares it. Declare
  `None = 0` if an empty combination is legitimate.
- **A failed `Schema.Any` reports at its own path**, with each branch's failures
  nested beneath it — not at the field's path.
- **`When` and `Unless` exist on `Schema<T, T>` only.** On a transforming schema
  they are a missing-method error, because a skipped input would have no `TOut`
  to hand back.
- **`MaxCount` on a `Schema.List` counts the input before parsing anything**,
  which is what makes it a guard on untrusted input rather than a report
  afterwards. It stops the parse there, so an eleventh item is rejected with
  nothing said about the ten.
- **Read a `Violation`'s path through `Segments`, not the rendered string.**
  `PathSegment.Kind` is `Property`, `Index`, `Key` or `Branch`, and only the kind
  distinguishes them.
