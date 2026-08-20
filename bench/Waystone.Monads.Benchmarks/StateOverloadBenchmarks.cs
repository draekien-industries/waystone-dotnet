namespace Waystone.Monads.Benchmarks;

using BenchmarkDotNet.Attributes;
using Options;
using Results;

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
    public int MapOrWithClosure()
    {
        int addend = _addend;

        return _some.MapOr(0, value => value + addend);
    }

    [Benchmark]
    public int MapOrWithState() =>
        _some.MapOr(_addend, 0, static (value, addend) => value + addend);

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
    public Result<int, string> ResultMapWithClosure()
    {
        int addend = _addend;

        return _ok.Map(value => value + addend);
    }

    [Benchmark]
    public Result<int, string> ResultMapWithState() =>
        _ok.Map(_addend, static (value, addend) => value + addend);
}
