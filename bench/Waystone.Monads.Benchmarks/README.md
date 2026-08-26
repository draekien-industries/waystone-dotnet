# Waystone.Monads.Benchmarks

Allocation and throughput benchmarks for `Option<T>` and `Result<TOk, TErr>`.

The harness exists to keep the v6 performance work honest: no optimisation in
that milestone merges without a before/after figure from this project pasted
into its pull request. `[MemoryDiagnoser]` is on every class because allocated
bytes, not wall time, is the metric this library is judged on.

## Running

```
dotnet run -c Release --project bench/Waystone.Monads.Benchmarks -- --filter '*'
```

Release configuration is mandatory — BenchmarkDotNet refuses to run a Debug
build, and the numbers would be meaningless anyway. Narrow a run with a filter
(`--filter '*Option*'`) and shorten it with `--job short` while iterating; take
the final numbers from a default job.

## Artifacts, and comparing releases

Runs write to `artifacts/<label>/` at the repository root, not to the
`BenchmarkDotNet.Artifacts/` directory BenchmarkDotNet uses by default. The
label comes from the `WAYSTONE_BENCH_LABEL` environment variable and falls back
to `local`.

```
WAYSTONE_BENCH_LABEL=v5-baseline dotnet run -c Release --project bench/Waystone.Monads.Benchmarks -- --filter '*'
```

The point of the label is to keep more than one release's numbers side by side.
`artifacts/v5-baseline/` holds the numbers from `main` at 5.5.0, captured before
any v6 change landed. Capture the v6 run under its own label and the two sit in
the same tree, ready to diff:

```
git diff --no-index artifacts/v5-baseline/results/*.OptionBenchmarks-report-github.md artifacts/v6/results/*.OptionBenchmarks-report-github.md
```

Only the `*-report-github.md` reports are committed. The logs, CSV and HTML a
run also produces are ignored — they are large, machine-specific, and nobody
reviews them. Numbers taken on different machines are not comparable, so a
release comparison has to come from one machine in one sitting.

Passing `--artifacts <path>` on the command line overrides all of this, which is
the escape hatch for a throwaway run you do not want in the tree.

## What is covered

| Class | Covers |
| --- | --- |
| `OptionBenchmarks` | `Some`/`None` construction, the implicit conversion, `FromNullable`, `Match`, `Map`, `Filter`, `UnwrapOr`, each on both cases |
| `ResultBenchmarks` | `Ok`/`Err` construction, `Match`, `Map`, `MapErr`, `UnwrapOr`, each on both cases |
| `AsyncChainBenchmarks` | A single async link and a three-link chain off a synchronous receiver, on both cases |
| `HotPathBenchmarks` | The paths that produce a `None` — the factory, a rejecting `Filter`, `Map`/`Zip` on a `None`, `Xor`, and an async short circuit |
| `StateOverloadBenchmarks` | `Map`, `MapOr` and `Filter` on `Option`, `Map` on `Result`, and `Try` on both, each run twice — once with a capturing lambda and once with the state overload |
| `StateOverloadCandidateBenchmarks` | The nine delegate *shapes* DRA-108 found without a state overload, each run twice — once with a capturing lambda and once with the state overload. Both sides call shipped members; the `private static` prototypes the class was built on are gone |
| `TryDiagnosticsBenchmarks` | `Try` on the succeeding path and on the throwing path, the latter run twice — once with nothing listening and once with both a `MeterListener` and a `DiagnosticListener` subscriber attached |

`AsyncChainBenchmarks` is the one to watch across the v6 stack. A chain today
starts as `ValueTask` off an `Option<T>` receiver and degrades to `Task` at the
second link, so the allocation-free short-circuit survives exactly one hop.

## Deliberate omissions

**`net8.0` only.** The five-framework matrix is a correctness concern. Nobody
acts on a `net472` throughput number, and building BenchmarkDotNet for it costs
more than it tells us.

**Not wired into CI.** A benchmark job on a shared GitHub runner produces noise,
not signal, and gating a merge on it would be gating on the weather. Run it
locally and paste the numbers.

**Outside `src/`.** `release.yml` packs and pushes everything under `src/**` on
a push to `main`. A benchmark harness must never reach NuGet, so it lives here
and sets `IsPackable=false` as well.

## Baseline

`artifacts/v5-baseline/` holds the full reports, captured on `main` at 5.5.0
before any v6 change, with `--job short` on an AMD Ryzen 7 9800X3D under
Windows 11, .NET SDK 10.0.111, host runtime .NET 8.0.30. Short-run error bars
are wide and the means jitter between runs; the allocation column is
deterministic and is the one to read.

Three things in those numbers drive the v6 performance work.

