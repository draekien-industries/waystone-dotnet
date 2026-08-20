```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method               | Mean     | Error     | StdDev   | Gen0   | Allocated |
|--------------------- |---------:|----------:|---------:|-------:|----------:|
| SingleLinkOnSome     | 16.87 ns | 11.164 ns | 0.612 ns | 0.0029 |     144 B |
| SingleLinkOnNone     | 10.06 ns |  7.095 ns | 0.389 ns | 0.0005 |      24 B |
| ThreeLinkChainOnSome | 46.80 ns | 70.557 ns | 3.867 ns | 0.0086 |     432 B |
| ThreeLinkChainOnNone | 27.00 ns | 28.734 ns | 1.575 ns | 0.0043 |     216 B |
