---
title: XML doc comment audit for Waystone.Monads
date: 2026-09-12
status: active
---

# XML doc comment audit for Waystone.Monads

## Goal

Rewrite the XML doc comments on the public surface of `Waystone.Monads` so each
one tells a consumer what they need in order to call the member, in plain
language, with no figurative phrasing, and with nothing else in it.

These comments are unusually exposed. IntelliSense puts them in front of every
consumer of the package, and `CS1591` is suppressed here, so nothing in the build
reports a comment that is thin, stale or wrong. The only thing standing between a
defective comment and a caller acting on it is a review like this one.

This document proposes the changes. It does not make them. No `.cs` file was
edited to produce it.

## Scope

548 public members across 36 hand-written files.

| Partition | Files | Public members | Sections |
| --- | --- | ---: | ---: |
| `Option<T>` core | `Options/OptionOfT.cs` | 70 | 33 |
| `Result<TOk, TErr>` core | `Results/ResultOfTOkTErr.cs` | 65 | 45 |
| Option binders | `Options/OptionBound.cs`, `Options/OptionFactoryBound.cs` | 45 | 13 |
| Result binders | `Results/ResultBound.cs`, `Results/ResultFactoryBound.cs` | 43 | 8 |
| Option factory, cases, extensions | `Options/Option.cs`, `Some.cs`, `None.cs`, `Extensions/*` | 170 | 26 |
| Result factory, cases, extensions | `Results/Result.cs`, `Ok.cs`, `Err.cs`, `Extensions/*` | 89 | 22 |
| Configuration, diagnostics, errors | `Configs/*`, `Diagnostics/*`, `Results/Errors/*`, `Exceptions/*` | 66 | 21 |

Corpus measurements taken before the audit, excluding `obj/` and `bin/`:

```
doc comment lines      6,228
<remarks> blocks         233    144 of them in the four largest files
em dashes                 68
<param> lacking a
  terminal period         29    of 188 single-line tags
```

## Not covered

The awaited-receiver overloads are generated at build time by
`Waystone.SourceGenerators` and carry 122 `cref="T:…"` references that ship to
consumers through IntelliSense exactly as the hand-written ones do. They are not
in this audit, because editing them means editing the generator's emission in
`src/Waystone.SourceGenerators`, not the files listed above. They are not clean;
they are unexamined.

## Priority 1 — four comments state something the code does not do

These are separated from the other findings because they are wrong facts rather
than wording. Each was verified against the implementation.

### `Option<T>.IsNone` says it returns false for `None<T>`

`src/Waystone.Monads/Options/OptionOfT.cs:50`

```csharp
/// <summary>
/// Returns <see langword="false" /> if the option is a
/// <see cref="None{T}" /> value.
/// </summary>
public abstract bool IsNone { get; }
```

`src/Waystone.Monads/Options/None.cs:32` returns `true`, and
`src/Waystone.Monads/Options/Some.cs:69` returns `false`. The comment reads as
`IsSome`'s text with the wrong word negated. A caller reading IntelliSense is
told the property returns `false` for the one case it returns `true` for.

### `Error.FromException` says the message is copied verbatim

`src/Waystone.Monads/Results/Errors/Error.cs:68`

```csharp
/// The message is <see cref="Exception.Message" /> verbatim, so anything
/// the exception's text carries…
```

The constructor it forwards to, at `Error.cs:49`, does neither:

```csharp
Message = string.IsNullOrWhiteSpace(message)
    ? "An unexpected error occurred."
    : message.Trim();
```

An exception whose `Message` is `"  timeout  "` produces an error whose message
is `"timeout"`. One whose `Message` is empty produces
`"An unexpected error occurred."` The same file already documents this correctly
44 lines earlier, on the constructor, so the two comments contradict each other.

`FromException` also omits that a custom `ErrorCodeFactory` changes the resulting
code, which its sibling `ErrorCode.FromException` does state.

### `Result<TOk, TErr>.Expect` says it differs from `Unwrap` only in the message

`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:546`

```csharp
/// Throws on an <see cref="Err{TOk,TErr}" />, differing from
/// <see cref="Unwrap" /> only in that the thrown message leads with
/// <paramref name="message" />.
```

They throw different types. `Err.cs:206` throws `UnmetExpectationException`;
`Err.cs:213` throws `UnwrapException`, whose generic form `UnwrapException<T>`
carries a `Value` property that `UnmetExpectationException` does not have. A
`catch (UnwrapException<TErr>)` written around an `Expect` call on the strength
of this sentence compiles and never fires.

A fourth claim of the same kind is in `Results/ResultBound.cs:44`, and is covered
under the Result binders partition below: the `Bound<TState>` remarks say every
asynchronous member throws rather than returning a faulted task, but
`ResultBound.cs:474`, `:566` and `:910` are declared `async` and so return a
faulted task like any other `async` method.

## Priority 2 — one behaviour gap, now tracked separately

This is not a documentation defect and no comment should describe the current
behaviour, because documenting it would turn it into a contract.

### The state-bound `AndThenAsync` does not guard a null factory result

