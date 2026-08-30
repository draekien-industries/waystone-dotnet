namespace Waystone.Monads.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Options;
using Results;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadDiagnostics))]
public sealed class MonadDiagnosticsTests
{
    [Fact]
    public void GivenAThrowingFactory_WhenOptionTryRuns_ThenCountItAgainstOption()
    {
        using var counter = new Probe();

        Option.Try<int>(() => throw new ProbeException());

        counter.MonadTags().ShouldBe([MonadDiagnostics.OptionMonadTagValue]);
    }

    [Fact]
    public void GivenAThrowingFactory_WhenResultTryRuns_ThenCountItAgainstResult()
    {
        using var counter = new Probe();

        Result.Try<int>(() => throw new ProbeException());

        counter.MonadTags().ShouldBe([MonadDiagnostics.ResultMonadTagValue]);
    }

    [Fact]
    public void GivenAFactoryThatReturns_WhenOptionTryRuns_ThenCountNothing()
    {
        using var counter = new Probe();

        Option.Try(() => 1);

        counter.MonadTags().ShouldBeEmpty();
    }

    [Fact]
    public void GivenAThrowingFactory_WhenOptionTryRuns_ThenTagTheExceptionType()
    {
        using var counter = new Probe();

        Option.Try<int>(() => throw new ProbeException());

        counter.Measurements()
               .ShouldHaveSingleItem()
               .Tags[MonadDiagnostics.ErrorTypeTagKey]
               .ShouldBe(typeof(ProbeException).FullName);
    }

    [Fact]
    public void GivenASubscriber_WhenOptionTryThrows_ThenWriteTheHandledEvent()
    {
        using var recorder = NewRecorder();

        var exception = new ProbeException();
        Option.Try<int>(() => throw exception);

        ExceptionHandled handled = recorder.Recorded().ShouldHaveSingleItem();
        handled.Exception.ShouldBeSameAs(exception);
        handled.Monad.ShouldBe(MonadKind.Option);
        handled.Caller.MemberName.ShouldBe(
            nameof(GivenASubscriber_WhenOptionTryThrows_ThenWriteTheHandledEvent));
        handled.Caller.LineNumber.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GivenASubscriber_WhenResultTryThrows_ThenReportTheResultMonad()
    {
        using var recorder = NewRecorder();

        Result.Try<int>(() => throw new ProbeException());

        recorder.Recorded().ShouldHaveSingleItem().Monad.ShouldBe(MonadKind.Result);
    }

    private sealed class ProbeException() : Exception("Probe.");

    private static EventRecorder<ExceptionHandled> NewRecorder() =>
        new(
            MonadDiagnostics.ExceptionHandledEventName,
            static handled => handled.Exception is ProbeException);

    /// <summary>
    /// Collects the exceptions_handled counter, keeping only the measurements
    /// this test caused. The meter is process-wide, so a test running in another
    /// collection can land its own measurements in the same snapshot.
    /// </summary>
    private sealed class Probe : IDisposable
    {
        private readonly MetricCollector<long> _collector = new(
            null,
            MonadDiagnostics.MeterName,
            MonadDiagnostics.ExceptionsHandledInstrumentName);

        public void Dispose()
        {
            _collector.Dispose();
        }

        public IReadOnlyList<CollectedMeasurement<long>> Measurements() =>
            _collector.GetMeasurementSnapshot()
                      .Where(
                           measurement =>
                               Equals(
                                   measurement.Tags[
                                       MonadDiagnostics.ErrorTypeTagKey],
                                   typeof(ProbeException).FullName))
                      .ToList();

        public IReadOnlyList<object?> MonadTags() =>
            Measurements()
               .Select(
                    measurement =>
                        measurement.Tags[MonadDiagnostics.MonadTagKey])
               .ToList();
    }
}
