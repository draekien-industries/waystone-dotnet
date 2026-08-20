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
    public ValueTask<Option<int>> ThreeLinkChainOnSome() =>
        _some.MapAsync(static value => Task.FromResult(value + 1))
             .MapAsync(static value => value + 1)
             .MapAsync(static value => value + 1);

    [Benchmark]
    public ValueTask<Option<int>> ThreeLinkChainOnNone() =>
        _none.MapAsync(static value => Task.FromResult(value + 1))
             .MapAsync(static value => value + 1)
             .MapAsync(static value => value + 1);

    [Benchmark]
    public async Task<Option<int>> ThreeLinkChainOnCompletedTask()
    {
        var chain = Task.FromResult(_some)
                        .MapAsync(static value => value + 1)
                        .MapAsync(static value => value + 1)
                        .MapAsync(static value => value + 1);

        return await chain;
    }

    [Benchmark]
    public async Task<Option<int>> ThreeLinkChainOnPendingTask()
    {
        var chain = PendingAsync(_some)
                   .MapAsync(static value => value + 1)
                   .MapAsync(static value => value + 1)
                   .MapAsync(static value => value + 1);

        return await chain;
    }

    private static async Task<Option<int>> PendingAsync(Option<int> option)
    {
        await Task.Yield();

        return option;
    }
}
