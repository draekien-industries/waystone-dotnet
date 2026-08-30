namespace Newtonsoft.Json;

using System;
using Waystone.Monads.Options;
using Waystone.Monads.Results;

/// <summary>
/// Registers the Waystone.Monads converters on a
/// <see cref="JsonSerializerSettings" />.
/// </summary>
public static class JsonSerializerSettingsExtensions
{
    /// <summary>
    /// Adds the <see cref="Option{T}" /> and <see cref="Result{TOk,TErr}" />
    /// converters, so every closed monad in a model round-trips.
    /// </summary>
    /// <remarks>
    /// Call it once, while the settings are still being assembled. The two
    /// converters are appended to
    /// <see cref="JsonSerializerSettings.Converters" />, so a converter already
    /// registered for an option or a result keeps precedence - Json.NET takes the
    /// first converter that accepts the type.
    /// <para>
    /// Calling it twice registers each converter twice. That changes nothing on
    /// the wire, since the duplicates behave identically, but it is a sign the
    /// settings are being configured from two places.
    /// </para>
    /// </remarks>
    /// <param name="settings">The settings to register the converters on.</param>
    /// <returns>
    /// The same <paramref name="settings" />, so the call chains into the object
    /// initialiser that built it.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="settings" /> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// JsonSerializerSettings settings = new JsonSerializerSettings().AddMonadConverters();
    /// string json = JsonConvert.SerializeObject(model, settings);
    /// </code>
    /// </example>
    public static JsonSerializerSettings AddMonadConverters(
        this JsonSerializerSettings settings)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        settings.Converters.Add(new OptionJsonConverter());
        settings.Converters.Add(new ResultJsonConverter());

        return settings;
    }
}
