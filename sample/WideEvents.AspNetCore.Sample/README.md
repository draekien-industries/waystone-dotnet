# WideLogEvents ASP.NET Core Sample

This sample application demonstrates how to integrate and use the
`WideLogEvents` ecosystem in an ASP.NET Core application with Serilog.

## What are Wide Log Events?

A "wide" log event is a pattern where instead of logging many small,
disconnected events, you accumulate relevant information throughout a process (
like an HTTP request) and log it all at once at the end. This makes it much
easier to correlate data and understand the full context of an operation.

## Key Features Demonstrated

1. **Serilog Enrichment**: How to configure Serilog to automatically include
   properties from the `WideLogEventContext`.
2. **Middleware Integration**: How to use `UseWideLogEvents` to automatically
   capture request/response details and manage the event scope.
3. **Custom Configuration**: Using the `+=` operator to append custom logic to
   the default middleware behavior.
4. **Manual Property Pushing**: Using `WideLogEventContext.PushProperty` to add
   business-specific data to the wide log from anywhere in your code.
5. **Manual Scoping**: Using `WideLogEventContext.BeginScope()` for tracking
   operations outside of the standard HTTP request lifecycle.
6. **Custom Sampling**: Implementing `IWideLogEventsSampler` to control log
   levels and decide which events should be logged.

## Project Structure

- `Program.cs`: Contains the entire application setup, including Serilog
  configuration, middleware registration, and API endpoints.

## Getting Started

### 1. Configuration

In `Program.cs`, Serilog is configured to use the `FromWideLogEventsContext`
enricher:

```csharp
builder.Host.UseSerilog((context, config) => config.Enrich
   .FromWideLogEventsContext()
   .WriteTo.Console()
   .ReadFrom.Configuration(context.Configuration));
```

### 2. Middleware Registration

The middleware is registered with custom options:

```csharp
app.UseWideLogEvents(options =>
{
    // Append a CorrelationId to every log
    options.OnBeforeInvokeNext += (scope, context) =>
    {
        scope.PushProperty("CorrelationId", context.TraceIdentifier);
    };

    // Use a custom sampler for advanced log control
    options.Sampler = new MyCustomSampler();
});
```

### 3. Adding Properties in Endpoints

You can add properties to the current scope at any time:

```csharp
app.MapGet("/weatherforecast", () =>
{
    var forecast = // ...

    // This property will be included in the single "wide" log emitted at the end of the request
    WideLogEventContext.PushProperty("Forecasts", forecast);

    return forecast;
});
```

### 4. Running the Sample

1. Run the application: `dotnet run`
2. Open the Swagger/Scalar UI (usually at `https://localhost:7144/scalar/v1`)
3. Execute the `/weatherforecast` endpoint.
4. Check the console output. You will see a single log entry containing all the
   request details, response details, and the custom `Forecasts` property.

## Exploring the Logs

When you call `/weatherforecast`, look for a log message like:
`GET /weatherforecast completed in 15.2ms with status code 200`

If you inspect the structured data of that log (e.g., in a sink like Seq or by
using a JSON formatter for the console), you will find:

- `HttpRequest`: Method, Path, Query, etc.
- `HttpResponse`: StatusCode, ContentType.
- `Forecasts`: The full array of weather forecasts.
- `CorrelationId`: The request's trace identifier.
- `Outcome`: Success.
