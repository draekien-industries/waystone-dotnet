namespace Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using Linq;
using Waystone.Monads.Results;

/// <summary>
/// Converts every closed <see cref="Result{TOk,TErr}" /> to and from an object
/// naming its case, with the payload nested under <c>value</c>.
/// </summary>
/// <remarks>
/// Unlike an option, a result has no idiomatic JSON shape to borrow: both cases
/// carry ordinary values of different types, so the case has to be named on the
/// wire.
/// <code>
/// { "$type": "ok",  "value": 42 }
/// { "$type": "err", "value": { "Code": "validation.failed" } }
/// </code>
/// <para>
/// The four names are fixed and
/// <see cref="JsonSerializerSettings.ContractResolver" /> does not rename them,
/// so a camel-casing service and a snake-casing one still exchange the same
/// payload - and so does <c>Waystone.Monads.SystemTextJson</c>, which writes the
/// identical format. Nesting the payload rather than flattening it beside the
/// discriminator is what keeps that true: <c>$type</c> is also
/// System.Text.Json's own polymorphism discriminator, and a polymorphic payload
/// writing one of its own has to land a level below the result's.
/// </para>
/// <para>
/// One converter serves every closed result, because Json.NET has no equivalent
/// of a converter factory and keys
/// <see cref="JsonSerializerSettings.Converters" /> on instances rather than on
/// types. It closes an internal adapter over the result's two case types once
/// per pair and caches it, so only the first result of a given type costs any
/// reflection.
/// </para>
/// </remarks>
/// <example>
/// Registering this converter alongside <see cref="OptionJsonConverter" /> goes
/// through <c>AddMonadConverters</c>. Construct it directly only to add it to a
/// settings object you are assembling by hand:
/// <code>
/// JsonSerializerSettings settings = new();
/// settings.Converters.Add(new ResultJsonConverter());
/// </code>
/// </example>
public sealed class ResultJsonConverter : JsonConverter
{
    private const string TypeProperty = "$type";
    private const string ValueProperty = "value";
    private const string OkDiscriminator = "ok";
    private const string ErrDiscriminator = "err";

    private static readonly ConcurrentDictionary<Type, IResultAdapter> Adapters =
        new();

    /// <summary>
    /// Checks whether a type is a closed <see cref="Result{TOk,TErr}" /> or one
    /// of its two cases.
    /// </summary>
    /// <remarks>
    /// Both cases have to match, not just the result. Json.NET resolves a
    /// converter from the *runtime* type of the value it is writing, and the
    /// runtime type of a result is always <c>Ok&lt;TOk, TErr&gt;</c> or
    /// <c>Err&lt;TOk, TErr&gt;</c> - matching only <c>Result&lt;TOk, TErr&gt;</c>
    /// would leave every result serialized as its own properties instead. This is
    /// where the converter departs from its System.Text.Json counterpart, which
    /// resolves from the declared type and matches the result alone.
    /// </remarks>
    /// <param name="objectType">The type the serializer is about to handle.</param>
    /// <returns>
    /// True if the type is a <c>Result&lt;TOk, TErr&gt;</c>, an
    /// <c>Ok&lt;TOk, TErr&gt;</c> or an <c>Err&lt;TOk, TErr&gt;</c>; false
    /// otherwise.
    /// </returns>
    public override bool CanConvert(Type objectType)
    {
        if (!objectType.IsGenericType)
        {
            return false;
        }

        Type definition = objectType.GetGenericTypeDefinition();

        return definition == typeof(Result<,>)
            || definition == typeof(Ok<,>)
            || definition == typeof(Err<,>);
    }