Tracked as [DRA-216](https://linear.app/draekien-industries/issue/DRA-216/fix-guard-every-factory-returning-delegate-against-a-null-return),
which widens the scope from these two members to every factory-returning delegate
in the library. The two below are what this audit found; they are the starting
inventory, not the whole answer.

Every sibling guards it:

```csharp
// Some.cs:214
AndThen<TOut>(…)         => Option.NotNull(optionFactory(Value), nameof(optionFactory));
// Some.cs:219
AndThen<TState, TOut>(…) => Option.NotNull(optionFactory(Value, state), nameof(optionFactory));
// Some.cs:225
AndThenAsync<TOut>(…)    => Option.NotNullAsync(optionFactory(Value), nameof(optionFactory));
```

`OptionBound.cs:731` does not:

```csharp
public ValueTask<Option<TOut>> AndThenAsync<TOut>(
    Func<T, TState, ValueTask<Option<TOut>>> optionFactory)
    where TOut : notnull =>
    Source is Some<T> some
        ? optionFactory(some.Value, _state)
        : new ValueTask<Option<TOut>>(Option.None<TOut>());
```

A factory returning a null `Option<TOut>` throws `ArgumentNullException` through
all three siblings and propagates the null through this one, where it becomes a
`NullReferenceException` at some later call with nothing pointing back to the
factory that caused it.

`Results/ResultBound.cs:666` has the identical shape against `Ok.cs:177`, which
does guard with `Result.NotNullAsync`. Both members are added by PR #248.

`OrElse` and `OrElseAsync` were checked and guard nowhere — not in `None.cs`, not
in either binder. That is uniform, so it reads as a design choice rather than a
second gap, and DRA-216 carries the question of whether it stays.

## Analyzer claims: all three confirmed

The comments cite three diagnostics by id. Each was checked against the analyzer
source rather than assumed, because a comment naming a rule that does not fire is
worse than one naming none.

| Claim | Checked against | Verdict |
| --- | --- | --- |
| `WM2017` reports a capturing call and names `With` | `Waystone.Monads.Analyzers/Rules.cs:281` | Accurate |
| `WSG0003` explains the `ValueTask`-to-`Task` non-conversion and the `CS0411` it surfaces as | `Waystone.SourceGenerators/AsyncSurface/Rules.cs:14` | Accurate |
| `MapOrAsync`'s remarks say `WM2016` reports it against `MapOrElseAsync` | `Waystone.Monads.Analyzers/LazyVariantAnalyzer.cs:22` | Accurate |

The third was initially recorded as unresolved, because `WM2016`'s descriptor
text names only the synchronous members. The rule does not match on that text — it
matches on a dictionary, and that dictionary carries the async pair explicitly:

```csharp
// LazyVariantAnalyzer.cs:22
["MapOrAsync"] = "MapOrElseAsync",
```

## Priority 3 — the systemic patterns

Tag occurrences across the seven reports. Three modes account for most of the
volume, and they are not equally worth fixing.

| Mode | Count | Worth |
| --- | ---: | --- |
| `missing-punctuation` | 78 | Mechanical. Concentrated in the `MapOr*`/`MapOrNull*`/`MapOrElse*` family and the collection extensions' `<typeparam>` tags, which suggests those were written in a separate pass. |
| `figurative` | 75 | The bulk of the rewrite. |
| `inconsistent-sibling` | 27 | Adjacent members using different conventions. |
| `copy-paste-drift` | 18 | **The one with real consumer cost.** See below. |
| `restated-signature` | 17 | Comments that repeat the name and type. |
| `non-consumer-content` | 15 | Design rationale, library history, cross-language lineage. |
| `buried-summary` | 12 | Contract detail placed where a generator will not index it. |
| `over-long-remarks` | 11 | |
| `stale` | 7 | Includes the four wrong facts above. |
| `prose-instead-of-tag` | 6 | `<c>static</c>` where `<see langword="static" />` belongs. |
| `undocumented-failure` | 4 | |
| `throat-clearing` | 3 | |

`copy-paste-drift` deserves the attention that its count does not suggest.
Documentation generators index the first sentence alone, so two overloads opening
with the same sentence become one indistinguishable pair in IntelliSense:

```
✗ Map<TOut>(Func<T, TOut> map)
     "Maps an Option<T> to an Option<TOut> by applying a function to a contained value."
  Map<TState, TOut>(TState state, Func<T, TState, TOut> map)
     "Maps an Option<T> to an Option<TOut> by applying a function to a contained value."
```

The caller choosing between them is choosing between allocating a closure per
call and not allocating one, and the two entries give them nothing to choose on.
`Map`, `MapOr` and `MapOrElse` each do this with their `<TState>` sibling;
`Match<TOut>` and `Match(Action, Action)` do it across the value-returning and
side-effecting forms.

## Conventions this audit applied

Recorded so that whoever applies the changes stays consistent with them, and so a
later reviewer can tell a deliberate choice from an oversight.

**Summaries are never deleted, only expanded.** The rubric permits deleting a
summary that adds nothing beyond the member name. This repository documents every
public member and suppresses `CS1591`, so a deletion would be invisible and
permanent. Anemic summaries were rewritten instead.

**`<inheritdoc />` was left alone.** `Some.cs`, `None.cs`, `Ok.cs` and `Err.cs`
are almost entirely `<inheritdoc />`, which is correct. Those were checked for
whether the inherited text suits the override, not expanded into written text.

**Figurative phrasing is rewritten; the field's own vocabulary is not.** A dead
metaphor that is the domain's term is literal and stays. The test applied was
whether the author reached for the image or the field did.

```
✓ keep — the field's vocabulary        ✗ rewrite — the author's image
  "a faulted task"                       "so a None<T> costs no await"
  "the library swallowed the exception"  "the state is spent, not sticky"
  "abandoning the subscription leaks one" "the bridge out of Option<T>"
  "a lock is held"                       "a capture that creeps back in"
  "a value is wrapped"                   "the crossing point from absent to failed"
  "enumeration is deferred"              "the point is the delegate, not this type"
```

**Cost language is the boundary case, and was drawn at whether a measure is
given.** "Costs no await" was rewritten, because the literal fact — the delegate
is not invoked — is shorter and checkable. A bare "expensive" with nothing behind
it was rewritten for the same reason. Performance terms of art such as "hot path"
were kept. One residual inconsistency is recorded rather than resolved:
`OptionBound.cs`'s "costs something" and "costs anything" were left in place by
that partition's audit as standard performance vocabulary, while the equivalent
phrasing elsewhere was rewritten. Decide it one way when applying.

**Claims about analyzers were checked against the analyzer source**, not assumed.
Two were confirmed, one is open and recorded above.

## Steps

1. Apply Priority 1. Four wrong facts, four files, no dependency between them.
2. DRA-216 carries the Priority 2 gap. Nothing here waits on it, but the two
   members it names are added by PR #248, so settling it before that merges costs
   less than changing shipped behaviour afterwards.
3. Apply the mechanical sweep — `missing-punctuation`, `prose-instead-of-tag` —
   as one change. It touches many files and reads trivially.
4. Apply `copy-paste-drift` and `inconsistent-sibling` per family, so that each
   family's overloads are decided together rather than one at a time.
5. Apply the remaining per-member findings partition by partition.

Steps 3 to 5 change no public API, so they need no paired documentation PR in
`draekien-industries/docs`. They also do not trigger a release: `release.yml`
excludes `**/*.md` but these are `.cs` changes under `src/**`, so a merge does
publish. Version them as `docs:` so no bump is read from the subject.

## Done when

Every section below has been applied or explicitly declined with a reason, and a
fresh grep for the figurative phrases listed above returns nothing in
`src/Waystone.Monads`. DRA-216 closes on its own terms and does not gate this.

---

# Findings by partition

## Option<T> core

Scope: `src/Waystone.Monads/Options/OptionOfT.cs` only.
70 public declarations examined (the `Option<T>` type itself plus 69 members).

### `Option<T>` (type)
`src/Waystone.Monads/Options/OptionOfT.cs:13` — over-long-remarks, non-consumer-content, figurative

```diff
 /// <remarks>
- /// <para>
  /// A projection that returns null throws <see cref="ArgumentNullException" />
  /// rather than producing a <see cref="None{T}" />. Every projection here is
- /// constrained to a non-nullable output, so a null is a broken contract and not
- /// an absent value, and collapsing it to <see cref="None{T}" /> would make the
- /// two indistinguishable — the caller would read "no value" and never learn the
- /// projection was wrong. <see cref="Result{TOk,TErr}" /> has always behaved this
- /// way; this type was the outlier until 7.0.0.
- /// </para>
+ /// constrained to a non-nullable output, so a null is treated as a broken
+ /// contract rather than an absent value.
  /// <para>
  /// When a projection genuinely may yield nothing, project into an option with
- /// <c>AndThen</c> and <see cref="Option.FromNullable{T}(T)" />. That is the
- /// difference between mapping and binding, and it is deliberately explicit. The
- /// two lenient entry points are <see cref="Option.Try{T}" />, whose whole purpose
- /// is to absorb a failure, and <see cref="Option.FromNullable{T}(T)" /> itself.
+ /// <c>AndThen</c> and <see cref="Option.FromNullable{T}(T)" /> instead of
+ /// returning null. <see cref="Option.Try{T}" /> and
+ /// <see cref="Option.FromNullable{T}(T)" /> are the two entry points that
+ /// accept an absent value without throwing.
  /// </para>
 /// </remarks>
```

The "this type was the outlier until 7.0.0" line is library history, not something a caller needs to call the member correctly, and "the two indistinguishable — the caller would read..." and "that is the difference between mapping and binding, and it is deliberately explicit" are editorial argument rather than contract. What a caller needs — null throws, use `AndThen`/`FromNullable`/`Try` for a genuinely absent value — survives; the rationale for the design does not.

"The outlier" is also figurative — a metaphor casting the type as a statistical anomaly among its siblings. Literal replacement: this remarks block no longer mentions it at all, since which version changed the behavior isn't something a caller needs to call the member correctly.

### `Option<T>.IsSome`
`src/Waystone.Monads/Options/OptionOfT.cs:46` — inconsistent-sibling

```diff
 /// <summary>
- /// Returns <see langword="true" /> if the option is a
- /// <see cref="Some{T}" /> value.
+ /// Checks whether the option is a <see cref="Some{T}" /> value.
 /// </summary>
 public abstract bool IsSome { get; }
```

### `Option<T>.IsNone`
`src/Waystone.Monads/Options/OptionOfT.cs:52` — stale, inconsistent-sibling

```diff
 /// <summary>
- /// Returns <see langword="false" /> if the option is a
- /// <see cref="None{T}" /> value.
+ /// Checks whether the option is a <see cref="None{T}" /> value.
 /// </summary>
 public abstract bool IsNone { get; }
```

**This is a factual error in shipped documentation, not a style preference.** The XML doc comment on `IsNone` (`OptionOfT.cs:52-56`) states the getter "Returns `false` ... if the option is a `None<T>` value." That is the literal opposite of what the getter does:

- `None<T>.IsNone` (`src/Waystone.Monads/Options/None.cs:32`): `public override bool IsNone => true;`
- `Some<T>.IsNone` (`src/Waystone.Monads/Options/Some.cs:69`): `public override bool IsNone => false;`

So the case the summary describes ("the option is a `None<T>` value") is exactly the case where the getter returns `true`, not `false`. A caller who trusts the comment as written would get their branch backwards. This reads as `IsSome`'s comment (which is correct) copied and negated on the wrong word — every other reference to `None<T>` in the sentence stayed, only `true` became `false`.

### `Option<T>.IsSomeAnd(Func<T, bool>)`
`src/Waystone.Monads/Options/OptionOfT.cs:58` — missing-punctuation, restated-signature, inconsistent-sibling

```diff
 /// <summary>
- /// Returns <see langword="true" /> if the option is a
- /// <see cref="Some{T}" /> and the value inside of it matches a predicate.
+ /// Checks whether the option is a <see cref="Some{T}" /> whose value
+ /// satisfies <paramref name="predicate" />.
 /// </summary>
- /// <param name="predicate">The condition to evaluate the option against</param>
+ /// <param name="predicate">
+ /// The condition the contained value must satisfy. It is not invoked on a
+ /// <see cref="None{T}" />.
+ /// </param>
 public abstract bool IsSomeAnd(Func<T, bool> predicate);
```

Verified against `None<T>.IsSomeAnd` (`None.cs:35`): returns `false` without calling `predicate`.

### `Option<T>.IsSomeAnd<TState>(TState, Func<T, TState, bool>)`
`src/Waystone.Monads/Options/OptionOfT.cs:65` — missing-punctuation, inconsistent-sibling, copy-paste-drift

```diff
 /// <summary>
- /// Returns <see langword="true" /> if the option is a
- /// <see cref="Some{T}" /> and the value inside of it matches a predicate
- /// that takes state instead of capturing it.
+ /// Checks whether the option is a <see cref="Some{T}" /> whose value
+ /// satisfies <paramref name="predicate" />, with state passed to the
+ /// delegate rather than captured by it.
 /// </summary>
 /// <remarks>
 /// Handing the <paramref name="state" /> to the delegate rather than
 /// capturing it lets the delegate be <see langword="static" />, so the call
 /// allocates no closure. <c>WM2017</c> reports a capturing call, naming
 /// <c>With</c> rather than this overload.
 /// </remarks>
 /// <param name="state">
 /// The value the delegate would otherwise capture. It is passed through
 /// unchanged and is never inspected.
 /// </param>
- /// <param name="predicate">The condition to evaluate the option against</param>
+ /// <param name="predicate">
+ /// The condition the contained value must satisfy. It is not invoked on a
+ /// <see cref="None{T}" />.
+ /// </param>
 /// <typeparam name="TState">
 /// The type of the state passed to the predicate. It is unconstrained, so a
 /// null state is permitted.
 /// </typeparam>
 public abstract bool IsSomeAnd<TState>(
     TState state,
     Func<T, TState, bool> predicate);
```

### `Option<T>.IsSomeAndAsync(Func<T, Task<bool>>)`
`src/Waystone.Monads/Options/OptionOfT.cs:89` — figurative

```diff
 /// <param name="predicate">
 /// The condition to evaluate the contained value against. It is not invoked on
- /// a <see cref="None{T}" />, so a <see cref="None{T}" /> costs no await.
+ /// a <see cref="None{T}" />.
 /// </param>
```

Verified: `None<T>.IsSomeAndAsync` (`None.cs:44`) returns a completed `ValueTask<bool>(false)` without calling `predicate`. Kind: metaphor — "costs" casts the absence of an await as a payment. Literal replacement: state that the predicate is not invoked (already the first half of the sentence); no separate cost claim is needed.

### `Option<T>.IsNoneOr(Func<T, bool>)`
`src/Waystone.Monads/Options/OptionOfT.cs:105` — missing-punctuation, inconsistent-sibling

```diff
 /// <summary>
- /// Returns <see langword="true" /> if the option is a
- /// <see cref="None{T}" /> or the value inside of it matches a predicate.
+ /// Checks whether the option is a <see cref="None{T}" />, or its contained
+ /// value satisfies <paramref name="predicate" />.
 /// </summary>
- /// <param name="predicate">The condition to evaluate the option against</param>
+ /// <param name="predicate">
+ /// The condition the contained value must satisfy. It is not invoked on a
+ /// <see cref="None{T}" />.
+ /// </param>
 public abstract bool IsNoneOr(Func<T, bool> predicate);
```

### `Option<T>.IsNoneOr<TState>(TState, Func<T, TState, bool>)`
`src/Waystone.Monads/Options/OptionOfT.cs:112` — missing-punctuation, inconsistent-sibling

```diff
 /// <summary>
- /// Returns <see langword="true" /> if the option is a
- /// <see cref="None{T}" /> or the value inside of it matches a predicate
- /// that takes state instead of capturing it.
+ /// Checks whether the option is a <see cref="None{T}" />, or its contained
+ /// value satisfies <paramref name="predicate" />, with state passed to the
+ /// delegate rather than captured by it.
 /// </summary>
 /// <remarks>
 /// Handing the <paramref name="state" /> to the delegate rather than
 /// capturing it lets the delegate be <see langword="static" />, so the call
 /// allocates no closure. <c>WM2017</c> reports a capturing call, naming
 /// <c>With</c> rather than this overload.
 /// </remarks>
 /// <param name="state">
 /// The value the delegate would otherwise capture. It is passed through
 /// unchanged and is never inspected.
 /// </param>
- /// <param name="predicate">The condition to evaluate the option against</param>
+ /// <param name="predicate">
+ /// The condition the contained value must satisfy. It is not invoked on a
+ /// <see cref="None{T}" />.
+ /// </param>
 /// <typeparam name="TState">
 /// The type of the state passed to the predicate. It is unconstrained, so a
 /// null state is permitted.
 /// </typeparam>
 public abstract bool IsNoneOr<TState>(
     TState state,
     Func<T, TState, bool> predicate);
```

### `Option<T>.IsNoneOrAsync(Func<T, Task<bool>>)`
`src/Waystone.Monads/Options/OptionOfT.cs:136` — figurative, over-long-remarks

```diff
 /// <remarks>
- /// The inverse of <see cref="IsSomeAndAsync" /> in the case it lets through
- /// free: this one treats an absent value as passing, which is what makes it the
- /// right shape for a validation that only rejects a value it actually has.
+ /// The inverse of <see cref="IsSomeAndAsync" />: an absent value passes here
+ /// rather than failing.
 /// </remarks>
```

Kind: metaphor — "lets through free" figures the check as a toll booth waiving a value through. Literal replacement: state directly that an absent value passes rather than fails, as the rewrite does.

### `Option<T>.Match(Action<T>, Action)`
`src/Waystone.Monads/Options/OptionOfT.cs:203` — copy-paste-drift

```diff
 /// <summary>
- /// Performs a <see langword="switch" /> on the option, invoking the
- /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
- /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
+ /// Performs a <see langword="switch" /> on the option for its side effect,
+ /// invoking the <paramref name="onSome" /> callback when it is a
+ /// <see cref="Some{T}" /> and the <paramref name="onNone" /> callback when it
+ /// is a <see cref="None{T}" />.
 /// </summary>
 /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
 /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
 public abstract void Match(Action<T> onSome, Action onNone);
```

This first sentence is byte-for-byte identical to `Match<TOut>(Func<T,TOut>, Func<TOut>)`'s (line 156) — a doc generator indexing only the first sentence cannot tell the value-returning and side-effect overloads apart. (Also drops the accidental double space before `<see cref="None{T}" />` that both copies share.)

### `Option<T>.Expect(string)`
`src/Waystone.Monads/Options/OptionOfT.cs:331` — figurative, missing-punctuation, restated-signature

```diff
 /// <summary>
- /// Returns the contained <see cref="Some{T}" /> value, consuming the
- /// <see cref="Option{T}" />.
+ /// Returns the contained <see cref="Some{T}" /> value.
 /// </summary>
- /// <param name="message">A custom exception message</param>
+ /// <param name="message">The exception message to use on a <see cref="None{T}" />.</param>
 /// <exception cref="UnmetExpectationException">
- /// Thrown if the value is a
- /// <see cref="None{T}" /> with a custom message provided by
- /// <paramref name="message" />
+ /// Thrown when the option is a <see cref="None{T}" />, with
+ /// <paramref name="message" /> as the exception's message.
 /// </exception>
 public abstract T Expect(string message);
```

`Option<T>` is an immutable record; nothing is consumed. `None<T>.Expect` (`None.cs:129`) throws `new UnmetExpectationException(message)` directly, which the rewritten exception text states plainly instead of "with a custom message provided by message," which read as describing the `None<T>` rather than the exception.

Kind: metaphor — "consuming" borrows Rust's ownership-transfer vocabulary for a member that neither takes ownership of anything (the record is immutable, and the caller can keep using it) nor destroys anything. Unlike "wrapped" or "swallowed," there's no equivalent C# behavior for this word to describe here, so it isn't domain vocabulary in this codebase — it's a borrowed image that doesn't match what the code does. Literal replacement: drop the claim; the summary just states what's returned.

### `Option<T>.Unwrap()`
`src/Waystone.Monads/Options/OptionOfT.cs:343` — figurative, copy-paste-drift, missing-punctuation, restated-signature

```diff
- /// <summary>
- /// Returns the contained <see cref="Some{T}" /> value, consuming the
- /// <see cref="Option{T}" />.
- /// </summary>
+ /// <summary>Returns the contained <see cref="Some{T}" /> value.</summary>
 /// <remarks>
 /// Throws on a <see cref="None{T}" />, so prefer a member that cannot:
 /// <see cref="Match{TOut}(Func{T,TOut},Func{TOut})" /> to handle both cases
 /// explicitly, or
 /// <see cref="UnwrapOr" />, <see cref="UnwrapOrElse" /> or
 /// <see cref="UnwrapOrDefault" /> to supply a fallback.
 /// </remarks>
 /// <exception cref="UnwrapException">
- /// Throws if the option equals
- /// <see cref="None{T}" />
+ /// Thrown when the option is a <see cref="None{T}" />.
 /// </exception>
 public abstract T Unwrap();
```

Kind: metaphor (same one as `Expect` above — "consuming" borrows Rust's ownership-transfer vocabulary for a member that transfers no ownership). Literal replacement: drop the claim, as the diff does. Shares the exact opening with `Expect`, and since both summaries would otherwise be identical, a generator indexing the first sentence can't tell `Expect` and `Unwrap` apart either. "The option equals None&lt;T&gt;" is also imprecise: this is a type-case check (`is None<T>`), not an equality comparison.

### `Option<T>.UnwrapOr(T)`
`src/Waystone.Monads/Options/OptionOfT.cs:360` — missing-punctuation

```diff
 /// <param name="value">
- /// The default value to return on a <see cref="None{T}" />
+ /// The default value to return on a <see cref="None{T}" />.
 /// </param>
 public abstract T UnwrapOr(T value);
```

### `Option<T>.Map<TOut>(Func<T, TOut>)` / `Option<T>.Map<TState, TOut>(TState, Func<T, TState, TOut>)`
`src/Waystone.Monads/Options/OptionOfT.cs:431` and `:465` — copy-paste-drift, restated-signature

```diff
 /// <summary>
- /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;TOut&gt;</c> by
- /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
- /// <see cref="None{T}" /> (if <see cref="None{T}" />).
+ /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;TOut&gt;</c>, with state
+ /// passed to the map function rather than captured by it.
 /// </summary>
 /// <remarks>
 /// Handing the <paramref name="state" /> to the delegate rather than
 /// capturing it lets the delegate be <see langword="static" />, so the call
 /// allocates no closure. <c>WM2017</c> reports a capturing call, naming
 /// <c>With</c> rather than this overload.
 /// </remarks>
- /// <param name="state">The value passed to the map function.</param>
- /// <param name="map">The map function.</param>
- /// <typeparam name="TState">The type of the state passed to the map function.</typeparam>
+ /// <param name="state">The value passed to <paramref name="map" />.</param>
+ /// <param name="map">Transforms the contained value with the state.</param>
+ /// <typeparam name="TState">The type of the state passed to <paramref name="map" />.</typeparam>
 /// <typeparam name="TOut">The return type of the map function.</typeparam>
 /// <exception cref="ArgumentNullException">
 /// If <paramref name="map" /> returns null. See the remarks on
 /// <see cref="Option{T}" /> for why that throws rather than producing a
 /// <see cref="None{T}" />.
 /// </exception>
 public abstract Option<TOut> Map<TState, TOut>(
     TState state,
     Func<T, TState, TOut> map) where TOut : notnull;
```

`Map<TOut>` at line 431 and `Map<TState, TOut>` at line 445 open with the identical sentence quoted above — only the state overload is changed here, following the pattern already used elsewhere in this file (e.g. `Match<TState, TOut>` vs `Match<TOut>`) where the state overload's summary names the state instead of repeating the base one verbatim.

### `Option<T>.MapOr<TOut>(TOut, Func<T, TOut>)` / `Option<T>.MapOr<TState, TOut>(TState, TOut, Func<T, TState, TOut>)`
`src/Waystone.Monads/Options/OptionOfT.cs:606` and `:615` — copy-paste-drift, restated-signature

```diff
 /// <summary>
- /// Returns the provided default result (if <see cref="None{T}" />), or
- /// applies a function to the contained value (if <see cref="Some{T}" />).
+ /// Returns the provided default result (if <see cref="None{T}" />), or
+ /// applies a function to the contained value (if <see cref="Some{T}" />),
+ /// with state passed to the map function rather than captured by it.
 /// </summary>
 /// <remarks>
 /// Handing the <paramref name="state" /> to the delegate rather than
 /// capturing it lets the delegate be <see langword="static" />, so the call
 /// allocates no closure. <c>WM2017</c> reports a capturing call, naming
 /// <c>With</c> rather than this overload.
 /// </remarks>
- /// <param name="state">The value passed to the map function.</param>
+ /// <param name="state">The value passed to <paramref name="map" />.</param>
 /// <param name="defaultValue">The default value for a <see cref="None{T}" />.</param>
- /// <param name="map">The map function.</param>
- /// <typeparam name="TState">The type of the state passed to the map function.</typeparam>
+ /// <param name="map">Transforms the contained value with the state.</param>
+ /// <typeparam name="TState">The type of the state passed to <paramref name="map" />.</typeparam>
 /// <typeparam name="TOut">The return type of the map function.</typeparam>
 public abstract TOut MapOr<TState, TOut>(
     TState state,
     TOut defaultValue,
     Func<T, TState, TOut> map);
```

Same identical-first-sentence problem as `Map`/`Map<TState,...>` above, between lines 606 and 615.

### `Option<T>.MapOrNull<TOut>(Func<T, TOut>)`
`src/Waystone.Monads/Options/OptionOfT.cs:716` — figurative

```diff
 /// <remarks>
- /// The bridge out of <see cref="Option{T}" /> into <see cref="Nullable{T}" />,
- /// for handing a value to an API that speaks the latter.
- /// <typeparamref name="TOut" /> is constrained to a value type precisely so
- /// that null cannot also be a mapped result, which is what keeps the return
- /// unambiguous.
+ /// For passing the result to an API that expects <see cref="Nullable{T}" />.
+ /// <typeparamref name="TOut" /> is constrained to a value type so that null
+ /// cannot also be a mapped result, which keeps the return unambiguous.
 /// </remarks>
```

Two figurative phrases here, two kinds: "the bridge out of `Option<T>` into `Nullable<T>`" is a metaphor (a bridge connecting two structures); "an API that speaks the latter" is personification (giving the API a voice). Literal replacement for both: name what the API expects (`Nullable<T>`), as the rewrite does — no bridge, nothing that speaks.

### `Option<T>.MapOrElse<TOut>(Func<TOut>, Func<T, TOut>)` / `Option<T>.MapOrElse<TState, TOut>(TState, Func<TState, TOut>, Func<T, TState, TOut>)`
`src/Waystone.Monads/Options/OptionOfT.cs:783` and `:795` — copy-paste-drift, restated-signature, throat-clearing

```diff
 /// <summary>
- /// Computes a default from a function (if <see cref="None{T}" />), or
- /// applies a function to the contained value (if <see cref="Some{T}" />).
+ /// Computes a default from a function (if <see cref="None{T}" />), or
+ /// applies a function to the contained value (if <see cref="Some{T}" />),
+ /// with state passed to both delegates rather than captured by them.
 /// </summary>
 /// <remarks>
- /// Handing the <paramref name="state" /> to the delegate rather than
- /// capturing it lets the delegate be <see langword="static" />, so the call
+ /// Handing the <paramref name="state" /> to the delegates rather than
+ /// capturing it lets them be <see langword="static" />, so the call
 /// allocates no closure. <c>WM2017</c> reports a capturing call, naming
 /// <c>With</c> rather than this overload.
 /// </remarks>
 /// <param name="state">The value passed to both functions.</param>
- /// <param name="defaultFactory">
- /// The function that will create a default value for a
- /// <see cref="None{T}" />.
- /// </param>
- /// <param name="map">The map function.</param>
+ /// <param name="defaultFactory">
+ /// Produces the default value for a <see cref="None{T}" /> from the state.
+ /// </param>
+ /// <param name="map">Transforms the contained value with the state.</param>
 /// <typeparam name="TState">The type of the state passed to both functions.</typeparam>
 /// <typeparam name="TOut">The return type of the map function.</typeparam>
 public abstract TOut MapOrElse<TState, TOut>(
     TState state,
     Func<TState, TOut> defaultFactory,
     Func<T, TState, TOut> map);
```

Third instance of the same pattern (`Map`, `MapOr`, `MapOrElse` each pair an overload with an identical first sentence to its `<TState>` sibling) — worth fixing as a family rather than one at a time.

### `Option<T>.Inspect(Action<T>)`
`src/Waystone.Monads/Options/OptionOfT.cs:869` — missing-punctuation, inconsistent-sibling

```diff
 /// <summary>
- /// Calls a function with a reference to the contained value if
- /// <see cref="Some{T}" />
+ /// Calls a function with the contained value if <see cref="Some{T}" />.
 /// </summary>
 /// <param name="action">The function to execute against the value.</param>
- /// <returns>The original <see cref="Option{T}" /></returns>
+ /// <returns>The original <see cref="Option{T}" />, unchanged.</returns>
 public abstract Option<T> Inspect(Action<T> action);
```

"A reference to the contained value" implies `ref` semantics that don't exist here (compare `Inspect<TState>`'s wording two members below, which says "the contained value" with no "reference to"). The `<returns>` is also missing the "unchanged" that `Inspect<TState>`'s equivalent tag has.

### `Option<T>.Filter(Func<T, bool>)`
`src/Waystone.Monads/Options/OptionOfT.cs:917` — buried-summary, inconsistent-sibling

```diff
 /// <summary>
 /// Returns <see cref="None{T}" /> if the option is <see cref="None{T}" />,
- /// otherwise calls the <paramref name="predicate" /> with the wrapped value and
- /// returns:
- /// <list type="bullet">
- /// <item>
- /// <see cref="Some{T}" /> if the <paramref name="predicate" /> returns
- /// <see langword="true" /> (where <typeparamref name="T" /> is the wrapped value),
- /// and
- /// </item>
- /// <item>
- /// <see cref="None{T}" /> if the <paramref name="predicate" /> returns
- /// <see langword="false" />.
- /// </item>
- /// </list>
+ /// otherwise calls <paramref name="predicate" /> with the wrapped value,
+ /// returning <see cref="Some{T}" /> when it returns <see langword="true" />
+ /// and <see cref="None{T}" /> when it returns <see langword="false" />.
 /// </summary>
 /// <param name="predicate">The filter function.</param>
 public abstract Option<T> Filter(Func<T, bool> predicate);
```

A doc generator indexes only the first sentence, and here that sentence trails into a bullet list — the actual outcome (`Some<T>` vs `None<T>`) never appears in indexed text. `Filter<TState>` two members below states the same logic as one flowing sentence; this rewrite matches that style.

### `Option<T>.Or(Option<T>)`
`src/Waystone.Monads/Options/OptionOfT.cs:971` — missing-punctuation

```diff
 /// <summary>
 /// Returns the option if it contains a value, otherwise returns
- /// <paramref name="other" />
+ /// <paramref name="other" />.
 /// </summary>
 /// <param name="other">The other option.</param>
 public abstract Option<T> Or(Option<T> other);
```

### `Option<T>.OrElseAsync(Func<ValueTask<Option<T>>>)`
`src/Waystone.Monads/Options/OptionOfT.cs:1012` — figurative

```diff
 /// <param name="optionFactory">
- /// Produces the fallback option. It is not invoked on a
- /// <see cref="Some{T}" />, so a present value costs no await.
+ /// Produces the fallback option. It is not invoked on a
+ /// <see cref="Some{T}" />.
 /// </param>
```

Kind: metaphor — "costs" casts the absence of an await as a payment. Literal replacement: the "not invoked" sentence already says everything the caller needs.

### `Option<T>.ZipWith<TOther, TOut>(Option<TOther>, Func<T, TOther, TOut>)`
`src/Waystone.Monads/Options/OptionOfT.cs:1075` — restated-signature, inconsistent-sibling

```diff
- /// <param name="zip">The function that will perform the zip operation.</param>
+ /// <param name="zip">
+ /// Combines the two contained values. It is invoked only when both options
+ /// are a <see cref="Some{T}" />.
+ /// </param>
```

"The function that will perform the zip operation" just restates the parameter's name. The very next overload, `ZipWith<TState, TOther, TOut>`, describes the same role fully ("Combines the two contained values with the state. It is invoked only when both options are a `Some<T>`.") — this one should match.

### `Option<T>.ZipWithAsync<TOther, TOut>(Option<TOther>, Func<T, TOther, Task<TOut>>)`
`src/Waystone.Monads/Options/OptionOfT.cs:1123` — figurative

```diff
 /// <param name="zip">
 /// Combines the two contained values. It is invoked only when both options are a
- /// <see cref="Some{T}" />, so a <see cref="None{T}" /> on either side costs no
- /// await.
+ /// <see cref="Some{T}" />.
 /// </param>
```

Kind: metaphor — same "costs no await" payment image as the other `…Async` members above. Literal replacement: the "invoked only when" sentence already covers it.

The `<remarks>` on this member carry a second, separate figurative phrase:

```diff
 /// <remarks>
- /// Both options must hold a value. Where a single <see cref="Some{T}" /> should
- /// survive the other side being absent, use <see cref="ReduceAsync" /> instead.
+ /// Both options must hold a value. Use <see cref="ReduceAsync" /> instead when a
+ /// single <see cref="Some{T}" /> should be returned unchanged rather than
+ /// discarded when the other side is absent.
 /// </remarks>
```

Kind: personification — "survive" gives the option value an agency to live or die that it does not have; it is simply returned unchanged. Literal replacement as above.

### `Option<T>.Reduce(Option<T>, Func<T, T, T>)`
`src/Waystone.Monads/Options/OptionOfT.cs:1155` — figurative

```diff
 /// <remarks>
- /// Unlike <see cref="ZipWith{TOther,TOut}" />, an option that is a
- /// <see cref="Some{T}" /> survives a <see cref="None{T}" /> on the other side.
+ /// Unlike <see cref="ZipWith{TOther,TOut}" />, an option that is a
+ /// <see cref="Some{T}" /> is returned unchanged when the other side is a
+ /// <see cref="None{T}" />.
 /// </remarks>
 /// <param name="other">The option to merge with.</param>
 /// <param name="reduce">The function that combines two present values.</param>
```

Kind: personification — same "survives" image as `ZipWithAsync` above. Literal replacement as shown; `Reduce<TState>` two members below already states this literally ("is returned unchanged without it being consulted"), so this brings `Reduce` in line with its own state-passing sibling.

### `Option<T>.ReduceAsync(Option<T>, Func<T, T, Task<T>>)`
`src/Waystone.Monads/Options/OptionOfT.cs:1217` — figurative

```diff
 /// <remarks>
- /// Unlike <c>ZipWithAsync</c>, a <see cref="Some{T}" /> survives a
- /// <see cref="None{T}" /> on the other side and is returned unchanged, so the
+ /// Unlike <c>ZipWithAsync</c>, a <see cref="Some{T}" /> on either side is
+ /// returned unchanged when the other is a <see cref="None{T}" />, so the
 /// delegate runs only when there are genuinely two values to combine.
 /// </remarks>
```

Kind: personification — same "survives" image, redundant here with the "and is returned unchanged" that already follows it in the same sentence. Literal replacement drops "survives" and keeps only the literal half.

### `Option<T>.OkOrElse<TErr>(Func<TErr>)`
`src/Waystone.Monads/Options/OptionOfT.cs:1276` — restated-signature, throat-clearing, inconsistent-sibling

```diff
 /// <remarks>
- /// The <paramref name="errorFactory" /> is lazily evaluated, meaning it
- /// will only be invoked if the current option is a <see cref="None{T}" />.
+ /// <paramref name="errorFactory" /> is invoked only when the option is a
+ /// <see cref="None{T}" />.
 /// </remarks>
 /// <typeparam name="TErr">The type of the error value returned by the factory.</typeparam>
- /// <param name="errorFactory">
- /// The function, which when invoked, will return the
- /// error value.
- /// </param>
+ /// <param name="errorFactory">Produces the error for a <see cref="None{T}" />.</param>
```

"The function, which when invoked, will return the error value" only restates the signature. `OkOrElseAsync` below (line 1338) already states the same role plainly as "Produces the error for a `None<T>`."

### `Option<T>.OkOrElse<TState, TErr>(TState, Func<TState, TErr>)`
`src/Waystone.Monads/Options/OptionOfT.cs:1298` — restated-signature

```diff
- /// <param name="errorFactory">
- /// The function, which when invoked with the state, will return the error
- /// value.
- /// </param>
+ /// <param name="errorFactory">
+ /// Produces the error for a <see cref="None{T}" /> from the state.
+ /// </param>
```

### `Option<T>.OkOrElseAsync<TErr>(Func<Task<TErr>>)`
`src/Waystone.Monads/Options/OptionOfT.cs:1334` — figurative

```diff
 /// <param name="errorFactory">
 /// Produces the error for a <see cref="None{T}" />. It is not invoked on a
- /// <see cref="Some{T}" />, so a present value costs no await.
+ /// <see cref="Some{T}" />.
 /// </param>
```

Kind: metaphor — the fourth "costs no await" instance in this file (with `IsSomeAndAsync`, `OrElseAsync`, `ZipWithAsync` above). Literal replacement: the preceding "not invoked" sentence is sufficient on its own.

### `Option<T>.AsEnumerable()`
`src/Waystone.Monads/Options/OptionOfT.cs:1246` — restated-signature (minor)

```diff
- /// <summary>Returns a sequence over the possibly contained value.</summary>
+ /// <summary>Returns a sequence over the contained value, if any.</summary>
 /// <returns>
 /// A sequence yielding the contained value once if the option is a
 /// <see cref="Some{T}" />, otherwise an empty sequence.
 /// </returns>
 public abstract IEnumerable<T> AsEnumerable();
```

"The possibly contained value" is awkward; the `<returns>` tag already carries the real detail.

### Clean

- `Option<T>` — `typeparam T`
- `IsSomeAndAsync` `<returns>` tag
- `Match<TOut>(Func<T,TOut>, Func<TOut>)`
- `Match<TState, TOut>`
- `Match<TState>(TState, Action<T,TState>, Action<TState>)`
- `MatchAsync<TOut>(Func<T,Task<TOut>>, Func<Task<TOut>>)`
- `MatchAsync<TOut>(Func<T,TOut>, Func<Task<TOut>>)`
- `MatchAsync<TOut>(Func<T,Task<TOut>>, Func<TOut>)`
- `MatchAsync(Func<T,Task>, Func<Task>)`
- `MatchAsync(Func<T,Task>, Action)`
- `MatchAsync(Action<T>, Func<Task>)`
- `UnwrapOrDefault`
- `UnwrapOrElse(Func<T>)`
- `UnwrapOrElse<TState>`
- `UnwrapOrElseAsync`
- `MapAsync<TOut>`
- `And<TOut>`
- `AndThen<TOut>`
- `AndThen<TState, TOut>`
- `AndThenAsync<TOut>`
- `MapOrAsync<TOut>`
- `MapOrDefault<TOut>`
- `MapOrDefault<TState, TOut>`
- `MapOrDefaultAsync<TOut>`
- `MapOrNull<TState, TOut>`
- `MapOrNullAsync<TOut>`
- `MapOrElseAsync<TOut>` (all three overloads)
- `Inspect<TState>`
- `InspectAsync`
- `Filter<TState>`
- `FilterAsync`
- `OrElse(Func<Option<T>>)`
- `OrElse<TState>`
- `Xor`
- `Zip<TOther>`
- `ZipWith<TState, TOther, TOut>`
- `Reduce<TState>`
- `OkOr<TErr>`

### Figurative-language re-sweep

Re-read the whole file specifically for simile, personification, hyperbole and wordplay (not just the metaphor examples I'd already caught), and for anything that should be *exempted* as this codebase's or .NET's own domain vocabulary rather than authored imagery. Two outcomes beyond the nine metaphor/personification findings already listed above:

- **New finding**: "survives"/"survive" (`Reduce` line 1158, `ZipWithAsync` line 1129, `ReduceAsync` line 1222) is personification — folded into the `ZipWithAsync` section and two new sections, `Reduce` and `ReduceAsync`, above (both moved out of Clean).
- **Deliberately left alone**: "hot path" (`UnwrapOrElseAsync` remarks, line 418) and "not free"/"free" (`MapOrAsync` remarks line 641, matching `WM2016`'s own rule text "not provably free to evaluate"). Both are established field vocabulary — "hot path" is standard performance-engineering jargon with no single plainer equivalent, and "free" here is this library's own analyzer terminology for "zero-cost to evaluate," not an image the doc-comment author reached for. Consistent with keeping "wrapped"/"swallowed"/"faulted"/"held" — the test is whether the author or the field supplied the image, and the field did here.

No simile, hyperbole or wordplay found anywhere in the file.

### Confirmed (previously Unverified)

All three analyzer/generator claims flagged as Unverified in the previous revision are now checked against source and confirmed accurate; none is `stale`.

- **`WM2017`** (`src/Waystone.Monads.Analyzers/Rules.cs:280-284`, `DelegateCapturesInsteadOfState`): message is `"The delegate passed to '{0}' captures '{1}', so a closure is allocated on every call. Bind the state with 'With' and call '{0}' on the binder it returns instead."` This matches every `<remarks>` in this file that says "`WM2017` reports a capturing call, naming `With` rather than this overload" word for word in substance.
- **`WM2016`** (`Rules.cs:239-243`, `EagerArgumentNotFree`, backed by `src/Waystone.Monads.Analyzers/LazyVariantAnalyzer.cs:13-25`): the rule's own `LazySiblings` dictionary maps both `["MapOr"] = "MapOrElse"` and `["MapOrAsync"] = "MapOrElseAsync"`, so `MapOrAsync<TOut>`'s remarks claim ("Where producing it is not free, prefer the `MapOrElseAsync` overload... `WM2016` reports the difference") is confirmed for the async overload specifically, not just inferred from the sync one.
- **`WSG0003`** (`src/Waystone.SourceGenerators/AsyncSurface/Rules.cs:14-27`, `TaskReturningStepDelegate`): fires when a delegate parameter returns `Task<Option<T>>`/`Task<Result<TOk,TErr>>` instead of the `ValueTask` form, exactly the shape `AndThenAsync`'s `Func<T, ValueTask<Option<TOut>>>` and `OrElseAsync`'s `Func<ValueTask<Option<T>>>` parameters already take. Confirmed for both members that cite it.

### Unverified

None remain.

## Result<TOk, TErr> core

Scope: `src/Waystone.Monads/Results/ResultOfTOkTErr.cs`.
65 public members examined (the type declaration plus 64 members), cross-checked against `Ok<TOk,TErr>` and `Err<TOk,TErr>` in `Ok.cs`/`Err.cs`, `Exceptions/UnwrapException.cs`, `Exceptions/UnmetExpectationException.cs`, and the `WM2017` descriptor in `Waystone.Monads.Analyzers/Rules.cs`. Revised in place under a refined figurative-language rule: flag authored metaphor/simile/personification/hyperbole/wordplay, but leave the field's own dead metaphors (wrap, fault, drain, listen) alone. No instance of "swallow" or "leak" occurs in this file, so there was nothing to restore under that exemption. A second sweep checked specifically for cost/price/payment language, "consuming", "hands back", "falls through", personification verbs (speaks/knows/decides/wants/forgets), hyperbole ("never"/"always"/"every" against a documented exception), and bare cost judgements ("cheap"/"expensive") with no measure behind them.

### `Result<TOk, TErr>.IsOk`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:33` — inconsistent-sibling

```diff
     /// <summary>
-    /// Returns <see langword="true" /> if the result is
-    /// <see cref="Ok{TOk,TErr}" />.
+    /// Checks whether the result is <see cref="Ok{TOk,TErr}" />.
     /// </summary>
     public abstract bool IsOk { get; }
```
Boolean getter; sibling boolean members in this same file (`IsOkAndAsync`) already open with "Checks whether".

### `Result<TOk, TErr>.IsErr`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:39` — inconsistent-sibling

```diff
     /// <summary>
-    /// Returns <see langword="true" /> if the result is
-    /// <see cref="Err{TOk,TErr}" />.
+    /// Checks whether the result is <see cref="Err{TOk,TErr}" />.
     /// </summary>
     public abstract bool IsErr { get; }
```

### `Result<TOk, TErr>.IsOkAnd(Func<TOk, bool>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:46` — inconsistent-sibling, missing-punctuation

```diff
     /// <summary>
-    /// Returns <see langword="true" /> if the result is
-    /// <see cref="Ok{TOk,TErr}" /> and the value inside of it matches a predicate.
+    /// Checks whether the result is <see cref="Ok{TOk,TErr}" /> and the
+    /// contained value matches a predicate.
     /// </summary>
-    /// <param name="predicate">The condition that the ok value must satisfy</param>
+    /// <param name="predicate">The condition that the ok value must satisfy.</param>
     public abstract bool IsOkAnd(Func<TOk, bool> predicate);
```
The async sibling `IsOkAndAsync` already opens with "Checks whether"; this sync member opened with "Returns true if" instead.

### `Result<TOk, TErr>.IsOkAnd<TState>(TState, Func<TOk, TState, bool>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:68` — missing-punctuation, inconsistent-sibling

```diff
     /// <summary>
-    /// Returns <see langword="true" /> if the result is
-    /// <see cref="Ok{TOk,TErr}" /> and the value inside of it matches a predicate
-    /// that takes state instead of capturing it.
+    /// Checks whether the result is <see cref="Ok{TOk,TErr}" /> and the
+    /// contained value matches a predicate that takes state instead of
+    /// capturing it.
     /// </summary>
     /// ...
-    /// <param name="predicate">The condition that the ok value must satisfy</param>
+    /// <param name="predicate">The condition that the ok value must satisfy.</param>
```

### `Result<TOk, TErr>.IsOkAndAsync(Func<TOk, Task<bool>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:88` — figurative (personification)

```diff
     /// <remarks>
     /// <paramref name="predicate" /> is not invoked on an
-    /// <see cref="Err{TOk,TErr}" />, so any side effect it carries does not run in
-    /// that case and the call completes synchronously.
+    /// <see cref="Err{TOk,TErr}" />, so any side effect it performs does not run
+    /// in that case and the call completes synchronously.
     /// </remarks>
```
"it carries" treats the delegate as cargo-bearing. The literal fact: the predicate performs a side effect, or does not run at all.

### `Result<TOk, TErr>.IsErrAnd(Func<TErr, bool>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:95` — inconsistent-sibling, missing-punctuation

```diff
     /// <summary>
-    /// Returns <see langword="true" /> if the result is
-    /// <see cref="Err{TOk,TErr}" /> and the value inside of it matches a predicate.
+    /// Checks whether the result is <see cref="Err{TOk,TErr}" /> and the
+    /// contained value matches a predicate.
     /// </summary>
-    /// <param name="predicate">The condition that the error value must satisfy</param>
+    /// <param name="predicate">The condition that the error value must satisfy.</param>
     public abstract bool IsErrAnd(Func<TErr, bool> predicate);
```

### `Result<TOk, TErr>.IsErrAnd<TState>(TState, Func<TErr, TState, bool>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:117` — missing-punctuation, inconsistent-sibling

```diff
     /// <summary>
-    /// Returns <see langword="true" /> if the result is
-    /// <see cref="Err{TOk,TErr}" /> and the value inside of it matches a
-    /// predicate that takes state instead of capturing it.
+    /// Checks whether the result is <see cref="Err{TOk,TErr}" /> and the
+    /// contained value matches a predicate that takes state instead of
+    /// capturing it.
     /// </summary>
     /// ...
-    /// <param name="predicate">The condition that the error value must satisfy</param>
+    /// <param name="predicate">The condition that the error value must satisfy.</param>
```

### `Result<TOk, TErr>.IsErrAndAsync(Func<TErr, Task<bool>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:137` — figurative (personification)

```diff
     /// <remarks>
     /// <paramref name="predicate" /> is not invoked on an
-    /// <see cref="Ok{TOk,TErr}" />, so any side effect it carries does not run in
-    /// that case and the call completes synchronously.
+    /// <see cref="Ok{TOk,TErr}" />, so any side effect it performs does not run
+    /// in that case and the call completes synchronously.
     /// </remarks>
```

### `Result<TOk, TErr>.Match<TOut>(Func<TOk,TOut>, Func<TErr,TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:155` — copy-paste-drift, inconsistent-sibling

```diff
     /// <summary>
-    /// Performs a <see langword="switch" /> on the result, invoking the
-    /// <paramref name="onOk" /> callback when it is a <see cref="Ok{TOk,TErr}" /> and
-    /// the <paramref name="onErr" /> callback when it is a
-    /// <see cref="Err{TOk,TErr}" />.
+    /// Switches on the result and returns what the invoked callback produced.
     /// </summary>
```
This summary is verbatim identical to `Match(Action<TOk>, Action<TErr>)` below (line 208's summary). Doc generators index the first sentence only, so the value-returning and side-effect-only overloads become indistinguishable in a member list.

### `Result<TOk, TErr>.Match(Action<TOk>, Action<TErr>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:208` — copy-paste-drift, inconsistent-sibling

```diff
     /// <summary>
-    /// Performs a <see langword="switch" /> on the result, invoking the
-    /// <paramref name="onOk" /> callback when it is a <see cref="Ok{TOk,TErr}" /> and
-    /// the <paramref name="onErr" /> callback when it is a
-    /// <see cref="Err{TOk,TErr}" />.
+    /// Switches on the result and invokes the callback for its case, for
+    /// its side effect alone.
     /// </summary>
```
Same duplicate as above, from the other side. `Match<TState,TOut>` and `Match<TState>` (the state-taking pair) already avoid this — they distinguish "returns what the callback... produces" from "for its side effect". This pair should read the same way.

### `Result<TOk, TErr>.MatchAsync<TOut>(Func<TOk, Task<TOut>>, Func<TErr, Task<TOut>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:256` — figurative (metaphor)

```diff
     /// <remarks>
-    /// The overload to reach for when both branches do real asynchronous work.
-    /// Where only one does, prefer the overload taking the other branch
+    /// Use this overload when both branches do real asynchronous work. Where
+    /// only one does, prefer the overload taking the other branch
     /// synchronously — it avoids wrapping a value in an already-completed task.
     /// </remarks>
```
"Reach for" is a physical-grasping image standing in for "use". "Wrapping a value in a task" is left alone — that is this library's own vocabulary for constructing an already-completed `ValueTask`, not authored imagery.

### `Result<TOk, TErr>.MatchAsync(Func<TOk, Task>, Func<TErr, Task>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:304` — figurative (metaphor)

```diff
     /// <remarks>
-    /// The overload to reach for when both branches do real asynchronous work.
-    /// Neither branch returns a value, so this is the asynchronous counterpart of
+    /// Use this overload when both branches do real asynchronous work. Neither
+    /// branch returns a value, so this is the asynchronous counterpart of
     /// the <see cref="Match(Action{TOk},Action{TErr})" /> switch rather than of the
     /// mapping one.
     /// </remarks>
```

### `Result<TOk, TErr>.AndThen<TOut>(Func<TOk, Result<TOut, TErr>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:393` — over-long-remarks, non-consumer-content

```diff
     /// <exception cref="ArgumentNullException">
-    /// If <paramref name="resultFactory" /> returns a null result. Returning
-    /// null rather than an <see cref="Err{TOk,TErr}" /> is never meaningful, and
-    /// left alone it would surface as a <see cref="NullReferenceException" />
-    /// at whatever called into the result next.
+    /// Throws if <paramref name="resultFactory" /> returns a null result.
     /// </exception>
```
A caller needs to know this throws on a null result, not the design argument for why a null return would otherwise be dangerous. The same paragraph repeats near-verbatim on `AndThenAsync` and `AndThen<TState,TOut>` below.

### `Result<TOk, TErr>.AndThenAsync<TOut>(Func<TOk, ValueTask<Result<TOut, TErr>>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:424` — over-long-remarks, non-consumer-content, figurative (metaphor)

```diff
     /// <summary>
-    /// Chains an asynchronous operation onto an <see cref="Ok{TOk,TErr}" />,
-    /// carrying an <see cref="Err{TOk,TErr}" /> straight through.
+    /// Runs an asynchronous operation against the contained ok value, leaving
+    /// an <see cref="Err{TOk,TErr}" /> untouched.
     /// </summary>
```
"Chains onto" and "carrying... straight through" are link-and-transport imagery. The literal fact, already the house style for the sibling `Map`/`MapAsync` members: the delegate runs against the ok value and an `Err` is left untouched.

```diff
     /// <exception cref="ArgumentNullException">
-    /// If <paramref name="resultFactory" /> returns a null result. Returning
-    /// null rather than an <see cref="Err{TOk,TErr}" /> is never meaningful, and
-    /// left alone it would surface as a <see cref="NullReferenceException" />
-    /// at whatever called into the result next. It is thrown from the call when
-    /// the factory's task had already completed and faults the returned task
-    /// otherwise, so await the result to see it either way.
+    /// Throws if <paramref name="resultFactory" /> returns a null result: from
+    /// the call when its task had already completed, or from the returned
+    /// task otherwise, so await the result to see it either way.
     /// </exception>
```
Kept the completed-vs-faulted distinction (consumer-relevant, verified against `Result.NotNullAsync`); cut the design rationale about why a null return is dangerous.

### `Result<TOk, TErr>.AndThen<TState, TOut>(TState, Func<TOk, TState, Result<TOut, TErr>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:458` — over-long-remarks, non-consumer-content

```diff
     /// <exception cref="ArgumentNullException">
-    /// If <paramref name="resultFactory" /> returns a null result. Returning
-    /// null rather than an <see cref="Err{TOk,TErr}" /> is never meaningful, and
-    /// left alone it would surface as a <see cref="NullReferenceException" />
-    /// at whatever called into the result next.
+    /// Throws if <paramref name="resultFactory" /> returns a null result.
     /// </exception>
```

### `Result<TOk, TErr>.Or<TOut>(Result<TOk, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:470` — missing-punctuation, inconsistent-sibling

```diff
-    /// <typeparam name="TOut">The other result's error value type</typeparam>
+    /// <typeparam name="TOut">The other result's error value type.</typeparam>
     public abstract Result<TOk, TOut> Or<TOut>(Result<TOk, TOut> other)
```
Its own siblings `OrElse<TOut>` (line 484) and `OrElse<TState,TOut>` (line 510) already carry the period on the identical tag text.

### `Result<TOk, TErr>.Expect(string)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:562` — stale, missing-punctuation, figurative (metaphor)

```diff
     /// <summary>
-    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value, consuming the
-    /// result instance.
+    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value.
     /// </summary>
     /// <remarks>
-    /// Throws on an <see cref="Err{TOk,TErr}" />, differing from
-    /// <see cref="Unwrap" /> only in that the thrown message leads with
-    /// <paramref name="message" />. Prefer a member that cannot throw:
+    /// Throws on an <see cref="Err{TOk,TErr}" />. Prefer a member that cannot
+    /// throw:
     /// <see cref="Match{TOut}(Func{TOk,TOut},Func{TErr,TOut})" /> to handle both
     /// cases explicitly, or <see cref="UnwrapOr" />,
     /// <see cref="UnwrapOrElse(Func{TErr,TOk})" /> or
     /// <see cref="UnwrapOrDefault" /> to supply a fallback.
     /// </remarks>
     /// <exception cref="UnmetExpectationException">
     /// Throws if the value is an
     /// <see cref="Err{TOk,TErr}" />, with an exception message including the passed
     /// <paramref name="message" />, and the content of the
-    /// <see cref="Err{TOk,TErr}" />
+    /// <see cref="Err{TOk,TErr}" />.
     /// </exception>
```
Checked against `Exceptions/UnwrapException.cs` and `Exceptions/UnmetExpectationException.cs`: `Expect` throws `UnmetExpectationException` (message, then `: `, then the error, baked into the message string), while `Unwrap` throws the unrelated `UnwrapException<TErr>` (a fixed message, plus a `.Value` property carrying the error). They are not the same exception differing only in message content — the claim is inaccurate and should be cut rather than fixed in place.

"Consuming the result instance" borrows Rust's ownership-move semantics for `Result::unwrap(self)`. `Result<TOk,TErr>` is a C# record read by reference; calling `Expect` a second time on the same instance returns the same value, so nothing is actually consumed.

### `Result<TOk, TErr>.ExpectErr(string)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:574` — missing-punctuation

```diff
     /// <exception cref="UnmetExpectationException">
     /// Throws if the value is an
     /// <see cref="Ok{TOk,TErr}" />, with a message including the passed
-    /// <paramref name="message" />, and the content of the <see cref="Ok{TOk,TErr}" />
+    /// <paramref name="message" />, and the content of the <see cref="Ok{TOk,TErr}" />.
     /// </exception>
```

### `Result<TOk, TErr>.Unwrap()`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:592` — figurative (metaphor)

```diff
     /// <summary>
-    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value, consuming the
-    /// result instance.
+    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value.
     /// </summary>
```
Same borrowed Rust ownership language as `Expect`. `Unwrap` does not invalidate or move the receiver — it is a plain read of the `Ok<TOk,TErr>` case's value, callable again on the same instance with the same result.

### `Result<TOk, TErr>.UnwrapOr(TOk)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:602` — missing-punctuation

```diff
     /// <param name="defaultValue">
     /// The default value to return on an
-    /// <see cref="Err{TOk,TErr}" />
+    /// <see cref="Err{TOk,TErr}" />.
     /// </param>
```

### `Result<TOk, TErr>.UnwrapOrDefault()`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:608` — missing-punctuation

```diff
     /// <summary>
     /// Returns the contained <see cref="Ok{TOk,TErr}" /> value or the default
-    /// value for <typeparamref name="TOk" />
+    /// value for <typeparamref name="TOk" />.
     /// </summary>
```

### `Result<TOk, TErr>.UnwrapOrElseAsync(Func<TErr, Task<TOk>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:665` — figurative (personification)

```diff
     /// <remarks>
     /// The fallback that cannot throw, unlike <see cref="Unwrap" />:
-    /// <paramref name="valueFactory" /> sees the error and supplies a value for it.
+    /// <paramref name="valueFactory" /> receives the error and produces a value
+    /// from it.
     /// It is not invoked on an <see cref="Ok{TOk,TErr}" />, so that case completes
     /// synchronously.
     /// </remarks>
```
"Sees" gives the delegate a sense organ it does not have; it receives the error as an argument.

### `Result<TOk, TErr>.UnwrapErr()`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:682` — figurative (metaphor)

```diff
     /// <remarks>
-    /// Throws on an <see cref="Ok{TOk,TErr}" />, so reach for it only where the
-    /// result is already known to be an <see cref="Err{TOk,TErr}" />. Prefer
+    /// Throws on an <see cref="Ok{TOk,TErr}" />, so use it only where the
+    /// result is already known to be an <see cref="Err{TOk,TErr}" />. Prefer
     /// <see cref="GetErr" />, which returns a <see cref="None{T}" /> instead of
     /// throwing.
     /// </remarks>
```

### `Result<TOk, TErr>.Inspect(Action<TOk>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:690` — missing-punctuation

```diff
     /// <summary>
     /// Calls a function with a reference to the contained value if
-    /// <see cref="Ok{TOk,TErr}" />
+    /// <see cref="Ok{TOk,TErr}" />.
     /// </summary>
```

### `Result<TOk, TErr>.InspectErr(Action<TErr>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:739` — missing-punctuation

```diff
     /// <summary>
     /// Calls a function with a reference to the contained value if
-    /// <see cref="Err{TOk,TErr}" />
+    /// <see cref="Err{TOk,TErr}" />.
     /// </summary>
```

### `Result<TOk, TErr>.InspectErrAsync(Func<TErr, Task>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:778` — figurative (personification)

```diff
     /// <remarks>
     /// <paramref name="action" /> is not invoked on an
     /// <see cref="Ok{TOk,TErr}" />. Use this to observe a failure — logging or
-    /// metrics — without handling it; the error is still carried forward.
+    /// metrics — without handling it; the error is still returned unchanged.
     /// </remarks>
```

### `Result<TOk, TErr>.MapOr<TOut>(TOut, Func<TOk, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:847` — missing-punctuation

```diff
     /// <param name="defaultValue">
-    /// The default value for an <see cref="Err{TOk,TErr}" />
+    /// The default value for an <see cref="Err{TOk,TErr}" />.
     /// </param>
-    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" />.</param>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOr<TState, TOut>(TState, TOut, Func<TOk, TState, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:872` — missing-punctuation

```diff
     /// <param name="defaultValue">
-    /// The default value for an <see cref="Err{TOk,TErr}" />
+    /// The default value for an <see cref="Err{TOk,TErr}" />.
     /// </param>
-    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
+    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" />.</param>
     /// ...
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrAsync<TOut>(TOut, Func<TOk, Task<TOut>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:898` — missing-punctuation, figurative (metaphor, hyperbole)

```diff
     /// <remarks>
     /// <paramref name="defaultValue" /> is evaluated by the caller before the call,
-    /// so reach for <c>MapOrElseAsync</c> where computing it is expensive or
-    /// depends on the error. <paramref name="map" /> is not invoked on an
+    /// so use <c>MapOrElseAsync</c> where computing it should happen only when
+    /// the result is <see cref="Err{TOk,TErr}" />, or where it depends on the
+    /// error. <paramref name="map" /> is not invoked on an
     /// <see cref="Err{TOk,TErr}" />.
     /// </remarks>
     /// ...
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```
"Reach for" (metaphor, fixed above) and "is expensive" (hyperbole — a bare cost judgement with no measure behind it, unlike the benchmarked figures this repository's own `AGENTS.md` requires for a cost claim) are both replaced. The literal fact `defaultValue` being eager already supports: it is computed unconditionally, so this overload wastes that computation whenever the result turns out to be `Ok`.

### `Result<TOk, TErr>.MapOrDefault<TOut>(Func<TOk, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:909` — missing-punctuation

```diff
-    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" />.</param>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrDefault<TState, TOut>(TState, Func<TOk, TState, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:934` — missing-punctuation

```diff
-    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
+    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" />.</param>
     /// ...
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrDefaultAsync<TOut>(Func<TOk, Task<TOut>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:957` — missing-punctuation

```diff
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrNull<TOut>(Func<TOk, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:983` — missing-punctuation

```diff
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrNull<TState, TOut>(TState, Func<TOk, TState, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1018` — missing-punctuation

```diff
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrNullAsync<TOut>(Func<TOk, Task<TOut>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1041` — missing-punctuation

```diff
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrElse<TOut>(Func<TErr, TOut>, Func<TOk, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1061` — missing-punctuation

```diff
     /// <param name="defaultFactory">
     /// A function to create the default value for an
-    /// <see cref="Err{TOk,TErr}" />
+    /// <see cref="Err{TOk,TErr}" />.
     /// </param>
-    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" />.</param>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrElse<TState, TOut>(TState, Func<TErr, TState, TOut>, Func<TOk, TState, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1091` — missing-punctuation

```diff
     /// <param name="defaultFactory">
     /// A function to create the default value for an
-    /// <see cref="Err{TOk,TErr}" />
+    /// <see cref="Err{TOk,TErr}" />.
     /// </param>
-    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
+    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" />.</param>
     /// ...
-    /// <typeparam name="TOut">The mapped result value type</typeparam>
+    /// <typeparam name="TOut">The mapped result value type.</typeparam>
```

### `Result<TOk, TErr>.MapOrElseAsync<TOut>(Func<TErr, Task<TOut>>, Func<TOk, Task<TOut>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1114` — figurative (metaphor)

```diff
     /// <remarks>
-    /// The overload to reach for when both delegates do real asynchronous work.
+    /// Use this overload when both delegates do real asynchronous work.
     /// Where only one does, prefer the overload taking the other synchronously — it
     /// avoids wrapping a value in an already-completed task.
     /// </remarks>
```

### `Result<TOk, TErr>.MapErr<TOut>(Func<TErr, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1165` — missing-punctuation

```diff
     /// <param name="map">
-    /// The map function to apply to the <see cref="Err{TOk,TErr}" />
+    /// The map function to apply to the <see cref="Err{TOk,TErr}" />.
     /// </param>
-    /// <typeparam name="TOut">The output error value type</typeparam>
+    /// <typeparam name="TOut">The output error value type.</typeparam>
```

### `Result<TOk, TErr>.MapErr<TState, TOut>(TState, Func<TErr, TState, TOut>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1192` — missing-punctuation

```diff
     /// <param name="map">
-    /// The map function to apply to the <see cref="Err{TOk,TErr}" />
+    /// The map function to apply to the <see cref="Err{TOk,TErr}" />.
     /// </param>
     /// ...
-    /// <typeparam name="TOut">The output error value type</typeparam>
+    /// <typeparam name="TOut">The output error value type.</typeparam>
```

### `Result<TOk, TErr>.MapErrAsync<TOut>(Func<TErr, Task<TOut>>)`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1214` — missing-punctuation, figurative (metaphor)

```diff
     /// <remarks>
     /// <paramref name="map" /> is not invoked on an <see cref="Ok{TOk,TErr}" />,
     /// whose value is re-wrapped for the new error type instead. Use this to
-    /// translate an error into the vocabulary of the calling layer without deciding
-    /// whether the operation succeeded.
+    /// convert an error into the type the calling layer expects, without
+    /// deciding whether the operation succeeded.
     /// </remarks>
     /// ...
-    /// <typeparam name="TOut">The output error value type</typeparam>
+    /// <typeparam name="TOut">The output error value type.</typeparam>
```
"Vocabulary" and "translate" borrow a language metaphor for a plain type conversion.

### `Result<TOk, TErr>.GetOk()`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1226` — missing-punctuation

```diff
     /// <summary>
     /// Converts from a <see cref="Result{TOk,TErr}" /> into an
-    /// <c>Option&lt;TOk&gt;</c>
+    /// <c>Option&lt;TOk&gt;</c>.
     /// </summary>
```

### `Result<TOk, TErr>.GetErr()`
`src/Waystone.Monads/Results/ResultOfTOkTErr.cs:1238` — missing-punctuation

```diff
     /// <summary>
     /// Converts from a <see cref="Result{TOk,TErr}" /> to
-    /// <c>Option&lt;TErr&gt;</c>
+    /// <c>Option&lt;TErr&gt;</c>.
     /// </summary>
```

### Clean
- `Result<TOk, TErr>` (type summary and typeparams)
- `Match<TState, TOut>(TState, Func<TOk, TState, TOut>, Func<TErr, TState, TOut>)`
- `Match<TState>(TState, Action<TOk, TState>, Action<TErr, TState>)`
- `MatchAsync<TOut>(Func<TOk, Task<TOut>>, Func<TErr, TOut>)`
- `MatchAsync<TOut>(Func<TOk, TOut>, Func<TErr, Task<TOut>>)`
- `MatchAsync(Func<TOk, Task>, Action<TErr>)`
- `MatchAsync(Action<TOk>, Func<TErr, Task>)`
- `And<TOut>(Result<TOut, TErr>)`
- `OrElse<TOut>(Func<TErr, Result<TOk, TOut>>)`
- `OrElse<TState, TOut>(TState, Func<TErr, TState, Result<TOk, TOut>>)`
- `OrElseAsync<TOut>(Func<TErr, ValueTask<Result<TOk, TOut>>>)`
- `UnwrapOrElse(Func<TErr, TOk>)`
- `UnwrapOrElse<TState>(TState, Func<TErr, TState, TOk>)`
- `Inspect<TState>(TState, Action<TOk, TState>)`
- `InspectAsync(Func<TOk, Task>)`
- `InspectErr<TState>(TState, Action<TErr, TState>)`
- `Map<TOut>(Func<TOk, TOut>)`
- `Map<TState, TOut>(TState, Func<TOk, TState, TOut>)`
- `MapAsync<TOut>(Func<TOk, Task<TOut>>)`
- `MapOrElseAsync<TOut>(Func<TErr, TOut>, Func<TOk, Task<TOut>>)`
- `MapOrElseAsync<TOut>(Func<TErr, Task<TOut>>, Func<TOk, TOut>)`
- `AsEnumerable()`

`IsOkAndAsync`, `IsErrAndAsync`, `MatchAsync<TOut>(Func<TOk,Task<TOut>>,Func<TErr,Task<TOut>>)`, `MatchAsync(Func<TOk,Task>,Func<TErr,Task>)`, `Unwrap()`, `UnwrapErr()`, `UnwrapOrElseAsync`, `InspectErrAsync` and `MapOrElseAsync<TOut>(Func<TErr,Task<TOut>>,Func<TOk,Task<TOut>>)` were marked clean in an earlier pass of this report, before the figurative-language rule was refined and before the second sweep for cost language and "consuming"; each now has its own section above.

### Unverified
None. Every factual claim in this file (which branch invokes a delegate, which case completes synchronously, which exception type is thrown) was checked directly against `Ok<TOk,TErr>`/`Err<TOk,TErr>`, the two exception types, and the `WM2017` diagnostic descriptor.

## Option binders

Files: `src/Waystone.Monads/Options/OptionBound.cs` (41 public members, including the
`Bound<TState>` type), `src/Waystone.Monads/Options/OptionFactoryBound.cs` (4 public
members, including the `Bound<TState>` type). 45 public members examined.

### `Option<T>.Bound<TState>` — type-level remarks

`OptionBound.cs:16-52`

```diff
     /// <remarks>
     /// Created by <c>With</c> on an <see cref="Option{T}" />. Each member below
     /// takes the same delegate as the <see cref="Option{T}" /> member it shares
-    /// a name with, invokes it with the bound state, and returns the plain
-    /// <see cref="Option{T}" /> — the state is spent by the call rather than
-    /// carried onward, so a chain that needs it twice binds it twice.
-    /// <para>
-    /// Nested inside <see cref="Option{T}" /> rather than named beside it
-    /// because a second type parameter on a monad in this library already means
-    /// something else: <see cref="Result{TOk,TErr}" /> uses it for the error
-    /// type. Nesting keeps <see cref="Option{T}" /> a one-parameter type, so a
-    /// genuine <c>Option&lt;string, int&gt;</c> still fails to compile.
-    /// </para>
-    /// <para>
-    /// The point is the delegate, not this type. A lambda that reads the state
-    /// from its parameter captures nothing, so marking it
-    /// <see langword="static" /> costs nothing and the compiler caches it; a
-    /// lambda that reaches for an outer variable allocates a display class
-    /// every time the call site runs. Writing <see langword="static" /> is what
-    /// stops a later edit from quietly putting the allocation back.
-    /// </para>
-    /// <para>
-    /// A <see langword="default" /> instance has no option to act on and every
-    /// member throws <see cref="InvalidOperationException" />, in the manner of
-    /// <c>ImmutableArray&lt;T&gt;</c>. Reach one only by declaring it —
-    /// <c>With</c> cannot produce one.
-    /// </para>
-    /// <para>
-    /// The asynchronous members throw rather than returning a faulted task, which
-    /// is worth knowing because the opposite is the more common convention. Each
-    /// one evaluates its own body eagerly so that a
-    /// <see cref="None{T}" /> completes without building a state machine, and a
-    /// delegate that throws before it returns its
-    /// <see cref="System.Threading.Tasks.Task" /> throws through the call for the
-    /// same reason. Awaiting the result is not what surfaces either failure.
-    /// </para>
+    /// a name with, invokes it with the bound state, and returns a plain
+    /// <see cref="Option{T}" />. The bound state applies to that one call
+    /// only; bind it again for a second call.
+    /// <para>
+    /// Mark the delegate <see langword="static" /> if it does not otherwise
+    /// need to capture anything. A delegate that reads only its parameters
+    /// allocates nothing when marked <see langword="static" />; one that also
+    /// reads an outer variable allocates a new closure on every call.
+    /// </para>
+    /// <para>
+    /// A <see langword="default" /> instance of this type has no option to
+    /// call into, and every member throws
+    /// <see cref="InvalidOperationException" />. Only <c>With</c> produces a
+    /// usable instance.
+    /// </para>
+    /// <para>
+    /// An asynchronous member throws directly, rather than returning a
+    /// faulted task, when the delegate throws before it returns its
+    /// <see cref="System.Threading.Tasks.Task" />.
+    /// </para>
     /// </remarks>
```

`over-long-remarks`, `non-consumer-content`, `figurative`. The cut paragraph
explaining nesting is library design history, not something a caller needs to call
a member correctly. "Which is worth knowing because the opposite is the more common
convention" and "Awaiting the result is not what surfaces either failure" are
editorial asides restating the same fact a different way. Three separate figurative
phrases in the cut text, each with its kind and literal replacement:
- "the state is spent by the call rather than carried onward" — personification
  plus metaphor (state as a thing that gets used up and moved). Literal: "The
  bound state applies to that one call only."
- "The point is the delegate, not this type." — hyperbole (overstates that the
  type itself carries no information, for emphasis). Literal: cut; the sentences
  that follow already say what marking a delegate `static` does.
- "a later edit from quietly putting the allocation back" — personification (an
  allocation sneaking back in on its own). Literal: "a later edit that removes
  `static` reintroduces the allocation."

### `IsSomeAnd` / `IsNoneOr` — boolean `<returns>` slot shape

`OptionBound.cs:82-85` (`IsSomeAnd`), `OptionBound.cs:103-106` (`IsNoneOr`)

```diff
         /// <returns>
-        /// <see langword="true" /> only when the option holds a value and
-        /// <paramref name="predicate" /> accepts it.
+        /// True if the option holds a value and <paramref name="predicate" />
+        /// accepts it; false otherwise.
         /// </returns>
         public bool IsSomeAnd(Func<T, TState, bool> predicate) =>
```

```diff
         /// <returns>
-        /// <see langword="true" /> when the option is a <see cref="None{T}" />,
-        /// or when it holds a value <paramref name="predicate" /> accepts.
+        /// True if the option holds no value, or holds one
+        /// <paramref name="predicate" /> accepts; false otherwise.
         /// </returns>
         public bool IsNoneOr(Func<T, TState, bool> predicate) =>
```

`inconsistent-sibling`. Both async siblings (`IsSomeAndAsync`, `IsNoneOrAsync`)
already use the plain "True if …; false otherwise" shape the style calls for. The
sync members use `<see langword="true" />` instead and drop the "false otherwise"
half, so the pair reads as two different conventions rather than one.

### `Match<TOut>(Func<T,TState,TOut>, Func<TState,TOut>)` — remarks

`OptionBound.cs:114-120`

```diff
         /// <remarks>
-        /// The bound state reaches both delegates, which is what makes this the
-        /// most worthwhile member to bind for. Two capturing lambdas share one
-        /// display class but need a delegate each, so a capturing <c>Match</c>
-        /// allocates one delegate more than a capturing <c>Map</c> does.
-        /// <c>StateOverloadBenchmarks</c> measures both.
+        /// The bound state reaches both <paramref name="onSome" /> and
+        /// <paramref name="onNone" />, so a single bound value can serve both
+        /// branches.
         /// </remarks>
```

`over-long-remarks`, `non-consumer-content`. The allocation comparison against a
capturing `Map` and the benchmark class name are not something a caller needs to
call this member.

### Missing `ArgumentNullException` — the null-producing family

`OptionBound.cs:201-225` (`AndThen<TOut>`), `:409-435` (`ZipWith<TOther,TOut>`),
`:437-457` (`Reduce`), `:687-708` (`MapAsync<TOut>`), `:1003-1031`
(`ZipWithAsync<TOther,TOut>`), `:1033-1059` (`ReduceAsync`)

`undocumented-failure`. Verified against the implementation: each of these forwards
to a code path that ends in `Option.SomeOrThrow` or `Option.Some` (which throws
`ArgumentNullException` on a null value) — `Some<T>.AndThen<TState,TOut>` calls
`Option.NotNull`, `Some<T>.ZipWith<TState,…>` calls `other.Map` which calls
`SomeOrThrow`, `Some<T>.Reduce<TState>` calls `SomeOrThrow` directly, and the async
members wrap their delegate's result in `Option.Some` via the private `AwaitedSome`
helper. Every sibling on `Option<T>` itself (`Map`, `Map<TState,…>`, the non-bound
`AndThen`/`AndThenAsync`/`ZipWith`/`ZipWithAsync`) documents this with an
`<exception>` tag; only `Map<TOut>` does so on `Bound<TState>` — the other five do
not. Representative diff, for `AndThen<TOut>`:

```diff
         /// <typeparam name="TOut">
         /// The value type of the option <paramref name="optionFactory" />
         /// produces.
         /// </typeparam>
+        /// <exception cref="ArgumentNullException">
+        /// If <paramref name="optionFactory" /> returns a null option. See the
+        /// remarks on <see cref="Option{T}" /> for why that throws rather than
+        /// producing a <see cref="None{T}" />.
+        /// </exception>
         /// <returns>
         /// Whatever <paramref name="optionFactory" /> produced, or
         /// <see cref="None{T}" /> if it was never invoked.
         /// </returns>
         public Option<TOut> AndThen<TOut>(
```

Same tag, inserted in the same position (after the last `<typeparam>`, before
`<returns>`), on the other five, with the delegate name swapped in:
- `ZipWith<TOther,TOut>` — `If <paramref name="zip" /> returns null.`
- `Reduce` — `If <paramref name="reduce" /> returns null.`
- `MapAsync<TOut>` — `If <paramref name="map" /> returns null.`
- `ZipWithAsync<TOther,TOut>` — `If <paramref name="zip" /> returns null.`
- `ReduceAsync` — `If <paramref name="reduce" /> returns null.`

### `OkOrElse<TErr>` / `OkOrElseAsync<TErr>` — figurative remarks and a missing exception

`OptionBound.cs:382-407`, `:975-1001`

```diff
         /// <remarks>
-        /// This is the seam where an absence stops being acceptable and has to
-        /// be accounted for. <paramref name="errorFactory" /> runs only for a
-        /// <see cref="None{T}" />, so building an error that carries context is
-        /// affordable here.
+        /// <paramref name="errorFactory" /> runs only for a
+        /// <see cref="None{T}" />; it is never invoked for a
+        /// <see cref="Some{T}" />.
         /// </remarks>
         /// <param name="errorFactory">
         /// Produces the error from the bound state. It receives no value, there
         /// being none to hand it, and is not invoked for a
         /// <see cref="Some{T}" />.
         /// </param>
         /// <typeparam name="TErr">
         /// The error type <paramref name="errorFactory" /> produces.
         /// </typeparam>
+        /// <exception cref="ArgumentNullException">
+        /// If <paramref name="errorFactory" /> returns null. An
+        /// <see cref="Err{TOk,TErr}" /> cannot hold a null error.
+        /// </exception>
         /// <returns>
         /// <see cref="Ok{TOk,TErr}" /> of the contained value, or
         /// <see cref="Err{TOk,TErr}" /> of what
         /// <paramref name="errorFactory" /> produced.
         /// </returns>
         public Result<T, TErr> OkOrElse<TErr>(Func<TState, TErr> errorFactory)
```

`figurative`, `undocumented-failure`. "This is the seam where an absence stops
being acceptable and has to be accounted for" is metaphor (absence and acceptance
figured as edges meeting at a seam). Literal replacement: state plainly that
`errorFactory` only ever runs on the empty case — which the param doc already
half-said, so the remark now just confirms it rather than dressing it up.
Also verified: `None<T>.OkOrElse<TState,TErr>` calls
`Result.Err<T, TErr>(errorFactory(state))`, and `Err<TOk,TErr>`'s constructor throws
`ArgumentNullException` when its value is null — `TErr : notnull` is a compile-time
constraint only, not a runtime guarantee against a caller on an older, nullable-oblivious
target.

For `OkOrElseAsync<TErr>` (`:979-983` remarks, same insertion point for the
exception tag): the remarks read "The crossing point from 'absent' to 'failed for a
stated reason', which is why the error is produced rather than passed - an option
reaching here usually knows why it is empty." Replace with:

```diff
         /// <remarks>
-        /// The crossing point from "absent" to "failed for a stated reason",
-        /// which is why the error is produced rather than passed - an option
-        /// reaching here usually knows why it is empty.
+        /// <paramref name="errorFactory" /> runs only for a
+        /// <see cref="None{T}" />, so it can build an error using whatever
+        /// context caused the absence.
         /// </remarks>
```

Same `<exception>` tag as `OkOrElse`, added after the `<typeparam name="TErr">`
tag and before `<returns>` — verified via the same `AwaitedErr` → `Result.Err` path.
"The crossing point from 'absent' to 'failed for a stated reason'" is metaphor
(the same edge-crossing image as `OkOrElse`, restated); "an option reaching here
usually knows why it is empty" is personification (the option credited with
knowing something). Both are cut in the replacement above.

### `Option.With<TState>` — personification in the remarks

`OptionFactoryBound.cs:146-150`

```diff
     /// <para>
-    /// Pass a tuple to bind more than one value, and mark the factory
-    /// <see langword="static" /> so the compiler rejects a capture that creeps
-    /// back in.
+    /// Pass a tuple to bind more than one value. Mark the factory
+    /// <see langword="static" /> so the compiler reports an error if it
+    /// captures a variable.
     /// </para>
```

`figurative` — personification. "A capture that creeps back in" figures an
accidental closure as something that sneaks in on its own; the literal fact is
that marking the factory `static` turns an accidental capture into a compile
error (CS8421), which is what the replacement says directly.

### `MapOrNull<TOut>` / `MapOrNullAsync<TOut>` — metaphor in the remarks

`OptionBound.cs:280-286` (`MapOrNull`), `:795-805` (`MapOrNullAsync`)

```diff
         /// <remarks>
-        /// The bridge out of <see cref="Option{T}" /> into
-        /// <see cref="Nullable{T}" />. Prefer it to
-        /// <see cref="MapOrDefault{TOut}" /> wherever the produced type is a value
-        /// type, since a mapped zero and an absent value are the same
-        /// <see langword="default" /> and different nulls.
+        /// Converts the option's case into a <see cref="Nullable{T}" />.
+        /// Prefer it to <see cref="MapOrDefault{TOut}" /> wherever the produced
+        /// type is a value type, since a mapped zero and an absent value are
+        /// the same <see langword="default" /> and different nulls.
         /// </remarks>
         public TOut? MapOrNull<TOut>(Func<T, TState, TOut> map)
```

`figurative` — metaphor. "The bridge out of `Option<T>` into `Nullable<T>`"
figures the conversion as a crossing between two places; "converts … into" says
the same thing without the image. On `MapOrNullAsync`, the remarks open "The
asynchronous half of the bridge into `Nullable<T>`" — same image, reused; replace
with "The asynchronous counterpart of `MapOrNull<TOut>`."

### `Option.Bound<TState>` / `Option.With<TState>` — duplicated summary

`OptionFactoryBound.cs:12-15` (type), `:133-136` (method)

```diff
     /// <summary>
-    /// Binds a value so that the next <c>Try</c> can hand it to its factory
-    /// rather than have the factory capture it.
+    /// A value bound for whichever <c>Try</c> is called on it next.
     /// </summary>
     /// <remarks>
     /// The counterpart of <see cref="Option{T}.Bound{TState}" /> for the two
```

`copy-paste-drift`. The struct's summary and the `With<TState>` method's summary
(`:133-136`) are word-for-word identical. Doc generators index the first sentence
only, so the two members are indistinguishable in a member list — one should
describe the type, the other the action of creating it. Leave `With<TState>`'s
summary as-is; it already reads as the action.

### `Option.With<TState>` — `<returns>` restates the parameter instead of the type

`OptionFactoryBound.cs:157-159`

```diff
     /// <returns>
-    /// <paramref name="state" /> carrying the two factories that consume it.
+    /// A <see cref="Bound{TState}" /> ready to hand <paramref name="state" />
+    /// to whichever <c>Try</c> is called on it.
     /// </returns>
     public static Bound<TState> With<TState>(TState state) => new(state);
```

`restated-signature`. "`state` carrying the two factories that consume it" reads
backwards — the return value carries the state; it doesn't carry the factories,
which are supplied later by the caller.

### `Try<T>` / `TryAsync<T>` — a null result is silently folded into `None<T>` too, and neither the summary nor the returns tag says so

`OptionFactoryBound.cs:39-84` (`Try<T>`), `:86-131` (`TryAsync<T>`)

```diff
         /// <summary>
-        /// Runs <paramref name="factory" /> and turns anything it throws into a
-        /// <see cref="None{T}" />.
+        /// Runs <paramref name="factory" />, turning a thrown exception or a
+        /// null result into a <see cref="None{T}" />.
         /// </summary>
```

```diff
         /// <returns>
-        /// A <see cref="Some{T}" /> if <paramref name="factory" /> produced a
-        /// value a <see cref="Some{T}" /> can hold, otherwise a
-        /// <see cref="None{T}" />.
+        /// A <see cref="Some{T}" /> of what <paramref name="factory" />
+        /// produced, or a <see cref="None{T}" /> if it returned null or threw.
         /// </returns>
```

Same two changes on `TryAsync<T>`, replacing `<paramref name="factory" />` with
`<paramref name="asyncFactory" />` and "Runs" with "Awaits".

`stale`. Verified against `Option.Try<TState,T>` and `Option.TryAsync<TState,T>` in
`Option.cs`: both call `NoneIfNull(factory(state))` inside the `try`, so a null
result becomes `None<T>` the same way a caught exception does — silently, with no
exception at all. The summary says only "turns anything it throws into a
`None<T>`", which is true but incomplete: it describes the exception path and is
silent on the more surprising one, where nothing goes wrong and the value still
disappears. The old `<returns>` text ("a value a `Some<T>` can hold") was the only
place this was hinted at, and only obliquely.

### Clean

`Match(Action<T,TState>, Action<TState>)`
`UnwrapOrElse`
`MapOr<TOut>`
`MapOrDefault<TOut>`
`MapOrElse<TOut>`
`Inspect`
`Filter`
`OrElse`
`IsSomeAndAsync`
`IsNoneOrAsync`
`MatchAsync<TOut>(Func<T,TState,Task<TOut>>, Func<TState,Task<TOut>>)`
`MatchAsync<TOut>(Func<T,TState,Task<TOut>>, Func<TState,TOut>)`
`MatchAsync<TOut>(Func<T,TState,TOut>, Func<TState,Task<TOut>>)`
`MatchAsync(Func<T,TState,Task>, Func<TState,Task>)`
`MatchAsync(Func<T,TState,Task>, Action<TState>)`
`MatchAsync(Action<T,TState>, Func<TState,Task>)`
`UnwrapOrElseAsync`
`MapOrAsync<TOut>`
`MapOrDefaultAsync<TOut>`
`MapOrElseAsync<TOut>(Func<TState,Task<TOut>>, Func<T,TState,Task<TOut>>)`
`MapOrElseAsync<TOut>(Func<TState,TOut>, Func<T,TState,Task<TOut>>)`
`MapOrElseAsync<TOut>(Func<TState,Task<TOut>>, Func<T,TState,TOut>)`
`InspectAsync`
`FilterAsync`
`OrElseAsync`

The `MatchAsync`/`MapOrElseAsync` families were checked hardest against
`inconsistent-sibling`/`copy-paste-drift` per the brief — every summary in both
grids is distinct and correctly names its own mixed sync/async shape. No drift
found there.

### Open behaviour question — not a documentation defect

`AndThenAsync<TOut>` (`OptionBound.cs:731-736`) does not throw
`ArgumentNullException` on a null option from `optionFactory`, unlike every other
member in the null-producing family above and unlike its own non-bound siblings.
Confirmed by inspection, four sites:

- `Some.cs:214` `AndThen<TOut>` guards with `Option.NotNull`.
- `Some.cs:219` `AndThen<TState,TOut>` guards with `Option.NotNull`.
- `Some.cs:225` `AndThenAsync<TOut>` guards with `Option.NotNullAsync`.
- `OptionBound.cs:731` `AndThenAsync<TOut>` (the state-bound overload) has no
  guard at all: `Source is Some<T> some ? optionFactory(some.Value, _state) : …`
  returns the factory's result directly, so a null-returning factory propagates
  a null `Option<TOut>` instead of throwing.

This is a behaviour question for the team, not a documentation defect. I have not
written a comment describing the current behaviour, and no comment should be
written until the team decides whether the missing guard is intentional or a
bug — documenting the gap now would turn an inconsistency into a contract before
anyone has agreed it should stay one. No code change proposed; that decision, and
any resulting `.cs` edit, is outside this audit's remit.

### Note on the revised figurative-language rule

Re-checked both files against the narrowed rule. "Which exceptions are swallowed"
(`OptionFactoryBound.cs:44`, `Try<T>` remarks) is the one candidate that could have
been mis-flagged under the earlier broad instruction — it wasn't, and it stays:
"swallow" is .NET's own vocabulary for a caught-and-discarded exception, not
authored imagery, so no change. Nothing else previously flagged in this file was
dropped, so there is nothing else to restore.


## Result binders

Files: `src/Waystone.Monads/Results/ResultBound.cs`, `src/Waystone.Monads/Results/ResultFactoryBound.cs`.
Public members examined: 43 (2 types, 41 members).

### `Result<TOk,TErr>.Bound<TState>` class remarks

`ResultBound.cs:12-53` — stale, figurative, non-consumer-content, over-long-remarks

```diff
     /// <summary>
     /// A <see cref="Result{TOk,TErr}" /> with a value bound to it, ready to be
     /// handed to a delegate that would otherwise have captured it.
     /// </summary>
     /// <remarks>
     /// Created by <c>With</c> on a <see cref="Result{TOk,TErr}" />. Each member
     /// below takes the same delegate as the <see cref="Result{TOk,TErr}" />
     /// member it shares a name with, invokes it with the bound state, and
-    /// returns the plain <see cref="Result{TOk,TErr}" /> — the state is spent by
-    /// the call rather than carried onward, so a chain that needs it twice binds
-    /// it twice.
+    /// returns a plain <see cref="Result{TOk,TErr}" />. The bound state applies
+    /// to that one call only, so a chain that needs it twice binds it twice.
     /// <para>
     /// Every delegate here receives a value as well as the state, which is
     /// where this differs from <see cref="Options.Option{T}.Bound{TState}" />:
     /// a result always holds something, so there is no branch that has only the
     /// state to give. Which value arrives depends on the branch — the ok value
     /// or the error.
     /// </para>
     /// <para>
-    /// The point is the delegate, not this type. A lambda that reads the state
-    /// from its parameter captures nothing, so marking it
-    /// <see langword="static" /> costs nothing and the compiler caches it; a
-    /// lambda that reaches for an outer variable allocates a display class every
-    /// time the call site runs. Writing <see langword="static" /> is what stops
-    /// a later edit from quietly putting the allocation back.
+    /// Mark the delegate <see langword="static" />. A lambda that only reads
+    /// its parameters allocates nothing; one that captures an outer variable
+    /// allocates a display class on every call, and <see langword="static" />
+    /// is what makes the compiler reject a delegate that captures one.
     /// </para>
     /// <para>
-    /// A <see langword="default" /> instance has no result to act on and every
-    /// member throws <see cref="InvalidOperationException" />, in the manner of
-    /// <c>ImmutableArray&lt;T&gt;</c>. Reach one only by declaring it —
-    /// <c>With</c> cannot produce one.
+    /// A <see langword="default" /> instance has no result to act on, and every
+    /// member throws <see cref="InvalidOperationException" />. Declare a
+    /// default instance explicitly to get one — <c>With</c> never produces
+    /// one.
     /// </para>
     /// <para>
-    /// The asynchronous members throw rather than returning a faulted task, which
-    /// is worth knowing because the opposite is the more common convention. Each
-    /// one evaluates its own body eagerly so that an
-    /// <see cref="Err{TOk,TErr}" /> completes without building a state machine,
-    /// and a delegate that throws before it returns its
-    /// <see cref="System.Threading.Tasks.Task" /> throws through the call for the
-    /// same reason. Awaiting the result is not what surfaces either failure.
+    /// Most of the asynchronous members throw synchronously — at the call,
+    /// not from the returned task — when the bound instance is
+    /// <see langword="default" /> or when a delegate throws before it returns
+    /// its <see cref="System.Threading.Tasks.Task" />. Three members do not:
+    /// <c>MatchAsync(Func&lt;TOk,TState,Task&lt;TOut&gt;&gt;,
+    /// Func&lt;TErr,TState,Task&lt;TOut&gt;&gt;)</c> (line 474),
+    /// <c>MatchAsync(Func&lt;TOk,TState,Task&gt;,
+    /// Func&lt;TErr,TState,Task&gt;)</c> (line 566), and
+    /// <c>MapOrElseAsync(Func&lt;TErr,TState,Task&lt;TOut&gt;&gt;,
+    /// Func&lt;TOk,TState,Task&lt;TOut&gt;&gt;)</c> (line 910) keep the
+    /// <see langword="async" /> keyword and await both branches, so both
+    /// branches already build a state machine — a delegate that throws before
+    /// returning its task is caught by that state machine and surfaces from
+    /// the returned task instead.
     /// </para>
     /// </remarks>
     /// <typeparam name="TState">The type of the bound value.</typeparam>
```

Four defects in one block:

- **stale**: the last paragraph claimed *every* async member throws synchronously. Three named members stay `async` and await on both branches, so an exception thrown before either delegate returns its task is caught by the compiler-generated state machine and surfaces from the returned `ValueTask`, not from the call:
  - `MatchAsync<TOut>(Func<TOk,TState,Task<TOut>> onOk, Func<TErr,TState,Task<TOut>> onErr)` — `ResultBound.cs:474`
  - `MatchAsync(Func<TOk,TState,Task> onOk, Func<TErr,TState,Task> onErr)` — `ResultBound.cs:566`
  - `MapOrElseAsync<TOut>(Func<TErr,TState,Task<TOut>> defaultFactory, Func<TOk,TState,Task<TOut>> map)` — `ResultBound.cs:910`

  `ADefaultBoundThrowsFromTheCallRatherThanFromTheAwait` and `ADelegateThrowingBeforeItsTaskThrowsFromTheCall` in `ResultBoundAsyncTests.cs` only exercise `MapAsync` and `MapErrAsync` (the wrap-a-task overloads), never these three, so nothing in the suite backs the blanket claim.
- **figurative** (metaphor): "the state is spent by the call rather than carried onward" images the state as currency. Literal: "The bound state applies to that one call only."
- **figurative** (hyperbole + metaphor + personification, one paragraph): "The point is the delegate, not this type" overstates by contrast; "costs nothing" images allocation as a purchase; "reaches for an outer variable" and "quietly putting the allocation back" personify the lambda and a future code edit. Literal: state that marking the delegate `static` makes the compiler reject a capturing one, using "captures" — the term the same paragraph already uses correctly ("captures nothing").
- **figurative** (personification): "Reach one only by declaring it" images the reader physically reaching for the instance. Literal: "Declare a default instance explicitly to get one."
- **non-consumer-content / over-long-remarks** (unchanged from prior pass): the closure paragraph and the `ImmutableArray<T>` comparison are design rationale, not calling contract.

### Figurative language: the "vocabulary" / "mirror" family

`ResultBound.cs:181-189, 385-392, 678-687, 758-771, 985-1001` — figurative (metaphor), five members

The same two images recur across five members: **"vocabulary"** for an error's type, and **"mirror"** for a symmetric counterpart. Neither has a plainer synonym missing from the file — `OrElse` at line 185 uses "mirror" where `AndThen`'s own remarks (line 160) just say "step"; the file already has the literal word it needs.

```diff
         /// <summary>
         /// Recovers from a failure with another result, leaving a success alone.
         /// </summary>
         /// <remarks>
-        /// The mirror of <see cref="AndThen{TOut}" /> across the two cases: the
-        /// ok type is fixed and the error type is what the step may change, so
-        /// this is where a failure is retried, translated, or turned into a
-        /// success.
+        /// The counterpart of <see cref="AndThen{TOut}" /> across the two
+        /// cases: the ok type is fixed and the error type is what the step
+        /// may change, so this is where a failure is retried, converted to a
+        /// different error type, or turned into a success.
         /// </remarks>
```

The other four, same fix pattern (`mirror` → `counterpart`, `vocabulary`/`translate` → `type`/`convert`):

- `MapErr` (`ResultBound.cs:385-392`): `How a failure crosses a boundary: an error from one layer is restated in the vocabulary of the next without the success path being touched or the chain being broken open.` → `Use this to change an error's type between layers: an error from one layer is restated as the type the next layer expects, without changing the success path or interrupting the chain.` (also fixes the personification "crosses a boundary" and the metaphor "chain being broken open" — `Inspect`, two members up, already uses the literal phrasing "without interrupting it").
- `OrElseAsync` (`ResultBound.cs:678-687`): `so this is how a chain translates one failure vocabulary into another` → `so this is how a chain converts one error type into another`.
- `InspectErrAsync` (`ResultBound.cs:758-771`): `The mirror of <see cref="InspectAsync" /> on the failed branch` → `The counterpart of <see cref="InspectAsync" /> on the failed branch`.
- `MapErrAsync` (`ResultBound.cs:985-1001`): `this is the mirror of <see cref="MapAsync{TOut}" /> and the usual way to translate a failure into the vocabulary of the caller above` → `this is the counterpart of <see cref="MapAsync{TOut}" /> and the usual way to convert a failure into the error type the caller above expects`.

### `Match<TOut>`

`ResultBound.cs:111-121` — non-consumer-content

```diff
         /// <summary>
         /// Produces a value from whichever case the result is in, so both cases
         /// are answered in one expression.
         /// </summary>
-        /// <remarks>
-        /// The bound state reaches both delegates, which is what makes this the
-        /// most worthwhile member to bind for. Two capturing lambdas share one
-        /// display class but need a delegate each, so a capturing <c>Match</c>
-        /// allocates one delegate more than a capturing <c>Map</c> does.
-        /// <c>StateOverloadBenchmarks</c> measures both.
-        /// </remarks>
         /// <param name="onOk">
```

`StateOverloadBenchmarks` is an internal benchmark type a caller cannot see or run, and the delegate-count comparison is design rationale for why the overload exists, not something needed to call it. This also carries the superlative "most worthwhile member to bind for" — cutting the remark removes that along with the non-consumer content.

### `MatchAsync(Func<TOk,TState,TOut>, Func<TErr,TState,Task<TOut>>)`

`ResultBound.cs:513-527` — non-consumer-content

```diff
         /// <remarks>
         /// An <see cref="Ok{TOk,TErr}" /> completes synchronously, and neither
         /// branch builds a state machine.
-        /// <para>
-        /// The body is character-for-character the same as the overload above,
-        /// which is correct and reads like a copy-paste slip. The delegates are
-        /// the other way round, so each <c>new ValueTask&lt;TOut&gt;(…)</c> binds
-        /// to the other constructor — the one taking a <c>Task&lt;TOut&gt;</c>
-        /// here where it took a <c>TOut</c> there. Making the two bodies *look*
-        /// different is what would actually break one of them.
-        /// </para>
         /// </remarks>
```

This paragraph is a note to a future maintainer reading the source next to its sibling, not a calling contract — nothing here changes how a caller invokes the member. It is also written for a reviewer diffing the file, not for IntelliSense: `*look*` is Markdown emphasis, which XML doc rendering does not interpret, so a caller would see the literal asterisks.

### `IsOkAnd` / `IsErrAnd`

`ResultBound.cs:83-86, 104-107` — inconsistent-sibling

```diff
         /// <returns>
-        /// <see langword="true" /> only when the result succeeded and
-        /// <paramref name="predicate" /> accepts its value.
+        /// True if the result succeeded and <paramref name="predicate" />
+        /// accepts its value; false otherwise.
         /// </returns>
         public bool IsOkAnd(Func<TOk, TState, bool> predicate) =>
```

Same change for `IsErrAnd`'s `<returns>` (lines 104-107): replace `<see langword="true" /> only when the result failed and <paramref name="predicate" /> accepts its error.` with `True if the result failed and <paramref name="predicate" /> accepts its error; false otherwise.`

Their own async counterparts, `IsOkAndAsync` and `IsErrAndAsync` two members down, already use the "True if …; false otherwise." phrasing (lines 421-424, 443-446) — the sync pair is the odd one out in its own file, and it puts `true`/`false` in code font where the slot convention used elsewhere in the same file asks for plain sentence text.

### `Try<TOk,TErr>` / `Try<TOk>` / `TryAsync<TOk,TErr>` / `TryAsync<TOk>`

`ResultFactoryBound.cs:82-243` — undocumented-failure, figurative

```diff
         /// <summary>
         /// Runs <paramref name="factory" /> and turns anything it throws into an
         /// error of your own type.
         /// </summary>
         /// <remarks>
         /// Which exceptions are swallowed is decided by
         /// <see cref="Configs.MonadOptions" />, not by this call, and anything
         /// outside that set propagates.
+        /// <para>
+        /// A <paramref name="factory" /> that returns <see langword="null" />
+        /// is treated as a failure too: <paramref name="onError" /> is invoked
+        /// with an <see cref="ArgumentNullException" /> that was never thrown,
+        /// so it carries no stack trace and is not logged. An
+        /// <see cref="OperationCanceledException" /> is not caught at all and
+        /// propagates to the caller unless
+        /// <see cref="Configs.MonadOptionsBuilder.UseCancellationAsFailure" />
+        /// is configured.
+        /// </para>
         /// </remarks>
         /// <param name="factory">
         /// Produces the ok value from the bound state. Its exceptions are what
-        /// this method exists to absorb.
+        /// this method exists to catch.
         /// </param>
```

- **undocumented-failure** (unchanged from prior pass): `Try<TState,TOk,TErr>` and `TryAsync<TState,TOk,TErr>` on `Result` — what these four members forward to — both document, in their own `<remarks>`, that a null return becomes an `Err` and that `OperationCanceledException` passes through uncaught (`Result.cs:226-243`, `321-338`). None of that carries over to the `Bound<TState>` wrappers a caller actually calls through `Result.With(...).Try(...)`. Apply the same `<remarks>` addition to `Try<TOk>` and to both `TryAsync` overloads — each forwards to a core `Try`/`TryAsync` with the identical null-check and uncaught-cancellation behaviour (verified in `Result.cs:97-99`, `271-273`, `366-369`, `589-596`, `659-666`).
- **figurative** (metaphor): "this method exists to absorb" images the exception as a liquid being soaked up. `Try<TOk,TErr>` (line 55), `Try<TOk>` (line 110), and both task's-exceptions-too occurrences on `TryAsync<TOk,TErr>` (line 157) and `TryAsync<TOk>` (line 211) all use it. The literal replacement is "catch" — the word the same doc already uses one line down ("Describes a **caught** exception"), so this makes the two consistent rather than introducing new vocabulary. I did **not** touch "swallowed" (class remarks, line 49) or any use of "leak" — both are .NET's own vocabulary for this behaviour, not an image reached for by the author, and are correct as written.

### Figurative language in the rest of `ResultFactoryBound.cs`

`ResultFactoryBound.cs:17-20, 260-262` — figurative (personification)

```diff
     /// <remarks>
     /// The counterpart of <see cref="Result{TOk,TErr}.Bound{TState}" /> for the
-    /// factories here, which have no result to be called on and so cannot be
-    /// reached the same way.
+    /// factories here, which have no result yet to call members on, so this
+    /// type exists instead.
```

"Cannot be reached the same way" personifies the API as something a caller physically reaches for.

```diff
     /// <para>
     /// Pass a tuple to bind more than one value, and mark the factory
-    /// <see langword="static" /> so the compiler rejects a capture that creeps
-    /// back in.
+    /// <see langword="static" /> so the compiler rejects a factory that
+    /// captures a variable.
     /// </para>
```

"Creeps back in" personifies a future code edit as something that sneaks past the author, the same figure flagged in the class-remarks fix above ("quietly putting the allocation back").

### Clean

- `Bound<TState>` (`ResultBound.cs`) — struct itself, `<typeparam name="TState">`
- `AndThen`
- `UnwrapOrElse`
- `Inspect`
- `Map`
- `MapOr`
- `MapOrDefault`
- `MapOrNull`
- `MapOrElse`
- `IsOkAndAsync`
- `IsErrAndAsync`
- `MatchAsync(Func<TOk,TState,Task<TOut>>, Func<TErr,TState,Task<TOut>>)`
- `MatchAsync(Func<TOk,TState,Task<TOut>>, Func<TErr,TState,TOut>)`
- `MatchAsync(Func<TOk,TState,Task>, Func<TErr,TState,Task>)` (void)
- `MatchAsync(Func<TOk,TState,Task>, Action<TErr,TState>)`
- `MatchAsync(Action<TOk,TState>, Func<TErr,TState,Task>)`
- `AndThenAsync`
- `UnwrapOrElseAsync`
- `InspectAsync`
- `MapAsync`
- `MapOrAsync`
- `MapOrDefaultAsync`
- `MapOrNullAsync`
- `MapOrElseAsync(Func<TErr,TState,TOut>, Func<TOk,TState,Task<TOut>>)`
- `MapOrElseAsync(Func<TErr,TState,Task<TOut>>, Func<TOk,TState,TOut>)`
- `Result.Bound<TState>` (`ResultFactoryBound.cs`) — struct itself, `<typeparam name="TState">`
- `Result.With<TState>` — clean apart from the "creeps back in" fix above


## Option factory, cases and extensions

Scope: `Options/Option.cs`, `Options/Some.cs`, `Options/None.cs`,
`Options/Extensions/OptionExtensions.cs`, `Options/Extensions/OptionsCollectionExtensions.cs`.
Examined 9 public members in `Option.cs`, 69 in `Some.cs` (68 `<inheritdoc />` overrides plus `Deconstruct`), 68 in `None.cs` (all `<inheritdoc />`), 12 in `OptionExtensions.cs`, and 12 in `OptionsCollectionExtensions.cs` — 170 total.

### `Option` (class)

`Options/Option.cs:12` — missing-punctuation

```diff
-/// <summary>Creates <see cref="Option{T}" /> values</summary>
+/// <summary>Creates <see cref="Option{T}" /> values.</summary>
```

### `Option.Try<T>(Func<T>, ...)`

`Options/Option.cs:37` — missing-punctuation

```diff
-    /// <typeparam name="T">The factory return value's type</typeparam>
+    /// <typeparam name="T">The factory return value's type.</typeparam>
```

`Options/Option.cs:47` — figurative (personification)

```diff
     /// configured. An <see cref="OperationCanceledException" /> is not caught
-    /// at all: it leaves this method untouched, so it is neither logged nor
-    /// turned into a <see cref="None{T}" />, and the caller observes the
+    /// at all, so it is neither logged nor
+    /// turned into a <see cref="None{T}" />, and the caller observes the
     /// cancellation it asked for. Call
     /// <see cref="MonadOptionsBuilder.UseCancellationAsFailure" /> to catch it like
     /// any other exception.
```

"Leaves this method untouched" casts the exception as an agent that could
have touched something and chose not to. The preceding clause — "is not
caught at all" — already states the literal fact; the image adds nothing
and can be cut outright. Same fix applies verbatim to the three sibling
overloads below.

### `Option.TryAsync<T>(Func<Task<T>>, ...)`

`Options/Option.cs:93` — missing-punctuation

```diff
-    /// <typeparam name="T">The async factory return type</typeparam>
+    /// <typeparam name="T">The async factory return type.</typeparam>
```

`Options/Option.cs:103` — figurative (personification)

```diff
     /// configured. An <see cref="OperationCanceledException" /> is not caught
-    /// at all: it leaves this method untouched, so it is neither logged nor
-    /// turned into a <see cref="None{T}" />, and the caller observes the
+    /// at all, so it is neither logged nor
+    /// turned into a <see cref="None{T}" />, and the caller observes the
     /// cancellation it asked for. Call
     /// <see cref="MonadOptionsBuilder.UseCancellationAsFailure" /> to catch it like
     /// any other exception.
```

### `Option.Try<TState, T>(TState, Func<TState, T>, ...)`

`Options/Option.cs:154` — missing-punctuation

```diff
-    /// <typeparam name="T">The factory return value's type</typeparam>
+    /// <typeparam name="T">The factory return value's type.</typeparam>
```

`Options/Option.cs:173` — figurative (personification)

```diff
     /// configured. An <see cref="OperationCanceledException" /> is not caught
-    /// at all: it leaves this method untouched, so it is neither logged nor
-    /// turned into a <see cref="None{T}" />, and the caller observes the
+    /// at all, so it is neither logged nor
+    /// turned into a <see cref="None{T}" />, and the caller observes the
     /// cancellation it asked for. Call
     /// <see cref="MonadOptionsBuilder.UseCancellationAsFailure" /> to catch it like
     /// any other exception.
```

### `Option.TryAsync<TState, T>(TState, Func<TState, Task<T>>, ...)`

`Options/Option.cs:228` — missing-punctuation

```diff
-    /// <typeparam name="T">The async factory return type</typeparam>
+    /// <typeparam name="T">The async factory return type.</typeparam>
```

`Options/Option.cs:247` — figurative (personification)

```diff
     /// configured. An <see cref="OperationCanceledException" /> is not caught
-    /// at all: it leaves this method untouched, so it is neither logged nor
-    /// turned into a <see cref="None{T}" />, and the caller observes the
+    /// at all, so it is neither logged nor
+    /// turned into a <see cref="None{T}" />, and the caller observes the
     /// cancellation it asked for. Call
     /// <see cref="MonadOptionsBuilder.UseCancellationAsFailure" /> to catch it like
     /// any other exception.
```

The four `Try`/`TryAsync` overloads were checked against `MonadOptions.Catches`,
`MonadOptions.Log` and `MonadOptionsBuilder.UseCancellationAsFailure` (in
`Configs/`): every claim about null vs. thrown producing `None<T>`, the
console write gated only on `Debugger.IsAttached` (not on whether a listener
is subscribed), and cancellation propagating uncaught by default, holds.

### `Option.Some<T>(T)`

`Options/Option.cs:277` — missing-punctuation, prose-instead-of-tag, figurative (metaphor)

```diff
-    /// <summary>Creates a <see cref="Some{T}" /></summary>
+    /// <summary>Creates a <see cref="Some{T}" />.</summary>
```

```diff
     /// <exception cref="ArgumentNullException">
     /// <paramref name="value" /> is null. A <see cref="Some{T}" /> may hold the
     /// default of its type, but never null. The <c>notnull</c> constraint makes
-    /// this hard to reach rather than impossible, since <c>default!</c> and an
-    /// unconstrained caller both get through. Call
-    /// <c>Option.FromNullable</c> instead to turn null into a
+    /// this unlikely rather than impossible: <c>default!</c> and an
+    /// unconstrained caller can both still produce a null value. Call
+    /// <see cref="Option.FromNullable{T}(T?)" /> instead to turn null into a
     /// <see cref="None{T}" /> rather than a throw.
     /// </exception>
```

"Hard to reach" and "get through" are a spatial-barrier metaphor for
"unlikely"; the constraint does not make anything harder to physically
access, it makes null values rarer. `Option.FromNullable` is overloaded
(struct and class constraints); pick whichever overload's cref the compiler
accepts, or drop the parameter list to reference the method group if the
compiler allows it. Either resolves before prose, per the semantic-tags rule.

### `Option.None<T>()`

`Options/Option.cs:296` — missing-punctuation, inconsistent-sibling

```diff
-    /// <summary>Gets the <see cref="None{T}" /> for <typeparamref name="T" /></summary>
+    /// <summary>Returns the <see cref="None{T}" /> for <typeparamref name="T" />.</summary>
```

This is a static factory method, not a property getter, so it should open
"Creates a…"/"Returns a…" like its sibling `Some<T>` ("Creates a `Some<T>`"),
not "Gets the…". The existing `<remarks>` already explains the cached-instance
behaviour that "Gets" was presumably trying to signal — that content does not
need to live in the summary verb.

### `Some<T>.Deconstruct(out T)`

`Options/Some.cs:42` — over-long-remarks, non-consumer-content

The two middle sentences are about `None<T>` — a type this member's caller is
not holding — and about why `option is None<T>()` fails to compile, which
does not help a caller deconstruct a `Some<T>`. Trim to what a caller of
*this* member needs: what the pattern looks like, and where to go if the
value should not be bound directly.

```diff
     /// <summary>Binds the contained value in a positional pattern.</summary>
     /// <remarks>
     /// What makes <c>option is Some&lt;T&gt;(var value)</c> and an arm of
-    /// <c>option switch { Some&lt;T&gt;(var value) => …, None&lt;T&gt; => … }</c>
-    /// compile. <see cref="None{T}" /> deliberately has none — there is nothing
-    /// to bind, and <c>option is None&lt;T&gt;</c> already tests the case. Note
-    /// that the parenthesised <c>option is None&lt;T&gt;()</c> is therefore a
-    /// compiler error rather than a redundant spelling, since an empty positional
-    /// pattern still needs a <c>Deconstruct</c> to bind against.
-    /// <para>
+    /// <c>option switch { Some&lt;T&gt;(var value) => …, None&lt;T&gt; => … }</c>
+    /// compile.
+    /// <para>
     /// This is the only way to read the value off a <see cref="Some{T}" />
     /// directly; the property behind it is internal, so a caller who would
     /// rather not name the case type goes through
     /// <see cref="Option{T}.Match{TOut}(Func{T,TOut},Func{TOut})" /> or
     /// <see cref="Option{T}.Unwrap" /> instead.
     /// </para>
     /// </remarks>
```

### `None<T>` (class)

`Options/None.cs:15` — figurative (personification)

```diff
     /// <remarks>
     /// One of the two cases of <see cref="Option{T}" />, so matching both is
     /// exhaustive and no third case can be added from outside the library. Build
-    /// one with <see cref="Option.None{T}" />, which hands back a cached instance
+    /// one with <see cref="Option.None{T}" />, which returns a cached instance
     /// rather than constructing one.
     /// </remarks>
```

"Hands back" casts the factory method as a person passing something over;
it just returns a reference it already holds.

### All `<inheritdoc />` overrides in `Some.cs` / `None.cs`

Spot-checked override bodies against their inherited contract (`Match`,
`MapOr`, `Reduce`, `Zip`, `OkOrElse` and the `…Async` families) — none of the
68/67 inherited docs read wrong for what its override actually does. No
changes proposed.

### `OptionExtensions` (class)

`Options/Extensions/OptionExtensions.cs:12` — over-long-remarks, non-consumer-content

The generator/attribute mechanics are implementation detail; a caller sees
only that these members exist and forward. Keep the pointer to
`OptionsCollectionExtensions` for sequence operations — that part is
consumer-useful navigation — and drop the rest.

```diff
     /// <remarks>
-    /// Two kinds of member live here. Most are generated: the awaited-receiver
-    /// overloads that let a call chain stay in one expression when the option is
-    /// still inside a <see cref="Task{TResult}" /> or a
-    /// <see cref="ValueTask{TResult}" />, listed in the attributes below and
-    /// forwarding into the member of the same name on <see cref="Option{T}" />.
-    /// The rest are hand-written, and each is here because its receiver is a
-    /// *particular* option rather than any option — <c>Unzip</c> reads one holding
-    /// a tuple, <c>Flatten</c> a nested option, <c>Transpose</c> a
-    /// <see cref="Result{TOk,TErr}" />, <c>UnwrapOrNull</c> a value type — or
-    /// because the shape awaits an argument as well as the receiver, which the
-    /// generator has no way to reach.
+    /// Every member here is callable on an <see cref="Option{T}" />: most also
+    /// have an awaited-receiver overload that runs the same operation on a
+    /// <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" /> of one,
+    /// so a call chain stays in one expression before the option has been
+    /// awaited.
     /// <para>
     /// Operations over a *sequence* of options are the one thing that is not here.
     /// They take an <see cref="System.Collections.Generic.IEnumerable{T}" />
     /// receiver rather than an <see cref="Option{T}" />, so they share nothing with
     /// the members below and live in <see cref="OptionsCollectionExtensions" />.
     /// </para>
     /// </remarks>
```

### `OptionExtensions.With<TState>(TState)`

`Options/Extensions/OptionExtensions.cs:86` — figurative (personification)

```diff
     /// <summary>
-    /// Binds a value to the option so that the next call can hand it to a
+    /// Binds a value to the option so that the next call can pass it to a
     /// delegate rather than have the delegate capture it.
     /// </summary>
```

"Hand it to a delegate" personifies the call as someone physically passing
an object over; "pass" is the literal, already-standard term for giving an
argument to a delegate. The `state` param doc repeats the same image:

```diff
     /// <param name="state">
-    /// The value handed to the delegate of whichever member is called next.
+    /// The value passed to the delegate of whichever member is called next.
     /// Nothing reads it in the meantime, so it may be anything, including
     /// <see langword="null" />.
     /// </param>
```

`Options/Extensions/OptionExtensions.cs:90` — figurative (metaphor, personification)

```diff
     /// <remarks>
     /// The members on the returned <see cref="Option{T}.Bound{TState}" />
-    /// mirror the ones here, minus the state argument, and each returns the plain
-    /// <see cref="Option{T}" /> again. So the state is spent by the call that
-    /// uses it and a chain binds as many times as it needs to:
+    /// correspond to the ones here, minus the state argument, and each returns
+    /// the plain <see cref="Option{T}" /> again. The state is consumed by the
+    /// call that uses it, and a chain binds as many times as it needs to:
     /// <code>
     /// option.With(limit)
     ///       .Filter(static (v, s) => v &lt;= s)
     ///       .With(format)
     ///       .Map(static (v, s) => v.ToString(s));
     /// </code>
     /// <para>
-    /// Pass a tuple to bind more than one value. Mark every lambda
-    /// <see langword="static" />, which is what makes the compiler reject a
-    /// capture that creeps back in — binding the state achieves nothing on
-    /// its own if the delegate still reaches for an outer variable.
+    /// Pass a tuple to bind more than one value. Mark every lambda
+    /// <see langword="static" />, which makes the compiler reject any outer
+    /// variable the lambda still captures directly — binding the state
+    /// achieves nothing if the delegate captures one anyway.
     /// </para>
```

"Mirror" and "the state is spent" are both authored imagery (reflection,
money) for "match" and "consumed" — "consumed" is the domain's own term for
a value a caller cannot use again, so it stays. "Creeps back in" and
"reaches for an outer variable" personify the capture and the delegate; the
literal fact is that the lambda still references an outer variable directly.

`Options/Extensions/OptionExtensions.cs:119` — figurative (metaphor)

```diff
     /// <returns>
-    /// The option and <paramref name="state" /> together, carrying the
-    /// vocabulary that consumes both.
+    /// An <see cref="Option{T}.Bound{TState}" /> holding the option and
+    /// <paramref name="state" /> together, whose members forward to the
+    /// state-accepting overload of the same name on <see cref="Option{T}" />.
     /// </returns>
```

### `OptionsCollectionExtensions` (class)

`Options/Extensions/OptionsCollectionExtensions.cs:10` — buried-summary

The summary only restates the class name. It should say what distinguishes
these members from `OptionExtensions` — the receiver is a sequence, not a
single option — since `OptionExtensions`'s own remarks already point here for
that reason.

```diff
-/// <summary>Extensions for <see cref="Option{T}" /> collections.</summary>
+/// <summary>Extensions over a sequence of <see cref="Option{T}" /> values.</summary>
+/// <remarks>
+/// Operations on a single <see cref="Option{T}" /> are declared on the type
+/// itself or in <see cref="OptionExtensions" />; these instead treat an
+/// <see cref="System.Collections.Generic.IEnumerable{T}" /> of options as the
+/// receiver.
+/// </remarks>
```

### `OptionsCollectionExtensions.Filter<T>(IEnumerable<Option<T>>, Func<T, bool>)`

`Options/Extensions/OptionsCollectionExtensions.cs:30` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

### `OptionsCollectionExtensions.Map<TIn, TOut>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:55` — missing-punctuation

```diff
-    /// <typeparam name="TIn">The input option value's type</typeparam>
-    /// <typeparam name="TOut">The output option value's type</typeparam>
+    /// <typeparam name="TIn">The input option value's type.</typeparam>
+    /// <typeparam name="TOut">The output option value's type.</typeparam>
```

### `OptionsCollectionExtensions.Flatten<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:79` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

### `OptionsCollectionExtensions.Collect<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:104` — missing-punctuation, non-consumer-content

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

```diff
     /// <remarks>
     /// This is the all-or-nothing counterpart to <see cref="Flatten{T}" />, which
-    /// drops the absent elements instead of failing on them. It is the port of
-    /// Rust's <c>collect::&lt;Option&lt;Vec&lt;T&gt;&gt;&gt;()</c> and short-circuits
-    /// the same way: enumeration stops at the first <see cref="None{T}" />, so the
+    /// drops the absent elements instead of failing on them. It short-circuits:
+    /// enumeration stops at the first <see cref="None{T}" />, so the
     /// tail of <paramref name="options" /> is never visited and a side-effecting
     /// source is left partly consumed. Enumerates when it is called rather than
     /// when its result is read, and builds a list as it goes, so do not call it on
     /// an unbounded sequence.
     /// </remarks>
```

The Rust cross-reference is library history a caller of this C# method does
not need to call it correctly.

### `OptionsCollectionExtensions.CollectAsync<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:156` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

`Options/Extensions/OptionsCollectionExtensions.cs:143` — figurative (hyperbole, metaphor)

```diff
     /// <remarks>
-    /// The asynchronous counterpart of <see cref="Collect{T}" />, and it
-    /// short-circuits for real: the stream stops being pulled at the first
+    /// The asynchronous counterpart of <see cref="Collect{T}" />: pulling from
+    /// the stream stops at the first
     /// <see cref="None{T}" />, so whatever would have produced the later elements
-    /// never runs. That is the reason to reach for this over materialising the
+    /// never runs. Prefer this over materialising the
     /// stream and calling <see cref="Collect{T}" /> on the result.
     /// </remarks>
```

"For real" is an intensifier with no literal content beyond what the rest of
the sentence already states. "Reach for" personifies choosing an overload as
physically grasping something; the sibling members (`FirstOrElse`,
`LastOrElse`) already say "pick this over" for the same idea — "prefer"
matches that literal register.

### `OptionsCollectionExtensions.FirstOrNone<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:203` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

### `OptionsCollectionExtensions.FirstOr<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:227` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

`Options/Extensions/OptionsCollectionExtensions.cs:219` — figurative (metaphor)

```diff
     /// <remarks>
     /// <paramref name="defaultValue" /> is evaluated at the call site whether or not a
-    /// match is found; use <see cref="FirstOrElse{T}" /> when producing it is
-    /// expensive. Enumeration stops at the first match.
+    /// match is found; use <see cref="FirstOrElse{T}" /> instead so the
+    /// fallback is computed only when nothing matches. Enumeration stops at
+    /// the first match.
     /// </remarks>
```

"Expensive" is a money metaphor for computational cost, and an unquantified
one: it gives the caller nothing to check their own case against. The
literal fact — `FirstOrElse` only runs the factory when needed, `FirstOr`
always evaluates its argument — is what decides which to call, and is
already stated for the sibling in `FirstOrElse`'s own remarks.

### `OptionsCollectionExtensions.FirstOrElse<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:250` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

### `OptionsCollectionExtensions.LastOrNone<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:275` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

### `OptionsCollectionExtensions.LastOr<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:300` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

`Options/Extensions/OptionsCollectionExtensions.cs:291` — figurative (metaphor)

```diff
     /// <remarks>
     /// <paramref name="defaultValue" /> is evaluated at the call site whether or not a
-    /// match is found; use <see cref="LastOrElse{T}" /> when producing it is
-    /// expensive. The whole of <paramref name="options" /> is enumerated, so do
+    /// match is found; use <see cref="LastOrElse{T}" /> instead so the
+    /// fallback is computed only when nothing matches. The whole of
+    /// <paramref name="options" /> is enumerated, so do
     /// not call this on an unbounded sequence.
     /// </remarks>
```

Same fix as `FirstOr<T>`: "expensive" is the same money metaphor standing in
for the actual, checkable fact about when the factory runs.

### `OptionsCollectionExtensions.LastOrElse<T>(...)`

`Options/Extensions/OptionsCollectionExtensions.cs:324` — missing-punctuation

```diff
-    /// <typeparam name="T">The option value's type</typeparam>
+    /// <typeparam name="T">The option value's type.</typeparam>
```

### Clean

- `Option.FromNullable<T>(T?)` (struct overload)
- `Option.FromNullable<T>(T?)` (class overload)
- `Some<T>` (type-level summary/remarks)
- All 68 `<inheritdoc />` overrides in `Some.cs`
- All 67 `<inheritdoc />` overrides in `None.cs`
- `OptionExtensions.Unzip()`
- `OptionExtensions.Flatten()` (the single-option one, on `Option<Option<T>>`)
- `OptionExtensions.Transpose()`
- `OptionExtensions.UnwrapOrNull()`
- `OptionExtensions.ZipWithAsync` (`Task<Option<TSelf>>` receiver)
- `OptionExtensions.ZipWithAsync` (`ValueTask<Option<TSelf>>` receiver)
- `OptionsCollectionExtensions.Map<TIn, TOut>` (body text, aside from the typeparam punctuation above)
- `OptionsCollectionExtensions.Flatten<T>` (body text, aside from the typeparam punctuation above)
- `OptionsCollectionExtensions.FirstOrNone<T>` / `FirstOrElse<T>` (body text)
- `OptionsCollectionExtensions.LastOrNone<T>` / `LastOrElse<T>` (body text)

### Unverified

None. Every factual claim in scope (exception-catching behaviour of `Try`/`TryAsync`,
the console-write and cancellation-propagation claims, and every `<returns>`
claim in `OptionExtensions.cs` and `OptionsCollectionExtensions.cs`) was traced
against the implementation it describes, including `MonadOptions.cs`,
`MonadOptionsBuilder.cs` and `MonadDiagnostics.cs` for the `Try`/`TryAsync` remarks.


## Result factory, cases and extensions

Scope: `Results/Result.cs` (13 public static members incl. the class), `Results/Ok.cs` and `Results/Err.cs` (63 `<inheritdoc />` overrides each plus one hand-written `Deconstruct` and the record itself), `Results/Extensions/ResultExtensions.cs` (4 members) and `Results/Extensions/ResultsCollectionExtensions.cs` (5 members).
89 public members examined in total; every factual claim below was checked against the method body it documents.

### `Result` (static class)

`Result.cs:13` — missing-punctuation

```diff
-/// <summary>Creates <see cref="Result{TOk,TErr}" /> values</summary>
+/// <summary>Creates <see cref="Result{TOk,TErr}" /> values.</summary>
```

### `Result.Try<TOk, TErr>(factory, onError, …)`

`Result.cs:24-51` — buried-summary, missing-punctuation

```diff
     /// <summary>
     /// Tries to store the result of a <paramref name="factory" /> into a
     /// <see cref="Result{TOk,TErr}" />, invoking <paramref name="onError" /> if the
-    /// factory throws an exception.
+    /// factory throws an exception or returns null.
     /// </summary>
     /// <param name="factory">
-    /// A method which when executed will return the value
-    /// contained in the <see cref="Result{TOk,TErr}" />
+    /// A method which when executed will return the value
+    /// contained in the <see cref="Result{TOk,TErr}" />.
     /// </param>
     /// <param name="onError">
-    /// A callback method that will be invoked for any exceptions
-    /// thrown by the <paramref name="factory" />
+    /// A callback method that will be invoked for any exceptions
+    /// thrown by the <paramref name="factory" />.
     /// </param>
     ...
-    /// <typeparam name="TOk">The factory method return value's type</typeparam>
-    /// <typeparam name="TErr">The error handler return value's type</typeparam>
+    /// <typeparam name="TOk">The factory method return value's type.</typeparam>
+    /// <typeparam name="TErr">The error handler return value's type.</typeparam>
```

The summary says `onError` is invoked only "if the factory throws an exception," but the body also invokes it when the factory returns null (`Err<TOk, TErr>(onError(FactoryReturnedNull(...)))`, `Result.cs:97-99`). Doc generators index only the first sentence, so a reader who never opens the remarks would not learn that a null return is treated as a failure too.

### `Result.TryAsync<TOk, TErr>(asyncFactory, onError, …)`

`Result.cs:102-129` — buried-summary, missing-punctuation

```diff
     /// Tries to store the result of an <paramref name="asyncFactory" /> into
     /// a <see cref="Result{TOk, TErr}" />, invoking <paramref name="onError" /> if the
-    /// factory throws an exception.
+    /// factory throws an exception or returns null.
     /// </summary>
     /// <param name="asyncFactory">
-    /// An asynchronous method which when executed will
-    /// produce the value of the <see cref="Result{TOk,TErr}" />
+    /// An asynchronous method which when executed will
+    /// produce the value of the <see cref="Result{TOk,TErr}" />.
     /// </param>
     /// <param name="onError">
-    /// A callback method that will be invoked for any exceptions
-    /// thrown by the <paramref name="asyncFactory" />
+    /// A callback method that will be invoked for any exceptions
+    /// thrown by the <paramref name="asyncFactory" />.
     /// </param>
     ...
-    /// <typeparam name="TOk">The factory method return value's type</typeparam>
-    /// <typeparam name="TErr">The error handler return value's type</typeparam>
+    /// <typeparam name="TOk">The factory method return value's type.</typeparam>
+    /// <typeparam name="TErr">The error handler return value's type.</typeparam>
```

Same buried-summary gap as the sync overload (verified at `Result.cs:175-178`).

### `Result.Try<TState, TOk, TErr>(state, factory, onError, …)`

`Result.cs:181-244` — buried-summary, missing-punctuation, prose-instead-of-tag

```diff
     /// <paramref name="state" /> and invoking <paramref name="onError" /> if the
-    /// factory throws an exception.
+    /// factory throws an exception or returns null.
     /// </summary>
     ...
     /// <param name="factory">
-    /// A method which when executed will return the value
-    /// contained in the <see cref="Result{TOk,TErr}" />
+    /// A method which when executed will return the value
+    /// contained in the <see cref="Result{TOk,TErr}" />.
     /// </param>
     /// <param name="onError">
-    /// A callback method that will be invoked for any exceptions
-    /// thrown by the <paramref name="factory" />
+    /// A callback method that will be invoked for any exceptions
+    /// thrown by the <paramref name="factory" />.
     /// </param>
     ...
-    /// <typeparam name="TOk">The factory method return value's type</typeparam>
-    /// <typeparam name="TErr">The error handler return value's type</typeparam>
+    /// <typeparam name="TOk">The factory method return value's type.</typeparam>
+    /// <typeparam name="TErr">The error handler return value's type.</typeparam>
```

```diff
     /// The <paramref name="state" /> is handed to the factory rather than
-    /// captured by it, so the factory can be <c>static</c> and the call
+    /// captured by it, so the factory can be <see langword="static" /> and the call
     /// allocates no closure. A
```

`ResultExtensions.cs:103-104` uses `<see langword="static" />` for the same word; `Result.cs` uses plain `<c>` for it in all four `TState` overloads, an avoidable inconsistency inside one partition.

### `Result.TryAsync<TState, TOk, TErr>(state, asyncFactory, onError, …)`

`Result.cs:276-340` — buried-summary, missing-punctuation, prose-instead-of-tag

```diff
     /// <paramref name="state" /> and invoking <paramref name="onError" /> if the
-    /// factory throws an exception.
+    /// factory throws an exception or returns null.
     /// </summary>
     ...
     /// <param name="asyncFactory">
-    /// An asynchronous method which when executed will
-    /// produce the value of the <see cref="Result{TOk,TErr}" />
+    /// An asynchronous method which when executed will
+    /// produce the value of the <see cref="Result{TOk,TErr}" />.
     /// </param>
     /// <param name="onError">
-    /// A callback method that will be invoked for any exceptions
-    /// thrown by the <paramref name="asyncFactory" />
+    /// A callback method that will be invoked for any exceptions
+    /// thrown by the <paramref name="asyncFactory" />.
     /// </param>
     ...
-    /// <typeparam name="TOk">The factory method return value's type</typeparam>
-    /// <typeparam name="TErr">The error handler return value's type</typeparam>
+    /// <typeparam name="TOk">The factory method return value's type.</typeparam>
+    /// <typeparam name="TErr">The error handler return value's type.</typeparam>
```

```diff
     /// The <paramref name="state" /> is handed to the factory rather than
-    /// captured by it, so the factory can be <c>static</c> and the call
+    /// captured by it, so the factory can be <see langword="static" /> and the call
     /// allocates no closure. A
```

### `Result.Ok<TOk, TErr>(value)`

`Result.cs:376-396` — missing-punctuation

```diff
-    /// <typeparam name="TOk">The ok result value's type</typeparam>
-    /// <typeparam name="TErr">The error result value's type</typeparam>
+    /// <typeparam name="TOk">The ok result value's type.</typeparam>
+    /// <typeparam name="TErr">The error result value's type.</typeparam>
```

### `Result.Err<TOk, TErr>(value)`

`Result.cs:398-418` — missing-punctuation

```diff
-    /// <typeparam name="TOk">The ok result value's type</typeparam>
-    /// <typeparam name="TErr">The error result value's type</typeparam>
+    /// <typeparam name="TOk">The ok result value's type.</typeparam>
+    /// <typeparam name="TErr">The error result value's type.</typeparam>
```

### `Result.Try<TOk>(factory, …)` (the `Error`-typed overload)

`Result.cs:420-472` — copy-paste-drift, buried-summary, missing-punctuation

```diff
     /// <summary>
     /// Tries to store the result of a <paramref name="factory" /> into a
     /// <see cref="Result{TOk,TErr}" /> which uses <see cref="Error" /> as its error
-    /// type, converting any thrown exception using
+    /// type, converting any thrown exception or a null return using
     /// <see cref="Error.FromException" />.
     /// </summary>
     /// <param name="factory">
-    /// A method which when executed will return the value
-    /// contained in the <see cref="Result{TOk,TErr}" />
+    /// A method which when executed will return the value
+    /// contained in the <see cref="Result{TOk,TErr}" />.
     /// </param>
     ...
-    /// <typeparam name="TOk">The factory method return value's type</typeparam>
+    /// <typeparam name="TOk">The factory method return value's type.</typeparam>
     /// <returns>
-    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
-    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
+    /// An <see cref="Ok{TOk,Error}" /> if the factory produces a non-null
+    /// value, otherwise an <see cref="Err{TOk,Error}" />.
     /// </returns>
     /// <remarks>
     /// <para>
     /// A factory that returns null also produces an
-    /// <see cref="Err{TOk,TErr}" />, carrying an <see cref="Error" /> converted
+    /// <see cref="Err{TOk,Error}" />, carrying an <see cref="Error" /> converted
     /// from an <see cref="ArgumentNullException" /> that was never thrown and so
```

This method has only one type parameter, `TOk` — there is no `TErr` in scope, because the error type is fixed to `Error`. The `returns` tag and the first `remarks` paragraph both cite `Ok{TOk,TErr}` / `Err{TOk,TErr}`, copied from the two-type-parameter sibling without updating the second slot to `Error`, which is what the method actually constructs (`Result.cs:467-472` calls `Try(factory, Error.FromException, …)`, and `Ok<TOk>`/`Err<TOk>` below both instantiate `Ok<TOk, Error>` / `Err<TOk, Error>`).

### `Result.TryAsync<TOk>(asyncFactory, …)` (the `Error`-typed overload)

`Result.cs:474-526` — copy-paste-drift, buried-summary, missing-punctuation

```diff
     /// <summary>
     /// Tries to store the result of an <paramref name="asyncFactory" /> into
     /// a <see cref="Result{TOk,TErr}" /> which uses <see cref="Error" /> as its error
-    /// type, converting any thrown exception using
+    /// type, converting any thrown exception or a null return using
     /// <see cref="Error.FromException" />.
     /// </summary>
     /// <param name="asyncFactory">
-    /// An asynchronous method which when executed will
-    /// produce the value of the <see cref="Result{TOk,TErr}" />
+    /// An asynchronous method which when executed will
+    /// produce the value of the <see cref="Result{TOk,TErr}" />.
     /// </param>
     ...
-    /// <typeparam name="TOk">The factory method return value's type</typeparam>
+    /// <typeparam name="TOk">The factory method return value's type.</typeparam>
     /// <returns>
-    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
-    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
+    /// An <see cref="Ok{TOk,Error}" /> if the factory produces a non-null
+    /// value, otherwise an <see cref="Err{TOk,Error}" />.
     /// </returns>
     /// <remarks>
     /// <para>
     /// A factory that returns null also produces an
-    /// <see cref="Err{TOk,TErr}" />, carrying an <see cref="Error" /> converted
+    /// <see cref="Err{TOk,Error}" />, carrying an <see cref="Error" /> converted
     /// from an <see cref="ArgumentNullException" /> that was never thrown and so
```

### `Result.Try<TState, TOk>(state, factory, …)` (the `Error`-typed overload)

`Result.cs:528-596` — copy-paste-drift, buried-summary, missing-punctuation, prose-instead-of-tag

```diff
     /// type, handing the factory the provided <paramref name="state" /> and
-    /// converting any thrown exception using <see cref="Error.FromException" />.
+    /// converting any thrown exception or a null return using
+    /// <see cref="Error.FromException" />.
     /// </summary>
     ...
     /// <param name="factory">
-    /// A method which when executed will return the value
-    /// contained in the <see cref="Result{TOk,TErr}" />
+    /// A method which when executed will return the value
+    /// contained in the <see cref="Result{TOk,TErr}" />.
     /// </param>
     ...
-    /// <typeparam name="TOk">The factory method return value's type</typeparam>
+    /// <typeparam name="TOk">The factory method return value's type.</typeparam>
     /// <returns>
-    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
-    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
+    /// An <see cref="Ok{TOk,Error}" /> if the factory produces a non-null
+    /// value, otherwise an <see cref="Err{TOk,Error}" />.
     /// </returns>
     /// <remarks>
     /// <para>
     /// The <paramref name="state" /> is handed to the factory rather than
-    /// captured by it, so the factory can be <c>static</c> and the call
+    /// captured by it, so the factory can be <see langword="static" /> and the call
     /// allocates no closure. A
     /// <see cref="System.Threading.CancellationToken" /> is the state this
     /// exists for.
     /// </para>
     /// <para>
     /// A factory that returns null also produces an
-    /// <see cref="Err{TOk,TErr}" />, carrying an <see cref="Error" /> converted
+    /// <see cref="Err{TOk,Error}" />, carrying an <see cref="Error" /> converted
     /// from an <see cref="ArgumentNullException" /> that was never thrown and so
```

### `Result.TryAsync<TState, TOk>(state, asyncFactory, …)` (the `Error`-typed overload)

`Result.cs:598-666` — copy-paste-drift, buried-summary, missing-punctuation, prose-instead-of-tag

```diff
     /// type, handing the factory the provided <paramref name="state" /> and
-    /// converting any thrown exception using <see cref="Error.FromException" />.
+    /// converting any thrown exception or a null return using
+    /// <see cref="Error.FromException" />.
     /// </summary>
     ...
     /// <param name="asyncFactory">
-    /// An asynchronous method which when executed will
-    /// produce the value of the <see cref="Result{TOk,TErr}" />
+    /// An asynchronous method which when executed will
+    /// produce the value of the <see cref="Result{TOk,TErr}" />.
     /// </param>
     ...
-    /// <typeparam name="TOk">The factory method return value's type</typeparam>
+    /// <typeparam name="TOk">The factory method return value's type.</typeparam>
     /// <returns>
-    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
-    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
+    /// An <see cref="Ok{TOk,Error}" /> if the factory produces a non-null
+    /// value, otherwise an <see cref="Err{TOk,Error}" />.
     /// </returns>
     /// <remarks>
     /// <para>
     /// The <paramref name="state" /> is handed to the factory rather than
-    /// captured by it, so the factory can be <c>static</c> and the call
+    /// captured by it, so the factory can be <see langword="static" /> and the call
     /// allocates no closure. A
     /// <see cref="System.Threading.CancellationToken" /> is the state this
     /// exists for.
     /// </para>
     /// <para>
     /// A factory that returns null also produces an
-    /// <see cref="Err{TOk,TErr}" />, carrying an <see cref="Error" /> converted
+    /// <see cref="Err{TOk,Error}" />, carrying an <see cref="Error" /> converted
     /// from an <see cref="ArgumentNullException" /> that was never thrown and so
```

### `Result.Ok<TOk>(value)` (the `Error`-typed overload)

`Result.cs:668-679` — copy-paste-drift, inconsistent-sibling, missing-punctuation

```diff
     /// <summary>
-    /// Creates an <see cref="Ok{TOk,TErr}" /> result containing the provided
+    /// Creates an <see cref="Ok{TOk,Error}" /> result containing the provided
     /// value, using <see cref="Error" /> as the error type.
     /// </summary>
     /// <param name="value">The success value the result will hold.</param>
-    /// <typeparam name="TOk">The ok result value's type</typeparam>
+    /// <typeparam name="TOk">The ok result value's type.</typeparam>
+    /// <returns>
+    /// A <see cref="Result{TOk,TErr}" /> that is always an
+    /// <see cref="Ok{TOk,Error}" />.
+    /// </returns>
     /// <exception cref="ArgumentNullException">
     /// <paramref name="value" /> is null.
     /// </exception>
```

The summary's own cref is drifted the same way as the `Try` overloads above — this method has no `TErr` type parameter, and the value returned is always `Ok<TOk, Error>` (`Result.cs:679`). It is also the only factory method in this file with no `<returns>` tag; its two-type-parameter sibling `Ok<TOk, TErr>` has one that states the static type is always `Result<TOk, TErr>` — useful because the case type and the static type differ.

### `Result.Err<TOk>(error)` (the `Error`-typed overload)

`Result.cs:681-692` — copy-paste-drift, inconsistent-sibling, missing-punctuation

```diff
     /// <summary>
-    /// Creates an <see cref="Err{TOk,TErr}" /> result containing the provided
+    /// Creates an <see cref="Err{TOk,Error}" /> result containing the provided
     /// <see cref="Error" />.
     /// </summary>
     /// <param name="error">The error contained in the result.</param>
-    /// <typeparam name="TOk">The ok result value's type</typeparam>
+    /// <typeparam name="TOk">The ok result value's type.</typeparam>
+    /// <returns>
+    /// A <see cref="Result{TOk,TErr}" /> that is always an
+    /// <see cref="Err{TOk,Error}" />.
+    /// </returns>
     /// <exception cref="ArgumentNullException">
     /// <paramref name="error" /> is null.
     /// </exception>
```

Same drift and the same missing `<returns>` tag as `Ok<TOk>` above, relative to its two-type-parameter sibling `Err<TOk, TErr>`.

### `Ok<TOk,TErr>.Deconstruct(out TOk value)`

`Ok.cs:41-60` — figurative (metaphor)

```diff
     /// What makes <c>result is Ok&lt;TOk, TErr&gt;(var value)</c> and an arm of
     /// <c>result switch { Ok&lt;TOk, TErr&gt;(var value) => …,
     /// Err&lt;TOk, TErr&gt;(var error) => … }</c> compile. A pattern over a
     /// result names both type arguments even though only one is bound, which is
-    /// the cost of the case types being generic in both.
+    /// what follows from the case types being generic in both, even though only
+    /// one type argument is bound.
```

Metaphor: "cost" borrows a financial-loss image for a plain consequence of how the case types are declared — there is no price being paid, just a fact about the type parameters. This member does not otherwise need a change; the rest of the doc holds up, so it moves out of Clean only for this line.

### `ResultExtensions.With<TState>(state)`

`ResultExtensions.cs:84-126` — figurative (metaphor, personification)

```diff
     /// <remarks>
     /// The members on the returned
-    /// <see cref="Result{TOk,TErr}.Bound{TState}" /> mirror the ones here,
+    /// <see cref="Result{TOk,TErr}.Bound{TState}" /> expose the same members as
+    /// the ones here,
     /// minus the state argument, and each returns the plain
-    /// <see cref="Result{TOk,TErr}" /> again. So the state is spent by the
-    /// call that uses it and a chain binds as many times as it needs to:
+    /// <see cref="Result{TOk,TErr}" /> again. Only the next call in the chain
+    /// receives the state; a chain calls <c>With</c> again each time it needs
+    /// to bind a new one:
     /// <code>
     /// result.With(logger)
     ///       .InspectErr(static (e, s) => s.LogError(e.Message))
     ///       .With(format)
     ///       .Map(static (v, s) => v.ToString(s));
     /// </code>
     /// <para>
     /// Pass a tuple to bind more than one value. Mark every lambda
     /// <see langword="static" />, which is what makes the compiler reject a
-    /// capture that creeps back in — binding the state achieves nothing on
+    /// capture that is reintroduced later — binding the state achieves nothing on
     /// its own if the delegate still reaches for an outer variable.
     /// </para>
     ...
     /// <returns>
     /// The result and <paramref name="state" /> together, carrying the
-    /// vocabulary that consumes both.
+    /// same members as <see cref="Result{TOk,TErr}" />, each now supplied with
+    /// <paramref name="state" />.
     /// </returns>
```

Three separate images in one member. "Mirror" personifies the returned type as reflecting this one — the literal fact is that it exposes the same members. "Spent" borrows a money image for "used once, by the next call" — the state is not consumed the way a coin is; the next call in the chain simply receives it and no later call does. "Creeps back in" personifies a variable capture as something that sneaks — the literal fact is that the compiler rejects any capture the delegate contains, however it got there. "Vocabulary" in the `<returns>` restates "members" with an unneeded metaphor. None of these are this codebase's own settled terminology the way "swallowed" or "untouched" are — each recurs only where this specific doc author reached for it, in this file and its siblings under `Result{TOk,TErr}.Bound{TState}` and `Option{T}.Bound{TState}` (outside this partition, not audited here, but worth the same pass if not already done).

### `ResultsCollectionExtensions.Flatten<TOk, TErr>(results)`

`ResultsCollectionExtensions.cs:12-35` — missing-punctuation

```diff
-    /// <typeparam name="TOk">The result's ok value type</typeparam>
-    /// <typeparam name="TErr">The result's error value type</typeparam>
+    /// <typeparam name="TOk">The result's ok value type.</typeparam>
+    /// <typeparam name="TErr">The result's error value type.</typeparam>
```

### `ResultsCollectionExtensions.FlattenErr<TOk, TErr>(results)`

`ResultsCollectionExtensions.cs:37-61` — missing-punctuation

```diff
-    /// <typeparam name="TOk">The result's ok value type</typeparam>
-    /// <typeparam name="TErr">The result's error value type</typeparam>
+    /// <typeparam name="TOk">The result's ok value type.</typeparam>
+    /// <typeparam name="TErr">The result's error value type.</typeparam>
```

### `ResultsCollectionExtensions.Collect<TOk, TErr>(results)`

`ResultsCollectionExtensions.cs:63-116` — figurative (hyperbole), non-consumer-content, missing-punctuation

```diff
     /// <remarks>
     /// The all-or-nothing counterpart to <see cref="Partition{TOk,TErr}" />, which
-    /// reports every failure and always succeeds. This is the port of Rust's
-    /// <c>collect::&lt;Result&lt;Vec&lt;T&gt;, E&gt;&gt;()</c> and short-circuits the
-    /// same way: enumeration stops at the first <see cref="Err{TOk,TErr}" />, so
+    /// reports every failure instead of stopping at the first one. Enumeration
+    /// stops at the first <see cref="Err{TOk,TErr}" />, so
     /// later elements are never visited, later errors are never seen, and a
     /// side-effecting source is left partly consumed. Reach for
     /// <see cref="Partition{TOk,TErr}" /> when the caller needs to report all of the
     /// failures rather than fail on one. Enumerates when it is called rather than
     /// when its result is read, so do not call it on an unbounded sequence.
     /// </remarks>
     /// <param name="results">The sequence to gather. Enumerated immediately.</param>
-    /// <typeparam name="TOk">The result's ok value type</typeparam>
-    /// <typeparam name="TErr">The result's error value type</typeparam>
+    /// <typeparam name="TOk">The result's ok value type.</typeparam>
+    /// <typeparam name="TErr">The result's error value type.</typeparam>
```

Three problems in the same paragraph. First, hyperbole: "always succeeds" asserts a success state for `Partition`, which returns a plain tuple, not a `Result` — there is no documented exception where it "fails" because there is no success/failure outcome defined for it at all; the literal claim is only that it never stops early, i.e. it processes every element regardless of how many are `Err<TOk,TErr>`. Second, the Rust `collect::<Result<Vec<T>, E>>()` cross-reference is library history/rationale, not something a caller needs to call `Collect` correctly — `CollectAsync` right below documents the identical short-circuit behavior without it. Third, missing-punctuation on the two `typeparam` tags.

### `ResultsCollectionExtensions.CollectAsync<TOk, TErr>(results, cancellationToken)`

`ResultsCollectionExtensions.cs:118-177` — missing-punctuation

```diff
-    /// <typeparam name="TOk">The result's ok value type</typeparam>
-    /// <typeparam name="TErr">The result's error value type</typeparam>
+    /// <typeparam name="TOk">The result's ok value type.</typeparam>
+    /// <typeparam name="TErr">The result's error value type.</typeparam>
```

### `ResultsCollectionExtensions.Partition<TOk, TErr>(results)`

`ResultsCollectionExtensions.cs:179-208` — missing-punctuation

```diff
-    /// <typeparam name="TOk">The result's ok value type</typeparam>
-    /// <typeparam name="TErr">The result's error value type</typeparam>
+    /// <typeparam name="TOk">The result's ok value type.</typeparam>
+    /// <typeparam name="TErr">The result's error value type.</typeparam>
```

### Clean

- `ResultExtensions.Flatten()` (on `Result<Result<TOk,TErr>, TErr>`)
- `ResultExtensions.Transpose()` (on `Result<Option<TOk>, TErr>`)
- `ResultExtensions.UnwrapOrNull()` (on `Result<TOk, TErr>` where `TOk : struct`)
- `Err<TOk,TErr>.Deconstruct(out TErr error)` — hand-written, not `<inheritdoc />`; checked against the base members it cross-references (`GetErr`, `UnwrapErr`) and against the internal `Value` property it says is unreachable directly, including the contrast drawn with `None{T}` — all correct, and no figurative language in it.
- `IsOk`, `IsErr` on both `Ok<TOk,TErr>` and `Err<TOk,TErr>`
- Every `<inheritdoc />` override on `Ok<TOk,TErr>` and `Err<TOk,TErr>` (`IsOkAnd*`, `IsErrAnd*`, `Match*`, `MatchAsync*`, `And`, `AndThen*`, `Or`, `OrElse*`, `Expect`, `ExpectErr`, `Unwrap*`, `UnwrapOr*`, `UnwrapErr`, `Inspect*`, `InspectErr*`, `Map*`, `MapOr*`, `MapOrDefault*`, `MapOrNull*`, `MapOrElse*`, `MapErr*`, `AsEnumerable`, `GetOk`, `GetErr`) — the inherited text describes the base member, and each override's behavior (traced through every branch) matches what that text promises for its case.
- The `never`/`always`/`every` claims checked across `Result.cs` (e.g. "unchanged and is never inspected" on every `state` param, "that was never thrown" on the synthetic `ArgumentNullException`, "always an `Ok{TOk,TErr}`"/"always an `Err{TOk,TErr}`" on the two-type-parameter factories), `Ok.cs` and `Err.cs` (the value/error "is never null"), and `ResultsCollectionExtensions.cs` (`Flatten`'s "every `Ok{TOk,TErr}`", `Collect`'s "later elements are never visited, later errors are never seen") — every one of these verified true against the code with no documented exception, so they are accurate absolutes, not hyperbole. The one exception, `Collect`'s "always succeeds" describing `Partition`, is written up above.

### Unverified

None remain. The one open item from the previous revision — whether `<see cref="Ok{TOk,Error}" />`, a generic cref mixing a real type-parameter placeholder (`TOk`) with a closed type argument (`Error`), resolves the way `<see cref="Ok{TOk,TErr}" />` does today — is resolved above under `Result.Try<TOk>` rather than left open: `grep -rnE 'cref="[A-Za-z_]+\{[^},"]+,[A-Za-z_.]+\}"' src/Waystone.Monads`, after excluding the ordinary `{TOk,TErr}`-shaped hits, and a narrower search for `cref="*{*Error}"`, both return nothing out of 810 generic crefs in the package — every one of them uses type-parameter placeholders on both sides, never a closed type argument. So the mixed form has no precedent here either way; nothing proves it works, and nothing proves it fails. Verify it by building `Waystone.Monads` after applying the fix — `src/**` has `TreatWarningsAsErrors`, so an unresolved cref fails the build on `CS1574` rather than shipping a dead link silently.

## Configuration, diagnostics, errors and exceptions

Scope: `Configs/` (7 files), `Diagnostics/` (6 files), `Results/Errors/` (4 files),
`Exceptions/` (2 files) — 19 files, approximately 66 public members examined.

### MonadOptions.Configure(Action\<MonadOptionsBuilder\>)

src/Waystone.Monads/Configs/MonadOptions.cs:117 — restated-signature, missing-punctuation

```diff
     /// <param name="configure">
-    /// The action that will configure the
-    /// <see cref="MonadOptionsBuilder" />
+    /// Applied to a builder seeded from the options already in effect. Only the
+    /// properties it sets change; anything it leaves untouched is carried
+    /// forward.
     /// </param>
```

The old text names the parameter's type rather than what happens when it runs, and never terminates the sentence with a period.

### MonadOptions.BeginScope(Action\<MonadOptionsBuilder\>)

src/Waystone.Monads/Configs/MonadOptions.cs:150 — restated-signature, missing-punctuation, copy-paste-drift

```diff
     /// <param name="configure">
-    /// The action that will configure the scoped
-    /// <see cref="MonadOptionsBuilder" />
+    /// Applied to a builder seeded from the options in effect when the scope
+    /// opens. Only the properties it sets change for the scope; anything it
+    /// leaves untouched is inherited.
     /// </param>
```

Same defect as `Configure`'s `configure` parameter, carried over by copy-paste; both need the fix.

### MonadOptionsBuilder.UseCancellationAsFailure(bool)

src/Waystone.Monads/Configs/MonadOptionsBuilder.cs:78 — non-consumer-content, throat-clearing, figurative (personification)

```diff
     /// <remarks>
     /// By default an <see cref="OperationCanceledException" /> is not caught,
     /// so it leaves <c>Try</c> and <c>TryAsync</c> untouched and is neither
     /// logged nor converted. Call this and a cancellation instead produces a
     /// <see cref="Options.None{T}" /> or an <see cref="Results.Err{TOk,TErr}" />
-    /// like any other exception, which is what versions before 6.0.0 did.
-    /// Prefer the default: a cancelled operation produced no answer, and
-    /// reporting that as an absent or failed value hides the cancellation from
-    /// the caller that requested it.
+    /// like any other exception.
     /// <see cref="System.Threading.Tasks.TaskCanceledException" /> derives from
     /// <see cref="OperationCanceledException" /> and is covered by this option
     /// too.
     /// <para>
     /// Pass <c>false</c> to put the setting back. A builder seeded from options
     /// configured elsewhere — by <see cref="MonadOptions.Configure" />, or by an
     /// earlier registration in a container — carries that decision forward, and
     /// passing <c>false</c> is the only way to reverse it.
     /// </para>
     /// </remarks>
     /// <param name="catchesCancellation">
     /// If true, a cancellation is caught and becomes a
     /// <see cref="Options.None{T}" /> or an <see cref="Results.Err{TOk,TErr}" />.
-    /// If false, it propagates to the caller that requested it. Default: true,
-    /// since turning the behaviour on is why you would call this. Absent any
-    /// call at all the setting is false.
+    /// If false, it propagates to the caller that requested it. Default: true.
+    /// The setting is false if this method is never called.
     /// </param>
```

"which versions before 6.0.0 did" is library history outside a deprecation notice, and the cut sentence both argues a case rather than stating a fact and personifies the consequence ("hides the cancellation from the caller," as if the value itself conceals something); the `<param>` rationale ("since turning the behaviour on is why you would call this") is the same kind of aside.

### MonadOptionsBuilder.UseErrorCodeFactory(ErrorCodeFactory)

src/Waystone.Monads/Configs/MonadOptionsBuilder.cs:120 — restated-signature

```diff
     /// <param name="factory">
-    /// The implementation of <see cref="ErrorCodeFactory" /> you
-    /// want the library to use.
+    /// Every <see cref="ErrorCode" /> built from an exception after this call is
+    /// produced by this factory instead of the default one.
     /// </param>
```

The old text names the parameter's type; the new text says what changes as a result of calling this.

### MonadOptionsBuilder.UseFallbackErrorCode(string)

src/Waystone.Monads/Configs/MonadOptionsBuilder.cs:139 — restated-signature, missing-punctuation

```diff
-    /// <param name="errorCode">The fallback error code to use</param>
+    /// <param name="errorCode">
+    /// The value <see cref="ErrorCode" /> falls back to when a null, empty or
+    /// whitespace code is supplied.
+    /// </param>
```

### MonadOptionsBuilder.UseFallbackErrorMessage(string)

src/Waystone.Monads/Configs/MonadOptionsBuilder.cs:166 — restated-signature, missing-punctuation

```diff
-    /// <param name="errorMessage">The fallback error message to use</param>
+    /// <param name="errorMessage">
+    /// The value <see cref="Error" /> falls back to when a null, empty or
+    /// whitespace message is supplied.
+    /// </param>
```

### MonadOptionsScope.Dispose()

src/Waystone.Monads/Configs/MonadOptionsScope.cs:32, :51 — buried-summary, figurative (personification)

```diff
-    /// <summary>Restores the options that were in effect before this scope.</summary>
+    /// <summary>
+    /// Restores the options that were in effect before this scope, if this
+    /// scope is still the innermost live one.
+    /// </summary>
```

Doc generators index the summary alone, and the unqualified claim reads as unconditional; the three-way decline-vs-restore behaviour that follows in `<remarks>` is exactly the case a reader skimming only the summary would miss.

```diff
     /// <para>
-    /// Safe to call more than once, and quiet about it on the path that restored:
-    /// a second call finds the options it already put back and returns without
-    /// writing anything, so an explicit <c>Dispose</c> inside a <c>using</c> is
-    /// never reported as misuse. A scope that already declined has no such
+    /// Safe to call more than once: on the path that restored, a second call
+    /// writes nothing. It finds the options it already put back and returns
+    /// without writing anything, so an explicit <c>Dispose</c> inside a
+    /// <c>using</c> is never reported as misuse. A scope that already declined has no such
     /// guarantee — a <c>readonly struct</c> cannot record that it reported, so
     /// every further <c>Dispose</c> writes the event again. Deduplicate in the
     /// subscriber if that matters; the events are identical.
     /// </para>
```

"Quiet about it" personifies the restore path as choosing silence; the literal fact is that the second call writes nothing.

### ErrorCodeFactory.FromException(Exception)

src/Waystone.Monads/Configs/ErrorCodeFactory.cs:27, :39 — inconsistent-sibling, prose-instead-of-tag

```diff
     /// <summary>
-    /// Creates a new instance of <see cref="ErrorCode" /> from an Exception
-    /// value.
+    /// Creates a new instance of <see cref="ErrorCode" /> from an
+    /// <see cref="Exception" /> value.
     /// </summary>
```
```diff
-    /// <param name="exception">The exception value to convert into an Error Code.</param>
+    /// <param name="exception">The exception value to convert into an <see cref="ErrorCode" />.</param>
```

`Error.FromException` and `ErrorCode.FromException` both write "an exception" (lowercase, generic word); this one capitalizes "Exception" as if naming the type but never tags it, and "Error Code" in the `<param>` has the same problem.

### MonadDiagnostics (class)

src/Waystone.Monads/Diagnostics/MonadDiagnostics.cs:20 — figurative (metaphor)

```diff
     /// <para>
     /// Emission is gated on whether anything is listening, so an unobserved process
-    /// pays for a pair of boolean checks and allocates nothing.
+    /// performs a pair of boolean checks and allocates nothing.
     /// </para>
```

"Pays for" borrows a financial image for a CPU cost; the literal fact is simpler than the metaphor.

### MonadDiagnostics.ConfigurationNotAppliedEventName

src/Waystone.Monads/Diagnostics/MonadDiagnostics.cs:58 — figurative (personification, metaphor)

```diff
     /// <summary>The name of the event written when options are read before container-registered configuration reaches the library.</summary>
     /// <remarks>
     /// Its payload is a <see cref="ConfigurationNotApplied" />. Only a package that
-    /// configures the library through a container arms this event, so a process
+    /// configures the library through a container can write this event, so a process
     /// that configures itself through <see cref="MonadOptions.Configure" /> alone
     /// never sees it.
     /// <para>
     /// Configuring through a container splits registration from application: the
     /// settings are described when the container is built and installed later. A
     /// read in between is answered from the bootstrap options, which are valid
     /// settings rather than a broken state — so this reports a wiring omission, not
-    /// a failure, and nothing throws. Left unfixed it is silent, which is what the
-    /// event is for.
+    /// a failure, and nothing throws. Left unfixed, nothing else reports it, which
+    /// is what this event is for.
     /// </para>
     /// <para>
     /// Written only while something is subscribed: with no listener attached the
-    /// signal is held rather than spent, so a subscriber attached at any point
-    /// before the configuration lands still receives it. Configuration arriving by
-    /// any route disarms the event, whether or not it was ever written.
+    /// no event is written and the pending state is left unchanged, so a
+    /// subscriber attached at any point before the configuration lands still
+    /// receives it on a later read. Configuration arriving by any route clears
+    /// the pending state, whether or not the event was ever written.
     /// </para>
     /// <para>
     /// It reports reads that go through the options the monads themselves consult,
     /// which is what an early read in a container-configured application does. A
     /// satellite package that reaches past that for the global snapshot is not
     /// instrumented and will not raise this. Two threads reading at the same moment
-    /// can each write the event before either disarms it, so deduplicate in the
+    /// can each write the event before either clears the pending state, so deduplicate in the
     /// subscriber if that matters — the payloads are identical.
     /// </para>
```

"Arms"/"disarms" reach for a weapon image, and "the signal is held rather than spent" reaches for a money image, for what is literally a pending flag that clears once observed; "it is silent" personifies the unfixed state as choosing not to speak.

### MonadDiagnostics.ConfigurationNotAppliedEvent

src/Waystone.Monads/Diagnostics/MonadDiagnostics.cs:113 — figurative (metaphor), copy-paste-drift

```diff
     /// <summary>Gets the event written when options are read before container-registered configuration reaches the library, ready to subscribe to.</summary>
     /// <remarks>
     /// Pairs <see cref="ConfigurationNotAppliedEventName" /> with the payload it
-    /// carries. The signal is held while nothing is subscribed rather than spent,
-    /// so a subscription made at any point before the configuration lands still
-    /// receives it — subscribing late costs nothing here, unlike the other two
-    /// events.
+    /// carries. No event is written while nothing is subscribed, and the pending
+    /// state is left unchanged, so a subscription made at any point before the
+    /// configuration lands still receives it on a later read — unlike the other
+    /// two events, where a subscription made after the write already happened
+    /// misses it.
     /// </remarks>
```

Same held/spent and cost imagery as the const's remarks, carried over by copy-paste.

### MonadDiagnosticEvent\<TPayload\> (class)

src/Waystone.Monads/Diagnostics/MonadDiagnosticEvent.cs:13 — figurative (personification)

```diff
     /// <see cref="object" />. Getting any of the three wrong fails silently — no
-    /// exception, no warning, an empty dashboard. This type carries all three
+    /// Getting any of the three wrong produces no exception, no warning, and an
+    /// empty dashboard. This type carries all three
     /// together, so <see cref="Subscribe" /> cannot be pointed at the wrong event or
     /// handed the wrong payload type.
```

"Fails silently" personifies the failure as choosing not to speak; the three concrete consequences that already follow it say the same thing literally.

### ScopeDisposedOutOfOrder

src/Waystone.Monads/Diagnostics/ScopeDisposedOutOfOrder.cs:17 — figurative (personification)

```diff
     /// <see cref="MonadOptionsScope" /> was disposed while a scope begun after
     /// it was still live. The library leaves the live scope alone in that case, so
-    /// nothing is silently reconfigured — but disposing the live scope later restores
-    /// <em>its</em> predecessor, which is the options the early-disposed scope
-    /// installed, so those options outlive the scope that installed them until the
-    /// flow unwinds.
+    /// the live scope's options do not change at that moment — but disposing the
+    /// live scope later restores <em>its</em> predecessor, which is the options
+    /// the early-disposed scope installed, so those options outlive the scope
+    /// that installed them until the flow unwinds.
```

"Silently reconfigured" personifies the (non-)event; the literal fact is that nothing changes at that moment.

### MonadKind

src/Waystone.Monads/Diagnostics/MonadKind.cs:5 — figurative (personification)

```diff
     /// <remarks>
     /// Reported alongside every handled exception because the two cases lose
     /// different amounts of information. An exception caught by
-    /// <c>Option.Try</c> is discarded and survives only in this signal, whereas one
-    /// caught by <c>Result.Try</c> is also converted into the resulting
-    /// <c>Err</c> and is still available to the caller.
+    /// <c>Option.Try</c> is discarded, and this event is the only remaining
+    /// record of it, whereas one caught by <c>Result.Try</c> is also converted
+    /// into the resulting <c>Err</c> and is still available to the caller.
     /// </remarks>
```

"Survives" attributes a life-or-death quality to information that was simply discarded.

### Error.FromException(Exception)

src/Waystone.Monads/Results/Errors/Error.cs:63 — stale, inconsistent-sibling

```diff
     /// <remarks>
-    /// The code is the exception's type name with a trailing <c>Exception</c>
-    /// removed, from the <see cref="ErrorCodeFactory" /> configured in
-    /// <see cref="MonadOptions" />. The message is
-    /// <see cref="Exception.Message" /> verbatim, so anything the exception's text
-    /// exposes reaches whoever reads the error.
+    /// The code comes from the <see cref="ErrorCodeFactory" /> configured in
+    /// <see cref="MonadOptions" />; by default that is the exception's type name
+    /// with a trailing <c>Exception</c> removed, but a custom factory can
+    /// produce anything. The message is <see cref="Exception.Message" /> with
+    /// surrounding whitespace trimmed, so anything the exception's text exposes
+    /// reaches whoever reads the error.
     /// </remarks>
```

Two problems: "verbatim" is wrong — the constructor this factory calls trims surrounding whitespace off the message like any other `Error`, and the doc contradicts the constructor's own remarks a few lines above it. And unlike its sibling `ErrorCode.FromException`, this one omits that a custom factory changes the code, stating the default scheme as if it were unconditional.

### ErrorCode(string) constructor

src/Waystone.Monads/Results/Errors/ErrorCode.cs:26 — missing-punctuation, restated-signature

```diff
-    /// <param name="value">The error code string value</param>
+    /// <param name="value">
+    /// The value <see cref="Value" /> takes after trimming, or falls back from
+    /// when null, empty or whitespace.
+    /// </param>
```

### ErrorCode.FromException(Exception)

src/Waystone.Monads/Results/Errors/ErrorCode.cs:42 — figurative (personification)

```diff
     /// <remarks>
     /// Prefer an <c>[ErrorCodeCatalog]</c> enum. The code here is the exception's
     /// type name with a trailing <c>Exception</c> removed —
     /// <see cref="InvalidOperationException" /> gives <c>InvalidOperation</c> — so
-    /// renaming or swapping the exception type silently changes the code a consumer
-    /// observes. <see cref="Exception" /> itself is left as <c>Exception</c>. Uses
-    /// the <see cref="ErrorCodeFactory" /> configured in
-    /// <see cref="MonadOptions" />, so a custom factory changes all of this.
+    /// renaming or swapping the exception type changes the code a consumer
+    /// observes, with nothing in the build to catch it. <see cref="Exception" />
+    /// itself is left as <c>Exception</c>. Uses the <see cref="ErrorCodeFactory" />
+    /// configured in <see cref="MonadOptions" />, so a custom factory changes all
+    /// of this.
     /// </remarks>
```

"Silently changes" personifies the change as choosing not to announce itself; the literal fact is that nothing in the build catches it.

### ExceptionHandled

src/Waystone.Monads/Diagnostics/ExceptionHandled.cs:23 — figurative (metaphor)

```diff
     /// <para>
     /// A subscriber runs synchronously on the thread that threw, still inside the
     /// <c>catch</c>, and before the caller receives its <c>None</c> or <c>Err</c>.
     /// Slow work in a subscriber delays that caller, and an exception thrown from one
-    /// propagates out of the <c>Try</c> that was meant to swallow the original. Hand
-    /// off anything expensive.
+    /// propagates out of the <c>Try</c> that was meant to swallow the original. Move
+    /// expensive work to another thread instead of running it in the callback.
     /// </para>
```

"Hand off" borrows a relay-race image for moving work between threads; the replacement names the thread and the action instead of the image.

### Kept as field vocabulary, not authored imagery

Checked against the same figurative-language rule and left alone, with why:

- **"outlive"/"outlives its scope"** (`MonadOptionsScope.cs`, `MonadOptionsBuilder.cs`, `ScopeDisposedOutOfOrder.cs`) — standard object-lifetime terminology; no plainer equivalent exists for "exists past the end of its owning scope."
- **"racing a Configure call"** (`MonadOptions.cs`) — "race"/"racing" is the field's own name for the concurrency hazard being described, not an image the author reached for.
- **"neither loses its changes"** (`MonadOptions.cs`, `Configure` remarks) — invokes the standard "lost update" concurrency term; there is no more literal phrase for that guarantee.
- **"declined"/"decline"** (`MonadOptionsScope.Dispose` remarks) — the codebase's own established term of art for "returns without restoring," used pervasively including in this area's `AGENTS.md`; replacing it here would fight the project's own vocabulary rather than an author's one-off image.
- **"reaches"/"reaches past"** (`ErrorCodeFactory.cs`, `MonadDiagnostics.cs`, `ErrorCodeCatalogAttribute.cs`, `Error.cs`) — ordinary technical description of data/config flow ("the value reaches the caller"), as dead as "value flows through"; no figurative work is being done.
- **"sole hook"** (`ExceptionHandled.cs`) — "hook" is the standard term for an extension/callback point (event hook), not an image.
- **"held"** as in "the value the result actually held" (`UnwrapException.cs`, `UnmetExpectationException.cs`) — the container-holds-a-value idiom, explicitly exempted by the rule alongside "a lock is held."
- **"swallow"/"swallowed"** (`ExceptionHandled.cs`, `MonadDiagnostics.cs`, `UnwrapException.cs` doc): "the exception the library swallowed" — the field's standard term for an exception caught and not rethrown.
- **"leaks"** (`MonadDiagnosticEvent.cs`, `Subscribe` remarks): "abandoning the subscription without disposing it leaks one observer" — the field's standard term for an unreleased resource.
- **"fires"/"written"** and **"a listener subscribes"** (`MonadDiagnostics.cs`, `MonadDiagnosticEvent.cs`) — standard event-system vocabulary.
- **"faulted"** is not used in this partition's files, but would be kept as the field's term for `Task` status if it were.

### Clean

MonadOptions (class)
MonadOptionsBuilder (class)
MonadOptionsScope (struct, summary/remarks)
CallerInfo — confirmed against `Options/Option.cs` and `Results/Result.cs`: every `Try`/`TryAsync` overload declares `callerMemberName`/`callerLineNumber`/`callerArgumentExpression` as `[CallerMemberName]`/`[CallerLineNumber]`/`[CallerArgumentExpression]` optional parameters, constructs a `CallerInfo` from them in the `catch`, and passes it to `MonadOptions.Current.Log`, which reaches `MonadDiagnostics.RecordExceptionHandled` and the `ExceptionHandledEvent`. Since these are ordinary optional parameters, a caller passing explicit arguments would override the compiler-supplied values, which is exactly what "do not supply these values yourself" warns against.
MonadOptionsSlot — internal, not audited
ISatelliteBuilder — internal, not audited
MonadDiagnostics.MeterName, ListenerName, ExceptionHandledEventName, ScopeDisposedOutOfOrderEventName, ExceptionHandledEvent, ScopeDisposedOutOfOrderEvent, ExceptionsHandledInstrumentName, ErrorTypeTagKey, MonadTagKey, OptionMonadTagValue, ResultMonadTagValue
MonadDiagnosticEvent.Name, MonadDiagnosticEvent.Subscribe
ConfigurationNotApplied
Error (record, constructor, Code, Message, ToString)
ErrorCode (record, Value, both implicit operators, ToString)
ErrorCodeCatalogAttribute (class and Format) — verified against `ErrorCodeCatalogGenerator`/`ErrorCodeCatalogWriter`/`Rules.cs` in `Waystone.Monads.SourceGenerators`
ErrorCodeFormatAttribute (class, constructor, Format) — verified against `ErrorCodeFormat.cs`/`Casing`
UnwrapException / UnwrapException\<T\> (class, Value) — confirmed against `Options/OptionOfT.cs:354` (`<exception cref="UnwrapException">` on `Unwrap`, thrown for `None<T>`, matches the base type since there is no value to carry) and `Results/ResultOfTOkTErr.cs:587,678` (`Unwrap` on `Err` and `UnwrapErr` on `Ok`, matching `UnwrapException<T>`'s carried value)
UnmetExpectationException — confirmed against `Options/OptionOfT.cs:336` (`Expect` on `None<T>`) and `Results/ResultOfTOkTErr.cs:555,569` (`Expect` on `Err`, `ExpectErr` on `Ok`)

### Unverified

None remaining. Both items previously listed here are now confirmed above by reading `Options/Option.cs`, `Options/OptionOfT.cs`, `Results/Result.cs` and `Results/ResultOfTOkTErr.cs`, which sit outside this partition's edit scope but not outside what I could read to check a claim.

