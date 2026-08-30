# Serializing with Newtonsoft.Json

`Waystone.Monads.NewtonsoftJson` registers converters for both monads:

```csharp
JsonSerializerSettings settings = new JsonSerializerSettings().AddMonadConverters();
```

Everything ships in the root `Newtonsoft.Json` namespace, where `JsonConverter`
and `JsonSerializerSettings` already live. The two converters are *appended* to
`Converters` and Json.NET takes the first that accepts a type, so a converter
registered for an option or a result beforehand keeps precedence.

## The two wire formats

`Option<T>` writes the value itself, or `null` — serde's format, so adopting
`Option<T>` in a model does not change the contract on the wire.

`Result<TOk, TErr>` names its case and nests the payload:

```jsonc
{ "$type": "ok",  "value": 42 }
{ "$type": "err", "value": { "Code": "validation.failed", "Message": "…" } }
```

**Nesting exists because `$type` is a busy name.** Json.NET writes one of its own
when `TypeNameHandling` is on; nesting puts it inside `value`, a level below the
result's, rather than making the two siblings.

`$type`, `value`, `ok` and `err` are fixed. A `CamelCasePropertyNamesContractResolver`
does not rename them, and `Waystone.Monads.SystemTextJson` writes the identical
format, so the two serializers interoperate. Property order does not matter on
the way back in.

Reading throws `JsonSerializationException` when the payload is not an object,
when `$type` is missing, is not a string, or names neither case, or when `value`
is missing. A null `value` is not rejected on sight — it is deserialized as the
case's own type first, so `Result<Option<T>, TErr>` round-trips.

## Three traps in a model

**Initialise every `Option<T>` member.** An absent property never invokes the
converter, so the member keeps whatever the model gave it — `null` without an
initialiser, not `None<T>()`.

```csharp
public sealed class Person
{
    public Option<string> Nickname { get; set; } = Option.None<string>();
}
```

**A `None` writes the property as null rather than omitting it.**
`NullValueHandling.Ignore` does not help: it tests the CLR value, and
`Option.None<T>()` is an object like any other. Omitting the property takes a
contract resolver setting `ShouldSerialize` per property, or a
`ShouldSerialize{PropertyName}` method on the model for a single one. The package
ships neither, because that is a decision about your wire contract.

**`Option<Option<T>>` does not survive a round trip.** `Some(None)` and `None`
both write `null` and read back as `None`. `WM2009` reports the declaration,
which is where to catch it.

## Do not publish this with NativeAOT

Json.NET resolves a converter from the *runtime* type, which for a monad is
always `Some<T>`, `None<T>`, `Ok<TOk, TErr>` or `Err<TOk, TErr>`, so both
converters close an internal adapter over the type arguments once per closed type
and cache it. Only the first monad of a given type costs any reflection.

That first construction is what fails under `PublishAot`, and there is no
explicit-registration escape here, because Json.NET cannot register a converter
for one closed generic type. Use `Waystone.Monads.SystemTextJson` where the
application publishes ahead of time.
