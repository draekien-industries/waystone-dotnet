namespace Waystone.Monads.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        using var recorder = new EventRecorder();

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
        using var recorder = new EventRecorder();

        Result.Try<int>(() => throw new ProbeException());

        recorder.Recorded().ShouldHaveSingleItem().Monad.ShouldBe(MonadKind.Result);
    }

    private sealed class ProbeException() : Exception("Probe.");

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

    /// <summary>
    /// Subscribes the way a consumer does — through
    /// <see cref="DiagnosticListener.AllListeners" /> rather than through the
    /// internal listener field — and keeps only this test's own events.
    /// </summary>
    private sealed class EventRecorder : IDisposable
    {
        private readonly IDisposable _allListeners;

        private readonly ConcurrentQueue<ExceptionHandled> _events = new();

        private IDisposable? _subscription;

        public EventRecorder()
        {
            _allListeners = DiagnosticListener.AllListeners.Subscribe(
                new Observer<DiagnosticListener>(Attach));
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _allListeners.Dispose();
        }

        public IReadOnlyList<ExceptionHandled> Recorded() =>
            _events.Where(handled => handled.Exception is ProbeException)
                   .ToList();

        private void Attach(DiagnosticListener listener)
        {
            if (listener.Name != MonadDiagnostics.ListenerName)
            {
                return;
            }

            _subscription = listener.Subscribe(
                new Observer<KeyValuePair<string, object?>>(Record),
                name => name == MonadDiagnostics.ExceptionHandledEventName);
        }

        private void Record(KeyValuePair<string, object?> written)
        {
            if (written.Value is ExceptionHandled handled)
            {
                _events.Enqueue(handled);
            }
        }
    }

    private sealed class Observer<T>(Action<T> onNext) : IObserver<T>
    {
        public void OnCompleted()
        { }

        public void OnError(Exception error)
        { }

        public void OnNext(T value)
        {
            onNext(value);
        }
    }
}
