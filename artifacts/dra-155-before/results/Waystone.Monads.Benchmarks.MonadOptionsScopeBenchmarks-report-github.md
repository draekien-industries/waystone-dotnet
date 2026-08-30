```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                         | Mean        | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |------------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ReadFallbackCode               |   4.0744 ns |  6.1649 ns | 0.3379 ns |  1.00 |    0.10 | 0.0005 |      24 B |        1.00 |
| ScopeEntryAndExit              |  24.2166 ns | 21.0522 ns | 1.1539 ns |  5.97 |    0.51 | 0.0025 |     128 B |        5.33 |
| ScopeEntryAndExitWithSatellite | 156.1507 ns | 35.5432 ns | 1.9482 ns | 38.51 |    2.89 | 0.0253 |    1280 B |       53.33 |
| ConfigureTheGlobal             |   0.9108 ns |  0.7371 ns | 0.0404 ns |  0.22 |    0.02 |      - |         - |        0.00 |
