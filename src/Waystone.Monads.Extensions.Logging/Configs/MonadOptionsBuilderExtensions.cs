namespace Waystone.Monads.Extensions.Logging.Configs;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Monads.Configs;

/// <summary>
/// Extensions for chaining <see cref="MonadLoggingOptions" /> configuration onto
/// a <see cref="MonadOptionsBuilder" />.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MonadOptionsBuilderExtensions
{
    /// <summary>Sends handled exceptions to a logger you already hold.</summary>
    /// <remarks>
    /// The logger keeps its own category. See
    /// <see cref="MonadLoggingOptionsBuilder.UseLogger" /> for the trade against
    /// <see cref="UseLoggerFactory" />.
    /// </remarks>
    /// <param name="builder">
    /// The <see cref="MonadOptionsBuilder" /> whose logging options will be
    /// configured.
    /// </param>
    /// <param name="logger">The logger to write handled exceptions to.</param>
    /// <param name="level">
    /// The level to write them at. Default: <see cref="LogLevel.Debug" />.
    /// </param>
    /// <returns>
    /// The <see cref="MonadLoggingOptionsBuilder" /> for chaining more
    /// configurations.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger" /> is null.</exception>
    public static MonadLoggingOptionsBuilder UseLogger(
        this MonadOptionsBuilder builder,
        ILogger logger,
        LogLevel level = LogLevel.Debug) =>
        MonadLoggingOptionsBuilder.For(builder).UseLogger(logger, level);

    /// <summary>Sends handled exceptions to a logger created under this library's own category.</summary>
    /// <remarks>
    /// The logger is created in the
    /// <see cref="MonadLoggingOptions.LoggerCategory" /> category, so the
    /// library's output can be filtered on its own.
    /// </remarks>
    /// <param name="builder">
    /// The <see cref="MonadOptionsBuilder" /> whose logging options will be
    /// configured.
    /// </param>
    /// <param name="loggerFactory">The factory to create the logger from.</param>
    /// <param name="level">
    /// The level to write them at. Default: <see cref="LogLevel.Debug" />.
    /// </param>
    /// <returns>
    /// The <see cref="MonadLoggingOptionsBuilder" /> for chaining more
    /// configurations.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="loggerFactory" /> is null.</exception>
    public static MonadLoggingOptionsBuilder UseLoggerFactory(
        this MonadOptionsBuilder builder,
        ILoggerFactory loggerFactory,
        LogLevel level = LogLevel.Debug) =>
        MonadLoggingOptionsBuilder.For(builder)
                                  .UseLoggerFactory(loggerFactory, level);

    /// <summary>
    /// Sends handled exceptions to a logger created from the
    /// <see cref="ILoggerFactory" /> a service provider holds.
    /// </summary>
    /// <remarks>
    /// The provider is resolved through <see cref="IServiceProvider" /> itself
    /// rather than through any dependency-injection package, so this works with
    /// whatever container produced it — including
    /// <c>app.Services</c> on a host, and including containers that are not
    /// Microsoft's. Call it once at start-up, after the provider is built.
    /// </remarks>
    /// <param name="builder">
    /// The <see cref="MonadOptionsBuilder" /> whose logging options will be
    /// configured.
    /// </param>
    /// <param name="provider">The service provider to resolve the factory from.</param>
    /// <param name="level">
    /// The level to write them at. Default: <see cref="LogLevel.Debug" />.
    /// </param>
    /// <returns>
    /// The <see cref="MonadLoggingOptionsBuilder" /> for chaining more
    /// configurations.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider" /> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="provider" /> has no <see cref="ILoggerFactory" />
    /// registered. Call <c>AddLogging</c> on the service collection, or pass a
    /// factory directly to <see cref="UseLoggerFactory" />.
    /// </exception>
    public static MonadLoggingOptionsBuilder UseLoggerFactoryFrom(
        this MonadOptionsBuilder builder,
        IServiceProvider provider,
        LogLevel level = LogLevel.Debug)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (provider.GetService(typeof(ILoggerFactory)) is not ILoggerFactory
            loggerFactory)
        {
            throw new InvalidOperationException(
                $"No {nameof(ILoggerFactory)} is registered in the service provider. "
              + $"Call AddLogging on the service collection, or pass a factory to {nameof(UseLoggerFactory)}.");
        }

        return builder.UseLoggerFactory(loggerFactory, level);
    }
}
