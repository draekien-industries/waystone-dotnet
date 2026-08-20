```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                        | Mean       | Error     | StdDev    | Gen0   | Allocated |
|------------------------------ |-----------:|----------:|----------:|-------:|----------:|
| SingleLinkOnSome              |  13.956 ns | 12.470 ns | 0.6835 ns | 0.0029 |     144 B |
| SingleLinkOnNone              |   8.461 ns |  3.538 ns | 0.1940 ns | 0.0005 |      24 B |
| ThreeLinkChainOnSome          |  33.999 ns |  5.649 ns | 0.3097 ns | 0.0057 |     288 B |
| ThreeLinkChainOnNone          |  22.498 ns |  6.829 ns | 0.3743 ns | 0.0014 |      72 B |
| ThreeLinkChainOnCompletedTask |  41.914 ns | 38.502 ns | 2.1104 ns | 0.0072 |     360 B |
| ThreeLinkChainOnPendingTask   | 619.115 ns | 31.040 ns | 1.7014 ns | 0.0172 |     866 B |
