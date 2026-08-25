```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]     : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4


```
| Method                     | Categories      | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------------- |----------:|----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| InspectWithClosure         | Inspect         |  4.427 ns | 0.2399 ns | 0.6921 ns |  4.196 ns |  1.02 |    0.21 | 0.0018 |      88 B |        1.00 |
| InspectWithState           | Inspect         |  3.065 ns | 0.0873 ns | 0.1596 ns |  3.056 ns |  0.71 |    0.11 |      - |         - |        0.00 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| MapOrDefaultWithClosure    | MapOrDefault    |  6.084 ns | 0.1368 ns | 0.1918 ns |  6.075 ns |  1.00 |    0.04 | 0.0018 |      88 B |        1.00 |
| MapOrDefaultWithState      | MapOrDefault    |  3.203 ns | 0.0825 ns | 0.1260 ns |  3.212 ns |  0.53 |    0.03 |      - |         - |        0.00 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| MatchActionWithClosure     | MatchAction     |  9.351 ns | 0.2157 ns | 0.6326 ns |  9.313 ns |  1.00 |    0.09 | 0.0030 |     152 B |        1.00 |
| MatchActionWithState       | MatchAction     |  3.328 ns | 0.0792 ns | 0.1210 ns |  3.306 ns |  0.36 |    0.03 |      - |         - |        0.00 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| MatchFuncWithClosure       | MatchFunc       |  8.943 ns | 0.2497 ns | 0.7123 ns |  8.800 ns |  1.01 |    0.11 | 0.0030 |     152 B |        1.00 |
| MatchFuncWithState         | MatchFunc       |  3.444 ns | 0.1064 ns | 0.2983 ns |  3.345 ns |  0.39 |    0.04 |      - |         - |        0.00 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| OkOrElseWithClosure        | OkOrElse        | 10.723 ns | 0.2492 ns | 0.3880 ns | 10.626 ns |  1.00 |    0.05 | 0.0022 |     112 B |        1.00 |
| OkOrElseWithState          | OkOrElse        |  6.034 ns | 0.1519 ns | 0.2129 ns |  5.947 ns |  0.56 |    0.03 | 0.0005 |      24 B |        0.21 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| OrElseWithClosure          | OrElse          |  3.933 ns | 0.1121 ns | 0.1678 ns |  3.938 ns |  1.00 |    0.06 | 0.0018 |      88 B |        1.00 |
| OrElseWithState            | OrElse          |  2.901 ns | 0.0900 ns | 0.0963 ns |  2.840 ns |  0.74 |    0.04 |      - |         - |        0.00 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| IsSomeAndWithClosure       | Predicate       |  4.005 ns | 0.0830 ns | 0.0988 ns |  4.015 ns |  1.00 |    0.03 | 0.0018 |      88 B |        1.00 |
| IsSomeAndWithState         | Predicate       |  2.996 ns | 0.0794 ns | 0.1304 ns |  2.925 ns |  0.75 |    0.04 |      - |         - |        0.00 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| ResultMatchFuncWithClosure | ResultMatchFunc |  8.731 ns | 0.1873 ns | 0.2157 ns |  8.659 ns |  1.00 |    0.03 | 0.0030 |     152 B |        1.00 |
| ResultMatchFuncWithState   | ResultMatchFunc |  3.906 ns | 0.0965 ns | 0.1255 ns |  3.912 ns |  0.45 |    0.02 |      - |         - |        0.00 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| IsOkAndWithClosure         | ResultPredicate |  3.779 ns | 0.0246 ns | 0.0218 ns |  3.779 ns |  1.00 |    0.01 | 0.0018 |      88 B |        1.00 |
| IsOkAndWithState           | ResultPredicate |  3.269 ns | 0.0366 ns | 0.0305 ns |  3.274 ns |  0.86 |    0.01 |      - |         - |        0.00 |
|                            |                 |           |           |           |           |       |         |        |           |             |
| UnwrapOrElseWithClosure    | UnwrapOrElse    |  3.608 ns | 0.0909 ns | 0.0806 ns |  3.577 ns |  1.00 |    0.03 | 0.0018 |      88 B |        1.00 |
| UnwrapOrElseWithState      | UnwrapOrElse    |  2.834 ns | 0.0760 ns | 0.1066 ns |  2.791 ns |  0.79 |    0.03 |      - |         - |        0.00 |
