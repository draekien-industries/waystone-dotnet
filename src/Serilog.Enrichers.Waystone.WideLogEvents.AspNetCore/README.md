# Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore

Provides ASP.NET Core integration for `Waystone.WideLogEvents`, allowing you to
automatically capture request and response details and include them in your
Serilog logs.

## Installation

Install the NuGet package:

```bash
dotnet add package Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore
```

This package transitively includes `Waystone.WideLogEvents` and
`Serilog.Enrichers.Waystone.WideLogEvents`.

## Usage

### 1. Configure Serilog

In your `Program.cs`, configure Serilog to use the Wide Log Events enricher and
the sampling filter:

```csharp
using Serilog;
using Serilog.Enrichers.Waystone.WideLogEvents;

builder.Host.UseSerilog((context, config) => config
   .Enrich.FromWideLogEventsContext()
   .Filter.WithWideLogEventsSampling()
   .ReadFrom.Configuration(context.Configuration));
```

### 2. Register Middleware

Add the `UseWideLogEventsContext` middleware to your application pipeline. This
middleware should typically be placed early in the pipeline to capture as much
information as possible.

```csharp
using Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

var app = builder.Build();

app.UseWideLogEventsContext();

// ... other middleware
```

### 3. Use in Endpoints

You can now push properties to the `WideLogEventContext` anywhere in your
request lifecycle (e.g., in Controllers, Minimal API handlers, or Services):

```csharp
using Waystone.WideLogEvents;

app.MapGet("/weatherforecast", () =>
{
    var forecast = // ... get forecast

    // This property will be included in the final "wide" log for this request
    WideLogEventContext.PushProperty("ForecastCount", forecast.Length);

    return forecast;
});
```

## Configuration Options

You can customize the sampling behavior of the log events by configuring the
sampling filter in Serilog:

```csharp
builder.Host.UseSerilog((context, config) => config
   .Enrich.FromWideLogEventsContext()
   .Filter.WithWideLogEventsSampling(options =>
   {
       options.InformationSampleRate = 0.5; // Log 50% of information logs
       options.ErrorSampleRate = 1.0;       // Log 100% of error logs

       // Custom random provider for sampling decisions
       options.RandomDoubleProvider = new MyRandomProvider();
   })
   .ReadFrom.Configuration(context.Configuration));
```

### Custom Random Provider

By default, the sampling filter uses an internal implementation that creates a
new `Random` instance. If you want to use a specific random number generator
(like `Random.Shared` in .NET 6+), you can implement `IRandomDoubleProvider`:

```csharp
public class MyRandomProvider : IRandomDoubleProvider
{
    public double NextDouble() => Random.Shared.NextDouble();
}
```

## Features

- **Automatic Request Capture**: Captures Method, Path, Scheme, Host,
  ContentType, etc.
- **Automatic Response Capture**: Captures StatusCode, ContentType, etc.
- **Outcome Tracking**: Automatically sets the outcome to `Success` or
  `Failure` (including the Exception).
- **Duration Tracking**: Logs the total time taken for the request to be
  processed.
- **Sampling**: Built-in support for sampling logs based on log level.
