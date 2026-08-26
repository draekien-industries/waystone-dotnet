```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]     : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4


```
| Method                  | Mean         | Error      | StdDev     | Ratio    | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |-------------:|-----------:|-----------:|---------:|--------:|-------:|----------:|------------:|
| OptionTrySucceeds       |     2.138 ns |  0.0779 ns |  0.1013 ns |     1.00 |    0.06 | 0.0005 |      24 B |        1.00 |
| OptionTryThrows         | 3,172.512 ns | 61.6896 ns | 60.5874 ns | 1,486.83 |   72.31 | 0.0076 |     480 B |       20.00 |
| ResultTryThrows         | 3,255.127 ns | 64.3034 ns | 74.0519 ns | 1,525.54 |   76.50 | 0.0114 |     616 B |       25.67 |
| OptionTryThrowsObserved | 3,111.532 ns | 36.6796 ns | 30.6291 ns | 1,458.25 |   67.03 | 0.0076 |     520 B |       21.67 |
| ResultTryThrowsObserved | 3,236.225 ns | 63.4667 ns | 65.1756 ns | 1,516.69 |   74.38 | 0.0114 |     656 B |       27.33 |
