using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using Serilog.Enrichers.Waystone.WideLogEvents;
using Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;
using Vagrant.Host.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config
       .ReadFrom.Configuration(context.Configuration)
       .Enrich.FromWideLogEventsContext());

// One wide event per request carries the shop's own facts — which patron, which item,
// why a purchase was refused — so a failure is diagnosed from one line rather than by
// correlating several.
builder.Services.AddProblemDetails();

// Option<T> reaches a response body through these converters. Result<T, Error> never
// does: an endpoint unwraps it into a status code, so the wire stays ordinary HTTP.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.AddMonadConverters());

builder.AddWaystoneMonads();

WebApplication app = builder.Build();

await ShopDatabase.OpenAsync(app.Services).ConfigureAwait(false);

app.UseWideLogEventsContext();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapGet("/", () => Results.Ok(new { shop = "The Invulnerable Vagrant", city = "Zadash" }));

await app.RunAsync().ConfigureAwait(false);
