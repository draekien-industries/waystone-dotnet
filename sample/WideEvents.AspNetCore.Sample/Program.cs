using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Enrichers.Waystone.WideLogEvents;
using Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;
using Waystone.WideLogEvents;
using ILogger = Microsoft.Extensions.Logging.ILogger;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config
       .ReadFrom.Configuration(context.Configuration)
       .Enrich.FromWideLogEventsContext()
       .Filter.WithWideLogEventsSampling(options =>
        {
            options.RandomDoubleProvider = new MyRandomProvider();
            options.VerboseSampleRate = 1.0;
        })
       .WriteTo.Console());

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

// Configure the Wide Log Events middleware
app.UseWideLogEvents(options =>
{
    // You can append custom properties to every request
    options.OnBeforeInvokeNext += (scope, context) =>
    {
        scope.PushProperty("CorrelationId", context.TraceIdentifier);
    };

    // You can also add custom logic to the end of every request
    options.OnFinally += (scope, context) =>
    {
        if (context.Response.StatusCode >= 400)
        {
            scope.PushProperty("IsError", true);
        }
    };

    // You can even customize the sampling logic and log levels
    options.Sampler = new MyCustomSampler();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseStatusCodePages();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

// Example of using the WideLogEventContext in a Minimal API
app.MapGet(
        "/weatherforecast",
        () =>
        {
            // Simulate some business logic
            WeatherForecast[] forecast = Enumerable.Range(1, 5)
               .Select(index =>
                    new WeatherForecast(
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]))
               .ToArray();

            // Push custom properties that will be included in the single "wide"
            // log at the end of the request
            WideLogEventContext.PushProperty("Forecasts", forecast);
            WideLogEventContext.PushProperty("SummaryCount", summaries.Length);

            return forecast;
        })
   .WithName("GetWeatherForecast");

// Example of manual scoping
app.MapGet(
    "/manual-scope",
    ([FromServices] ILoggerFactory loggerFactory) =>
    {
        // This resets the scope to an empty state
        using WideLogEventScope scope = WideLogEventContext.BeginScope();

        ILogger logger = loggerFactory.CreateLogger("ManualScope");

        scope.PushProperty("ManualProperty", "Value");

        // Logic inside manual scope
        var result = new { Message = "Inside manual scope" };

        scope.SetOutcome(WideLogEventOutcome.Success);

        logger.LogInformation("Log with Manual Scope");

        return result;
    });

// Example of an endpoint that fails to demonstrate error capturing
app.MapGet(
    "/error",
    () => { throw new InvalidOperationException("Something went wrong!"); });

app.Run();

/// <summary>
/// A custom sampler that demonstrates how to control when logs are emitted and at
/// what level.
/// </summary>
internal sealed class MyCustomSampler : IWideLogEventsSampler
{
    public LogLevel GetLogLevel(WideLogEventScope scope)
    {
        // Always log failures as Errors
        if (scope.Outcome == WideLogEventOutcome.Failure)
        {
            return LogLevel.Error;
        }

        // Log manual scopes as Debug
        if (WideLogEventContext.GetScopedProperties()
           .ContainsKey("ManualProperty"))
        {
            return LogLevel.Debug;
        }

        return LogLevel.Information;
    }

    public bool ShouldSample(WideLogEventScope scope) =>

        // In a real app, you might only log 10% of successful requests
        // For this sample, we log everything
        true;
}

internal record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal sealed class MyRandomProvider : IRandomDoubleProvider
{
    /// <inheritdoc />
    public double NextDouble() => Random.Shared.NextDouble();
}
