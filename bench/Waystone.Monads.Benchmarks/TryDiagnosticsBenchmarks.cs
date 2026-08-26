namespace Waystone.Monads.Benchmarks;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using BenchmarkDotNet.Attributes;
using Diagnostics;
using Options;
using Results;
using Results.Errors;

[MemoryDiagnoser]
public class TryDiagnosticsBenchmarks
{
    private IDisposable? _allListeners;
    private IDisposable? _eventSubscription;
    private MeterListener? _meterListener;

    [GlobalSetup(
        Targets =
        [
            nameof(OptionTryThrowsObserved), nameof(ResultTryThrowsObserved),
        ])]
    public void Observe()
    {
        _meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == MonadDiagnostics.MeterName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            },
        };

        _meterListener.SetMeasurementEventCallback<long>(
            static (_, _, _, _) => { });
        _meterListener.Start();

        _allListeners = DiagnosticListener.AllListeners.Subscribe(
            new Observer<DiagnosticListener>(Attach));
    }

    [GlobalCleanup]
    public void StopObserving()
    {
        _eventSubscription?.Dispose();
        _allListeners?.Dispose();
        _meterListener?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public Option<int> OptionTrySucceeds() => Option.Try(static () => 42);

    [Benchmark]
    public Option<int> OptionTryThrows() =>
        Option.Try<int>(static () => throw new InvalidOperationException());

    [Benchmark]
    public Result<int, Error> ResultTryThrows() =>
        Result.Try<int>(static () => throw new InvalidOperationException());

    [Benchmark]
    public Option<int> OptionTryThrowsObserved() =>
        Option.Try<int>(static () => throw new InvalidOperationException());

    [Benchmark]
    public Result<int, Error> ResultTryThrowsObserved() =>
        Result.Try<int>(static () => throw new InvalidOperationException());

    private void Attach(DiagnosticListener listener)
    {
        if (listener.Name != MonadDiagnostics.ListenerName)
        {
            return;
        }

        _eventSubscription = listener.Subscribe(
            new Observer<KeyValuePair<string, object?>>(static _ => { }),
            static name => name == MonadDiagnostics.ExceptionHandledEventName);
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
