```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]     : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4


```
| Method               | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| MapWithClosure       | 8.712 ns | 0.1211 ns | 0.1133 ns |  1.00 |    0.02 | 0.0022 |     112 B |        1.00 |
| MapWithState         | 5.044 ns | 0.0705 ns | 0.0660 ns |  0.58 |    0.01 | 0.0005 |      24 B |        0.21 |
| MapOrWithClosure     | 5.891 ns | 0.0951 ns | 0.0890 ns |  0.68 |    0.01 | 0.0018 |      88 B |        0.79 |
| MapOrWithState       | 2.906 ns | 0.0220 ns | 0.0205 ns |  0.33 |    0.00 |      - |         - |        0.00 |
| FilterWithClosure    | 3.906 ns | 0.0645 ns | 0.0603 ns |  0.45 |    0.01 | 0.0018 |      88 B |        0.79 |
| FilterWithState      | 3.087 ns | 0.0396 ns | 0.0370 ns |  0.35 |    0.01 |      - |         - |        0.00 |
| ResultMapWithClosure | 9.499 ns | 0.1259 ns | 0.1178 ns |  1.09 |    0.02 | 0.0022 |     112 B |        1.00 |
| ResultMapWithState   | 6.139 ns | 0.0771 ns | 0.0644 ns |  0.70 |    0.01 | 0.0005 |      24 B |        0.21 |
| TryWithClosure       | 7.377 ns | 0.1183 ns | 0.1107 ns |  0.85 |    0.02 | 0.0022 |     112 B |        1.00 |
| TryWithState         | 4.231 ns | 0.0709 ns | 0.0629 ns |  0.49 |    0.01 | 0.0005 |      24 B |        0.21 |
| ResultTryWithClosure | 7.806 ns | 0.0736 ns | 0.0688 ns |  0.90 |    0.01 | 0.0022 |     112 B |        1.00 |
| ResultTryWithState   | 3.880 ns | 0.0746 ns | 0.0623 ns |  0.45 |    0.01 | 0.0005 |      24 B |        0.21 |
