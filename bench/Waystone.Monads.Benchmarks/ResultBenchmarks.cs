namespace Waystone.Monads.Benchmarks;

using System;
using BenchmarkDotNet.Attributes;
using Results;

[MemoryDiagnoser]
public class ResultBenchmarks
{
    private static readonly Func<int, int> Increment = value => value + 1;
    private static readonly Func<string, string> Shout = error => error + "!";
    private static readonly Func<int, string> Describe = value => value.ToString();
    private static readonly Func<string, string> DescribeErr = error => error;

    private Result<int, string> _ok = null!;
    private Result<int, string> _err = null!;

    [GlobalSetup]
    public void Setup()
    {
        _ok = Result.Ok<int, string>(42);
        _err = Result.Err<int, string>("boom");
    }

    [Benchmark]
    public Result<int, string> OkConstruction() => Result.Ok<int, string>(42);

    [Benchmark]
    public Result<int, string> ErrConstruction() =>
        Result.Err<int, string>("boom");

    [Benchmark]
    public string MatchOnOk() => _ok.Match(Describe, DescribeErr);

    [Benchmark]
    public string MatchOnErr() => _err.Match(Describe, DescribeErr);

    [Benchmark]
    public Result<int, string> MapOnOk() => _ok.Map(Increment);

    [Benchmark]
    public Result<int, string> MapOnErr() => _err.Map(Increment);

    [Benchmark]
    public Result<int, string> MapErrOnErr() => _err.MapErr(Shout);

    [Benchmark]
    public int UnwrapOrOnOk() => _ok.UnwrapOr(0);

    [Benchmark]
    public int UnwrapOrOnErr() => _err.UnwrapOr(0);
}
