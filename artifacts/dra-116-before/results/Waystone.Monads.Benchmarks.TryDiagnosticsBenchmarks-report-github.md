```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]     : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4


```
| Method            | Mean         | Error      | StdDev     | Ratio    | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------------:|-----------:|-----------:|---------:|--------:|-------:|----------:|------------:|
| OptionTrySucceeds |     2.159 ns |  0.0773 ns |  0.0890 ns |     1.00 |    0.06 | 0.0005 |      24 B |        1.00 |
| OptionTryThrows   | 3,217.457 ns | 61.6444 ns | 80.1551 ns | 1,492.29 |   68.97 | 0.0076 |     480 B |       20.00 |
| ResultTryThrows   | 3,220.745 ns | 62.3224 ns | 74.1904 ns | 1,493.81 |   67.58 | 0.0114 |     616 B |       25.67 |
