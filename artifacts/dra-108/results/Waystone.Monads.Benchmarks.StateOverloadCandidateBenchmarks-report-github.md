```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.111
  [Host]   : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.30 (8.0.30, 8.0.3026.36720), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                     | Categories      | Mean       | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------------- |-----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| InspectWithClosure         | Inspect         |  4.5038 ns |  5.7718 ns | 0.3164 ns |  1.00 |    0.08 | 0.0018 |      88 B |        1.00 |
| InspectWithState           | Inspect         |  3.4448 ns |  7.0661 ns | 0.3873 ns |  0.77 |    0.09 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| MapOrDefaultWithClosure    | MapOrDefault    |  8.0365 ns | 15.4336 ns | 0.8460 ns |  1.01 |    0.13 | 0.0017 |      88 B |        1.00 |
| MapOrDefaultWithState      | MapOrDefault    |  3.5632 ns |  4.1661 ns | 0.2284 ns |  0.45 |    0.05 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| MatchActionWithClosure     | MatchAction     |  9.6571 ns |  8.6566 ns | 0.4745 ns |  1.00 |    0.06 | 0.0030 |     152 B |        1.00 |
| MatchActionWithState       | MatchAction     |  3.6501 ns |  3.4496 ns | 0.1891 ns |  0.38 |    0.02 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| MatchFuncWithClosure       | MatchFunc       |  9.0276 ns |  7.8386 ns | 0.4297 ns |  1.00 |    0.06 | 0.0030 |     152 B |        1.00 |
| MatchFuncWithState         | MatchFunc       |  3.0628 ns |  1.0501 ns | 0.0576 ns |  0.34 |    0.02 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| OkOrElseWithClosure        | OkOrElse        | 11.1338 ns | 22.9530 ns | 1.2581 ns |  1.01 |    0.14 | 0.0022 |     112 B |        1.00 |
| OkOrElseWithState          | OkOrElse        |  6.1185 ns |  9.0614 ns | 0.4967 ns |  0.55 |    0.06 | 0.0005 |      24 B |        0.21 |
|                            |                 |            |            |           |       |         |        |           |             |
| OrElseWithClosure          | OrElse          |  4.3261 ns |  9.3939 ns | 0.5149 ns |  1.01 |    0.14 | 0.0018 |      88 B |        1.00 |
| OrElseWithState            | OrElse          |  3.2333 ns |  2.0777 ns | 0.1139 ns |  0.75 |    0.08 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| IsSomeAndWithClosure       | Predicate       |  4.4622 ns |  6.2800 ns | 0.3442 ns |  1.00 |    0.10 | 0.0018 |      88 B |        1.00 |
| IsSomeAndWithState         | Predicate       |  3.1174 ns |  2.9793 ns | 0.1633 ns |  0.70 |    0.06 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| ResultMatchFuncWithClosure | ResultMatchFunc |  9.2628 ns |  8.4240 ns | 0.4617 ns |  1.00 |    0.06 | 0.0030 |     152 B |        1.00 |
| ResultMatchFuncWithState   | ResultMatchFunc |  0.5048 ns |  0.4219 ns | 0.0231 ns |  0.05 |    0.00 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| IsOkAndWithClosure         | ResultPredicate |  4.1471 ns |  3.2546 ns | 0.1784 ns |  1.00 |    0.05 | 0.0018 |      88 B |        1.00 |
| IsOkAndWithState           | ResultPredicate |  0.3844 ns |  0.8408 ns | 0.0461 ns |  0.09 |    0.01 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| UnwrapOrElseWithClosure    | UnwrapOrElse    |  3.7812 ns |  0.4657 ns | 0.0255 ns |  1.00 |    0.01 | 0.0018 |      88 B |        1.00 |
| UnwrapOrElseWithState      | UnwrapOrElse    |  3.3243 ns |  2.1444 ns | 0.1175 ns |  0.88 |    0.03 |      - |         - |        0.00 |
