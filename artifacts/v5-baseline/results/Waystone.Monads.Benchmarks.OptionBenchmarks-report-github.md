```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method             | Mean      | Error      | StdDev    | Median    | Gen0   | Allocated |
|------------------- |----------:|-----------:|----------:|----------:|-------:|----------:|
| SomeConstruction   | 5.5756 ns |  3.2414 ns | 0.1777 ns | 5.6270 ns | 0.0014 |      72 B |
| NoneConstruction   | 1.9409 ns |  3.3880 ns | 0.1857 ns | 1.8669 ns | 0.0005 |      24 B |
| ImplicitConversion | 7.0191 ns |  0.9092 ns | 0.0498 ns | 7.0207 ns | 0.0024 |     120 B |
| FromNullable       | 8.9815 ns | 28.1271 ns | 1.5417 ns | 8.1246 ns | 0.0019 |      96 B |
| MatchOnSome        | 4.0094 ns |  1.2018 ns | 0.0659 ns | 3.9912 ns |      - |         - |
| MatchOnNone        | 2.7702 ns |  2.1224 ns | 0.1163 ns | 2.7361 ns |      - |         - |
| MapOnSome          | 9.6447 ns |  2.2819 ns | 0.1251 ns | 9.5951 ns | 0.0024 |     120 B |
| MapOnNone          | 4.3308 ns |  0.7980 ns | 0.0437 ns | 4.3500 ns | 0.0005 |      24 B |
| FilterKeeping      | 0.2529 ns |  0.3834 ns | 0.0210 ns | 0.2421 ns |      - |         - |
| FilterRejecting    | 1.9651 ns |  0.2917 ns | 0.0160 ns | 1.9720 ns | 0.0005 |      24 B |
| UnwrapOrOnSome     | 0.0070 ns |  0.1408 ns | 0.0077 ns | 0.0057 ns |      - |         - |
| UnwrapOrOnNone     | 0.0063 ns |  0.1671 ns | 0.0092 ns | 0.0021 ns |      - |         - |
