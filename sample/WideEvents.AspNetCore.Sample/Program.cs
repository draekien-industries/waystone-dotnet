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

            if (!builder.Environment.IsDevelopment()) return;

            options.VerboseSampleRate = 1.0;
            options.DebugSampleRate = 1.0;
            options.InformationSampleRate = 1.0;
            options.WarningSampleRate = 1.0;
        }));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// place the middleware at the boundary where you want the log scope to be
// initialized
app.UseWideLogEventsContext();
app.UseSerilogRequestLogging();
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

        logger.LogInformation("Log with Manual Scope");

        return result;
    });

// Example of an endpoint that fails to demonstrate error capturing
app.MapGet(
    "/error",
    () => { throw new InvalidOperationException("Something went wrong!"); });

app.Run();

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
