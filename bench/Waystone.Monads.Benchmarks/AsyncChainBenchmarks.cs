namespace Waystone.Monads.Benchmarks;

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Options;
using Options.Extensions;

[MemoryDiagnoser]
public class AsyncChainBenchmarks
{
    private Option<int> _some = null!;
    private Option<int> _none = null!;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option.Some(42);
        _none = Option.None<int>();
    }

    [Benchmark]
    public ValueTask<Option<int>> SingleLinkOnSome() =>
        _some.MapAsync(static value => Task.FromResult(value + 1));

    [Benchmark]
    public ValueTask<Option<int>> SingleLinkOnNone() =>
        _none.MapAsync(static value => Task.FromResult(value + 1));

    [Benchmark]
    public Task<Option<int>> ThreeLinkChainOnSome() =>
        _some.MapAsync(static value => Task.FromResult(value + 1))
             .MapAsync(static value => value + 1)
             .MapAsync(static value => value + 1);

    [Benchmark]
    public Task<Option<int>> ThreeLinkChainOnNone() =>
        _none.MapAsync(static value => Task.FromResult(value + 1))
             .MapAsync(static value => value + 1)
             .MapAsync(static value => value + 1);
}
