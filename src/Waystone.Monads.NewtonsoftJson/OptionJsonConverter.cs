namespace Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using Waystone.Monads.Options;

/// <summary>
/// Converts every closed <see cref="Option{T}" /> to and from the JSON its value
/// would have produced on its own, with <see langword="null" /> standing for the
/// none case.
/// </summary>
/// <remarks>
/// The wire format is the one Rust's serde uses, and the one
/// <c>Waystone.Monads.SystemTextJson</c> writes: a some option contributes
/// nothing of its own to the JSON, and a none option contributes
/// <see langword="null" />. Adopting <see cref="Option{T}" /> in a model
/// therefore leaves the payload a consumer already agreed to unchanged.
/// <para>
/// One converter serves every closed option, because Json.NET has no equivalent
/// of a converter factory and keys <see cref="JsonSerializerSettings.Converters" />
/// on instances rather than on types. It closes an internal adapter over the
/// option's value type once per type and caches it, so only the first option of a
/// given type costs any reflection.
/// </para>
/// <para>
/// Two consequences are worth knowing before adopting it. Writing a none option
/// emits the property with a <see langword="null" /> value rather than omitting
/// the property, and <see cref="NullValueHandling.Ignore" /> does not omit it
/// either - that setting tests the member for <see langword="null" />, and a none
/// option is an object like any other. Reading a payload where the property is
/// absent altogether never reaches this converter, so the member keeps its CLR
/// default, which is <see langword="null" /> rather than a none option unless the
/// model initialises it. The package README carries the contract resolver that
/// does omit a none property.
/// </para>
/// <para>
/// A nested <c>Option&lt;Option&lt;T&gt;&gt;</c> does not survive the round trip:
/// both <c>Some(None)</c> and <c>None</c> write <see langword="null" /> and both
/// read back as <c>None</c>. This converter accepts that shape rather than
/// throwing on it; the WM2009 analyzer already reports the declaration.
/// </para>
/// </remarks>
/// <example>
/// Registering this converter alongside <see cref="ResultJsonConverter" /> goes
/// through <c>AddMonadConverters</c>. Construct it directly only to add it to a
/// settings object you are assembling by hand:
/// <code>
/// JsonSerializerSettings settings = new();
/// settings.Converters.Add(new OptionJsonConverter());
/// </code>
/// </example>
public sealed class OptionJsonConverter : JsonConverter
{
    private static readonly ConcurrentDictionary<Type, IOptionAdapter> Adapters =
        new();

    /// <summary>
    /// Checks whether a type is a closed <see cref="Option{T}" /> or one of its
    /// two cases.
    /// </summary>
    /// <remarks>
    /// Both cases have to match, not just the option. Json.NET resolves a
    /// converter from the *runtime* type of the value it is writing, and the
    /// runtime type of an option is always <c>Some&lt;T&gt;</c> or
    /// <c>None&lt;T&gt;</c> - matching only <c>Option&lt;T&gt;</c> would leave
    /// every option serialized as its own properties instead. This is where the
    /// converter departs from its System.Text.Json counterpart, which resolves
    /// from the declared type and matches the option alone.
    /// </remarks>
    /// <param name="objectType">The type the serializer is about to handle.</param>
    /// <returns>
    /// True if the type is an <c>Option&lt;T&gt;</c>, a <c>Some&lt;T&gt;</c> or a
    /// <c>None&lt;T&gt;</c>; false otherwise.
    /// </returns>
    public override bool CanConvert(Type objectType)
    {
        if (!objectType.IsGenericType)
        {
            return false;
        }

        Type definition = objectType.GetGenericTypeDefinition();

        return definition == typeof(Option<>)
            || definition == typeof(Some<>)
            || definition == typeof(None<>);
    }

    /// <summary>
    /// Reads a none option from a <see langword="null" /> token, and a some
    /// option from anything else.
    /// </summary>
    /// <remarks>
    /// A value that deserializes to <see langword="null" /> from a non-null token
    /// - which only a converter supplied for the value type can produce - reads
    /// as a none option rather than throwing, since a some option cannot hold
    /// <see langword="null" />.
    /// </remarks>
    /// <param name="reader">The reader, positioned on the option's value.</param>
    /// <param name="objectType">
    /// The closed <see cref="Option{T}" /> being read. Its type argument decides
    /// how the payload is deserialized.
    /// </param>
    /// <param name="existingValue">
    /// The member's current value. Unused: an option is immutable, so there is
    /// nothing to populate in place.
    /// </param>
    /// <param name="serializer">
    /// The serializer used to read the option's value, so a converter registered
    /// for the payload type still applies.
    /// </param>
    /// <returns>The option the JSON describes, never <see langword="null" />.</returns>
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer) =>
        AdapterFor(objectType.GetGenericArguments()[0])
           .Read(reader, serializer);

    /// <summary>
    /// Writes a some option's value as if the option were not there, and a none
    /// option as <see langword="null" />.
    /// </summary>
    /// <param name="writer">The writer, positioned where the value belongs.</param>
    /// <param name="value">
    /// The option to write. A <see langword="null" /> reference writes
    /// <see langword="null" />, matching a none option, since a model that never
    /// initialised the member should not fail serialization over it.
    /// </param>
    /// <param name="serializer">
    /// The serializer used to write the option's value, so a converter registered
    /// for the payload type still applies.
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

        AdapterFor(value.GetType().GetGenericArguments()[0])
           .Write(writer, value, serializer);
    }

    private static IOptionAdapter AdapterFor(Type valueType) =>
        Adapters.GetOrAdd(
            valueType,
            static type => (IOptionAdapter)Activator.CreateInstance(
                typeof(OptionAdapter<>).MakeGenericType(type)));

    private interface IOptionAdapter
    {
        object Read(JsonReader reader, JsonSerializer serializer);

        void Write(JsonWriter writer, object value, JsonSerializer serializer);
    }

    private sealed class OptionAdapter<T> : IOptionAdapter
        where T : notnull
    {
        public object Read(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return Option.None<T>();
            }

            T? value = serializer.Deserialize<T>(reader);

            return value is null ? Option.None<T>() : Option.Some(value);
        }

        public void Write(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            if (value is Some<T>(var inner))
            {
                serializer.Serialize(writer, inner);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