**`Some` costs three times what `Ok` costs, and holds the same thing.**
`SomeConstruction` allocates 72 B against `OkConstruction`'s 24 B. The object is
the same size; the extra 48 B is two boxed `int`s, because `Some`'s constructor
guard calls the static `object.Equals(value, default(T))` and boxes both
operands. `Ok` has no guard and allocates only itself.

**The implicit conversion pays that tax twice.** `ImplicitConversion` allocates
120 B: 48 B boxing in the conversion's own `Equals(value, default(T))`, 48 B
again in the `Some` constructor it calls, and 24 B for the object. That is the
measurable cost of encoding one invariant in two places. `MapOnSome` allocates
the same 120 B because `Map` returns a bare value and goes back through the
conversion — against 24 B for `MapOnOk`, which does the same work.

**Absence is not free.** `NoneConstruction`, `MapOnNone` and `FilterRejecting`
each allocate 24 B for a `None<T>` that holds nothing and is indistinguishable
from every other `None<T>` of its type.

The async rows show the `ValueTask` to `Task` degradation: `ThreeLinkChainOnNone`
allocates 216 B to short-circuit three times and produce nothing.

## The uniform `ValueTask` result

`artifacts/v6-dra66/` holds the run after DRA-66, on the same machine and
settings. It is the reason the two `Task`-receiver chain benchmarks exist.

| Three-link chain, by head | before | after | Δ |
| -- | --: | --: | --: |
| sync `Option`, `Some` | 432 B | 288 B | −144 B |
| sync `Option`, `None` | 216 B | 72 B | −144 B |
| completed `Task` | 576 B | 360 B | −216 B |
| pending `Task` | 782 B | 866 B | **+84 B** |

The last row is a real regression and was measured rather than predicted. A
fluent chain is built eagerly, so every link attaches before the one above it
completes: if the head is pending, link two awaits an incomplete `ValueTask`
and suspends as well, and so does link three. The chain is either wholly
synchronous or wholly pending, and there is no partial case where `ValueTask`
saves one hop out of three. When it is wholly pending, `ValueTask` costs more
than `Task`, because an `AsyncValueTaskMethodBuilder` box holds the state
machine inline and is larger than the `Task` it replaces.

`ValueTask` therefore pays only when the head completes synchronously — which
is why the first three rows win and the fourth loses. The trade was taken
deliberately: the loss lands on a chain that has just done real I/O, where 84 B
against a 619 ns await is noise, and the win lands on the synchronous path,
which is the hot one.

The `before` column for the first two rows comes from `artifacts/v5-baseline/`.
The last two benchmarks did not exist at 5.5.0, so their `before` column was
captured from a scratch build with the `Task`-receiver row left on `Task` and
the `ValueTask`-receiver row converted. For a chain whose head is a `Task`,
every link binds to the `Task`-receiver row and never reaches the other one, so
that configuration is byte-identical to 5.5.0 for these two rows only.

## The closure-avoiding state overloads

`artifacts/dra-84/` holds the run for the transform methods and
`artifacts/dra-104/` the run for the factories, same machine and settings. Each
pair calls the same method twice: once with a lambda closing over a local, and
once passing that local as state to a `static` lambda.

| Call | closure | state | Δ |
| -- | --: | --: | --: |
| `Option.Map` | 112 B | 24 B | −88 B |
| `Option.MapOr` | 88 B | 0 B | −88 B |
| `Option.Filter` | 88 B | 0 B | −88 B |
| `Result.Map` | 112 B | 24 B | −88 B |
| `Option.Try` | 112 B | 24 B | −88 B |
| `Result.Try` | 112 B | 24 B | −88 B |

The closure costs exactly 88 B in every row — 24 B for the display class and
64 B for the delegate built over it — and the state overload removes all of it.
The 24 B the four `Map` and `Try` rows keep is the returned `Some<int>` or
`Ok<int, string>`, which no overload can avoid; `MapOr` and `Filter` return a
value or the receiver and so reach zero.

The two `Try` rows are the ones worth reaching for. A `Try` factory almost
always needs an argument — that is why it is a factory — so the closure is
harder to avoid by accident there than on a `Map`.

Read the allocation column, not the timings. The means do move the same way
(`MapOr` runs at 0.35× the closure version) but short-run error bars here are
wider than several of the differences.

A `static` lambda is what makes this work. Passing a non-`static` lambda that
happens not to capture gets the same result, but nothing stops a later edit
from capturing again and silently putting the 88 B back — `static` makes that a
compile error.

## The None singleton

