```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]     : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4


```
| Method               | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| CreateNone           |  0.1807 ns | 0.0125 ns | 0.0111 ns |   1.00 |    0.08 |      - |         - |          NA |
| FilterThatRejects    |  0.3066 ns | 0.0178 ns | 0.0157 ns |   1.70 |    0.13 |      - |         - |          NA |
| MapOnNone            |  4.8986 ns | 0.0454 ns | 0.0402 ns |  27.21 |    1.58 |      - |         - |          NA |
| ZipOnNone            |  2.5846 ns | 0.0362 ns | 0.0321 ns |  14.36 |    0.85 |      - |         - |          NA |
| XorOnTwoSome         |  0.1837 ns | 0.0104 ns | 0.0087 ns |   1.02 |    0.08 |      - |         - |          NA |
| MapAsyncShortCircuit | 19.0779 ns | 0.2357 ns | 0.2205 ns | 105.96 |    6.22 |      - |         - |          NA |
| SomeThenUnwrap       |  2.4583 ns | 0.0589 ns | 0.0551 ns |  13.65 |    0.84 | 0.0005 |      24 B |          NA |
