namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using global::Waystone.WideLogEvents;
using Microsoft.Extensions.Logging;

internal sealed class DisabledWideLogEventsSampler : IWideLogEventsSampler
{
    /// <inheritdoc />
    public LogLevel GetLogLevel(WideLogEventScope scope) => LogLevel.None;

    /// <inheritdoc />
    public bool ShouldSample(WideLogEventScope scope) => false;
}