`artifacts/dra-100-before/` and `artifacts/dra-100/` hold the two runs.
`None<T>` is stateless, so `Option.None<T>()` was allocating a fresh 24 B
record on every cold branch in the library. It now returns a
`static readonly` instance the runtime builds once per closed generic.

| Call | before | after |
| -- | --: | --: |
| `Option.None<int>()` | 24 B | 0 B |
| `Filter` that rejects | 24 B | 0 B |
| `Map` on a `None` | 24 B | 0 B |
| `Zip` on a `None` | 24 B | 0 B |
| `Xor` on two `Some` | 24 B | 0 B |
| `MapAsync` short circuit | 24 B | 0 B |

`Option.None<int>()` also drops from 1.65 ns to 0.18 ns, which is the
allocation and nothing else — there was never any work to do.

`Some` is untouched and still allocates its 24 B, because it holds a value.

### What was measured and dropped

Replacing the `IsNone`-then-`Expect` pair in the async extensions with a
single `is not Some<T>` type test was measured on `MapAsync` over a `Some`
before it was applied anywhere: 29.30 ns to 28.81 ns, with error bars of
±0.44 and ±0.15, and no change in allocation. That is under two percent and
inside the noise, against a diff touching 43 files. It was reverted rather
than argued for.

## The remaining delegate shapes

`artifacts/dra-108/` holds the run, same machine and settings. DRA-84 and
DRA-104 gave state overloads to the transforms and the factories; DRA-108
audited what was left and found twenty delegate-taking members without one,
across nine distinct shapes. The run committed here is the final one, taken
once every overload had landed. The evidence for adding them was an earlier run
of the same class against prototypes, and the allocation column below is
byte-identical between the two — which is the point of the postscript at the end
of this section.

| Shape | Members it decides | closure | state | Δ |
| -- | -- | --: | --: | --: |
| `Func<T, bool>` | `IsSomeAnd`, `IsNoneOr`, `IsOkAnd`, `IsErrAnd` | 88 B | 0 B | −88 B |
| `Action<T>` | `Option.Inspect`, `Result.Inspect`, `InspectErr` | 88 B | 0 B | −88 B |
| `Func<T, TOut>` | `Option.MapOrDefault`, `Result.MapOrDefault` | 88 B | 0 B | −88 B |
| value factory | `Option.UnwrapOrElse`, `Result.UnwrapOrElse` | 88 B | 0 B | −88 B |
| monad factory | `Option.OrElse`, `Result.OrElse` | 88 B | 0 B | −88 B |
| error factory | `Option.OkOrElse` | 112 B | 24 B | −88 B |
| two `Func` branches | `Option.Match<TOut>` | 152 B | 0 B | −152 B |
| two `Func` branches | `Result.Match<TOut>` | 152 B | 0 B | −152 B |
| two `Action` branches | `Option.Match`, `Result.Match` | 152 B | 0 B | −152 B |

Nothing failed. Every shape clears the bar DRA-84 set, and two findings go
beyond simply repeating it.

**A two-branch `Match` costs 152 B, not 88 B.** One display class is shared
between the branches at 24 B, but each branch gets its own delegate over it at
64 B, so a second lambda adds 64 B rather than another 88 B. `Match` is the
most-called member on both types and it is the most expensive one to call with
a closure. It is the strongest row in the table.

**The factory family pays for a delegate it never invokes.** `UnwrapOrElse`,
`OrElse` and `OkOrElse` are benchmarked on a `Some`, so the fallback delegate is
built at the call site and then thrown away unused. The 88 B is not the price of
the fallback — it is the price of *having* a fallback on the path that does not
need one. Those three are the closest analogue to the `Try` rows above, which
DRA-104 called the ones worth reaching for.

`OkOrElse` is the only row that does not reach zero. Its 24 B is the
`Ok<int, string>` it returns, which no overload can avoid — the same 24 B the
`Map` and `Try` rows keep above. The delta is −88 B, identical to the rest.

`Reduce` and `ZipWith` are the two of the twenty that are not in this table, and
they were declined without measuring. Both take a delegate whose every operand
already arrives as a parameter of the call, so there is nothing for a lambda to
capture and a `TState` form would add a parameter callers pass `null` to. That
is a signature argument, not a performance one, and no benchmark would have
settled it either way.

### Measuring an overload that does not exist yet

There is nothing to call on the `WithState` side of these pairs, so each one
calls a `private static` prototype in the benchmark file that takes the proposed
signature and does what the real override will do — type-test the receiver, then
invoke the delegate with the value and the state.

The prototype is faithful on allocation, which is the column this decision turns
on: the call site builds exactly the delegate the real overload would, and the
prototype adds nothing to the heap. It is *not* faithful on timing. A prototype
is a `private static` the JIT can inline through; the shipped member will be a
virtual call on a `record`. The `WithState` means are a floor, not a prediction
— `OrElseWithState` tripped BenchmarkDotNet's `ZeroMeasurement` warning, which
the real member will not.

