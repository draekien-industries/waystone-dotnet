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
2. **Middleware Integration**: How to use `UseWideLogEventsContext` to
   automatically capture request/reponse details and manage the event scope.
3. **Log Sampling**: Using `.Filter.WithWideLogEventsSampling()` to control
   which log events are emitted based on their level.
4. **Custom Random Provider**: Implementing `IRandomDoubleProvider` to use
   `Random.Shared` for sampling decisions.
5. **Manual Property Pushing**: Using `WideLogEventContext.PushProperty` to add
   business-specific data to the wide log from anywhere in your code.
6. **Manual Scoping**: Using `WideLogEventContext.BeginScope()` for tracking
   operations outside of the standard HTTP request lifecycle.

## Project Structure

- `Program.cs`: Contains the entire application setup, including Serilog
  configuration, middleware registration, and API endpoints.

## Getting Started

### 1. Configuration

In `Program.cs`, Serilog is configured to use the `FromWideLogEventsContext`
enricher and the `WithWideLogEventsSampling` filter. Note that we provide a
custom `IRandomDoubleProvider` that uses `Random.Shared` for better performance
and thread safety:

```csharp
builder.Host.UseSerilog((context, config) =>
    config
       .ReadFrom.Configuration(context.Configuration)
       .Enrich.FromWideLogEventsContext()
       .Filter.WithWideLogEventsSampling(options =>
        {
            options.RandomDoubleProvider = new MyRandomProvider();

            // ... sample rate configuration
        }));
```

### 2. Middleware Registration

The middleware is registered:

```csharp
app.UseWideLogEventsContext();
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
- `Outcome`: Success.

## Implementation Details

### Use of `Random.Shared`

This sample uses `Random.Shared` (available in .NET 6+) in two places:

1. **In the Endpoint**: To generate mock weather data.
2. **In the Sampler**: Via `MyRandomProvider`, which implements
   `IRandomDoubleProvider`. This ensures that the sampling logic uses the
   thread-safe `Random.Shared` instance rather than creating new `Random`
   objects.
