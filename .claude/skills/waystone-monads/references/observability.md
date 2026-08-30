# Observability

`Option.Try` and `Result.Try` catch the exception and hand back a `None` or an
`Err`. The `Option` case discards it entirely. The library reports what it caught
so nothing has to be lost, on channels a consumer opts into:

| Channel | Install | What arrives |
| --- | --- | --- |
| Metrics | Nothing | A counter of handled exceptions, tagged by exception type and by monad |
| Logs | `Waystone.Monads.Extensions.Logging` | One entry per handled exception, carrying the call site |
| Events | Nothing | Three `DiagnosticListener` events, subscribed to through `MonadDiagnostics` |

Metrics need no package because a metrics pipeline discovers meters by name — the
library publishes one and the pipeline names it. `Microsoft.Extensions.Logging`
publishes no ambient logger to discover, so logging always costs a configuration
call. That call is what the package exists to be.

## Count them

Add the library's meter to the pipeline that already collects meters:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter(MonadDiagnostics.MeterName));
```

One instrument arrives, `waystone.monads.exceptions_handled`, a `Counter<long>`
counted in `{exception}`. It carries two tags:

| Tag | Values | Answers |
| --- | --- | --- |
| `error.type` | The exception's full type name | Which exception. The OpenTelemetry attribute of the same name |
| `waystone.monads.monad` | `option` or `result` | Whether the exception survived anywhere else |

The second tag is the one to build an alert on. An exception counted as `option`
was thrown away, so the counter is the only record it happened at all. One counted
as `result` also went into the `Err`, where error handling still has it.

**Name every string through `MonadDiagnostics`,** never as a literal. The constants
are `MeterName`, `ListenerName`, `ExceptionHandledEventName`,
`ScopeDisposedOutOfOrderEventName`, `ConfigurationNotAppliedEventName`,
`ExceptionsHandledInstrumentName`, `ErrorTypeTagKey`, `MonadTagKey`,
`OptionMonadTagValue` and `ResultMonadTagValue`. A mistyped literal subscribes to
nothing and reports nothing — no exception, no warning, an empty dashboard. To
subscribe, do not reach for the event-name constants at all: use the tokens under
[Subscribe to an event](#subscribe-to-an-event), which name the event for you.

## Log them

Install `Waystone.Monads.Extensions.Logging` and make one configuration call at
start-up. Pick by what the application already has:

| The application has | Call |
| --- | --- |
| A service provider | `UseLoggerFactoryFrom(app.Services)` |
| An `ILoggerFactory` and no container | `UseLoggerFactory(factory)` |
| An `ILogger` already in hand | `UseLogger(logger)` |

```csharp
MonadOptions.Configure(options => options.UseLoggerFactoryFrom(app.Services));
```

`UseLoggerFactoryFrom` resolves through `IServiceProvider.GetService` and takes no
dependency-injection package, so any container that produces a provider works. It
throws when no `ILoggerFactory` is registered, naming `UseLoggerFactory` as the way
round it.

**`LoggerFactory.Create` is not in `Microsoft.Extensions.Logging.Abstractions`,**
which is all the package brings with it. A console application building a factory
by hand also needs `Microsoft.Extensions.Logging` and a provider package such as
`Microsoft.Extensions.Logging.Console`. An application with a host already has both.

Each entry carries the exception plus three properties the compiler captured at the
`Try` call site: `MemberName`, `ArgumentExpression` — the source text of the
delegate that threw — and `LineNumber`.

The exception travels in `ILogger.Log`'s exception parameter, not as properties of
its own, so an OpenTelemetry logging bridge derives `exception.type`,
`exception.message` and `exception.stacktrace` from it once rather than twice.

**Log property names are PascalCase and metric tag keys are dotted.** That is not
an inconsistency to tidy up. Serilog only binds property names matching
`[A-Za-z0-9_]+`, and `{code.function.name}` in a message template renders as
literal text instead of binding. The dotted semantic-convention spellings are kept
where nothing parses a template, on the metric tags.

### Level and category

The default is `LogLevel.Debug`. A `Try` producing a `None` or an `Err` did what it
was asked to, and warning on it is noise. Semantic conventions suggest `WARN` for a
handled exception — pass it deliberately, as the second argument to any of the
three calls, rather than inheriting it.

`UseLoggerFactory` and `UseLoggerFactoryFrom` create the logger in the
`Waystone.Monads` category, so the library is filterable on its own:

```json
{ "Logging": { "LogLevel": { "Waystone.Monads": "Warning" } } }
```

`UseLogger` does not — the logger passed in keeps whatever category it had.

Both the logger and the level live on the `MonadOptions` scope, so `BeginScope`
redirects them for one asynchronous flow and leaves the rest of the process alone.

## UseExceptionLogger is gone

`MonadOptions.UseExceptionLogger` was removed in `7.0.0`, so a call carried over
from 6.x fails as `CS1061`. Replace it with a configuration call, not a
suppression:

```csharp
MonadOptions.Configure(options => options.UseLoggerFactoryFrom(app.Services));
```

Nothing is lost in the move. Every entry still carries the exception and the same
call-site details, and level and category filtering arrive with them — which an
opaque delegate could never give. The hand-written hook held exactly one
delegate, so configuring a second observer replaced the first silently; a
`DiagnosticListener` is shared by any number of subscribers, and the logging
package is simply one of them.

Where a release still carries both, they both fire — so the old call comes out in
the same change the package goes in, or every handled exception is logged twice.

## Subscribe to an event

Reach for this only when building an integration the logging package does not
cover. There are three events, each with a token on `MonadDiagnostics` pairing its
name with its payload:

| Token | Payload | Written when |
| --- | --- | --- |
| `ExceptionHandledEvent` | `ExceptionHandled(Exception, CallerInfo, MonadKind)` | `Try` or `TryAsync` swallowed an exception |
| `ScopeDisposedOutOfOrderEvent` | `ScopeDisposedOutOfOrder(MonadOptions?, MonadOptions?)` | A `MonadOptionsScope` was disposed out of order |
| `ConfigurationNotAppliedEvent` | `ConfigurationNotApplied()` | Options were read before container-registered configuration landed |

The third fires only where configuration is registered through a container —
`Waystone.Monads.Extensions.DependencyInjection` arms it, and
`Waystone.Monads.Extensions.Hosting` is what usually stops it firing. In a
process configuring `MonadOptions` by a static call, it is dormant.

`Subscribe` takes the delegate and returns the subscription:

```csharp
using IDisposable watching = MonadDiagnostics.ExceptionHandledEvent.Subscribe(
    handled => queue.Enqueue(handled));
