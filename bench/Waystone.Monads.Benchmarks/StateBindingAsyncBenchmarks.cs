namespace Waystone.Monads.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Options;
using Options.Extensions;
using Results;
using Results.Extensions;
using System.Threading.Tasks;

/// <remarks>
/// <see cref="StateOverloadBenchmarks" /> compares three ways of reaching a
/// synchronous member: a closure, a state parameter, and a binding. The
/// asynchronous members have only two, because no state overload takes an async
/// delegate — before the binder existed, a closure was the only way to hand state
/// to one. So each pair here is the whole choice a caller has, not a subset of it.
/// <para>
/// The empty cases are measured because the doc comments claim them: a
/// <see cref="None{T}" /> or an <see cref="Err{TOk,TErr}" /> never invokes the
/// delegate, so the returned <see cref="ValueTask{TResult}" /> is already complete
/// and allocates no state machine. The closure baseline allocates its display
/// class on that branch anyway, having built it before the case was known.
/// </para>
/// <para>
/// Read that claim off the two predicate categories, not off
/// <c>MapAsync</c>. A predicate returns a <see cref="bool" />, so nothing but a
/// state machine could allocate and the binding column falls to zero. Mapping an
/// <see cref="Err{TOk,TErr}" /> changes <c>TOk</c> and so must build a fresh
/// result, which shows up as a non-zero figure that is the new instance rather
/// than a state machine — the right number for what mapping costs, the wrong one
/// to read the claim from.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class StateBindingAsyncBenchmarks
{
    private Option<int> _some = null!;
    private Option<int> _none = null!;
    private Result<int, string> _ok = null!;
    private Result<int, string> _err = null!;
    private int _addend;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option.Some(42);
        _none = Option.None<int>();
        _ok = Result.Ok<int, string>(42);
        _err = Result.Err<int, string>("failed");
        _addend = 1;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("IsSomeAndAsyncOnNone")]
    public ValueTask<bool> IsSomeAndAsyncOnNoneWithClosure()
    {
        int addend = _addend;

        return _none.IsSomeAndAsync(value => Task.FromResult(value > addend));
    }

    [Benchmark]
    [BenchmarkCategory("IsSomeAndAsyncOnNone")]
    public ValueTask<bool> IsSomeAndAsyncOnNoneWithBinding() =>
        _none.With(_addend)
             .IsSomeAndAsync(
                  static (value, addend) => Task.FromResult(value > addend));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("IsErrAndAsyncOnErr")]
    public ValueTask<bool> IsErrAndAsyncOnErrWithClosure()
    {
        int addend = _addend;

        return _err.IsErrAndAsync(
            error => Task.FromResult(error.Length > addend));
    }

    [Benchmark]
    [BenchmarkCategory("IsErrAndAsyncOnErr")]
    public ValueTask<bool> IsErrAndAsyncOnErrWithBinding() =>
        _err.With(_addend)
            .IsErrAndAsync(
                 static (error, addend) =>
                     Task.FromResult(error.Length > addend));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("MapAsyncOnSome")]
    public ValueTask<Option<int>> MapAsyncOnSomeWithClosure()
    {
        int addend = _addend;

        return _some.MapAsync(value => Task.FromResult(value + addend));
    }

    [Benchmark]
    [BenchmarkCategory("MapAsyncOnSome")]
    public ValueTask<Option<int>> MapAsyncOnSomeWithBinding() =>
        _some.With(_addend)
             .MapAsync(
                  static (value, addend) => Task.FromResult(value + addend));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("MapAsyncOnNone")]
    public ValueTask<Option<int>> MapAsyncOnNoneWithClosure()
    {
        int addend = _addend;

        return _none.MapAsync(value => Task.FromResult(value + addend));
    }

    [Benchmark]
    [BenchmarkCategory("MapAsyncOnNone")]
    public ValueTask<Option<int>> MapAsyncOnNoneWithBinding() =>
        _none.With(_addend)
             .MapAsync(
                  static (value, addend) => Task.FromResult(value + addend));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("AndThenAsyncOnSome")]
    public ValueTask<Option<int>> AndThenAsyncOnSomeWithClosure()
    {
        int addend = _addend;

        return _some.AndThenAsync(
            value => new ValueTask<Option<int>>(Option.Some(value + addend)));
    }

    [Benchmark]
    [BenchmarkCategory("AndThenAsyncOnSome")]
    public ValueTask<Option<int>> AndThenAsyncOnSomeWithBinding() =>
        _some.With(_addend)
             .AndThenAsync(
                  static (value, addend) => new ValueTask<Option<int>>(
                      Option.Some(value + addend)));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("MatchAsyncOnSome")]
    public ValueTask<int> MatchAsyncOnSomeWithClosure()
    {
        int addend = _addend;

        return _some.MatchAsync(
            value => Task.FromResult(value + addend),
            () => Task.FromResult(addend));
    }

    [Benchmark]
    [BenchmarkCategory("MatchAsyncOnSome")]
    public ValueTask<int> MatchAsyncOnSomeWithBinding() =>
        _some.With(_addend)
             .MatchAsync(
                  static (value, addend) => Task.FromResult(value + addend),
                  static addend => Task.FromResult(addend));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ResultMapAsyncOnOk")]
    public ValueTask<Result<int, string>> ResultMapAsyncOnOkWithClosure()
    {
        int addend = _addend;

        return _ok.MapAsync(value => Task.FromResult(value + addend));
    }

    [Benchmark]
    [BenchmarkCategory("ResultMapAsyncOnOk")]
    public ValueTask<Result<int, string>> ResultMapAsyncOnOkWithBinding() =>
        _ok.With(_addend)
           .MapAsync(static (value, addend) => Task.FromResult(value + addend));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ResultMapAsyncOnErr")]
    public ValueTask<Result<int, string>> ResultMapAsyncOnErrWithClosure()
    {
        int addend = _addend;

        return _err.MapAsync(value => Task.FromResult(value + addend));
    }

    [Benchmark]
    [BenchmarkCategory("ResultMapAsyncOnErr")]
    public ValueTask<Result<int, string>> ResultMapAsyncOnErrWithBinding() =>
        _err.With(_addend)
            .MapAsync(static (value, addend) => Task.FromResult(value + addend));
}
