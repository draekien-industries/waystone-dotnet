namespace Serilog.Enrichers.Waystone.WideLogEvents;

using Core;
using Events;
using static Events.LogEventLevel;

/// <summary>
/// Provides a sampling filter for Serilog log events based on their level and
/// configured sampling rates.
/// </summary>
/// <remarks>
/// The <see cref="WideLogEventsSamplingFilter" /> is used to control the logging
/// of events at different levels
/// by applying sampling rates defined in the
/// <see cref="WideLogEventsSamplingOptions" />.
/// It utilizes an implementation of <see cref="IRandomDoubleProvider" /> to
/// generate random values for comparison
/// against the defined sampling rates.
/// </remarks>
internal sealed class WideLogEventsSamplingFilter(
    WideLogEventsSamplingOptions options) : ILogEventFilter
{
    /// <inheritdoc />
    public bool IsEnabled(LogEvent logEvent)
    {
        double randomDouble = options.RandomDoubleProvider.NextDouble();

        return logEvent.Level switch
        {
            Debug => options.DebugSampleRate >= randomDouble,
            Information => options.InformationSampleRate >= randomDouble,
            Warning => options.WarningSampleRate >= randomDouble,
            Error => options.ErrorSampleRate >= randomDouble,
            Fatal => options.FatalSampleRate >= randomDouble,
            Verbose => options.VerboseSampleRate >= randomDouble,
            var _ => true,
        };
    }
}
