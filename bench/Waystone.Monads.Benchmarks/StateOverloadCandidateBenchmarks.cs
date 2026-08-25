namespace Waystone.Monads.Benchmarks;

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Options;
using Results;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class StateOverloadCandidateBenchmarks
{
    private Option<int> _some = null!;
    private Result<int, string> _ok = null!;
    private int _threshold;
    private int _addend;
    private int _fallback;
    private string _fallbackError = null!;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option.Some(42);
        _ok = Result.Ok<int, string>(42);
        _threshold = 0;
        _addend = 1;
        _fallback = 7;
        _fallbackError = "boom";
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Predicate")]
    public bool IsSomeAndWithClosure()
    {
        int threshold = _threshold;

        return _some.IsSomeAnd(value => value > threshold);
    }

    [Benchmark]
    [BenchmarkCategory("Predicate")]
    public bool IsSomeAndWithState() =>
        _some.IsSomeAnd(
            _threshold,
            static (value, threshold) => value > threshold);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("MatchFunc")]
    public int MatchFuncWithClosure()
    {
        int addend = _addend;

        return _some.Match(value => value + addend, () => addend);
    }

    [Benchmark]
    [BenchmarkCategory("MatchFunc")]
    public int MatchFuncWithState() =>
        _some.Match(
            _addend,
            static (value, addend) => value + addend,
            static addend => addend);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("MatchAction")]
    public void MatchActionWithClosure()
    {
        int addend = _addend;

        _some.Match(value => Consume(value + addend), () => Consume(addend));
    }

    [Benchmark]
    [BenchmarkCategory("MatchAction")]
    public void MatchActionWithState() =>
        _some.Match(
            _addend,
            static (value, addend) => Consume(value + addend),
            static addend => Consume(addend));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Inspect")]
    public Option<int> InspectWithClosure()
    {
        int addend = _addend;

        return _some.Inspect(value => Consume(value + addend));
    }

    [Benchmark]
    [BenchmarkCategory("Inspect")]
    public Option<int> InspectWithState() =>
        _some.Inspect(
            _addend,
            static (value, addend) => Consume(value + addend));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("MapOrDefault")]
    public int MapOrDefaultWithClosure()
    {
        int addend = _addend;

        return _some.MapOrDefault(value => value + addend);
    }

    [Benchmark]
    [BenchmarkCategory("MapOrDefault")]
    public int MapOrDefaultWithState() =>
        _some.MapOrDefault(
            _addend,
            static (value, addend) => value + addend);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("UnwrapOrElse")]
    public int UnwrapOrElseWithClosure()
    {
        int fallback = _fallback;

        return _some.UnwrapOrElse(() => fallback);
    }

    [Benchmark]
    [BenchmarkCategory("UnwrapOrElse")]
    public int UnwrapOrElseWithState() =>
        _some.UnwrapOrElse(_fallback, static fallback => fallback);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("OrElse")]
    public Option<int> OrElseWithClosure()
    {
        int fallback = _fallback;

        return _some.OrElse(() => Option.Some(fallback));
    }

    [Benchmark]
    [BenchmarkCategory("OrElse")]
    public Option<int> OrElseWithState() =>
        _some.OrElse(_fallback, static fallback => Option.Some(fallback));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("OkOrElse")]
    public Result<int, string> OkOrElseWithClosure()
    {
        string fallbackError = _fallbackError;

        return _some.OkOrElse(() => fallbackError);
    }

    [Benchmark]
    [BenchmarkCategory("OkOrElse")]
    public Result<int, string> OkOrElseWithState() =>
        _some.OkOrElse(
            _fallbackError,
            static fallbackError => fallbackError);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ResultPredicate")]
    public bool IsOkAndWithClosure()
    {
        int threshold = _threshold;

        return _ok.IsOkAnd(value => value > threshold);
    }

    [Benchmark]
    [BenchmarkCategory("ResultPredicate")]
    public bool IsOkAndWithState() =>
        IsOkAnd(
            _ok,
            _threshold,
            static (value, threshold) => value > threshold);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ResultMatchFunc")]
    public int ResultMatchFuncWithClosure()
    {
        int addend = _addend;

        return _ok.Match(value => value + addend, _ => addend);
    }

    [Benchmark]
    [BenchmarkCategory("ResultMatchFunc")]
    public int ResultMatchFuncWithState() =>
        Match(
            _ok,
            _addend,
            static (value, addend) => value + addend,
            static (string _, int addend) => addend);

    private static int Consume(int value) => value;

    private static bool IsOkAnd<TOk, TErr, TState>(
        Result<TOk, TErr> result,
        TState state,
        Func<TOk, TState, bool> predicate)
        where TOk : notnull where TErr : notnull =>
        result is Ok<TOk, TErr> ok && predicate(ok.Unwrap(), state);

    private static TOut Match<TOk, TErr, TState, TOut>(
        Result<TOk, TErr> result,
        TState state,
        Func<TOk, TState, TOut> onOk,
        Func<TErr, TState, TOut> onErr)
        where TOk : notnull where TErr : notnull =>
        result is Ok<TOk, TErr> ok
            ? onOk(ok.Unwrap(), state)
            : onErr(result.UnwrapErr(), state);
}
