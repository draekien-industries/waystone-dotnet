namespace Waystone.Monads.Benchmarks;

using System;
using BenchmarkDotNet.Attributes;
using Options;

[MemoryDiagnoser]
public class OptionBenchmarks
{
    private static readonly Func<int, int> Increment = value => value + 1;
    private static readonly Func<int, bool> IsPositive = value => value > 0;
    private static readonly Func<int, string> Describe = value => value.ToString();
    private static readonly Func<string> DescribeNone = () => "none";

    private Option<int> _some = null!;
    private Option<int> _none = null!;

    [GlobalSetup]
    public void Setup()
    {
        _some = Option.Some(42);
        _none = Option.None<int>();
    }

    [Benchmark]
    public Option<int> SomeConstruction() => Option.Some(42);

    [Benchmark]
    public Option<int> NoneConstruction() => Option.None<int>();

    [Benchmark]
    public Option<int> ImplicitConversion() => 42;

    [Benchmark]
    public Option<int> FromNullable() => Option.FromNullable((int?)42);

    [Benchmark]
    public string MatchOnSome() => _some.Match(Describe, DescribeNone);

    [Benchmark]
    public string MatchOnNone() => _none.Match(Describe, DescribeNone);

    [Benchmark]
    public Option<int> MapOnSome() => _some.Map(Increment);

    [Benchmark]
    public Option<int> MapOnNone() => _none.Map(Increment);

    [Benchmark]
    public Option<int> FilterKeeping() => _some.Filter(IsPositive);

    [Benchmark]
    public Option<int> FilterRejecting() => _some.Filter(static value => value < 0);

    [Benchmark]
    public int UnwrapOrOnSome() => _some.UnwrapOr(0);

    [Benchmark]
    public int UnwrapOrOnNone() => _none.UnwrapOr(0);
}