```

**Use the token, never the name constants, to subscribe.** The constants are still
public and still the contract a dashboard binds to, but a subscriber typing one by
hand can mistype it, cast the payload to the wrong type, or forget the listener
name — and every one of those fails silently with an empty dashboard rather than an
exception. The token cannot be pointed at the wrong event.

Disposing the return value detaches. A subscription meant to last the life of the
process can be abandoned; anything shorter-lived must be disposed, or it leaks an
observer on the process-wide `DiagnosticListener.AllListeners`.

**The subscriber runs on the thread that wrote the event, synchronously.** For
`ExceptionHandledEvent` that is the throwing thread, inside the `catch`. Slow work
there delays the caller waiting for its `None` or `Err`, and an exception thrown
from the subscriber escapes the `Try` that was meant to swallow the original one.
Queue the work and return. Nothing is swallowed on your behalf — that is deliberate,
and throwing from the subscriber is the supported way to make
`ScopeDisposedOutOfOrderEvent` fatal in a test suite.

### Without the helper

The library guarantees a consumer needs no Waystone package to observe it, and
the raw path still works untouched: watch `DiagnosticListener.AllListeners`, match
`MonadDiagnostics.ListenerName`, then subscribe with a predicate matching the event
name. `AllListeners` replays listeners that already exist, so subscribing before or
after the first `Try` makes no difference.

Two traps the helper handles and a hand-written subscriber must handle itself.
`DiagnosticListener.Write` does not apply your predicate — the predicate only gates
`IsEnabled`, so a subscriber receives every event written to that listener and must
check `written.Key` itself. And the payload arrives as `object?`, so it needs a type
check rather than a cast. Two places in the repository still write it by hand and
should stay that way: `EventRecorder`, which is the oracle proving the library
writes its events at all, and `TryDiagnosticsBenchmarks`, which measures this path
on purpose.

## What is never reported

- **Exceptions the library lets through.** All three fire only when `Try` or
  `TryAsync` catches something.
- **Cancellations,** unless `UseCancellationAsFailure` has been called. Without it
  an `OperationCanceledException` propagates, so nothing counts or logs it. With
  it, it is an ordinary caught exception and is counted like any other.
- **Traces.** The library publishes no `ActivitySource` and creates no spans. Mark
  a span you own at the call site instead:

  ```csharp
  result.InspectErr(
      error => Activity.Current?.SetStatus(
          ActivityStatusCode.Error,
          error.Message));
  ```

Both the counter and the listener check whether anything is subscribed before doing
any work, so an unobserved process allocates exactly what it allocated before the
instrumentation existed.

## Test through the logger, not the event

A test asserting that a `Try` swallowed something should configure a recording
`ILogger` inside a `MonadOptions.BeginScope` and assert on what it received. The
scope is per-flow, so parallel tests do not see each other.

The raw event looks like the more direct probe and is the wrong one. A
`DiagnosticListener` and a `Meter` are both process-wide: every subscriber sees
every other test's exceptions, and the test flakes under parallelism. Where a test
must read the meter or the listener directly, throw a marker exception type private
to that test class and filter every snapshot down to it.
