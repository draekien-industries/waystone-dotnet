```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]     : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4


```
| Method               | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CreateNone           |  1.646 ns | 0.0220 ns | 0.0184 ns |  1.00 |    0.02 | 0.0005 |      24 B |        1.00 |
| FilterThatRejects    |  1.834 ns | 0.0311 ns | 0.0291 ns |  1.11 |    0.02 | 0.0005 |      24 B |        1.00 |
| MapOnNone            |  4.888 ns | 0.0611 ns | 0.0542 ns |  2.97 |    0.05 | 0.0005 |      24 B |        1.00 |
| ZipOnNone            |  3.907 ns | 0.0675 ns | 0.0631 ns |  2.37 |    0.05 | 0.0005 |      24 B |        1.00 |
| XorOnTwoSome         |  1.661 ns | 0.0234 ns | 0.0219 ns |  1.01 |    0.02 | 0.0005 |      24 B |        1.00 |
| MapAsyncShortCircuit | 18.385 ns | 0.2318 ns | 0.2168 ns | 11.17 |    0.18 | 0.0005 |      24 B |        1.00 |
| SomeThenUnwrap       |  2.441 ns | 0.0380 ns | 0.0355 ns |  1.48 |    0.03 | 0.0005 |      24 B |        1.00 |
