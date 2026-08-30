namespace System.Text.Json.Serialization;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using Waystone.Monads.Options;

/// <summary>
/// Supplies an <see cref="OptionJsonConverter{T}" /> for every closed
/// <see cref="Option{T}" /> the serializer meets.
/// </summary>
/// <remarks>
/// A factory rather than a converter because
/// <see cref="JsonSerializerOptions.Converters" /> is keyed on closed types, so
/// an open <c>Option&lt;&gt;</c> cannot be registered once and reused - the same
/// reason the serializer ships a factory for <see cref="Nullable{T}" />.
/// Register it through <c>AddMonadConverters</c> rather than by hand.
/// <para>
/// Closing the converter costs one reflective construction the first time the
/// serializer meets a given <c>Option&lt;T&gt;</c>, after which the serializer
/// caches it. Under NativeAOT that construction can fail when the option's value
/// type is a value type, since a generic instantiation over one needs code the
/// compiler emits ahead of time and cannot see through this call.
/// Register those explicitly - <c>new OptionJsonConverter&lt;int&gt;()</c> -
/// which involves no reflection at all.
/// </para>
/// </remarks>
public sealed class OptionJsonConverterFactory : JsonConverterFactory
{
    /// <summary>Checks whether a type is a closed <see cref="Option{T}" />.</summary>
    /// <remarks>
    /// Matches the option itself, not its cases: the serializer asks about the
    /// declared type of a member, and a member declared as <c>Some&lt;T&gt;</c>
    /// or <c>None&lt;T&gt;</c> has narrowed to one case already and does not need
    /// a discriminator written for it.
    /// </remarks>
    /// <param name="typeToConvert">The declared type the serializer is about to handle.</param>
    /// <returns>True if the type is an <c>Option&lt;T&gt;</c>; false otherwise.</returns>
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
     && typeToConvert.GetGenericTypeDefinition() == typeof(Option<>);

    /// <summary>
    /// Creates the <see cref="OptionJsonConverter{T}" /> closed over the option's
    /// value type.
    /// </summary>
    /// <param name="typeToConvert">
    /// The closed <see cref="Option{T}" /> to build a converter for. Call
    /// <see cref="CanConvert" /> first; anything else throws.
    /// </param>
    /// <param name="options">
    /// The options the converter will be registered against. Unused: the
    /// converter reads the options handed to each call instead, so one instance
    /// serves every <see cref="JsonSerializerOptions" />.
    /// </param>
    /// <returns>A converter for <paramref name="typeToConvert" />.</returns>
#if NET8_0_OR_GREATER
    [DynamicDependency(
        DynamicallyAccessedMemberTypes.PublicConstructors,
        typeof(OptionJsonConverter<>))]
#endif
    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(
            typeof(OptionJsonConverter<>).MakeGenericType(
                typeToConvert.GetGenericArguments()))!;
}
