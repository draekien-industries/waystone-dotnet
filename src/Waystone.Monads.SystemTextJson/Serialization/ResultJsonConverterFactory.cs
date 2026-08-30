namespace System.Text.Json.Serialization;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using Waystone.Monads.Results;

/// <summary>
/// Supplies a <see cref="ResultJsonConverter{TOk,TErr}" /> for every closed
/// <see cref="Result{TOk,TErr}" /> the serializer meets.
/// </summary>
/// <remarks>
/// A factory for the same reason <see cref="OptionJsonConverterFactory" /> is
/// one: <see cref="JsonSerializerOptions.Converters" /> is keyed on closed types,
/// so an open <c>Result&lt;,&gt;</c> cannot be registered once and reused.
/// Register it through <c>AddMonadConverters</c> rather than by hand.
/// <para>
/// Closing the converter costs one reflective construction the first time the
/// serializer meets a given <c>Result&lt;TOk, TErr&gt;</c>, after which the
/// serializer caches it. Under NativeAOT that construction can fail when either
/// type argument is a value type, since a generic instantiation over one needs
/// code the compiler emits ahead of time and cannot see through this call.
/// Register those explicitly - <c>new ResultJsonConverter&lt;int, string&gt;()</c>
/// - which involves no reflection at all.
/// </para>
/// </remarks>
public sealed class ResultJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Checks whether a type is a closed <see cref="Result{TOk,TErr}" />.
    /// </summary>
    /// <remarks>
    /// Matches the result itself, not its cases: the serializer asks about the
    /// declared type of a member, and a member declared as <c>Ok&lt;,&gt;</c> or
    /// <c>Err&lt;,&gt;</c> has narrowed to one case already and does not need a
    /// discriminator written for it.
    /// </remarks>
    /// <param name="typeToConvert">The declared type the serializer is about to handle.</param>
    /// <returns>True if the type is a <c>Result&lt;TOk, TErr&gt;</c>; false otherwise.</returns>
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
     && typeToConvert.GetGenericTypeDefinition() == typeof(Result<,>);

    /// <summary>
    /// Creates the <see cref="ResultJsonConverter{TOk,TErr}" /> closed over both
    /// of the result's case types.
    /// </summary>
    /// <param name="typeToConvert">
    /// The closed <see cref="Result{TOk,TErr}" /> to build a converter for. Call
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
        typeof(ResultJsonConverter<,>))]
#endif
    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(
            typeof(ResultJsonConverter<,>).MakeGenericType(
                typeToConvert.GetGenericArguments()))!;
}
