namespace Waystone.Monads.Diagnostics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Configs;

/// <summary>The names under which Waystone.Monads emits observability signals about itself.</summary>
/// <remarks>
/// The library instruments itself, so a consumer needs no Waystone package to
/// observe it: adding <c>Waystone.Monads</c> to the meters an OpenTelemetry
/// pipeline already collects is enough to receive the metrics, and any
/// <see cref="DiagnosticListener" /> subscriber can read the richer events. Every
/// name here is part of the public contract — dashboards and alert rules bind to
/// the strings rather than to the symbols, so no compiler and no deprecation
/// notice would catch a rename.
/// <para>
/// Emission is gated on whether anything is listening, so an unobserved process
/// pays for a pair of boolean checks and allocates nothing.
/// </para>
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
public static class MonadDiagnostics
{
    /// <summary>The name of the <see cref="System.Diagnostics.Metrics.Meter" /> the library's instruments belong to.</summary>
    /// <remarks>Pass this to whatever enrolls a meter in your metrics pipeline, such as OpenTelemetry's <c>AddMeter</c>.</remarks>
    public const string MeterName = "Waystone.Monads";

    /// <summary>The name of the <see cref="DiagnosticListener" /> the library's events are written to.</summary>
    /// <remarks>
    /// Shares its spelling with <see cref="MeterName" /> without colliding, because
    /// listeners and meters are resolved through separate registries.
    /// </remarks>
    public const string ListenerName = "Waystone.Monads";

    /// <summary>The name of the event written when the library swallows an exception.</summary>
    /// <remarks>
    /// Its payload is an <see cref="ExceptionHandled" />. Event names share one
    /// process-wide namespace, hence the qualified spelling.
    /// </remarks>
    public const string ExceptionHandledEventName =
        "Waystone.Monads.ExceptionHandled";

    /// <summary>The name of the event written when a scope is disposed out of order.</summary>
    /// <remarks>
    /// Its payload is a <see cref="ScopeDisposedOutOfOrder" />. The event is the
    /// only report of the misuse: <see cref="MonadOptionsScope.Dispose" /> declines
    /// to restore rather than throwing, because throwing from a <c>using</c> would
    /// displace whatever exception was already unwinding through it. Subscribe and
    /// throw from the subscriber to make it fatal in a test suite.
    /// </remarks>
    public const string ScopeDisposedOutOfOrderEventName =
        "Waystone.Monads.ScopeDisposedOutOfOrder";

    /// <summary>The name of the counter of exceptions the library has swallowed.</summary>
    /// <remarks>
    /// A monotonic <see cref="Counter{T}" /> of <see cref="long" />, counted in
    /// exceptions and tagged with <see cref="ErrorTypeTagKey" /> and
    /// <see cref="MonadTagKey" />.
    /// <para>
    /// It counts only exceptions <c>Try</c> and <c>TryAsync</c> caught, so an
    /// exception the library let propagate never reaches it. A cancellation is
    /// among those unless
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> has been called, which
    /// makes it a caught exception like any other and therefore counted.
    /// </para>
    /// </remarks>
    public const string ExceptionsHandledInstrumentName =
        "waystone.monads.exceptions_handled";

    /// <summary>The tag carrying the exception's fully qualified type name.</summary>
    /// <remarks>
    /// The OpenTelemetry semantic convention attribute of the same name. Exception
    /// types are a bounded set in any given application, so this needs no
    /// normalisation to stay within a metrics backend's cardinality budget.
    /// </remarks>
    public const string ErrorTypeTagKey = "error.type";

    /// <summary>The tag distinguishing an exception handled by an <c>Option</c> from one handled by a <c>Result</c>.</summary>
    /// <remarks>
    /// Takes <see cref="OptionMonadTagValue" /> or <see cref="ResultMonadTagValue" />
    /// and nothing else.
    /// </remarks>
    public const string MonadTagKey = "waystone.monads.monad";

    /// <summary>The <see cref="MonadTagKey" /> value meaning <see cref="MonadKind.Option" />.</summary>
    public const string OptionMonadTagValue = "option";

    /// <summary>The <see cref="MonadTagKey" /> value meaning <see cref="MonadKind.Result" />.</summary>
    public const string ResultMonadTagValue = "result";

    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> ExceptionsHandled =
        Meter.CreateCounter<long>(
            ExceptionsHandledInstrumentName,
            "{exception}",
            "The number of exceptions caught and handled by Try and TryAsync.");

    internal static readonly DiagnosticListener Listener = new(ListenerName);

    internal static void RecordExceptionHandled(
        Exception exception,
        CallerInfo callerInfo,
        MonadKind monad)
    {
        if (ExceptionsHandled.Enabled)
        {
            ExceptionsHandled.Add(
                1,
                new KeyValuePair<string, object?>(
                    ErrorTypeTagKey,
                    exception.GetType().FullName),
                new KeyValuePair<string, object?>(MonadTagKey, TagValue(monad)));
        }

        if (Listener.IsEnabled(ExceptionHandledEventName))
        {
            Listener.Write(
                ExceptionHandledEventName,
                new ExceptionHandled(exception, callerInfo, monad));
        }
    }

    internal static void RecordScopeDisposedOutOfOrder(
        MonadOptions? scope,
        MonadOptions? live)
    {
        if (Listener.IsEnabled(ScopeDisposedOutOfOrderEventName))
        {
            Listener.Write(
                ScopeDisposedOutOfOrderEventName,
                new ScopeDisposedOutOfOrder(scope, live));
        }
    }

    private static string TagValue(MonadKind monad) =>
        monad == MonadKind.Result
            ? ResultMonadTagValue
            : OptionMonadTagValue;
}
