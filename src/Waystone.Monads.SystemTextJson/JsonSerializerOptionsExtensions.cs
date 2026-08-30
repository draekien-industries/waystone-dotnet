namespace System.Text.Json;

using Serialization;

/// <summary>
/// Extension methods that register the Waystone.Monads converters on a
/// <see cref="JsonSerializerOptions" />.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Registers the converters that let the monad types round-trip through
    /// <see cref="JsonSerializer" />.
    /// </summary>
    /// <remarks>
    /// Call this once, while the options are still being built. Adding a
    /// converter to a <see cref="JsonSerializerOptions" /> that has already
    /// serialized something throws, because the serializer freezes the options
    /// on first use.
    /// <para>
    /// Calling it twice adds a second copy of each converter. That is harmless -
    /// the serializer takes the last converter registered for a type - but it is
    /// wasted work, so prefer one call at composition time over a defensive one
    /// per call site.
    /// </para>
    /// </remarks>
    /// <param name="options">The options to register the converters on.</param>
    /// <returns>
    /// The same <paramref name="options" /> instance, so the call chains with the
    /// rest of the options setup.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="options" /> is null.
    /// </exception>
    public static JsonSerializerOptions AddMonadConverters(
        this JsonSerializerOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Converters.Add(new OptionJsonConverterFactory());
        options.Converters.Add(new ResultJsonConverterFactory());

        return options;
    }
}
