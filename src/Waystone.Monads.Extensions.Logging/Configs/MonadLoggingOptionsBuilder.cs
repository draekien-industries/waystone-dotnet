namespace Waystone.Monads.Extensions.Logging.Configs;

using System;
using Microsoft.Extensions.Logging;
using Monads.Configs;

/// <summary>Assembles the <see cref="MonadLoggingOptions" /> for one snapshot.</summary>
/// <remarks>
/// Reached through the <see cref="MonadOptionsBuilder" /> extension methods in
/// <see cref="MonadOptionsBuilderExtensions" /> rather than constructed. The
/// settings it starts from are the ones already in effect, so a scope that
/// changes the level keeps the logger the surrounding scope installed.
/// </remarks>
public sealed class MonadLoggingOptionsBuilder : ISatelliteBuilder
{
    private MonadLoggingOptionsBuilder(MonadLoggingOptions source)
    {
        Logger = source.Logger;
        Level = source.Level;
    }

    internal ILogger Logger { get; set; }

    internal LogLevel Level { get; set; }

    object ISatelliteBuilder.Build() => new MonadLoggingOptions(Logger, Level);

    /// <summary>Sends handled exceptions to a logger you already hold.</summary>
    /// <remarks>
    /// The logger keeps its own category, so the output is attributed to whatever
    /// type you resolved <see cref="ILogger{TCategoryName}" /> for rather than to
    /// this library. Prefer <see cref="UseLoggerFactory" /> when you want the
    /// library's output filterable on its own.
    /// <para>
    /// One logger is held at a time, so calling this again replaces the previous
    /// one rather than adding to it. It also installs the process-wide
    /// subscription that feeds the logging, which is why nothing is logged until
    /// some snapshot has configured a logger.
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
    /// <returns>This builder, for chaining more configurations.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="logger" /> is null. Leave the logging unconfigured rather
    /// than clearing it with null.
    /// </exception>
    public MonadLoggingOptionsBuilder UseLogger(
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
    /// Creates one logger in the
    /// <see cref="MonadLoggingOptions.LoggerCategory" /> category, so the
    /// library's output can be filtered without touching the rest of your
    /// application's logging.
    /// </remarks>
    /// <param name="loggerFactory">The factory to create the logger from.</param>
    /// <param name="level">
    /// The level to write them at. Default: <see cref="LogLevel.Debug" />. See
    /// <see cref="UseLogger" /> for why.
    /// </param>
    /// <returns>This builder, for chaining more configurations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loggerFactory" /> is null.</exception>
    public MonadLoggingOptionsBuilder UseLoggerFactory(
        ILoggerFactory loggerFactory,
        LogLevel level = LogLevel.Debug)
    {
        if (loggerFactory is null)
        {
            throw new ArgumentNullException(nameof(loggerFactory));
        }

        return UseLogger(
            loggerFactory.CreateLogger(MonadLoggingOptions.LoggerCategory),
            level);
    }

    internal static MonadLoggingOptionsBuilder For(MonadOptionsBuilder builder) =>
        builder.Satellite(
            MonadLoggingOptions.Slot,
            static existing => new MonadLoggingOptionsBuilder(
                existing as MonadLoggingOptions ?? MonadLoggingOptions.Default));
}
