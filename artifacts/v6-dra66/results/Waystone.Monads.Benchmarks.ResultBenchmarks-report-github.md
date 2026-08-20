```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method          | Mean      | Error     | StdDev    | Median    | Gen0   | Allocated |
|---------------- |----------:|----------:|----------:|----------:|-------:|----------:|
| OkConstruction  | 1.7137 ns | 0.2466 ns | 0.0135 ns | 1.7171 ns | 0.0005 |      24 B |
| ErrConstruction | 1.7241 ns | 0.3773 ns | 0.0207 ns | 1.7350 ns | 0.0005 |      24 B |
| MatchOnOk       | 4.2647 ns | 0.9305 ns | 0.0510 ns | 4.2892 ns |      - |         - |
| MatchOnErr      | 2.6638 ns | 0.5031 ns | 0.0276 ns | 2.6584 ns |      - |         - |
| MapOnOk         | 6.1086 ns | 3.4975 ns | 0.1917 ns | 6.0620 ns | 0.0005 |      24 B |
| MapOnErr        | 7.4728 ns | 6.7275 ns | 0.3688 ns | 7.5452 ns | 0.0005 |      24 B |
| MapErrOnErr     | 9.6964 ns | 4.7454 ns | 0.2601 ns | 9.7792 ns | 0.0011 |      56 B |
| UnwrapOrOnOk    | 0.0025 ns | 0.0585 ns | 0.0032 ns | 0.0014 ns |      - |         - |
| UnwrapOrOnErr   | 0.0148 ns | 0.2824 ns | 0.0155 ns | 0.0134 ns |      - |         - |
