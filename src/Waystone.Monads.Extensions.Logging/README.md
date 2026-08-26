# Waystone.Monads.Extensions.Logging

Routes the exceptions `Waystone.Monads` swallows to a
`Microsoft.Extensions.Logging` `ILogger`.

`Option.Try` and `Result.Try` catch an exception and hand back a `None` or an
`Err`. The exception itself is gone from the caller's point of view — in the
`Option` case, gone entirely. This package makes it turn up in the logging you
already have, without a `try`/`catch` at any call site.

## Install and configure

```
dotnet add package Waystone.Monads.Extensions.Logging
```

On a host, where a service provider exists:

```csharp
var app = builder.Build();

MonadOptions.Configure(options => options.UseLoggerFactoryFrom(app.Services));
```

Without a container — a console application, a test, a worker built by hand:

```csharp
using ILoggerFactory factory = LoggerFactory.Create(
    builder => builder.AddConsole());

MonadOptions.Configure(options => options.UseLoggerFactory(factory));
```

Or hand over a logger you already hold, which then keeps its own category:

```csharp
MonadOptions.Configure(options => options.UseLogger(logger));
```

`UseLoggerFactoryFrom` resolves through `IServiceProvider.GetService` and takes no
dependency-injection package, so any container that can produce a provider works.
It throws if no `ILoggerFactory` is registered, naming `UseLoggerFactory` as the
way round it.

## What gets written

One entry per swallowed exception, carrying the exception itself plus the call
site the compiler recorded:

| Property | Meaning |
| --- | --- |
| `ArgumentExpression` | The source text of the delegate that threw |
| `MemberName` | The member that called `Try` |
| `LineNumber` | The line the `Try` call is on |

The exception travels in `ILogger.Log`'s exception parameter rather than as
properties of its own, so an OpenTelemetry logging bridge derives
`exception.type`, `exception.message` and `exception.stacktrace` from it without
double-reporting.

Properties are PascalCase rather than the dotted `code.function.name` form the
semantic conventions use. Serilog property names must match `[A-Za-z0-9_]+`, and a
dotted token in a message template is not parsed as a property at all — it renders
literally. The dotted form is used where it is safe, on the metric tags the core
package emits.

## Level

`LogLevel.Debug` by default. A `Try` that produces a `None` or an `Err` is an
ordinary outcome, and warning on it is noise in the applications this library was
built for. Semantic conventions suggest `WARN` for a handled exception; pass it if
you would rather follow them:

```csharp
MonadOptions.Configure(
    options => options.UseLoggerFactoryFrom(app.Services, LogLevel.Warning));
```

Both the logger and the level are held on a `MonadOptions` satellite, so a scope
overrides them for one asynchronous flow and leaves the rest of the process alone:

```csharp
using (MonadOptions.BeginScope(
    options => options.UseLogger(logger, LogLevel.Warning)))
{
    // Everything in here logs at Warning, to this logger.
}
```

A logger created by `UseLoggerFactory` or `UseLoggerFactoryFrom` is in the
`Waystone.Monads` category, so it can be filtered without touching the rest of
your application:

```json
{ "Logging": { "LogLevel": { "Waystone.Monads": "Warning" } } }
```

## Relationship to the rest of the library

**This package replaces `MonadOptions.UseExceptionLogger`,** which is obsolete and
removed in `7.0.0`. Configuring both logs every handled exception twice.

**Metrics need nothing from this package.** `Waystone.Monads` emits them itself,
under a meter named after itself. Add `Waystone.Monads` to the meters your
OpenTelemetry pipeline collects and you have counts of handled exceptions with no
Waystone package installed beyond the core one:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter("Waystone.Monads"));
```

Logging is the signal that cannot work that way. `Microsoft.Extensions.Logging`
publishes no ambient logger to discover, so a configuration call is permanently
required — which is what this package exists to be.

## How it works

Core writes a `Waystone.Monads.ExceptionHandled` event to a `DiagnosticListener`
named `Waystone.Monads`. This package subscribes to it once, on the first
configuration call, and writes what arrives to the configured logger. Until then
there is no subscriber, so core's `IsEnabled` check short-circuits and the event
payload is never built.
