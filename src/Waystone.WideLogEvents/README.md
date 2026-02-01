# Waystone.WideLogEvents

A core library for managing "wide" log events in .NET applications. This package
provides the foundational infrastructure for capturing and managing a set of
properties throughout a logical scope, which can then be enriched into your log
events.

## Key Concepts

### Wide Log Events

A "wide" log event is a pattern where instead of logging many small,
disconnected events, you accumulate relevant information throughout a process (
like an HTTP request) and log it all at once at the end. This makes it much
easier to correlate data and understand the full context of an operation.

### WideLogEventContext

The central point for managing properties within a scope. It uses `AsyncLocal`
to ensure properties are correctly tracked across asynchronous calls.

### WideLogEventScope

A disposable scope that manages the lifecycle of properties. When a scope is
created, it starts a new set of properties. When disposed, it restores the
previous scope's properties.

## Usage

### 1. Start a Scope

Wrap your operation in a `WideLogEventScope`:

```csharp
using Waystone.WideLogEvents;

using (var scope = WideLogEventContext.BeginScope())
{
    // Your code here
}
```

### 2. Push Properties

You can push properties to the current scope at any time:

```csharp
WideLogEventContext.PushProperty("CustomerId", 12345);
WideLogEventContext.PushProperty("OrderDetails", new { Total = 100.00, ItemCount = 3 });
```

### 3. Set Outcome

You can track the outcome of the operation:

```csharp
scope.SetOutcome(WideLogEventOutcome.Success);
// Or on failure
scope.SetOutcome(WideLogEventOutcome.Failure(exception));
```

## Integration

This package is intended to be used with an enrichment library like
`Serilog.Enrichers.Waystone.WideLogEvents` to actually include these properties
in your logs.
