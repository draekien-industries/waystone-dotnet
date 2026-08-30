namespace Waystone.Monads.Benchmarks;

using BenchmarkDotNet.Attributes;
using Options;
using System.Threading.Tasks;

[MemoryDiagnoser]
public class HotPathBenchmarks
{
    private Option<int> _some = null!;
    private Option<int> _none = null!;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option.Some(42);
        _none = Option.None<int>();
    }

    [Benchmark(Baseline = true)]
    public Option<int> CreateNone() => Option.None<int>();

    [Benchmark]
    public Option<int> FilterThatRejects() =>
        _some.Filter(static value => value < 0);

    [Benchmark]
    public Option<string> MapOnNone() =>
        _none.Map(static value => value.ToString());

    [Benchmark]
    public Option<(int, int)> ZipOnNone() => _none.Zip(_none);

    [Benchmark]
    public Option<int> XorOnTwoSome() => _some.Xor(_some);

    [Benchmark]
    public async ValueTask<Option<string>> MapAsyncShortCircuit() =>
        await _none.MapAsync(static value => Task.FromResult(value.ToString()));

    [Benchmark]
    public async ValueTask<Option<string>> MapAsyncOnSome() =>
        await _some.MapAsync(static value => Task.FromResult(value.ToString()));

    [Benchmark]
    public Option<int> SomeThenUnwrap() => Option.Some(_some.Unwrap());
}
