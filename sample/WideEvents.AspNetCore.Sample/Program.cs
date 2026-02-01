using Scalar.AspNetCore;
using Serilog;
using Serilog.Enrichers.Waystone.WideLogEvents;
using Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.Enrich
   .FromWideLogEventsContext()
   .ReadFrom.Configuration(context.Configuration));

builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.UseWideLogEvents();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

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

app.MapGet(
        "/weatherforecast",
        () =>
        {
            WeatherForecast[] forecast = Enumerable.Range(1, 5)
               .Select(index =>
                    new WeatherForecast(
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]))
               .ToArray();

            return forecast;
        })
   .WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
