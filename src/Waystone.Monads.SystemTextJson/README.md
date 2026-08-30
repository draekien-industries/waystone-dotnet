# Waystone.Monads.SystemTextJson

An interop package for using System.Text.Json with Waystone.Monads

## Namespaces

The package shadows `System.Text.Json`'s own namespaces, so its types sit where
a consumer already looks for them rather than under a parallel `Waystone` tree:

| Member | Namespace |
| --- | --- |
| `AddMonadConverters` | `System.Text.Json` |
| `OptionJsonConverter<T>`, `OptionJsonConverterFactory` | `System.Text.Json.Serialization` |
| `ResultJsonConverter<TOk, TErr>`, `ResultJsonConverterFactory` | `System.Text.Json.Serialization` |

The converters follow `JsonConverter<T>` down into
`System.Text.Json.Serialization`; the extension method sits beside the
`JsonSerializerOptions` it extends.

## Supported System.Text.Json versions

`System.Text.Json >= 8.0.5 && < 11.0.0`. Bring your own version inside that
range; the package does not pin you to one. Every version in the range ships a
`netstandard2.0` and a `net462` asset, so .NET Framework consumers are covered.

## Registration

```csharp
JsonSerializerOptions options = new();
options.AddMonadConverters();

string json = JsonSerializer.Serialize(model, options);
```

Call it once, while the options are still being built. `System.Text.Json`
freezes a `JsonSerializerOptions` the first time it serializes with it, and
adding a converter afterwards throws.

## Option

`Option<T>` serializes as the value itself, or as `null`. This is the format
Rust's serde uses, and it means a payload is the payload you would have written
had the property been a plain `T` — adopting `Option<T>` in a model does not
change the contract on the wire.

```csharp
public sealed record Person
{
    public Option<string> Nickname { get; init; } = Option.None<string>();
}
```

```jsonc
{ "nickname": "Ally" }   // Option.Some("Ally")
{ "nickname": null }     // Option.None<string>()
```

### None writes the property, it does not remove it

A converter cannot delete its own property from the enclosing object, so a
`None` writes `"nickname": null` rather than omitting `nickname`.

**`JsonIgnoreCondition.WhenWritingNull` does not help here.** It tests the CLR
value for null, and `Option.None<T>()` is an object like any other — never null
— so the property is written regardless. The same goes for
`[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`.

What does work is a type-info modifier, which decides per property whether to
write it at all:

```csharp
options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
{
    Modifiers = { SkipNoneProperties },
};

static void SkipNoneProperties(JsonTypeInfo typeInfo)
{
    foreach (JsonPropertyInfo property in typeInfo.Properties)
    {
        property.ShouldSerialize = static (_, value) =>
            value is null
         || value.GetType() is not { IsGenericType: true } type
         || type.GetGenericTypeDefinition() != typeof(None<>);
    }
}
```

This package does not ship that modifier. Omitting a property is a decision
about your wire contract, not about `Option<T>`, and a consumer who wants it
usually wants it for some models and not others.

### An absent property does not read back as None

If the property is missing from the payload entirely, `System.Text.Json` never
invokes the converter for that member and the CLR member keeps its default —
which for `Option<T>` is `null`, not `None<T>()`. Initialise the member, as
`Person.Nickname` does above, or the model will hold a null where it promised an
option.

### Nested options collapse

`Option<Option<T>>` does not survive a round trip. `Some(None)` and `None` both
write `null`, and both read back as `None`:

```csharp
Option<Option<int>> before = Option.Some(Option.None<int>());
string json = JsonSerializer.Serialize(before, options);   // "null"
Option<Option<int>> after = JsonSerializer.Deserialize<Option<Option<int>>>(json, options)!;
// after is None<Option<int>>(), not Some(None<int>())
```

The converter accepts this rather than throwing, because throwing on a shape the
type system allows is worse than losing a distinction nobody should be relying
on. The `WM2009` analyzer in `Waystone.Monads.Analyzers` already reports the
declaration, which is the right place to catch it.

## Result

`Result<TOk, TErr>` serializes as an object naming its case, with the payload
nested under `value`:

```jsonc
{ "$type": "ok",  "value": 42 }
{ "$type": "err", "value": { "Code": "validation.failed", "Message": "..." } }
```

Unlike an option, a result has no idiomatic JSON shape to borrow. Both cases
carry ordinary values of different types, so the case has to be named on the
wire.

### Why the payload is nested

`$type` is also `System.Text.Json`'s own polymorphism discriminator. If your
`TOk` or `TErr` is a polymorphic base carrying `[JsonDerivedType]`, it writes a
`$type` of its own. Nesting puts that one *inside* `value`, a level below the
result's:

```jsonc
{ "$type": "ok", "value": { "$type": "cat", "Name": "Tom" } }
```

Flattening the payload alongside the discriminator would have made the two
siblings, and the collision would surface only for consumers whose payload
happens to be polymorphic. Nesting rules it out by construction.

### The wire contract ignores your naming policy

`$type`, `value`, `ok` and `err` are fixed. `JsonSerializerOptions.PropertyNamingPolicy`
does not rename them, so a camel-casing service and a snake-casing one still
exchange the same payload — and so does `Waystone.Monads.NewtonsoftJson`, which
writes the identical format.

### Reading rejects what a result cannot hold

Deserializing throws `JsonException` when the payload is not an object, when
`$type` is missing or is not a string, when `value` is missing, when `$type`
names neither case, or when `value` is null. A result has no null case, so
accepting one would push the failure somewhere later and harder to trace.

Property order does not matter — `{"value":42,"$type":"ok"}` reads the same as
the canonical order.

## Trimming and NativeAOT

Both factories close their converter reflectively, once per monad type, and the
serializer caches the result. Under NativeAOT that **throws** when a type
argument is a value type:

```
NotSupportedException: 'OptionJsonConverter`1[System.Int32]' is missing native
code or metadata.
```

A generic instantiation over a value type needs its own compiled code, and the
compiler cannot see through `MakeGenericType` to know it will be asked for one.
Reference types all share a single compiled converter, so they are unaffected.

This is measured under `PublishAot` on .NET 10, not inferred:

| Registered through | Type argument | Under NativeAOT |
| --- | --- | --- |
| `AddMonadConverters()` | reference type | works |
| `AddMonadConverters()` | value type | throws |
| `options.Converters.Add(new …)` | either | works |

For `Result<TOk, TErr>` it is enough for one of the two arguments to be a value
type.

Register value-type monads explicitly instead. The concrete converters are public with
public parameterless constructors precisely so this path exists, and it involves
no reflection at all:

```csharp
options.Converters.Add(new OptionJsonConverter<int>());
options.Converters.Add(new ResultJsonConverter<int, string>());
```

Reference types are unaffected — every one of them shares a single compiled
converter — so a model made of `Option<string>` and `Result<Uri, Error>` needs
nothing extra.

`Option<T>` and `Result<TOk, TErr>` members do not get the source-generation fast
path from a `JsonSerializerContext`. A factory-produced converter works from one,
but only correctness is guaranteed, not the performance benefit.
