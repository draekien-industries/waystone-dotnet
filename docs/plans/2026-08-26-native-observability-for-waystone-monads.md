---
title: Native observability for Waystone.Monads
date: 2026-08-26
status: active
---

Follows [ADR 0004](../adr/0004-emit-observability-signals-natively-from-core.md) and the
two explorations dated 2026-08-26. Covers
[DRA-93](https://linear.app/draekien-industries/issue/DRA-93) and rescopes
[DRA-94](https://linear.app/draekien-industries/issue/DRA-94).

## Goal

`Waystone.Monads` reports what it does through sources named after itself, so a consumer
gets metrics from the OpenTelemetry configuration they already maintain and logs from one
opt-in call. `MonadOptions.ExceptionLogger` is obsoleted in favour of the consumer's own
`ILogger`.

Done when: a consumer adding `.AddMeter("Waystone.Monads")` sees handled-exception counts
with no Waystone package installed; a consumer calling
`MonadOptions.Configure(o => o.UseLoggerFactoryFrom(app.Services))` sees structured logs;
`UseExceptionLogger` carries an `[Obsolete]` naming its replacement and `7.0.0`; and the
full framework matrix passes.

## The naming contract

These names are permanent public API from first release — dashboards and alerts bind to
them, no compiler catches a rename, and no deprecation path exists that a dashboard
observes. They are settled here rather than improvised during implementation.

Verified against OpenTelemetry semantic conventions and the .NET metrics guidance, both
captured 2026-08-26.

### Sources

| Thing | Name | Rationale |
| --- | --- | --- |
| `Meter` | `Waystone.Monads` | .NET guidance: use the assembly or namespace name of the code being instrumented. |
| `DiagnosticListener` | `Waystone.Monads` | Same name, different registry; no collision. |
| Diagnostic event | `Waystone.Monads.ExceptionHandled` | Event names are globally visible, so they carry their own namespace. |

### Instruments

Lowercase, dot-delimited namespaces, `_` between words within an element, units declared
on the instrument and never in the name — per OTel general naming rules and the .NET
guidance that mirrors them. Counters are pluralised because they count discrete things;
namespaces are not.

| Instrument | Type | Unit | Tags | Ships in |
| --- | --- | --- | --- | --- |
| `waystone.monads.exceptions_handled` | `Counter<long>` | `{exception}` | `error.type`, `waystone.monads.monad` | PR 1 |
| `waystone.monads.errors` | `Counter<long>` | `{error}` | `error.type` | later PR |

### Tag keys and values

| Key | Values | Notes |
| --- | --- | --- |
| `error.type` (on `exceptions_handled`) | The exception's fully-qualified type name | The semconv-registry attribute, stable. Exception types are a bounded set, so cardinality is safe without normalisation. |
| `error.type` (on `errors`) | Normalised `ErrorCode.Value`, else `_OTHER` | See below. |
| `waystone.monads.monad` | `option` \| `result` | Two values. Distinguishes the lossy conversion from the lossless one — an exception swallowed by `Option.Try` is gone, one caught by `Result.Try` is in the `Err`. |

**The `ErrorCode` cardinality problem has a standard answer.** `error.type` is specified
as low-cardinality and defines `_OTHER` as the fallback for values outside a known set.
So the `errors` counter takes a consumer-supplied allowlist and emits `_OTHER` for
anything not in it, rather than tagging with free text. This is the normalisation hook
DRA-94 asked for, and it is the spec's own mechanism rather than an invention of ours.

### Log properties — deliberately *not* semconv-shaped

| Property | Source |
| --- | --- |
| `{MemberName}` | `CallerInfo.MemberName` |
| `{LineNumber}` | `CallerInfo.LineNumber` |
| `{ArgumentExpression}` | `CallerInfo.ArgumentExpression` |

PascalCase, not the dotted `code.function.name` form, and this asymmetry with the metric
tags is intentional. **Serilog property names must match `[A-Za-z0-9_]+`**; a dotted token
in a message template is not parsed as a property and renders literally, so
`{code.function.name}` would silently produce broken output for a large share of .NET
consumers. Dotted semconv keys are safe for metric tags because the key is passed as an
explicit string with no template parser involved.

**The exception is passed through `ILogger.Log`'s exception parameter, never as manual
properties.** The OpenTelemetry logging bridge derives `exception.type`,
`exception.message` and `exception.stacktrace` from it; duplicating them by hand would
double-report.

### One deliberate deviation, flagged

Semantic conventions state that exceptions expected to be handled by application code
SHOULD be reported at `WARN`. Our default is `LogLevel.Debug`, chosen on the grounds that
a `Try` returning `None` or `Err` is an ordinary outcome and warning on it is noise. The
level is configurable globally and per scope, so a consumer who wants the spec default
sets it in one line. Recorded here so the deviation is a decision rather than an
oversight.

## The stack

Built bottom to top with `gh stack init` before the first PR is opened, extended with
`gh stack add`. All of PRs 1–3 land in `6.6.0`. None carries `!`.

| # | PR title | Milestone |
| --- | --- | --- |
| 1 | `feat: emit handled exceptions as native diagnostics` | v6.x — Ecosystem packages |
| 2 | `feat: add Waystone.Monads.Extensions.Logging for ILogger configuration` | v6.x — Ecosystem packages |
| 3 | `feat: obsolete UseExceptionLogger in favour of the ILogger configuration` | v7 preparation |

PR 3 must not precede PR 2, or the obsoletion names a replacement nobody can install.

### PR 1 — core emission

- Pin `System.Diagnostics.DiagnosticSource` in `Directory.Packages.props`; reference it
  from `Waystone.Monads`.
- New `Waystone.Monads/Diagnostics/`: `MonadDiagnostics` holding the meter, the listener,
  the `exceptions_handled` counter and the public name constants; an `ExceptionHandled`
  payload record.
- `MonadOptions.Log` emits the counter and the diagnostic event, **and keeps**
  `ExceptionLogger` so both paths work through 6.x.
- Gate both on their enablement checks — `Instrument.Enabled` and
  `DiagnosticListener.IsEnabled(eventName)` — so an unobserved process allocates nothing.
- `PublicAPI.Unshipped.txt` rows and doc comments for every new public member, written
  through `engineering-skills:with-doc-comments`.
- Benchmark: `Try` on the success and throwing paths, observed and unobserved, with
  before/after `[MemoryDiagnoser]` figures in the PR body. This sits on the paths DRA-100
  optimised, so a regression here is a blocker rather than a note.
- Carries ADR 0004, both explorations and this plan.

### PR 2 — the logging package

- New `src/Waystone.Monads.Extensions.Logging`, matching the
  `Waystone.Monads.FluentValidation` shape: `netstandard2.0`, `LangVersion latestMajor`,
  `Nullable enable`, PolySharp with `PrivateAssets all`, `ProjectReference` to
  `Waystone.Monads`, `README.md` packed at root. Sole package reference:
  `Microsoft.Extensions.Logging.Abstractions`.
- Three extension methods on `MonadOptions`, each with `LogLevel level = LogLevel.Debug`:
  - `UseLogger(ILogger)` — the caller's own category
  - `UseLoggerFactory(ILoggerFactory)` — creates the `Waystone.Monads` category
  - `UseLoggerFactoryFrom(IServiceProvider)` — resolves via
    `GetService(typeof(ILoggerFactory))`, so it needs no DI package and works with any
    container; throws a directed `InvalidOperationException` naming `UseLoggerFactory`
    when none is registered.
- `MonadLoggingOptions : IMonadOptionsSatellite` holds the `ILogger` — defaulting to
  `NullLogger.Instance`, so no null check is needed — and the `LogLevel`. Required
  because a subscription is process-wide and cannot carry `AsyncLocal` state; the
  satellite is what keeps `BeginScope(o => o.UseLoggerFactory(f, LogLevel.Warning))`
  working. Subscriber callbacks run synchronously inside `Write` on the caller's async
  context, so `MonadLoggingOptions.Current` resolves correctly.
- One subscription, created lazily on first use behind an idempotency latch, so repeated
  configuration does not double-log.
- `InternalsVisibleTo("Waystone.Monads.Extensions.Logging")` in core, for
  `MonadOptions.Satellite<T>` and `IMonadOptionsSatellite`.
- Solution entry, `README.md`, and a PublicAPI baseline for the new package.

### PR 3 — the obsoletion

- `[Obsolete]` on `UseExceptionLogger`, shaped like `ErrorCodeFactory.FromEnum`'s, naming
  the replacement and `7.0.0`.
- Update `CallerInfo`'s `<remarks>` cref and the core `README.md` configuration section.
- Grep for our own call sites — `CS0618` is suppressed here, so the build will not say.
- File the v7 removal issue.

### Deferred to later PRs

- `feat: count Err results as a native metric` — the `waystone.monads.errors` counter.
  Held back because it needs a hook in `Err` construction, which is the hot path DRA-100
  optimised, plus the allowlist design for `error.type`. It deserves its own benchmark
  and its own review.
- `feat: add LogIfErr and LogIfNone` — opt-in per-`Result` logging with an explicit level
  per call, the original DRA-93 scope.

## Verification

- `dotnet test` across the full local matrix — `net472`, `net481`, `net8.0`, `net10.0`.
  CI runs `net8.0` only, so the matrix is a local responsibility.
- Metric assertions through `MetricCollector<T>`. It binds to a global `Meter` when scope
  is null, so these tests must not run in parallel with anything else touching the meter.
- `git diff --stat -- '*PublicAPI*'` reviewed deliberately on every PR — new public
  surface must appear in `Unshipped.txt` and move to `Shipped.txt` before merge.
- Benchmarks showing the unobserved path unchanged.
- One paired `draekien-industries/docs` PR covering the whole stack, merged after this
  repository's, with a `SUMMARY.md` entry for any new page.

## Settled since drafting

**`Waystone.Monads.OpenTelemetry` is cancelled** — decided 2026-08-26, DRA-94 closed as
won't-do. With metrics native and tracing out of scope it retained only per-call
`Activity.Current` helpers and semantic-convention wrappers, which does not justify a
package that is republished on every release of every other package here. A consumer
marking their own span writes
`Activity.Current?.SetStatus(ActivityStatusCode.Error, error.Message)` at the call site
they chose. The `error.type` allowlist lives in core, with the counter that needs it.

So the observability surface for this milestone is, in total: a native `Meter`, a
`DiagnosticListener`, and one opt-in logging package. Nothing else.

## Open

- The log level deviation from semantic conventions — `Debug` rather than the
  recommended `WARN`. Recorded as a decision above; reopen only if the default proves
  wrong in practice.
