---
title: ILogger integration via a diagnostic source
date: 2026-08-26
status: done
---

> **Outcome — B′ adopted, 2026-08-26.** The narrow design B was not taken; the native
> instrumentation design B′ was. Recorded as
> [ADR 0004](../adr/0004-emit-observability-signals-natively-from-core.md), which is the
> binding decision. This document is the evidence behind it.

Option B of two for [DRA-93](https://linear.app/draekien-industries/issue/DRA-93).
The alternative is
[ILogger integration via the ExceptionLogger hook](2026-08-26-ilogger-via-the-exception-logger-hook.md).
Read both before acting on either.

**Scope note.** This document was originally about swapping one private hook for another
private mechanism. It grew once the human raised automatic instrumentation, because that
requirement cannot be met by either private mechanism. What follows covers both the
narrow form (B) and the form the requirement actually demands (B′).

## Pros and cons

| | |
| --- | --- |
| **Pro** | The only option that supports **automatic instrumentation**. A consumer opts in once and the library instruments itself, which is impossible over a single-delegate hook. |
| **Pro** | One emission point serves every observer — logs, traces, metrics — with multicast, subscription lifetime and enablement gating supplied by the platform rather than hand-rolled. |
| **Pro** | In its B′ form, metrics need **no Waystone package at all**: `.AddMeter("Waystone.Monads")` in the consumer's existing OpenTelemetry setup is the whole integration. |
| **Pro** | `IsEnabled` gating means payloads allocate only when something is listening — consistent with the discipline DRA-100 established. |
| **Pro** | One dependency buys all three primitives: `ActivitySource`, `Meter` and `DiagnosticListener` all ship in `System.Diagnostics.DiagnosticSource`. |
| **Con** | Source, meter, event and tag **names become public contract** — under automatic instrumentation the names *are* the API, so this cannot be deferred by keeping a payload internal. |
| **Con** | Core gains a permanent runtime dependency on `System.Diagnostics.DiagnosticSource`. |
| **Con** | Materially rescopes DRA-94, which currently says core is untouched. |
| **Con** | More code, and a listener/meter test harness rather than a delegate assertion. |
| **Con** | A subscription is process-wide, so `AsyncLocal` scoping no longer falls out for free — it needs an ADR-0003 satellite to survive. |
| **Con** | Through all of 6.x, a consumer holding both the obsolete hook and this package logs the same exception twice. |

## The question

`MonadOptions.ExceptionLogger` is to be obsoleted in favour of the consumer's own
`ILogger`. How does an `ILogger` reach the eight call sites that report a swallowed
exception — and can the same route carry DRA-94's traces and metrics?

## What exists today

See the sibling exploration for the full survey. The load-bearing facts:

- `MonadOptions.Log` (`src/Waystone.Monads/Configs/MonadOptions.cs:106`) is the single
  choke point through which all eight swallow sites report.
- `ExceptionLogger` holds **one** delegate. It cannot serve two observers.
- Core already ships three non-private package references —
  `Microsoft.Bcl.AsyncInterfaces`, `System.Threading.Tasks.Extensions` and
  `System.ValueTuple` — so adding a fourth is not precedent-breaking.
- ADR 0003 established the satellite registry, which is how an extension package
  attaches `AsyncLocal`-scoped configuration to `MonadOptions`.
- `Microsoft.Extensions.Logging` has **no ambient static logger** — deliberately. Only
  `NullLogger.Instance` and the `LoggerFactory.Create` factory method. This is why logs
  cannot be made native the way traces and metrics can, and why a configuration call
  survives in every design below.

## How this sits with DRA-94

[DRA-94](https://linear.app/draekien-industries/issue/DRA-94) is the OpenTelemetry
counterpart in the same milestone. **As written**, it does not share an emission point
with this package:

| | DRA-93 (this) | DRA-94 as written |
| --- | --- | --- |
| Fact observed | An exception swallowed inside `Try`/`TryAsync` | An `Err` the consumer has decided is notable |
| Data carried | `Exception` + `CallerInfo` | `Error` + `ErrorCode` |
| Where it fires | Inside core, at `MonadOptions.Log` | At a call site the consumer chose |
| Trigger | Ambient, once configured | Explicit, per call |
| Touches core? | Only `InternalsVisibleTo` | **No** — stated explicitly |

DRA-94 specifies `result.RecordOnActivity(activity)` and a `Counter<long>` keyed by
`ErrorCode.Value`, both emitted directly from the extension the consumer invoked, and it
puts OTEL Logs explicitly out of scope so that *"signal ownership"* is not duplicated
across two packages.

On that reading the sibling exploration wins, and this one is speculative.

### What automatic instrumentation changes

The human has since raised two things: that DRA-94 could be opt-in at the global level,
and — decisively — **automatic instrumentation**.

Automatic instrumentation is a term of art. The consumer installs a package, opts in
once, and the library instruments itself with no per-call-site code:
`AddAspNetCoreInstrumentation()`, `AddHttpClientInstrumentation()`. It works only
because the instrumented library **emits on a standard source** that an instrumentation
package can subscribe to.

That requirement cannot be satisfied over `ExceptionLogger`. The field holds one
delegate, so a logging package and an OpenTelemetry package would overwrite each other.
Retrofitting multicast, ordering and per-observer disposal onto it is a hand-rolled
reconstruction of `DiagnosticSource`. **If automatic instrumentation is wanted, the
sibling option is disqualified outright** — not merely outscored.

## Design B — narrow: a diagnostic source for logs

The minimum that satisfies multicast. Core gains:

```csharp
public static class MonadDiagnostics
{
    public const string ListenerName = "Waystone.Monads";
    public const string ExceptionHandledEventName = "Waystone.Monads.ExceptionHandled";

    internal static readonly DiagnosticListener Listener = new(ListenerName);
}

public sealed record ExceptionHandled(Exception Exception, CallerInfo Caller);
```

`MonadOptions.Log` gains the emission and **keeps** the old hook, so both work through
6.x:

```csharp
internal void Log(Exception exception, CallerInfo callerInfo)
{
    if (Debugger.IsAttached) { /* unchanged */ }

    if (MonadDiagnostics.Listener.IsEnabled(MonadDiagnostics.ExceptionHandledEventName))
    {
        MonadDiagnostics.Listener.Write(
            MonadDiagnostics.ExceptionHandledEventName,
            new ExceptionHandled(exception, callerInfo));
    }

    ExceptionLogger.Inspect(logger => logger.Invoke(exception, callerInfo));
}
```

The package's consumer-facing surface is **identical** to the sibling option —
`UseLogger(ILogger)`, `UseLoggerFactory(ILoggerFactory)`,
`UseLoggerFactoryFrom(IServiceProvider)`, each with `LogLevel level = LogLevel.Debug`. Only the
internals differ, which is why the choice can be made on engineering grounds without
consulting any consumer's call sites.

**The satellite returns, and here it earns its keep.** A subscription is process-wide and
cannot carry `AsyncLocal` configuration, which would silently kill the `BeginScope` level
override. So a `MonadLoggingOptions : IMonadOptionsSatellite` holds the `ILogger` —
defaulting to `NullLogger.Instance`, so no null check is needed — and the `LogLevel`, and
the single subscription reads `MonadLoggingOptions.Current` at event time. Subscriber
callbacks run synchronously inside `Write` on the caller's async context, so the scoped
value resolves correctly. The subscription is created lazily on the first `UseLogger*`
call behind an idempotency latch, so calling it twice does not double-log.

`InternalsVisibleTo("Waystone.Monads.Extensions.Logging")` is still required — for
`MonadOptions.Satellite<T>` and `IMonadOptionsSatellite` rather than for
`ExceptionLogger`.

## Design B′ — native instrumentation

What automatic instrumentation actually asks for, and the stronger design.

Rather than emitting a private event for our own packages to bridge, core owns a
first-class diagnostic surface named after itself. All three primitives live in the one
`System.Diagnostics.DiagnosticSource` assembly, whose `netstandard2.0`/`net462` floor
DRA-94 already confirmed:

- a `Meter` named `Waystone.Monads`, carrying counters for handled exceptions and for
  `Err` results;
- the `DiagnosticListener` above, carrying the rich payload logs need.

**No `ActivitySource`, on the spec's own advice.** Semantic conventions deprecated
`exception.escaped` and state that it is no longer recommended to record exceptions that
are handled and do not escape the scope of a span — which is exactly what a swallowed
`Try` exception is — directing them to logs instead. We also create no spans of our own,
and a span per monad operation would be far too heavy to consider. An `ActivitySource` is
required only to *create* activities; enriching the ambient `Activity.Current` needs
none. So tracing stays with the opt-in per-call helpers and ships no source.

The consequence is the interesting part. **Metrics stop needing a Waystone package at
all**, because the consumer already has a mechanism for enabling them:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter("Waystone.Monads"));
```

That is automatic instrumentation, delivered by the consumer's existing OpenTelemetry
configuration rather than by us. It also makes the signal genuinely vendor-neutral — a
Datadog or Prometheus user gets it without any Waystone dependency.

Logs cannot work this way, because MEL has no ambient logger to discover. So
`Waystone.Monads.Extensions.Logging` survives unchanged, with its
`UseLogger`/`UseLoggerFactory`/`UseLoggerFactoryFrom` opt-in.

### What this does to DRA-94

It **shrinks** the package rather than growing it. The emission moves into core, and
`Waystone.Monads.OpenTelemetry` is left holding only what genuinely belongs to it: the
opt-in per-call helpers for consumers who want to mark a specific `Err`, semantic
convention helpers, the `ErrorCode` cardinality normalisation hook, and the
documentation. That residue was judged not to justify a package at all, given the
milestone's own warning that each new package is a permanent maintenance surface
republished on every release. **DRA-94 was cancelled on 2026-08-26 as a consequence of
this exploration.**

### What it costs

The names become the contract. Under automatic instrumentation the source name, meter
name, instrument names and tag keys are what consumers bind their dashboards and
alerts to, so **they are public API from the first release** and cannot be kept internal
and promoted later. That was the hedge proposed for design B, and B′ removes it. The
cardinality risk DRA-94 raises for `ErrorCode.Value` also becomes core's problem rather
than a package's, since core would own the counter.

## Rejected within this option

**`EventSource` instead of `DiagnosticSource`.** In-box on `netstandard2.0`, so it would
avoid the dependency. Rejected: payloads are primitive-only, so `CallerInfo` would be
flattened into loose parameters, bridging in-process through `EventListener` is clunkier
than `IObserver<KeyValuePair<string, object>>`, and it provides no `ActivitySource` or
`Meter`, so it cannot serve B′ at all.

**An internal static multicast event in core.** Cheaper than either, zero-dependency,
and enough for two first-party subscribers. Rejected because it gives no path to
automatic instrumentation: third parties cannot discover it, and OpenTelemetry has no
way to consume it.

## Verdict

**If automatic instrumentation is wanted, take B′.** It is the only design here that
supports it, the sibling option is disqualified rather than outscored, and B′ delivers it
more cheaply than a bridging package would — the consumer's own OTel configuration does
the work.

**If it is not, take the sibling option**, and treat this document as the recorded
upgrade path. The narrow design B is the weakest of the three: it pays most of B′'s cost
in dependency and machinery while delivering only what the sibling option already
delivers, and it hedges a contract that B′ shows we would have to expose anyway.

This turns on a decision that has not been made: **does the milestone want
`Waystone.Monads` to be automatically instrumentable, or to expose observability only
through opt-in extension methods?** DRA-93 and DRA-94 as written both chose the latter,
deliberately and with reasons. Choosing the former is a real rescope of DRA-94 and needs
to be recorded there before either package is built.

### Long-term consequences of taking B′

1. **Instrument and tag names are permanent public API from day one.** Dashboards and
   alerts bind to them. Renaming the meter, an instrument or a tag key is a breaking
   change for consumers with no compiler to catch it.
2. **Core owns the `ErrorCode` cardinality problem.** Free-text codes as a metric
   dimension is a cardinality trap in most backends; owning the counter means owning
   the normalisation hook and the warning in the docs.
3. **Core carries `System.Diagnostics.DiagnosticSource` permanently.** Dropping a
   dependency is itself a break for anyone relying on the transitive reference. Low
   risk — it is near-universally present — but a one-way door.
4. **`MonadOptions.Log` becomes the library's instrumentation choke point.** Anything we
   later want to observe must route through the `Try` swallow path or arrive as a new
   event, and each new event is a new contract.
5. **`Waystone.Monads.OpenTelemetry` did not survive this.** With the signals native the
   package held only helpers, and it was cancelled rather than shipped. Deciding that
   now was much cheaper than un-shipping it later, but it does mean the observability
   story for this milestone is a native meter, a diagnostic listener and one logging
   package — with no OpenTelemetry-branded package for a consumer to search for.
6. **Two logging paths coexist through all of 6.x.** Double-logging for a consumer using
   both is a supported, documented state we own until removal in `7.0.0`.
7. **`UseLogger` / `UseLoggerFactory` / `UseLoggerFactoryFrom` is public API the moment it
   ships.** Deprecate-never-remove applies; a wrong dependency-injection shape lives
   until `8.0.0`.
