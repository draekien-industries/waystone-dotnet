namespace Waystone.Monads.Benchmarks;

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Options;
using Options.Extensions;

/// <summary>
/// The cost of making an async chain reusable as a step. The chain itself returns
/// <c>ValueTask</c>, which no <c>*Async</c> member accepts as a delegate result, so
/// a chain can only become a step by being converted to a <c>Task</c>. These
/// measure the two ways of converting it against not converting at all, on a
/// synchronously completing source and on a pending one.
/// </summary>
[MemoryDiagnoser]
public class AsyncChainReuseBenchmarks
{
    private Option<int> _some = null!;

    [GlobalSetup]
    public void Setup() => _some = Option.Some(42);

    private static async Task<Option<int>> PendingAsync(Option<int> option)
    {
        await Task.Yield();

        return option;
    }

    private ValueTask<Option<int>> CompletedChain() =>
        Task.FromResult(_some)
            .MapAsync(static value => value + 1)
            .MapAsync(static value => value + 1);

    private ValueTask<Option<int>> PendingChain() =>
        PendingAsync(_some)
           .MapAsync(static value => value + 1)
           .MapAsync(static value => value + 1);

    [Benchmark]
    public ValueTask<Option<int>> Completed_ValueTask_NotAStep() =>
        CompletedChain();

    [Benchmark]
    public Task<Option<int>> Completed_AsTask() => CompletedChain().AsTask();

    [Benchmark]
    public async Task<Option<int>> Completed_AsyncAwait() =>
        await CompletedChain();

    [Benchmark]
    public ValueTask<Option<int>> Pending_ValueTask_NotAStep() => PendingChain();

    [Benchmark]
    public Task<Option<int>> Pending_AsTask() => PendingChain().AsTask();

    [Benchmark]
    public async Task<Option<int>> Pending_AsyncAwait() => await PendingChain();
}