    /// <summary>
    /// Reads the case named by <c>$type</c>, deserializing <c>value</c> as that
    /// case's payload type.
    /// </summary>
    /// <remarks>
    /// Property order does not matter. Anything a result cannot hold is rejected
    /// rather than coerced, since accepting it would push the failure somewhere
    /// later and harder to trace.
    /// </remarks>
    /// <param name="reader">The reader, positioned on the result's object.</param>
    /// <param name="objectType">
    /// The closed <see cref="Result{TOk,TErr}" /> being read. Its two type
    /// arguments decide how the payload is deserialized.
    /// </param>
    /// <param name="existingValue">
    /// The member's current value. Unused: a result is immutable, so there is
    /// nothing to populate in place.
    /// </param>
    /// <param name="serializer">
    /// The serializer used to read the payload, so a converter registered for
    /// either case type still applies.
    /// </param>
    /// <returns>The result the JSON describes, never <see langword="null" />.</returns>
    /// <exception cref="JsonSerializationException">
    /// Thrown when the payload is not an object, when <c>$type</c> is missing or
    /// is not a string, when <c>$type</c> names neither case, when <c>value</c>
    /// is missing, or when <c>value</c> is null - a result has no null case.
    /// </exception>
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer)
    {
        JToken token = JToken.ReadFrom(reader);

        if (token is not JObject root)
        {
            throw new JsonSerializationException(
                $"A result must be a JSON object, but the payload was {token.Type}.");
        }

        if (root[TypeProperty] is not JValue { Type: JTokenType.String } discriminator)
        {
            throw new JsonSerializationException(
                $"A result must carry a string \"{TypeProperty}\" property naming its case.");
        }

        if (!root.TryGetValue(ValueProperty, out JToken? payload))
        {
            throw new JsonSerializationException(
                $"A result must carry a \"{ValueProperty}\" property holding its payload.");
        }

        if (payload.Type == JTokenType.Null)
        {
            throw new JsonSerializationException(
                $"A result's \"{ValueProperty}\" cannot be null; neither case can hold one.");
        }

        IResultAdapter adapter = AdapterFor(objectType);

        return (string?)discriminator.Value switch
        {
            OkDiscriminator => adapter.Ok(payload, serializer),
            ErrDiscriminator => adapter.Err(payload, serializer),
            var other => throw new JsonSerializationException(
                $"\"{other}\" is not a result case; expected \"{OkDiscriminator}\" or \"{ErrDiscriminator}\"."),
        };
    }

    /// <summary>
    /// Writes the object naming the result's case, with the payload nested under
    /// <c>value</c>.
    /// </summary>
    /// <param name="writer">The writer, positioned where the object belongs.</param>
    /// <param name="value">
    /// The result to write. A <see langword="null" /> reference writes
    /// <see langword="null" />, so a model that never initialised the member does
    /// not fail serialization - though nothing reads that back.
    /// </param>
    /// <param name="serializer">
    /// The serializer used to write the payload, so a converter registered for
    /// either case type still applies.
    /// </param>
    public override void WriteJson(
        JsonWriter writer,
        object? value,
        JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();

            return;
        }

        AdapterFor(value.GetType()).Write(writer, value, serializer);
    }

    private static IResultAdapter AdapterFor(Type resultType) =>
        Adapters.GetOrAdd(
            resultType,
            static type => (IResultAdapter)Activator.CreateInstance(
                typeof(ResultAdapter<,>).MakeGenericType(
                    type.GetGenericArguments())));

    private interface IResultAdapter
    {
        object Ok(JToken payload, JsonSerializer serializer);

        object Err(JToken payload, JsonSerializer serializer);

        void Write(JsonWriter writer, object value, JsonSerializer serializer);
    }

    private sealed class ResultAdapter<TOk, TErr> : IResultAdapter
        where TOk : notnull where TErr : notnull
    {
        public object Ok(JToken payload, JsonSerializer serializer) =>
            Result.Ok<TOk, TErr>(Payload<TOk>(payload, serializer));

        public object Err(JToken payload, JsonSerializer serializer) =>
            Result.Err<TOk, TErr>(Payload<TErr>(payload, serializer));

        public void Write(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            writer.WriteStartObject();

            ((Result<TOk, TErr>)value).Match(
                ok => WriteCase(writer, OkDiscriminator, ok, serializer),
                err => WriteCase(writer, ErrDiscriminator, err, serializer));

            writer.WriteEndObject();
        }

        private static void WriteCase<TPayload>(
            JsonWriter writer,
            string discriminator,
            TPayload payload,
            JsonSerializer serializer)
        {
            writer.WritePropertyName(TypeProperty);
            writer.WriteValue(discriminator);
            writer.WritePropertyName(ValueProperty);
            serializer.Serialize(writer, payload);
        }

        private static TPayload Payload<TPayload>(
            JToken payload,
            JsonSerializer serializer)
            where TPayload : notnull =>
            payload.ToObject<TPayload>(serializer)
         ?? throw new JsonSerializationException(
                $"A result's \"{ValueProperty}\" cannot be null; neither case can hold one.");
    }
}