Read the allocation column. The timing column shows the sign of the change, not
its size. Re-running this class against the shipped overloads once they land is
the honest way to get the timings, and the prototypes come out when they do.

### What the prototypes got wrong

Every row now calls a shipped member. The `Option<T>` overloads landed first
and the two `Result` rows stayed on prototypes for one more run, which made that
intermediate run a controlled comparison of the two measurement styles — the
eight converted rows moved, the two held-back rows did not. Both columns below
are from the runs that produced them.

| `WithState` row | as a prototype | as the shipped member |
| -- | --: | --: |
| `IsSomeAnd` | 0.11× | 0.70× |
| `MatchFunc` | 0.10× | 0.34× |
| `MatchAction` | 0.05× | 0.38× |
| `Inspect` | 0.11× | 0.77× |
| `MapOrDefault` | 0.07× | 0.45× |
| `UnwrapOrElse` | 0.07× | 0.88× |
| `OrElse` | 0.08× | 0.75× |
| `OkOrElse` | 0.14× | 0.55× |
| `IsOkAnd` | 0.10× | 0.86× |
| `ResultMatchFunc` | 0.06× | 0.45× |

**The allocation column did not move at all.** Every byte the prototype run
reported is a byte the shipped run reports.

**The timing column moved by roughly an order of magnitude, on every row.** In
the intermediate run the two rows still on prototypes held their prototype
figures while the eight converted ones moved, which is what rules out drift
between runs rather than a real effect. Both then moved on conversion, by the
same order as the rest. A `private static` the JIT inlines through is not a
virtual call on a `record`, and the gap between 0.05× and 0.45× is the whole of
that difference. The prototypes were never wrong about *whether* to add the
overloads; they were wildly optimistic about how much time it would buy.

The narrow rows are the ones worth reading twice. `IsOkAnd` at 0.86× and
`UnwrapOrElse` at 0.79× save almost no time — the delegate they avoid building
was cheap to build. They still go from 88 B to zero, and that is the entire case
for them. An argument for these overloads that leaned on the means would have
been an argument the shipped members could not support.

The shipped-member ratios jitter by a few points between runs — `UnwrapOrElse`
read 0.88× on the intermediate run and 0.79× here, on unchanged code. Read them
as a band, not a figure. The allocation column does not jitter, which is the
third time on this page that it is the column to read.

`Match` is the exception that keeps its headline everywhere: 0.36×, 0.39× and
0.45× across the three `Match` rows, against 152 B eliminated.

## Native diagnostics on the `Try` paths

`artifacts/dra-116-before/` and `artifacts/dra-116/` hold the two runs, same
machine and settings. DRA-116 made `Waystone.Monads` emit a counter and a
diagnostic event every time a `Try` swallows an exception, which puts new code on
a path DRA-100 had just finished emptying. The question this class answers is what
that costs a consumer who is not listening.

| Call | before | after | Δ |
| -- | --: | --: | --: |
| `Option.Try`, succeeds | 24 B | 24 B | 0 B |
| `Option.Try`, throws, nothing listening | 480 B | 480 B | 0 B |
| `Result.Try`, throws, nothing listening | 616 B | 616 B | 0 B |
| `Option.Try`, throws, observed | — | 520 B | +40 B |
| `Result.Try`, throws, observed | — | 656 B | +40 B |

**Nobody listening pays nothing.** Both gates — `Instrument.Enabled` and
`DiagnosticListener.IsEnabled` — resolve to a field read against a null
subscriber list, so the unobserved rows are byte-identical before and after and
their means sit inside each other's error bars.

**Listening costs 40 B, and all of it is the event payload.** The counter itself
allocates nothing: two tags fit the `Counter<long>.Add` overload that takes them
as arguments, so they travel in a stack `TagList` and never reach the heap. The
40 B is one `ExceptionHandled` record — three fields and a header — built because
`DiagnosticListener` is untyped and a subscriber needs an object to cast.

The `before` column has no entry for the observed rows: with the emission call
removed there is nothing to observe, so those two benchmarks measure the
unobserved path and the comparison is meaningless. The `before` run was captured
by commenting out the single call in `MonadOptions.Log` rather than by building
`main`, because the benchmark class references the diagnostic names and would not
compile against `main` at all.

Read this table against the exception, not against the counter. A thrown
exception costs 3.2 µs and 480 B before anything of ours runs; 40 B on top of it,
paid only by a consumer who asked to observe, is under nine percent of an
allocation they had already decided to accept.
