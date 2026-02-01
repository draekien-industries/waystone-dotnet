# Serilog.Enrichers.Waystone.WideLogEvents

A Serilog enricher that integrates with `Waystone.WideLogEvents` to include
scoped properties in your log events.

## Installation

Install the NuGet package:

```bash
dotnet add package Serilog.Enrichers.Waystone.WideLogEvents
```

## Usage

Configure Serilog to use the Wide Log Events enricher:

```csharp
using Serilog;
using Serilog.Enrichers.Waystone.WideLogEvents;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromWideLogEventsContext()
    // ... other configuration
    .CreateLogger();
```

Once configured, any properties pushed to `WideLogEventContext` within an active
scope will be added to all log events emitted within that scope.

### Example

```csharp
using Waystone.WideLogEvents;
using Serilog;

using (WideLogEventContext.BeginScope())
{
    WideLogEventContext.PushProperty("OperationId", Guid.NewGuid());

    // This log message will include the "OperationId" property
    Log.Information("Processing request");
}
```

## How it works

The enricher looks for an active `WideLogEventScope` and extracts all properties
managed by `WideLogEventContext`. These properties are then added to the Serilog
`LogEvent`.
