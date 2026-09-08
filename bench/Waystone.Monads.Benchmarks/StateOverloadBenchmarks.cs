namespace Waystone.Monads.Benchmarks;

using BenchmarkDotNet.Attributes;
using Options;
using Options.Extensions;
using Results;
using Results.Extensions;

[MemoryDiagnoser]
public class StateOverloadBenchmarks
{
    private Option<int> _some = null!;
    private Result<int, string> _ok = null!;
    private int _addend;
    private int _threshold;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option.Some(42);
        _ok = Result.Ok<int, string>(42);
        _addend = 1;
        _threshold = 0;
    }

    [Benchmark(Baseline = true)]
    public Option<int> MapWithClosure()
    {
        int addend = _addend;

        return _some.Map(value => value + addend);
    }

    [Benchmark]
    public Option<int> MapWithState() =>
        _some.Map(_addend, static (value, addend) => value + addend);

    [Benchmark]
    public Option<int> MapWithBinding() =>
        _some.With(_addend).Map(static (value, addend) => value + addend);

    [Benchmark]
    public int MapOrWithClosure()
    {
        int addend = _addend;

        return _some.MapOr(0, value => value + addend);
    }

    [Benchmark]
    public int MapOrWithState() =>
        _some.MapOr(_addend, 0, static (value, addend) => value + addend);

    [Benchmark]
    public int MapOrWithBinding() =>
        _some.With(_addend).MapOr(0, static (value, addend) => value + addend);

    [Benchmark]
    public Option<int> FilterWithClosure()
    {
        int threshold = _threshold;

        return _some.Filter(value => value > threshold);
    }

    [Benchmark]
    public Option<int> FilterWithState() =>
        _some.Filter(
            _threshold,
            static (value, threshold) => value > threshold);

    [Benchmark]
    public Option<int> FilterWithBinding() =>
        _some.With(_threshold)
             .Filter(static (value, threshold) => value > threshold);

    [Benchmark]
    public Result<int, string> ResultMapWithClosure()
    {
        int addend = _addend;

        return _ok.Map(value => value + addend);
    }

    [Benchmark]
    public Result<int, string> ResultMapWithState() =>
        _ok.Map(_addend, static (value, addend) => value + addend);

    [Benchmark]
    public Result<int, string> ResultMapWithBinding() =>
        _ok.With(_addend).Map(static (value, addend) => value + addend);

    [Benchmark]
    public Option<int> TryWithClosure()
    {
        int addend = _addend;

        return Option.Try(() => 42 + addend);
    }

    [Benchmark]
    public Option<int> TryWithState() =>
        Option.Try(_addend, static addend => 42 + addend);

    [Benchmark]
    public Option<int> TryWithBinding() =>
        Option.With(_addend).Try(static addend => 42 + addend);

    [Benchmark]
    public Result<int, string> ResultTryWithClosure()
    {
        int addend = _addend;

        return Result.Try(() => 42 + addend, static ex => ex.Message);
    }

    [Benchmark]
    public Result<int, string> ResultTryWithState() =>
        Result.Try(
            _addend,
            static addend => 42 + addend,
            static ex => ex.Message);

    [Benchmark]
    public Result<int, string> ResultTryWithBinding() =>
        Result.With(_addend)
              .Try(static addend => 42 + addend, static ex => ex.Message);
}
