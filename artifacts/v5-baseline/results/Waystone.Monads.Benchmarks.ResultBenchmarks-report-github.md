```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method          | Mean       | Error     | StdDev    | Gen0   | Allocated |
|---------------- |-----------:|----------:|----------:|-------:|----------:|
| OkConstruction  |  1.7617 ns | 0.3002 ns | 0.0165 ns | 0.0005 |      24 B |
| ErrConstruction |  1.7827 ns | 0.1962 ns | 0.0108 ns | 0.0005 |      24 B |
| MatchOnOk       |  4.3346 ns | 1.5549 ns | 0.0852 ns |      - |         - |
| MatchOnErr      |  2.6633 ns | 0.9228 ns | 0.0506 ns |      - |         - |
| MapOnOk         |  6.4611 ns | 5.0836 ns | 0.2786 ns | 0.0005 |      24 B |
| MapOnErr        |  7.4263 ns | 3.4197 ns | 0.1874 ns | 0.0005 |      24 B |
| MapErrOnErr     | 10.0962 ns | 5.7980 ns | 0.3178 ns | 0.0011 |      56 B |
| UnwrapOrOnOk    |  0.0223 ns | 0.1107 ns | 0.0061 ns |      - |         - |
| UnwrapOrOnErr   |  0.0166 ns | 0.2958 ns | 0.0162 ns |      - |         - |
