---
id: 0004
title: Emit observability signals natively from core
status: accepted
date: 2026-08-26
deciders: [william-pei]
tags: [observability, package-boundaries, monads]
supersedes:
superseded-by:
---

# 0004 — Emit observability signals natively from core

## Context

`Waystone.Monads` reports a swallowed exception through `MonadOptions.ExceptionLogger`,
an `internal Option<Action<Exception, CallerInfo>>` set by the public
`UseExceptionLogger` and read at one place, `MonadOptions.Log`. Eight `Try`/`TryAsync`
paths reach it. It carries a raw `Exception` and a `CallerInfo`; it never sees `Error` or
`ErrorCode`, and it holds exactly one delegate.

Two packages in the v6.x milestone want to observe what the library does.
[DRA-93](https://linear.app/draekien-industries/issue/DRA-93) wants a consumer's
`ILogger` to receive handled exceptions instead of a hand-written adapter delegate.
[DRA-94](https://linear.app/draekien-industries/issue/DRA-94) wants OpenTelemetry traces
and an `Err` counter. Both were originally scoped as opt-in extension methods invoked
per call site, and DRA-94 stated explicitly that core would not be touched.

The requirement that broke that scoping is **automatic instrumentation**: a consumer
installs a package or flips one switch, and the library instruments itself with no
per-call-site code, in the manner of `AddAspNetCoreInstrumentation()`. This only works
where the instrumented library emits on a standard, discoverable source. A single
delegate field cannot serve it — a logging observer and a tracing observer would
overwrite one another, and the first configured would silently win.

Two further constraints shaped the options.
`Microsoft.Extensions.Logging` has no ambient static logger, by design; only
`NullLogger.Instance` and the `LoggerFactory.Create` method exist. So log output can
never be enabled without the consumer handing us a logger or a provider, however the
other signals are delivered. And `System.Diagnostics.DiagnosticSource` carries
`ActivitySource`, `Meter` and `DiagnosticListener` in one assembly, targeting
`netstandard2.0` and `net462`, which clears this repository's framework floor.

## Decision

We will emit observability signals natively from `Waystone.Monads` core, under sources
named after the library itself, rather than exposing them only through opt-in extension
methods or a private delegate hook. Core takes a dependency on
`System.Diagnostics.DiagnosticSource` and owns the emission: a `Meter` named
`Waystone.Monads` makes metrics available to any consumer through the OpenTelemetry
configuration they already maintain, with no Waystone package in between, and a
`DiagnosticListener` of the same name carries the richer payload that log bridging needs.
`ILogger` output remains an opt-in configuration call in
`Waystone.Monads.Extensions.Logging`, because the logging abstraction offers nothing to
discover.

We will **not** ship an `ActivitySource`. Semantic conventions no longer recommend
recording exceptions that are handled and do not escape a span — which is precisely what
a swallowed `Try` exception is — and have deprecated `exception.escaped` accordingly,
steering such exceptions to logs. We create no spans of our own, so there is nothing for
an `ActivitySource` to carry. Consumers who want an error reflected on their own span
retain the opt-in per-call helpers, which enrich `Activity.Current` and need no source
from us.

## Consequences

Observability stops being a per-package feature and becomes a property of the library. A
consumer adds the library's name to the meter list they already maintain and gets
metrics, including from code paths they never explicitly instrumented. The signals are
vendor-neutral: a Prometheus or Datadog user is served identically to an OpenTelemetry
one, and neither takes a Waystone dependency to do it.

**Metrics are native; logs are not, and traces are not emitted at all.** The three
signals are deliberately asymmetric, and a reader expecting uniformity should not
"fix" it. Metrics are discoverable, so they need no configuration call. Logs cannot be,
because `Microsoft.Extensions.Logging` publishes no ambient logger, so a configuration
call survives for them permanently. Traces are absent because semantic conventions
direct handled, non-escaping exceptions to logs rather than span events.

**Signal names become permanent public API from the first release.** Source names, meter
names, instrument names and tag keys are what consumers bind dashboards and alerts to.
There is no compiler to catch a rename and no deprecation path that a dashboard
observes, so these names are governed by the same deprecate-never-remove rule as the type
surface, with less tooling to enforce it. An earlier proposal to keep the payload
internal and promote it once validated is **not available** under this decision — the
names are the contract, and automatic instrumentation means exposing them on day one.

**Core inherits the `ErrorCode` cardinality problem.** `ErrorCode` wraps free text with
no enum constraint, so any instrument tagged by an error code is a cardinality trap in
most metrics backends. DRA-94 raised this as a package concern; owning the counter makes
it core's, along with whatever normalisation or allowlist hook mitigates it.

**Emission sits on paths that were deliberately optimised.** DRA-100 cut allocations and
dispatch on the `Option` and `Result` hot paths. Every signal added here must be gated on
the platform's enablement checks so that an unobserved process pays close to nothing, and
must carry a benchmark showing it.

**`Waystone.Monads.OpenTelemetry` is not shipping.** DRA-94 was cancelled on 2026-08-26
as a direct consequence of this decision. With metrics native and tracing out of scope,
that package retained only per-call helpers and semantic-convention wrappers — and one
version covers every package here, so each one is republished on every release of any
other. A permanent maintenance surface was not justified by the residue.

What a consumer loses is nothing they cannot write themselves. Marking their own span
from an `Err` is `Activity.Current?.SetStatus(ActivityStatusCode.Error, error.Message)`
at the call site they chose, which needs no library. The `error.type` allowlist moved
into core along with the counter that needs it. Should demand for a package ever appear,
it can be created then, against evidence — which is a far better position than
un-shipping one.

We accept a permanent runtime dependency on `System.Diagnostics.DiagnosticSource` in
core. Removing a package dependency is itself a breaking change for anyone relying on the
transitive reference, so this is a one-way door. The risk is low — the assembly is
present in nearly every non-trivial .NET application already — and core already ships
three non-private package references, so it does not set a precedent.

`MonadOptions.ExceptionLogger` is obsoleted rather than removed, per the standing
deprecation rule, so two reporting paths coexist through all of 6.x. A consumer holding
both the old hook and the new package will see the same exception reported twice. This is
documented and resolves itself when the hook is removed in `7.0.0`.

## Alternatives considered

**Keep `ExceptionLogger` and adapt it to `ILogger` inside the new package.** The cheapest
option by a wide margin: one adapter delegate assigned to a field that already exists, no
new dependency in core, no new public contract, and `BeginScope` already clones the field
so scoped configuration needs no extra machinery. Rejected because a single delegate
cannot serve two observers, so automatic instrumentation is unreachable and a second
observability package would contend for the one slot. Making it multicast means
re-implementing `DiagnosticSource` inside `MonadOptions`. Recorded in full at
`docs/explorations/2026-08-26-ilogger-via-the-exception-logger-hook.md`, which was the
correct choice right up until automatic instrumentation entered scope.

**A private `DiagnosticSource` with an internal payload, bridged by our own packages.**
Multicast without freezing a contract, with the payload promoted to public later once two
consumers had validated its shape. Rejected because it pays most of the cost of the
adopted design — the dependency, the subscription machinery, the test harness — while
delivering only what the cheaper alternative already delivered, and because automatic
instrumentation requires third parties to discover the source, which an internal payload
prevents. The hedge it offered turned out to be unavailable for the thing we actually
wanted.

**`EventSource` instead of `DiagnosticSource`.** In-box on `netstandard2.0`, so it would
avoid the dependency entirely. Rejected because its payloads are primitive-only, so
`CallerInfo` would have to be flattened into loose parameters; because bridging in-process
through `EventListener` is clunkier than `IObserver<KeyValuePair<string, object>>`; and
decisively because it provides neither `ActivitySource` nor `Meter`, so it cannot carry
traces or metrics at all.

**An internal static multicast event in core.** Cheaper than every option above,
zero-dependency, and sufficient for two first-party subscribers. Rejected because
OpenTelemetry has no way to consume it and third parties cannot discover it, so it fails
the requirement that motivated the change.

**An injected entry point — `ITry` resolved from dependency injection.** Would remove the
ambient state entirely and let `ILogger<T>` categories work properly. Rejected because it
makes a container mandatory to construct a monad, which excludes console applications and
prevents `Result.Try` being called from a static helper or a pure function.
