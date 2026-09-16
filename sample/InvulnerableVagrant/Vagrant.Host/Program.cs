using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Enrichers.Waystone.WideLogEvents;
using Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;
using Vagrant.Catalog;
using Vagrant.Host.Endpoints;
using Vagrant.Host.Infrastructure;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

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

// One DbContext per bounded context over one SQLite file. The second registration is
// what ShopDatabase.OpenAsync enumerates; without it a context's tables never exist.
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString(ShopDatabase.ConnectionName)));
builder.Services.AddScoped<VagrantDbContext>(
    services => services.GetRequiredService<CatalogDbContext>());

builder.Services.AddScoped<IStockLedger, StockLedger>();

WebApplication app = builder.Build();

// The one place a Result reaches the edge of the process rather than the edge of a
// request. A shop whose seed prices do not make sense cannot open, and there is no
// patron to tell, so the reason becomes a log line and a non-zero exit code.
Result<int, Error> opened =
    await ShopDatabase.OpenAsync(app.Services).ConfigureAwait(false);

int exitCode = opened.Match(
    app.Logger,
    static (_, _) => 0,
    static (error, log) =>
    {
        log.LogCritical(
            "The Invulnerable Vagrant cannot open: {ErrorCode} {ErrorMessage}",
            error.Code.Value,
            error.Message);

        return 1;
    });

if (exitCode is not 0) return exitCode;

app.UseWideLogEventsContext();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapGet("/", () => Results.Ok(new { shop = "The Invulnerable Vagrant", city = "Zadash" }));
app.MapItems();

await app.RunAsync().ConfigureAwait(false);

return 0;
