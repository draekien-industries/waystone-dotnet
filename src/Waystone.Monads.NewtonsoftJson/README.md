# Waystone.Monads.NewtonsoftJson

An interop package for using Newtonsoft.Json with Waystone.Monads

## Namespaces

The package shadows `Newtonsoft.Json`'s own namespace, so its types sit where a
consumer already looks for them rather than under a parallel `Waystone` tree:

| Member | Namespace |
| --- | --- |
| `AddMonadConverters` | `Newtonsoft.Json` |
| `OptionJsonConverter` | `Newtonsoft.Json` |
| `ResultJsonConverter` | `Newtonsoft.Json` |

`JsonConverter` and `JsonSerializerSettings` both live in the root
`Newtonsoft.Json` namespace, so everything here lands there too.

## Supported Newtonsoft.Json versions

`Newtonsoft.Json >= 13.0.1 && < 14.0.0`. Bring your own version inside that
range; the package does not pin you to one. Every version in the range ships a
`netstandard2.0`, a `net45` and a `net20` asset, so .NET Framework consumers are
covered.

## Registration

```csharp
JsonSerializerSettings settings = new JsonSerializerSettings().AddMonadConverters();

string json = JsonConvert.SerializeObject(model, settings);
```

The two converters are appended to `Converters`, and Json.NET takes the first
one that accepts a type — so a converter you registered for an option or a
result beforehand keeps precedence.

## Option

`Option<T>` serializes as the value itself, or as `null`. This is the format
Rust's serde uses, and it means a payload is the payload you would have written
had the property been a plain `T` — adopting `Option<T>` in a model does not
change the contract on the wire.

```csharp
public sealed class Person
{
    public Option<string> Nickname { get; set; } = Option.None<string>();
}
```

```jsonc
{ "Nickname": "Ally" }   // Option.Some("Ally")
{ "Nickname": null }     // Option.None<string>()
```

### None writes the property, it does not remove it

A converter cannot delete its own property from the enclosing object, so a
`None` writes `"Nickname": null` rather than omitting `Nickname`.

**`NullValueHandling.Ignore` does not help here.** It tests the CLR value for
null, and `Option.None<T>()` is an object like any other — never null — so the
property is written regardless. The same goes for
`[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]`.

What does work is a contract resolver, which decides per property whether to
write it at all:

```csharp
settings.ContractResolver = new SkipNoneContractResolver();

public sealed class SkipNoneContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        property.ShouldSerialize = instance =>
            property.ValueProvider?.GetValue(instance)?.GetType() is not
                { IsGenericType: true } type
         || type.GetGenericTypeDefinition() != typeof(None<>);

        return property;
    }
}
```

A `ShouldSerialize{PropertyName}` method on the model itself does the same job
for one property.

This package does not ship that resolver. Omitting a property is a decision
about your wire contract, not about `Option<T>`, and a consumer who wants it
usually wants it for some models and not others.

### An absent property does not read back as None

If the property is missing from the payload entirely, Json.NET never invokes the
converter for that member and the CLR member keeps whatever the model gave it —
which without an initialiser is `null`, not `None<T>()`. Initialise the member,
as `Person.Nickname` does above, or the model will hold a null where it promised
an option.

### Nested options collapse

`Option<Option<T>>` does not survive a round trip. `Some(None)` and `None` both
write `null`, and both read back as `None`:

```csharp
Option<Option<int>> before = Option.Some(Option.None<int>());
string json = JsonConvert.SerializeObject(before, settings);   // "null"
Option<Option<int>> after = JsonConvert.DeserializeObject<Option<Option<int>>>(json, settings)!;
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

`$type` is a busy name. Json.NET writes one of its own when
`TypeNameHandling` is on, and it is also `System.Text.Json`'s polymorphism
discriminator. Nesting puts any such `$type` *inside* `value`, a level below the
result's:

```jsonc
{ "$type": "ok", "value": { "$type": "MyApp.Cat, MyApp", "Name": "Tom" } }
```

Flattening the payload alongside the discriminator would have made the two
siblings, and the collision would surface only for consumers who happen to turn
`TypeNameHandling` on. Nesting rules it out by construction.

### The wire contract ignores your naming policy

`$type`, `value`, `ok` and `err` are fixed. A `CamelCasePropertyNamesContractResolver`
does not rename them, so a camel-casing service and a snake-casing one still
exchange the same payload — and so does `Waystone.Monads.SystemTextJson`, which
writes the identical format.

### Reading rejects what a result cannot hold

Deserializing throws `JsonSerializationException` when the payload is not an
object, when `$type` is missing or is not a string, when `value` is missing,
when `$type` names neither case, or when `value` deserializes to null. A result
has no null case, so accepting one would push the failure somewhere later and
harder to trace.

A null `value` is not rejected on sight. It is deserialized as the case's type
first, so a payload whose own converter reads `null` still round-trips — most
usefully `Result<Option<T>, TErr>`, where `Ok(None)` writes `"value": null` and
reads back as `Ok(None)`. `Waystone.Monads.SystemTextJson` reads it the same
way.

Property order does not matter — `{"value":42,"$type":"ok"}` reads the same as
the canonical order.

## Reflection and NativeAOT

Json.NET resolves a converter from the *runtime* type of the value it is
writing, which for a monad is always `Some<T>`, `None<T>`, `Ok<TOk, TErr>` or
`Err<TOk, TErr>` rather than the option or result itself. Both converters
therefore accept all three shapes, and both close an internal adapter over the
type arguments once per closed type and cache it. Only the first monad of a
given type costs any reflection; nothing reflects per call.

That first construction is the same one that fails under NativeAOT for
`Waystone.Monads.SystemTextJson`, and there is no explicit-registration escape
here, because Json.NET has no way to register a converter for one closed generic
type. Json.NET has no first-class NativeAOT support of its own either. If you
publish with `PublishAot`, use `Waystone.Monads.SystemTextJson` instead.
