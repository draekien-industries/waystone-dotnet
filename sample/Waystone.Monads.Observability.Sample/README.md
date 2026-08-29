# Waystone.Monads observability sample

A console application that swallows four exceptions and then shows you all of them.
Run it:

```
dotnet run --project sample/Waystone.Monads.Observability.Sample
```

`PriceFeed` is the whole domain. `Read` returns an `Option<decimal>` and `Fetch`
returns a `Result<decimal, Error>`; both call the same parser, and the parser throws.
Nothing in `PriceFeed` mentions logging or metrics — that is the point. All the
wiring lives in `Program.cs`, at start-up.

## What the three channels show you

**Metrics, with no Waystone package beyond the core one.** `Program` adds the
library's meter to an OpenTelemetry provider and prints it at exit:

```csharp
Sdk.CreateMeterProviderBuilder()
   .AddMeter(MonadDiagnostics.MeterName)
   .AddConsoleExporter()
   .Build();
```

The dump at the end of the run is the payoff. Look at the `waystone.monads.monad`
tag: the `option` rows count exceptions that reached nobody, and this counter is
the only record they happened. The `result` row went into an `Err` as well.

**Logs, through `Waystone.Monads.Extensions.Logging`.** One configuration call
points the library at a factory you already own:

```csharp
MonadOptions.Configure(options => options.UseLoggerFactory(factory));
```

Every entry carries the exception plus the call site the compiler captured —
`ArgumentExpression` prints as `() => Parse(symbol)`, which is the delegate source
text, not a guess.

`RaiseTheLevelForOneFlow` then wraps one block in `MonadOptions.BeginScope` and
hands it a different logger at `Warning`. The entry inside the block goes to
`Sample.Reconciliation` at `warn`; the identical call after the block goes back to
`Waystone.Monads` at `dbug`. That scoping is what makes the logging usable in
parallel tests.

**The event, for anything the package does not cover.**
`MonadDiagnostics.ExceptionHandledEvent.Subscribe` in `Main` prints one line per
handled exception. The payload arrives already the right type, so the subscriber is
the lambda and nothing else — no `IObserver` to implement and no event name to
spell.

`Main` drops the `IDisposable` it gets back. That is the one case where it is safe:
the subscriber runs for the life of the process, so there is nothing to detach from
and no leak to outlive. Hold it and dispose it anywhere shorter-lived than that.

## The thing to notice

The logger and the subscriber both run. Nothing had to choose between them.

That is the reason `MonadOptions.UseExceptionLogger` is obsolete. It held a single
delegate, so a second integration replaced the first silently — you could have
logging or your own handler, never both.

## Two rough edges in the output, and why

**Log lines and `event:` lines interleave.** The console logging provider
writes on a background thread; the diagnostic subscriber runs synchronously on the
thread that threw. `Program` disposes the factory before the metrics dump for the
same reason — otherwise the last buffered entry lands after it.

**Nothing else in the process is instrumented.** A real application feeds these
into the same OpenTelemetry pipeline as everything else, so the counter arrives
next to your request metrics rather than on stdout.
