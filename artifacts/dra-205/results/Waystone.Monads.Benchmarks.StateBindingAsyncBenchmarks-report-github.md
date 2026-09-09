```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]     : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4


```
| Method                          | Categories           | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |--------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| AndThenAsyncOnSomeWithClosure   | AndThenAsyncOnSome   | 12.621 ns | 0.1325 ns | 0.1175 ns |  1.00 |    0.01 | 0.0022 |     112 B |        1.00 |
| AndThenAsyncOnSomeWithBinding   | AndThenAsyncOnSome   |  7.173 ns | 0.1441 ns | 0.1277 ns |  0.57 |    0.01 | 0.0005 |      24 B |        0.21 |
|                                 |                      |           |           |           |       |         |        |           |             |
| IsErrAndAsyncOnErrWithClosure   | IsErrAndAsyncOnErr   | 12.130 ns | 0.1743 ns | 0.1456 ns |  1.00 |    0.02 | 0.0017 |      88 B |        1.00 |
| IsErrAndAsyncOnErrWithBinding   | IsErrAndAsyncOnErr   |  8.080 ns | 0.1800 ns | 0.1926 ns |  0.67 |    0.02 |      - |         - |        0.00 |
|                                 |                      |           |           |           |       |         |        |           |             |
| IsSomeAndAsyncOnNoneWithClosure | IsSomeAndAsyncOnNone |  5.167 ns | 0.1225 ns | 0.1635 ns |  1.00 |    0.04 | 0.0018 |      88 B |        1.00 |
| IsSomeAndAsyncOnNoneWithBinding | IsSomeAndAsyncOnNone |  5.675 ns | 0.1324 ns | 0.1106 ns |  1.10 |    0.04 |      - |         - |        0.00 |
|                                 |                      |           |           |           |       |         |        |           |             |
| MapAsyncOnNoneWithClosure       | MapAsyncOnNone       |  7.863 ns | 0.1809 ns | 0.1857 ns |  1.00 |    0.03 | 0.0017 |      88 B |        1.00 |
| MapAsyncOnNoneWithBinding       | MapAsyncOnNone       |  5.759 ns | 0.0714 ns | 0.0667 ns |  0.73 |    0.02 |      - |         - |        0.00 |
|                                 |                      |           |           |           |       |         |        |           |             |
| MapAsyncOnSomeWithClosure       | MapAsyncOnSome       | 17.604 ns | 0.2549 ns | 0.2384 ns |  1.00 |    0.02 | 0.0037 |     184 B |        1.00 |
| MapAsyncOnSomeWithBinding       | MapAsyncOnSome       | 10.467 ns | 0.1456 ns | 0.1362 ns |  0.59 |    0.01 | 0.0019 |      96 B |        0.52 |
|                                 |                      |           |           |           |       |         |        |           |             |
| MatchAsyncOnSomeWithClosure     | MatchAsyncOnSome     | 16.742 ns | 0.2009 ns | 0.1879 ns |  1.00 |    0.02 | 0.0044 |     224 B |        1.00 |
| MatchAsyncOnSomeWithBinding     | MatchAsyncOnSome     | 17.406 ns | 0.3635 ns | 0.4726 ns |  1.04 |    0.03 | 0.0014 |      72 B |        0.32 |
|                                 |                      |           |           |           |       |         |        |           |             |
| ResultMapAsyncOnErrWithClosure  | ResultMapAsyncOnErr  | 12.689 ns | 0.2831 ns | 0.2907 ns |  1.00 |    0.03 | 0.0022 |     112 B |        1.00 |
| ResultMapAsyncOnErrWithBinding  | ResultMapAsyncOnErr  |  7.147 ns | 0.1369 ns | 0.1214 ns |  0.56 |    0.02 | 0.0005 |      24 B |        0.21 |
|                                 |                      |           |           |           |       |         |        |           |             |
| ResultMapAsyncOnOkWithClosure   | ResultMapAsyncOnOk   | 23.993 ns | 0.4105 ns | 0.6510 ns |  1.00 |    0.04 | 0.0037 |     184 B |        1.00 |
| ResultMapAsyncOnOkWithBinding   | ResultMapAsyncOnOk   | 15.616 ns | 0.3356 ns | 0.4480 ns |  0.65 |    0.02 | 0.0019 |      96 B |        0.52 |
