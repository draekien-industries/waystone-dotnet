namespace Waystone.Monads.Extensions.Logging.Configs;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Monads.Configs;

/// <summary>
/// Configuration for reporting the exceptions Waystone.Monads swallows through a
/// <see cref="ILogger" />.
/// </summary>
/// <remarks>
/// Attached to a <see cref="MonadOptions" /> snapshot, so it follows whatever
/// options scope is current when an exception is handled. The subscription that
/// feeds it is process-wide and cannot itself be scoped, which is why the logger
/// and the level live here rather than on the subscription:
/// <c>MonadOptions.BeginScope(o =&gt; o.UseLogger(other, LogLevel.Warning))</c>
/// redirects the logging for one asynchronous flow and leaves the rest alone.
/// <para>
/// Nothing is logged until one of the configuration methods is called. Until
/// then the logger is <see cref="NullLogger" /> and no subscription exists, so
/// the library's diagnostic event is never even built.
/// </para>
/// </remarks>
public sealed class MonadLoggingOptions
{
    /// <summary>The category assigned to loggers this package creates for itself.</summary>
    /// <remarks>
    /// Used by <c>UseLoggerFactory</c> and, through it, by
    /// <c>UseLoggerFactoryFrom</c>. Filter on this to raise or silence the
    /// library's own output — for example an <c>appsettings.json</c> entry of
    /// <c>"Waystone.Monads": "Warning"</c>. A logger supplied to
    /// <c>UseLogger</c> keeps whatever category it already had.
    /// </remarks>
    public const string LoggerCategory = "Waystone.Monads";

    internal static readonly int Slot = MonadOptionsSlot.Allocate();

    internal static readonly MonadLoggingOptions Default =
        new(NullLogger.Instance, LogLevel.Debug);

    internal MonadLoggingOptions(ILogger logger, LogLevel level)
    {
        Logger = logger;
        Level = level;
    }

    internal static MonadLoggingOptions Current =>
        MonadOptions.Current.Satellite<MonadLoggingOptions>(Slot) ?? Default;

    internal ILogger Logger { get; }

    internal LogLevel Level { get; }
}
