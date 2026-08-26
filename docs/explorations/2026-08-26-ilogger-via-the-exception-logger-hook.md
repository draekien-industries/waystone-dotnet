---
title: ILogger integration via the ExceptionLogger hook
date: 2026-08-26
status: done
---

> **Outcome — not adopted, 2026-08-26.** The milestone chose automatic instrumentation,
> which this option cannot deliver. See
> [ADR 0004](../adr/0004-emit-observability-signals-natively-from-core.md) for the
> decision and the sibling exploration's B′ design for what was built instead. Retained
> as the record of why the cheaper route was available and then closed off.

Option A of two for [DRA-93](https://linear.app/draekien-industries/issue/DRA-93).
The alternative is
[ILogger integration via a DiagnosticSource](2026-08-26-ilogger-via-a-diagnostic-source.md).
Read both before acting on either. **The verdict of the pair is recorded at the bottom
of this document**, because the evidence favours this option.

## Pros and cons

| | |
| --- | --- |
| **Pro** | Least machinery. The package assigns one adapter delegate to a field that already exists; there is no new emission mechanism to design, test or document. |
| **Pro** | No new dependency on `Waystone.Monads` core. The core package's runtime dependency set is untouched. |
| **Pro** | No new public contract in core. Nothing is frozen that we might want to reshape. |
| **Pro** | Testable with a plain delegate assertion — no listener harness, no subscription lifetime to manage. |
| **Pro** | `MonadOptions.BeginScope` already clones `ExceptionLogger`, so scoped level overrides work with no satellite and no extra code. |
| **Pro** | Leaves the door open. Because the consumer-facing surface is identical under either option, moving to a `DiagnosticSource` later is an internal change costing no consumer break. |
| **Con** | **Cannot support automatic instrumentation.** This is disqualifying rather than merely costly if that is wanted — see below. |
| **Con** | `ExceptionLogger` holds **one** delegate, so the swallowed-exception event can only ever have one observer. A second one requires multicasting, ordering and per-observer disposal — a hand-rolled reconstruction of `DiagnosticSource`. |
| **Con** | Third parties cannot observe handled exceptions without taking one of our packages. There is no vendor-neutral subscription point. |
| **Con** | Keeps a bespoke hook, rather than the platform mechanism, as the library's instrumentation story. |

## The question

`MonadOptions.ExceptionLogger` is to be obsoleted in favour of the consumer's own
`ILogger`. How does an `ILogger` reach the eight call sites that currently report a
swallowed exception?

## What exists today

`MonadOptions.ExceptionLogger` (`src/Waystone.Monads/Configs/MonadOptions.cs:71`) is
an `internal Option<Action<Exception, CallerInfo>>`, set through the public
`UseExceptionLogger` (`:213`) and read in exactly one place — `MonadOptions.Log`,
at `:106`. `Log` is reached from eight sites, all of them `Try`/`TryAsync` paths that
swallow an exception: `Options/Option.cs:71,126,198,271` and
`Results/Result.cs:92,170,266,361`. The hook never sees `Error` or `ErrorCode`, only a
raw `Exception` and a `CallerInfo`.

`Waystone.Monads` grants `InternalsVisibleTo` to `Waystone.Monads.Tests` and
`Waystone.Monads.FluentValidation`. `Microsoft.Extensions.Logging.Abstractions` is
already pinned centrally at `10.0.2`.

## The design

A new `Waystone.Monads.Extensions.Logging` package, following the
`Waystone.Monads.FluentValidation` shape: `netstandard2.0`, `LangVersion latestMajor`,
`Nullable enable`, PolySharp with `PrivateAssets all`, a `ProjectReference` to
`Waystone.Monads`, and a `README.md` packed at the root. Its only package reference is
`Microsoft.Extensions.Logging.Abstractions`.

Three extension methods on `MonadOptions`, each taking an optional
`LogLevel level = LogLevel.Debug`:

```csharp
MonadOptions.Configure(o => o.UseLogger(myLogger));           // caller's own category
MonadOptions.Configure(o => o.UseLoggerFactory(factory));     // "Waystone.Monads" category
MonadOptions.Configure(o => o.UseLoggerFactoryFrom(app.Services));   // resolves ILoggerFactory
```

`UseLoggerFactoryFrom` extends `System.IServiceProvider` and resolves through
`GetService(typeof(ILoggerFactory))`, so it needs no dependency-injection package and
works with any container that can produce a provider. It throws a directed
`InvalidOperationException` naming `UseLoggerFactory` when no factory is registered.

Each sets core's internal `ExceptionLogger` to an adapter that emits one structured
call carrying the exception plus `{MemberName}`, `{LineNumber}` and
`{ArgumentExpression}` as named properties, so they stay queryable in a structured
backend. This requires adding `InternalsVisibleTo("Waystone.Monads.Extensions.Logging")`
to core — a one-line change with no public surface impact.

**No satellite is needed.** `BeginScope` already copies `ExceptionLogger` from the
options it snapshots, so `BeginScope(o => o.UseLoggerFactoryFrom(sp, LogLevel.Warning))` gives
the scoped level override for free. The only thing a satellite would buy is changing the
level without re-passing the logger, which nothing currently asks for.

## How this sits with DRA-94

[DRA-94](https://linear.app/draekien-industries/issue/DRA-94) is the OpenTelemetry
counterpart in the same milestone, and the pair was initially assumed to want a shared
emission point in core. **Read against its actual scope, it does not.**

| | DRA-93 (this) | DRA-94 (OpenTelemetry) |
| --- | --- | --- |
| Fact observed | An exception swallowed inside `Try`/`TryAsync` | An `Err` result the consumer has decided is notable |
| Data carried | `Exception` + `CallerInfo` | `Error` + `ErrorCode` |
| Where it fires | Inside core, at `MonadOptions.Log` | At a call site the consumer chose |
| Trigger | Ambient, once configured | Explicit, per call |
| Mechanism | `ILogger` | Static `ActivitySource` / `Meter` |
| Touches core? | Only `InternalsVisibleTo` | **No** — the issue says so explicitly |

DRA-94 states *"No changes to `Waystone.Monads` core or `MonadOptions`"*, and specifies
opt-in extensions — `result.RecordOnActivity(activity)` setting
`SetStatus(ActivityStatusCode.Error, …)` on `Err`, and a `Counter<long>` on a `Meter`
keyed by `ErrorCode.Value`. Both emit directly from the extension the consumer invoked.
Neither subscribes to anything.

The boundary is also drawn deliberately in the other direction: DRA-94 puts OTEL Logs
**explicitly out of scope**, ceding the log signal to this package, *"do not also build
an OTEL Logs exporter path here — that would duplicate signal ownership across two
packages."* Two packages, two signals, one owner each.

Three consequences for this option:

1. **The single-delegate limitation has no committed consumer.** `ExceptionLogger`
   supporting only one observer is a genuine constraint, but nothing in the milestone
   is queued to become the second one.
2. **The dependency argument does not transfer.** DRA-94 needs
   `System.Diagnostics.DiagnosticSource` and will add the central pin — but inside
   *its own* package. That does nothing to justify putting the dependency in core.
3. **The `ErrorCode` cardinality problem DRA-94 raises is not ours.** The swallow hook
   never sees an `ErrorCode`, so free-text codes cannot reach a log property from here.

### The variable that decides it: automatic instrumentation

The analysis above holds for DRA-94 **as currently written**. The human has since raised
two things that change it — that DRA-94 could be opt-in at the global level, and,
decisively, **automatic instrumentation**.

Automatic instrumentation is a term of art: the consumer installs a package, opts in
once, and the library instruments itself with no per-call-site code, as with
`AddAspNetCoreInstrumentation()` or `AddHttpClientInstrumentation()`. It works only
because the instrumented library emits on a standard, discoverable source.

**This option cannot deliver it.** `ExceptionLogger` holds one delegate, so a logging
package and an OpenTelemetry package would silently overwrite each other; the first
`UseLogger*` call and the first OTel opt-in cannot both win. Adding multicast, ordering
and per-observer disposal to make them coexist is a hand-rolled reconstruction of
`DiagnosticSource` inside `MonadOptions`.

So if automatic instrumentation is wanted, this option is **disqualified, not
outscored**, and the sibling exploration's B′ design is the answer. If it is not wanted —
if both packages stay opt-in per call site, as DRA-93 and DRA-94 both currently
specify — this option remains the right one.

A narrower global opt-in that merely *counts* every `Err` sits between the two. It needs
a new core emission point rather than a second subscriber to the existing one, so it
does not by itself argue against this option. Worth noting that the reasoning both
issues used to reject automatic behaviour — log volume, and the library being unable to
judge whether an `Err` is a bug or an expected branch — was about *severity and noise*,
which does not transfer to a counter. A `Counter<long>` keyed by `ErrorCode` costs one
increment and carries no severity judgement. But it reopens a decision both issues made
deliberately, and it collides with the free-text `ErrorCode` cardinality risk DRA-94
already flags.

## Verdict — recommended only if automatic instrumentation is out of scope

**Take this option if DRA-93 and DRA-94 both stay opt-in per call site**, as both issues
currently specify. Ship `Waystone.Monads.Extensions.Logging` against the existing
`ExceptionLogger` hook and treat the sibling document as the recorded upgrade path.

On that scope the case for the alternative collapses. It rested on DRA-94 sharing the
emission point, and DRA-94 as written does not. What remains — multicast, vendor-neutral
subscription, `IsEnabled` gating — is real engineering value with **no committed
consumer**, which is the definition of speculative under this repository's rules.

**If automatic instrumentation is in scope, do not take this option.** Go to the sibling
document's B′ design and build the native diagnostic surface in core up front.
Retrofitting multicast onto `ExceptionLogger` afterwards is strictly worse than starting
from the mechanism that already provides it.

Where automatic instrumentation is *not* the trigger, the decision stays cheap to
revisit, and that is the point: `UseLogger`, `UseLoggerFactory` and `UseLoggerFactoryFrom` are
byte-identical under either option, so switching the internals later breaks nobody.

### The trigger to revisit

Move to the sibling design when a **second** observer of the swallowed exception event
actually exists. Concretely, any of:

- A decision to make `Waystone.Monads` automatically instrumentable.
- A consumer asking to observe handled exceptions without taking a Waystone package.
- A third ecosystem package in this milestone wanting the same event.

Until one of those lands, the hook that already exists is sufficient.

### Long-term consequences to live with

1. **`UseLogger` / `UseLoggerFactory` / `UseLoggerFactoryFrom` is public API the moment it
   ships.** Deprecate-never-remove applies; a wrong dependency-injection shape lives
   until `8.0.0`.
2. **Core's `InternalsVisibleTo` list grows with every ecosystem package** in this
   milestone. That is the road ADR 0003 chose, and this confirms it rather than
   reopening it.
3. **Two logging paths coexist through all of 6.x.** A consumer holding both the
   obsolete hook and this package logs twice — a supported, documented state we own
   until removal in `7.0.0`.
4. **Instrumentation stays per-package rather than unified.** DRA-93 owns logs, DRA-94
   owns traces and metrics, each emitting through its own mechanism. Coherent while
   there are two signals with one owner each; it is the arrangement that would need
   revisiting first if a third signal or a second observer appears.
5. **We keep a bespoke hook one release longer than necessary.** If the trigger above
   fires early, we will have shipped an adapter we then rewrite internally — cheap,
   but not free.
