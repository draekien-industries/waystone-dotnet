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
