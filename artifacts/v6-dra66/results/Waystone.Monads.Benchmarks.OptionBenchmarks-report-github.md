```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method             | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------------- |----------:|----------:|----------:|-------:|----------:|
| SomeConstruction   | 5.0852 ns | 3.0118 ns | 0.1651 ns | 0.0014 |      72 B |
| NoneConstruction   | 1.6389 ns | 0.8895 ns | 0.0488 ns | 0.0005 |      24 B |
| ImplicitConversion | 7.2088 ns | 6.5636 ns | 0.3598 ns | 0.0024 |     120 B |
| FromNullable       | 8.1177 ns | 1.7245 ns | 0.0945 ns | 0.0019 |      96 B |
| MatchOnSome        | 4.1024 ns | 0.8693 ns | 0.0476 ns |      - |         - |
| MatchOnNone        | 2.7529 ns | 1.3574 ns | 0.0744 ns |      - |         - |
| MapOnSome          | 9.2446 ns | 0.8126 ns | 0.0445 ns | 0.0024 |     120 B |
| MapOnNone          | 4.1512 ns | 0.6097 ns | 0.0334 ns | 0.0005 |      24 B |
| FilterKeeping      | 0.2765 ns | 0.3737 ns | 0.0205 ns |      - |         - |
| FilterRejecting    | 2.0426 ns | 1.6300 ns | 0.0893 ns | 0.0005 |      24 B |
| UnwrapOrOnSome     | 0.0061 ns | 0.0534 ns | 0.0029 ns |      - |         - |
| UnwrapOrOnNone     | 0.0066 ns | 0.0932 ns | 0.0051 ns |      - |         - |
