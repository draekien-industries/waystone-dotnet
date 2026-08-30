# Serializing with System.Text.Json

`Waystone.Monads.SystemTextJson` registers converters for both monads. Call it
once, while the options are still being built — `System.Text.Json` freezes a
`JsonSerializerOptions` the first time it serializes, and adding a converter
afterwards throws:

```csharp
JsonSerializerOptions options = new();
options.AddMonadConverters();
```

`AddMonadConverters` sits in `System.Text.Json`; the converters themselves sit in
`System.Text.Json.Serialization`, beside the `JsonConverter<T>` they follow.

## The two wire formats

`Option<T>` writes the value itself, or `null`. That is serde's format, and it
means adopting `Option<T>` in a model does not change the contract on the wire —
the payload is what a plain `T` would have written.

`Result<TOk, TErr>` has no idiomatic shape to borrow, since both cases carry
ordinary values of different types, so the case is named on the wire and the
payload nested beneath it:

```jsonc
{ "$type": "ok",  "value": 42 }
{ "$type": "err", "value": { "Code": "validation.failed", "Message": "…" } }
```

**The payload is nested rather than flattened because `$type` is also the
polymorphism discriminator.** A `TOk` carrying `[JsonDerivedType]` writes a
`$type` of its own; nesting puts it a level below the result's instead of making
the two siblings.

`$type`, `value`, `ok` and `err` are fixed. `PropertyNamingPolicy` does not
rename them, so a camel-casing service and a snake-casing one exchange the same
payload — and so does `Waystone.Monads.NewtonsoftJson`, which writes the
identical format. Property order does not matter on the way back in.

Reading throws `JsonException` when the payload is not an object, when `$type` is
missing, is not a string, or names neither case, or when `value` is missing. A
result has no null case, so accepting one would move the failure somewhere later
and harder to trace. A null `value` is not rejected on sight, though: it is read
as the case's own type first, so `Result<Option<T>, TErr>` round-trips with
`Ok(None)` written as `"value": null`.

## Three traps in a model

**Initialise every `Option<T>` member.** An absent property never invokes the
converter, so the member keeps its CLR default — which is `null`, not `None<T>()`.
A model that skips the initialiser holds a null where it promised an option.

```csharp
public sealed record Person
{
    public Option<string> Nickname { get; init; } = Option.None<string>();
}
```

**A `None` writes the property as null rather than omitting it,** because a
converter cannot delete its own property. `JsonIgnoreCondition.WhenWritingNull`
does not help — it tests the CLR value, and `Option.None<T>()` is an object like
any other. Omitting the property takes a type-info modifier setting
`ShouldSerialize` per property, which the package deliberately does not ship:
that is a decision about your wire contract, not about `Option<T>`.

**`Option<Option<T>>` does not survive a round trip.** `Some(None)` and `None`
both write `null` and both read back as `None`. The converter accepts that rather
than throwing, because `WM2009` already reports the declaration, which is the
right place to catch it.

## Under NativeAOT, register value-type monads by hand

Both factories close their converter reflectively, which throws under `PublishAot`
when a type argument is a value type — the compiler cannot see through
`MakeGenericType` to compile that instantiation. Reference types all share one
compiled converter and are unaffected, so a model of `Option<string>` and
`Result<Uri, Error>` needs nothing extra. For a value type, register the concrete
converter, which reflects not at all:

```csharp
options.Converters.Add(new OptionJsonConverter<int>());
options.Converters.Add(new ResultJsonConverter<int, string>());
```

For a `Result`, one value-type argument out of the two is enough to require this.
Monad members also get no source-generation fast path from a
`JsonSerializerContext` — a factory-produced converter works from one, but only
correctness is guaranteed, not the speed.
