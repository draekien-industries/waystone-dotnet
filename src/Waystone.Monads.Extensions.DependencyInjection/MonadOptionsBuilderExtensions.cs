namespace Microsoft.Extensions.DependencyInjection;

using System;
using Configuration;
using Waystone.Monads.Configs;

/// <summary>
/// Extensions for reading <see cref="MonadOptionsBuilder" /> settings out of an
/// <see cref="IConfiguration" />.
/// </summary>
public static class MonadOptionsBuilderExtensions
{
    /// <summary>The configuration section this reads from unless another is named.</summary>
    public const string DefaultSectionName = "WaystoneMonads";

    private const string FallbackErrorCodeKey = "FallbackErrorCode";

    private const string FallbackErrorMessageKey = "FallbackErrorMessage";

    private const string CatchesCancellationKey = "CatchesCancellation";

    /// <summary>Applies settings from a configuration section to the builder.</summary>
    /// <remarks>
    /// Reading configuration is opt-in: <c>AddWaystoneMonads</c> never reaches for
    /// an <see cref="IConfiguration" /> by itself, so call this from the delegate
    /// you pass it. Recognised keys, all optional:
    /// <list type="table">
    /// <item>
    /// <term><c>FallbackErrorCode</c></term>
    /// <description>See <see cref="MonadOptionsBuilder.UseFallbackErrorCode" />.</description>
    /// </item>
    /// <item>
    /// <term><c>FallbackErrorMessage</c></term>
    /// <description>See <see cref="MonadOptionsBuilder.UseFallbackErrorMessage" />.</description>
    /// </item>
    /// <item>
    /// <term><c>CatchesCancellation</c></term>
    /// <description>
    /// See <see cref="MonadOptionsBuilder.UseCancellationAsFailure" />.
    /// Either value is honoured, so <c>false</c> in configuration puts the
    /// setting back even where code earlier in the chain turned it on.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// An absent key leaves its setting as it was, so a section holding one key
    /// changes one setting. A key that is present but unusable throws, which is
    /// the point of opting in: configuration that cannot be honoured stops
    /// start-up where it is written rather than degrading quietly to a default
    /// nobody chose.
    /// </para>
    /// </remarks>
    /// <param name="builder">The builder to apply the settings to.</param>
    /// <param name="configuration">
    /// The configuration to read from. This is the root or any level above the
    /// section, not the section itself.
    /// </param>
    /// <param name="sectionName">
    /// The section to read, relative to <paramref name="configuration" />.
    /// Default: <see cref="DefaultSectionName" />.
    /// </param>
    /// <returns>The builder, for chaining more configurations.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder" /> or <paramref name="configuration" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="sectionName" /> is null, empty or whitespace;
    /// <c>CatchesCancellation</c> is present but is not <c>true</c> or
    /// <c>false</c>; or a fallback key is present but empty or whitespace.
    /// </exception>
    public static MonadOptionsBuilder ReadFromConfiguration(
        this MonadOptionsBuilder builder,
        IConfiguration configuration,
        string sectionName = DefaultSectionName)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentException(
                "The configuration section name cannot be null or whitespace.",
                nameof(sectionName));
        }

        IConfigurationSection section = configuration.GetSection(sectionName);

        if (section[FallbackErrorCodeKey] is { } fallbackErrorCode)
        {
            builder.UseFallbackErrorCode(fallbackErrorCode);
        }

        if (section[FallbackErrorMessageKey] is { } fallbackErrorMessage)
        {
            builder.UseFallbackErrorMessage(fallbackErrorMessage);
        }

        if (section[CatchesCancellationKey] is { } catchesCancellation)
        {
            if (!bool.TryParse(catchesCancellation, out bool catches))
            {
                throw new ArgumentException(
                    $"The value of '{section.Path}:{CatchesCancellationKey}' must be true or false, but was '{catchesCancellation}'.",
                    nameof(configuration));
            }

            builder.UseCancellationAsFailure(catches);
        }

        return builder;
    }
}
