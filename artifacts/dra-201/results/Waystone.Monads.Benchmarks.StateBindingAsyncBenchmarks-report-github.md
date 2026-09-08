```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]     : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4


```
| Method                          | Categories           | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |--------------------- |----------:|----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| AndThenAsyncOnSomeWithClosure   | AndThenAsyncOnSome   | 12.577 ns | 0.1480 ns | 0.1384 ns | 12.563 ns |  1.00 |    0.02 | 0.0022 |     112 B |        1.00 |
| AndThenAsyncOnSomeWithBinding   | AndThenAsyncOnSome   | 12.378 ns | 0.2599 ns | 0.4687 ns | 12.315 ns |  0.98 |    0.04 | 0.0005 |      24 B |        0.21 |
|                                 |                      |           |           |           |           |       |         |        |           |             |
| IsErrAndAsyncOnErrWithClosure   | IsErrAndAsyncOnErr   | 12.963 ns | 0.2259 ns | 0.2113 ns | 12.953 ns |  1.00 |    0.02 | 0.0017 |      88 B |        1.00 |
| IsErrAndAsyncOnErrWithBinding   | IsErrAndAsyncOnErr   | 12.268 ns | 0.1859 ns | 0.1739 ns | 12.211 ns |  0.95 |    0.02 |      - |         - |        0.00 |
|                                 |                      |           |           |           |           |       |         |        |           |             |
| IsSomeAndAsyncOnNoneWithClosure | IsSomeAndAsyncOnNone |  5.207 ns | 0.1234 ns | 0.1808 ns |  5.211 ns |  1.00 |    0.05 | 0.0018 |      88 B |        1.00 |
| IsSomeAndAsyncOnNoneWithBinding | IsSomeAndAsyncOnNone |  9.159 ns | 0.0993 ns | 0.0929 ns |  9.163 ns |  1.76 |    0.06 |      - |         - |        0.00 |
|                                 |                      |           |           |           |           |       |         |        |           |             |
| MapAsyncOnNoneWithClosure       | MapAsyncOnNone       |  8.155 ns | 0.1954 ns | 0.4079 ns |  8.000 ns |  1.00 |    0.07 | 0.0017 |      88 B |        1.00 |
| MapAsyncOnNoneWithBinding       | MapAsyncOnNone       |  9.061 ns | 0.2059 ns | 0.1825 ns |  9.079 ns |  1.11 |    0.06 |      - |         - |        0.00 |
|                                 |                      |           |           |           |           |       |         |        |           |             |
| MapAsyncOnSomeWithClosure       | MapAsyncOnSome       | 18.818 ns | 0.4103 ns | 0.6266 ns | 18.689 ns |  1.00 |    0.05 | 0.0037 |     184 B |        1.00 |
| MapAsyncOnSomeWithBinding       | MapAsyncOnSome       | 12.403 ns | 0.2817 ns | 0.6301 ns | 12.180 ns |  0.66 |    0.04 | 0.0019 |      96 B |        0.52 |
|                                 |                      |           |           |           |           |       |         |        |           |             |
| MatchAsyncOnSomeWithClosure     | MatchAsyncOnSome     | 17.439 ns | 0.3628 ns | 0.4456 ns | 17.421 ns |  1.00 |    0.04 | 0.0044 |     224 B |        1.00 |
| MatchAsyncOnSomeWithBinding     | MatchAsyncOnSome     | 18.142 ns | 0.3484 ns | 0.5425 ns | 18.005 ns |  1.04 |    0.04 | 0.0014 |      72 B |        0.32 |
|                                 |                      |           |           |           |           |       |         |        |           |             |
| ResultMapAsyncOnErrWithClosure  | ResultMapAsyncOnErr  | 13.231 ns | 0.2914 ns | 0.3118 ns | 13.193 ns |  1.00 |    0.03 | 0.0022 |     112 B |        1.00 |
| ResultMapAsyncOnErrWithBinding  | ResultMapAsyncOnErr  | 14.341 ns | 0.2380 ns | 0.2226 ns | 14.374 ns |  1.08 |    0.03 | 0.0005 |      24 B |        0.21 |
|                                 |                      |           |           |           |           |       |         |        |           |             |
| ResultMapAsyncOnOkWithClosure   | ResultMapAsyncOnOk   | 22.978 ns | 0.1570 ns | 0.1468 ns | 22.972 ns |  1.00 |    0.01 | 0.0037 |     184 B |        1.00 |
| ResultMapAsyncOnOkWithBinding   | ResultMapAsyncOnOk   | 15.751 ns | 0.1637 ns | 0.1451 ns | 15.688 ns |  0.69 |    0.01 | 0.0019 |      96 B |        0.52 |
