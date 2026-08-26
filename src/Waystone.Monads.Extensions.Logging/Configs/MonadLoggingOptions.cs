namespace Waystone.Monads.Extensions.Logging.Configs;

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Monads.Configs;

/// <summary>
/// Configuration for reporting the exceptions Waystone.Monads swallows through a
/// <see cref="ILogger" />.
/// </summary>
/// <remarks>
/// Registered as a satellite of <see cref="MonadOptions" />, so it follows
/// whatever options scope is current when an exception is handled. The
/// subscription that feeds it is process-wide and cannot itself be scoped, which
/// is why the logger and the level live here rather than on the subscription:
/// <c>MonadOptions.BeginScope(o =&gt; o.UseLogger(other, LogLevel.Warning))</c>
/// redirects the logging for one asynchronous flow and leaves the rest alone.
/// <para>
/// Nothing is logged until one of the configuration methods is called. Until
/// then the logger is <see cref="NullLogger" /> and no subscription exists, so
/// the library's diagnostic event is never even built.
/// </para>
/// </remarks>
public sealed class MonadLoggingOptions : IMonadOptionsSatellite
{
    /// <summary>The category assigned to loggers this package creates for itself.</summary>
    /// <remarks>
    /// Used by <see cref="UseLoggerFactory" /> and, through it, by
    /// <c>UseLoggerFactoryFrom</c>. Filter on this to raise or silence the
    /// library's own output — for example an <c>appsettings.json</c> entry of
    /// <c>"Waystone.Monads": "Warning"</c>. A logger supplied to
    /// <see cref="UseLogger" /> keeps whatever category it already had.
    /// </remarks>
    public const string LoggerCategory = "Waystone.Monads";

    private MonadLoggingOptions()
    {
        Logger = NullLogger.Instance;
        Level = LogLevel.Debug;
    }

    internal static MonadLoggingOptions Current => For(MonadOptions.Current);

    internal static MonadLoggingOptions For(MonadOptions options) =>
        options.Satellite(() => new MonadLoggingOptions());

    internal ILogger Logger { get; set; }

    internal LogLevel Level { get; set; }

    IMonadOptionsSatellite IMonadOptionsSatellite.Clone() =>
        new MonadLoggingOptions { Logger = Logger, Level = Level };

    /// <summary>Sends handled exceptions to a logger you already hold.</summary>
    /// <remarks>
    /// The logger keeps its own category, so the output is attributed to whatever
    /// type you resolved <see cref="ILogger{TCategoryName}" /> for rather than to
    /// this library. Prefer <see cref="UseLoggerFactory" /> when you want the
    /// library's output filterable on its own.
    /// <para>
    /// One logger is held at a time, so calling this again replaces the previous
    /// one rather than adding to it.
    /// </para>
    /// </remarks>
    /// <param name="logger">The logger to write handled exceptions to.</param>
    /// <param name="level">
    /// The level to write them at. Default: <see cref="LogLevel.Debug" />, because
    /// a <c>Try</c> producing a <c>None</c> or an <c>Err</c> is an ordinary
    /// outcome rather than a fault. OpenTelemetry's semantic conventions suggest
    /// <see cref="LogLevel.Warning" /> for a handled exception; pass it if you
    /// would rather follow them.
    /// </param>
    /// <returns>This instance, for chaining more configurations.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="logger" /> is null. Leave the logging unconfigured rather
    /// than clearing it with null.
    /// </exception>
    public MonadLoggingOptions UseLogger(
        ILogger logger,
        LogLevel level = LogLevel.Debug)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Level = level;
        ExceptionHandledLogger.Subscribe();
        return this;
    }

    /// <summary>Sends handled exceptions to a logger created under this library's own category.</summary>
    /// <remarks>
    /// Creates one logger in the <see cref="LoggerCategory" /> category, so the
    /// library's output can be filtered without touching the rest of your
    /// application's logging.
    /// </remarks>
    /// <param name="loggerFactory">The factory to create the logger from.</param>
    /// <param name="level">
    /// The level to write them at. Default: <see cref="LogLevel.Debug" />. See
    /// <see cref="UseLogger" /> for why.
    /// </param>
    /// <returns>This instance, for chaining more configurations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loggerFactory" /> is null.</exception>
    public MonadLoggingOptions UseLoggerFactory(
        ILoggerFactory loggerFactory,
        LogLevel level = LogLevel.Debug)
    {
        if (loggerFactory is null)
        {
            throw new ArgumentNullException(nameof(loggerFactory));
        }

        return UseLogger(loggerFactory.CreateLogger(LoggerCategory), level);
    }
}
