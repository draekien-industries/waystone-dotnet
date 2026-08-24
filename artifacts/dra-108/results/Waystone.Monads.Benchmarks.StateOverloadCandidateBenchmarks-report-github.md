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
| InspectWithClosure         | Inspect         |  4.1934 ns |  2.0456 ns | 0.1121 ns |  1.00 |    0.03 | 0.0018 |      88 B |        1.00 |
| InspectWithState           | Inspect         |  0.4620 ns |  1.2070 ns | 0.0662 ns |  0.11 |    0.01 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| MapOrDefaultWithClosure    | MapOrDefault    |  6.3900 ns |  2.9668 ns | 0.1626 ns |  1.00 |    0.03 | 0.0018 |      88 B |        1.00 |
| MapOrDefaultWithState      | MapOrDefault    |  0.4773 ns |  0.7885 ns | 0.0432 ns |  0.07 |    0.01 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| MatchActionWithClosure     | MatchAction     |  9.4342 ns |  1.6422 ns | 0.0900 ns |  1.00 |    0.01 | 0.0030 |     152 B |        1.00 |
| MatchActionWithState       | MatchAction     |  0.4269 ns |  0.4423 ns | 0.0242 ns |  0.05 |    0.00 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| MatchFuncWithClosure       | MatchFunc       |  9.4984 ns | 10.4495 ns | 0.5728 ns |  1.00 |    0.07 | 0.0030 |     152 B |        1.00 |
| MatchFuncWithState         | MatchFunc       |  0.9105 ns |  3.7945 ns | 0.2080 ns |  0.10 |    0.02 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| OkOrElseWithClosure        | OkOrElse        | 16.6005 ns | 32.5532 ns | 1.7844 ns |  1.01 |    0.13 | 0.0022 |     112 B |        1.00 |
| OkOrElseWithState          | OkOrElse        |  2.3377 ns |  4.6888 ns | 0.2570 ns |  0.14 |    0.02 | 0.0005 |      24 B |        0.21 |
|                            |                 |            |            |           |       |         |        |           |             |
| OrElseWithClosure          | OrElse          |  4.0902 ns |  2.1881 ns | 0.1199 ns |  1.00 |    0.04 | 0.0018 |      88 B |        1.00 |
| OrElseWithState            | OrElse          |  0.3202 ns |  0.6982 ns | 0.0383 ns |  0.08 |    0.01 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| IsSomeAndWithClosure       | Predicate       |  4.0320 ns |  1.3227 ns | 0.0725 ns |  1.00 |    0.02 | 0.0018 |      88 B |        1.00 |
| IsSomeAndWithState         | Predicate       |  0.4417 ns |  0.0873 ns | 0.0048 ns |  0.11 |    0.00 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| ResultMatchFuncWithClosure | ResultMatchFunc |  9.0878 ns |  3.8084 ns | 0.2088 ns |  1.00 |    0.03 | 0.0030 |     152 B |        1.00 |
| ResultMatchFuncWithState   | ResultMatchFunc |  0.5499 ns |  0.4387 ns | 0.0240 ns |  0.06 |    0.00 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| IsOkAndWithClosure         | ResultPredicate |  4.0789 ns |  1.0924 ns | 0.0599 ns |  1.00 |    0.02 | 0.0018 |      88 B |        1.00 |
| IsOkAndWithState           | ResultPredicate |  0.4075 ns |  0.7201 ns | 0.0395 ns |  0.10 |    0.01 |      - |         - |        0.00 |
|                            |                 |            |            |           |       |         |        |           |             |
| UnwrapOrElseWithClosure    | UnwrapOrElse    |  3.8000 ns |  1.5653 ns | 0.0858 ns |  1.00 |    0.03 | 0.0018 |      88 B |        1.00 |
| UnwrapOrElseWithState      | UnwrapOrElse    |  0.2835 ns |  0.1117 ns | 0.0061 ns |  0.07 |    0.00 |      - |         - |        0.00 |
