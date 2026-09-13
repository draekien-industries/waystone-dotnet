# `ListSchema` and `DictionarySchema`

Read before editing anything under `Internal/Structures/` in
`src/Waystone.Monads.Schemas`, or the `MinCount`/`MaxCount` extensions.

They are the only nodes that run an inner schema more than once, and the only callers of
`ParseContext.AtIndex` and `ParseContext.AtKey` — so they are the only producers of the
indexed and keyed segments `ViolationPath` carries.

**The accumulation lives in `Entries<T>` and `Pairs<TKey, TValue>`, not in the schemas.**
Both nodes hand-roll the synchronous and asynchronous paths — as `AllSchema` and `AnySchema`
do, and for the same reason `DecoratorSchema` cannot help here — so the "gather violations,
keep the value only if every entry produced one" rule would otherwise be written four times.
It is written twice, in the accumulators, and each schema's two paths differ only in the
`await`.

**The cap is `Caps.Report`, and neither accumulator tracks it.** Both hold one and delegate,
because a list and a dictionary differ in what an entry *is* and not in how a full report is
decided — and the decision has two halves that read alike and are not: `IsFull` is the signal
to stop examining entries, while `Truncated` asks whether anything was actually lost. A report
that fills the last slot exactly has lost nothing, so conflating them appends "there are more"
to a report listing every problem there was, and turns an outcome that could still be refined
into a failure. The sync/async duplication above does not extend to this: cap tracking has no
`await` in it.

**Both accumulators are classes, deliberately.** A struct does not work: the asynchronous path
mutates the accumulator across an `await`, and a `ref` local cannot cross one. This is the
same constraint that rules out a shared `CombinatorSchema` for `All` and `Any`.

**A null entry is reported as `incomplete` at its own path and never reaches the inner
schema.** That is what lets every item schema assume its input is there, so
`Schema.List(Schema.Text.NotEmpty())` needs no null guard of its own. `Required` and
`Optional` do the same job one level up, and this is the same promise applied per entry. Do
not remove it in the name of consistency with `Parse`, which does not guard its argument — a
caller passing null to `Parse` made a programming error, while a null *inside* a list is
untrusted input arriving exactly as expected from JSON.

**A dictionary reports a key failure and a value failure at the same path**, built from the
*incoming* key's text. That is the only spelling a caller can match against what they sent,
and splitting them into `rates["AUD"].key` would invent a segment that does not exist in the
input. The message distinguishes them.

**Two keys parsing to the same output key is a `Duplicate` and produces no dictionary.**
Keeping either one would silently drop data the caller sent. This is the first producer of
`ViolationCode.Duplicate`, and it honours that code's promise to report against the later
entry.

**`Pairs.Take` does `ContainsKey` then `Add`, and that stays until netstandard2.0 goes.**
`Dictionary<TKey, TValue>.TryAdd` would make it one hash lookup instead of two, and it does
not exist on netstandard2.0. Wrapping it in `#if NET8_0_OR_GREATER` buys one avoided lookup
per entry at the cost of a second code path through the only place duplicate keys are
detected. Not worth it; when the target framework list loses netstandard2.0, take `TryAdd`
unguarded.

## `MinCount` and `MaxCount` are two overloads apiece and cannot be one

Both ways of writing the single rule fail, for different reasons.

Writing it against the common base — `this Schema<TIn, IReadOnlyCollection<T>>` — fails
because `Schema<TIn, TOut>` is *invariant* in `TOut`. A receiver typed
`Schema<TIn, IReadOnlyList<T>>` is not a `Schema<TIn, IReadOnlyCollection<T>>`, so the method
is never a candidate.

Writing it generically over the collection — `this Schema<TIn, TColl>` with
`TColl : IReadOnlyCollection<TElement>` — compiles, and then cannot be called fluently.
`TElement` appears only in a constraint, never in a parameter, and C# does not infer a type
argument from a constraint. Every call site would have to spell all three out, which is worse
than the overload it replaced.
