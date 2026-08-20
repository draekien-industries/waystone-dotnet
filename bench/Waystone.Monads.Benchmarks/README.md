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
| `StateOverloadBenchmarks` | `Map`, `MapOr` and `Filter` on `Option`, `Map` on `Result`, and `Try` on both, each run twice — once with a capturing lambda and once with the state overload |

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
