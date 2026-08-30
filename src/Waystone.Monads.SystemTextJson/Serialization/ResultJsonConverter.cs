namespace System.Text.Json.Serialization;

using Waystone.Monads.Results;

/// <summary>
/// Converts a <see cref="Result{TOk,TErr}" /> to and from an object carrying a
/// <c>$type</c> discriminator and the payload under <c>value</c>.
/// </summary>
/// <remarks>
/// An ok result writes <c>{"$type":"ok","value":…}</c> and an err result writes
/// <c>{"$type":"err","value":…}</c>. Unlike an option, a result has no idiomatic
/// JSON shape to borrow - the two cases carry different types and both are
/// ordinary values - so the case has to be named on the wire.
/// <para>
/// The payload nests under <c>value</c> rather than sitting beside the
/// discriminator, and that is the whole reason the format has a nesting level
/// nobody would otherwise want. <c>$type</c> is also the serializer's own
/// polymorphism discriminator, so a payload type carrying
/// <see cref="JsonDerivedTypeAttribute" /> writes a <c>$type</c> of its own.
/// Nesting puts that one inside <c>value</c>, a level below this one, where the
/// two cannot collide. Flattening would have made them siblings and the collision
/// would surface only for consumers whose payload happens to be polymorphic.
/// </para>
/// <para>
/// The two property names and the two discriminator values are fixed. They are
/// the wire contract shared with <c>Waystone.Monads.NewtonsoftJson</c>, so
/// <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> does not rename
/// them - a camel-casing consumer and a snake-casing one still exchange the same
/// payload.
/// </para>
/// </remarks>
/// <typeparam name="TOk">The type held by an ok result.</typeparam>
/// <typeparam name="TErr">The type held by an err result.</typeparam>
/// <example>
/// Registering the converter for every closed <see cref="Result{TOk,TErr}" /> at
/// once goes through <c>AddMonadConverters</c>. Construct this type directly only
/// to register one closed result explicitly, which is what a NativeAOT consumer
/// does to keep the factory's reflection off the path:
/// <code>
/// JsonSerializerOptions options = new();
/// options.Converters.Add(new ResultJsonConverter&lt;int, string&gt;());
/// </code>
/// </example>
public sealed class ResultJsonConverter<TOk, TErr>
    : JsonConverter<Result<TOk, TErr>>
    where TOk : notnull where TErr : notnull
{
    private const string TypeProperty = "$type";
    private const string ValueProperty = "value";
    private const string OkDiscriminator = "ok";
    private const string ErrDiscriminator = "err";

    /// <summary>
    /// Always true, so that a <see langword="null" /> token reaches
    /// <see cref="Read" /> and is rejected there.
    /// </summary>
    /// <remarks>
    /// The wire format has no spelling for a missing result, so
    /// <see langword="null" /> is a malformed payload rather than a third case.
    /// Left to the serializer it would become a <see langword="null" /> result
    /// instead, which fails later and somewhere else.
    /// </remarks>
    public override bool HandleNull => true;

    /// <summary>
    /// Reads a result from its discriminated object, in whichever order the two
    /// properties appear.
    /// </summary>
    /// <remarks>
    /// Buffers the object before reading the payload, since the discriminator that
    /// says which type to read the payload as may follow the payload rather than
    /// precede it.
    /// </remarks>
    /// <param name="reader">The reader, positioned on the result's object.</param>
    /// <param name="typeToConvert">
    /// The closed <see cref="Result{TOk,TErr}" /> being read. Unused: the case is
    /// decided by the discriminator.
    /// </param>
    /// <param name="options">
    /// The options used to read the payload, so a converter registered for
    /// <typeparamref name="TOk" /> or <typeparamref name="TErr" /> still applies.
    /// </param>
    /// <returns>The result the JSON describes, never <see langword="null" />.</returns>
    /// <exception cref="JsonException">
    /// If the payload is not an object, if either <c>$type</c> or <c>value</c> is
    /// missing, if <c>$type</c> is not one of the two discriminators, or if
    /// <c>value</c> reads as null - none of which a result can represent.
    /// </exception>
    public override Result<TOk, TErr> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException(
                $"A result must be a JSON object, but the payload was {root.ValueKind}.");
        }

        if (!root.TryGetProperty(TypeProperty, out JsonElement discriminator)
         || discriminator.ValueKind != JsonValueKind.String)
        {
            throw new JsonException(
                $"A result must carry a string \"{TypeProperty}\" property naming its case.");
        }

        if (!root.TryGetProperty(ValueProperty, out JsonElement payload))
        {
            throw new JsonException(
                $"A result must carry a \"{ValueProperty}\" property holding its payload.");
        }

        return discriminator.GetString() switch
        {
            OkDiscriminator => Result.Ok<TOk, TErr>(Payload<TOk>(payload, options)),
            ErrDiscriminator => Result.Err<TOk, TErr>(
                Payload<TErr>(payload, options)),
            var other => throw new JsonException(
                $"\"{other}\" is not a result case; expected \"{OkDiscriminator}\" or \"{ErrDiscriminator}\"."),
        };
    }

    /// <summary>
    /// Writes a result as its discriminator and its payload, with the payload
    /// nested.
    /// </summary>
    /// <param name="writer">The writer, positioned where the object belongs.</param>
    /// <param name="value">The result to write.</param>
    /// <param name="options">
    /// The options used to write the payload, so a converter registered for
    /// <typeparamref name="TOk" /> or <typeparamref name="TErr" /> still applies.
    /// </param>
    public override void Write(
        Utf8JsonWriter writer,
        Result<TOk, TErr> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        value.Match(
            ok => WriteCase(writer, OkDiscriminator, ok, options),
            err => WriteCase(writer, ErrDiscriminator, err, options));

        writer.WriteEndObject();
    }

    private static void WriteCase<TPayload>(
        Utf8JsonWriter writer,
        string discriminator,
        TPayload payload,
        JsonSerializerOptions options)
    {
        writer.WriteString(TypeProperty, discriminator);
        writer.WritePropertyName(ValueProperty);
        JsonSerializer.Serialize(writer, payload, options);
    }

    private static TPayload Payload<TPayload>(
        JsonElement payload,
        JsonSerializerOptions options)
        where TPayload : notnull =>
        payload.Deserialize<TPayload>(options)
     ?? throw new JsonException(
            $"A result's \"{ValueProperty}\" cannot be null; neither case can hold one.");
}
