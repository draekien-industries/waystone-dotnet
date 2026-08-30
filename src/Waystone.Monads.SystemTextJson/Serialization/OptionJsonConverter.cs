namespace System.Text.Json.Serialization;

using Waystone.Monads.Options;

/// <summary>
/// Converts an <see cref="Option{T}" /> to and from the JSON its value would
/// have produced on its own, with <see langword="null" /> standing for the none
/// case.
/// </summary>
/// <remarks>
/// The wire format is the one Rust's serde uses, so a payload written by this
/// converter is the payload a consumer would have written by hand had the
/// property been a plain <typeparamref name="T" />: a some option contributes
/// nothing of its own to the JSON, and a none option contributes
/// <see langword="null" />. That keeps <see cref="Option{T}" /> an
/// implementation detail of the model rather than something the wire format has
/// to agree to.
/// <para>
/// Two consequences are worth knowing before adopting it. Writing a none option
/// emits the property with a <see langword="null" /> value rather than omitting
/// the property, because a converter cannot remove its own property from the
/// enclosing object, and
/// <see cref="JsonIgnoreCondition.WhenWritingNull" /> does not omit it either -
/// that condition tests the member for <see langword="null" />, and a none option
/// is an object like any other. Reading a payload where the property is absent
/// altogether never reaches this converter at all, so the member keeps its CLR
/// default, which is <see langword="null" /> rather than a none option unless the
/// model initialises it. The package README carries the type-info modifier that
/// does omit a none property.
/// </para>
/// <para>
/// A nested <c>Option&lt;Option&lt;T&gt;&gt;</c> does not survive the round trip:
/// both <c>Some(None)</c> and <c>None</c> write <see langword="null" /> and both
/// read back as <c>None</c>. This converter accepts that shape rather than
/// throwing on it; the WM2009 analyzer already reports the declaration.
/// </para>
/// </remarks>
/// <typeparam name="T">The type held by a some option.</typeparam>
/// <example>
/// Registering the converter for every closed <see cref="Option{T}" /> at once
/// goes through <c>AddMonadConverters</c>. Construct this type directly only to
/// register one closed option explicitly, which is what a NativeAOT consumer
/// does to keep the factory's reflection off the path:
/// <code>
/// JsonSerializerOptions options = new();
/// options.Converters.Add(new OptionJsonConverter&lt;int&gt;());
/// </code>
/// </example>
public sealed class OptionJsonConverter<T> : JsonConverter<Option<T>>
    where T : notnull
{
    /// <summary>
    /// Always true, so that a <see langword="null" /> token reaches
    /// <see cref="Read" /> instead of being handled by the serializer.
    /// </summary>
    /// <remarks>
    /// Load-bearing rather than a preference. <see cref="Option{T}" /> is a
    /// reference type, so the serializer would otherwise short-circuit a
    /// <see langword="null" /> token to a <see langword="null" /> option and the
    /// none case would never be constructed.
    /// </remarks>
    public override bool HandleNull => true;

    /// <summary>
    /// Reads a none option from a <see langword="null" /> token, and a some
    /// option from anything else.
    /// </summary>
    /// <remarks>
    /// A value that deserializes to <see langword="null" /> from a non-null token
    /// - which only a converter supplied for <typeparamref name="T" /> can
    /// produce - reads as a none option rather than throwing, since a some option
    /// cannot hold <see langword="null" />.
    /// </remarks>
    /// <param name="reader">The reader, positioned on the option's value.</param>
    /// <param name="typeToConvert">
    /// The closed <see cref="Option{T}" /> being read. Unused: the case is
    /// decided by the token and the payload by <typeparamref name="T" />.
    /// </param>
    /// <param name="options">
    /// The options used to read <typeparamref name="T" />, so a converter
    /// registered for the payload type still applies.
    /// </param>
    /// <returns>The option the JSON describes, never <see langword="null" />.</returns>
    public override Option<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Option.None<T>();
        }

        T? value = JsonSerializer.Deserialize<T>(ref reader, options);

        return value is null ? Option.None<T>() : Option.Some(value);
    }

    /// <summary>
    /// Writes a some option's value as if the option were not there, and a none
    /// option as <see langword="null" />.
    /// </summary>
    /// <param name="writer">The writer, positioned where the value belongs.</param>
    /// <param name="value">The option to write.</param>
    /// <param name="options">
    /// The options used to write <typeparamref name="T" />, so a converter
    /// registered for the payload type still applies.
    /// </param>
    public override void Write(
        Utf8JsonWriter writer,
        Option<T> value,
        JsonSerializerOptions options)
    {
        if (value is Some<T>(var inner))
        {
            JsonSerializer.Serialize(writer, inner, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
