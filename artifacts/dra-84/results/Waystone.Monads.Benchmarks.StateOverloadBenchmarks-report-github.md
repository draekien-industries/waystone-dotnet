```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method               | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| MapWithClosure       |  9.814 ns |  4.618 ns | 0.2531 ns |  1.00 |    0.03 | 0.0022 |     112 B |        1.00 |
| MapWithState         |  5.779 ns |  1.716 ns | 0.0941 ns |  0.59 |    0.02 | 0.0005 |      24 B |        0.21 |
| MapOrWithClosure     |  6.758 ns |  5.634 ns | 0.3088 ns |  0.69 |    0.03 | 0.0018 |      88 B |        0.79 |
| MapOrWithState       |  3.442 ns |  1.013 ns | 0.0555 ns |  0.35 |    0.01 |      - |         - |        0.00 |
| FilterWithClosure    |  4.331 ns |  3.410 ns | 0.1869 ns |  0.44 |    0.02 | 0.0018 |      88 B |        0.79 |
| FilterWithState      |  3.553 ns |  2.734 ns | 0.1498 ns |  0.36 |    0.02 |      - |         - |        0.00 |
| ResultMapWithClosure | 11.029 ns | 10.720 ns | 0.5876 ns |  1.12 |    0.06 | 0.0022 |     112 B |        1.00 |
| ResultMapWithState   |  7.607 ns |  5.910 ns | 0.3239 ns |  0.78 |    0.03 | 0.0005 |      24 B |        0.21 |
