namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using global::Waystone.WideLogEvents;
using Microsoft.Extensions.Logging;

public interface IWideLogEventsSampler
{
    LogLevel GetLogLevel(WideLogEventScope scope);
    bool ShouldSample(WideLogEventScope scope);
}

internal sealed class DefaultWideLogEventsSampler : IWideLogEventsSampler
{
    private const double SuccessSampleRate = 0.05;
    private const double FailureSampleRate = 1.0;
    private const double IndeterminateSampleRate = 1.0;

    /// <inheritdoc />
    public LogLevel GetLogLevel(WideLogEventScope scope) =>
        scope.Outcome switch
        {
            SuccessWideLogEventOutcome => LogLevel.Information,
            FailureWideLogEventOutcome => LogLevel.Error,
            IndeterminateWideLogEventOutcome => LogLevel.Warning,
            var _ => LogLevel.None,
        };

    /// <inheritdoc />
    public bool ShouldSample(WideLogEventScope scope)
    {
#if DEBUG
        return true;
#else
        return scope.Outcome.Type switch
        {
            WideLogEventOutcomeType.Failure => Random.Shared.NextDouble()
             <= FailureSampleRate,
            WideLogEventOutcomeType.Success => Random.Shared.NextDouble()
             <= SuccessSampleRate,
            WideLogEventOutcomeType.Indeterminate => Random.Shared.NextDouble()
             <= IndeterminateSampleRate,
            var _ => false,
        };
#endif
    }
}
