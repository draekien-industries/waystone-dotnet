using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Enrichers.Waystone.WideLogEvents;
using Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;
using Vagrant.Appraisal;
using Vagrant.Catalog;
using Vagrant.Host.Endpoints;
using Vagrant.Host.Infrastructure;
using Vagrant.Host.OpenApi;
using Vagrant.Ordering;
using Vagrant.Staffing;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config
       .ReadFrom.Configuration(context.Configuration)
       .Enrich.FromWideLogEventsContext());

// One wide event per request carries the shop's own facts — which patron, which item,
// why a purchase was refused — so a failure is diagnosed from one line rather than by
// correlating several.
builder.Services.AddProblemDetails();

// The document has to describe what the converters above put on the wire, and neither
// half of that is inferred. An Option<T> is a converted type, so the schema exporter
// finds no properties on it; every handler returns IResult, so nothing says what a 200
// carries — the endpoints declare it.
builder.Services.AddOpenApi(options =>
{
    // The one place the option becomes a nullable, because the delegate's null means
    // "inline this schema" and nothing else can say it. ReferenceId cannot just return
    // the string? itself: WM3001 rejects a member declared that way, and a lambda is
    // the only shape the rule does not reach.
    options.CreateSchemaReferenceId = static info =>
        OptionSchemaTransformer.ReferenceId(info).UnwrapOrDefault();
    options.AddSchemaTransformer<OptionSchemaTransformer>();
    options.AddSchemaTransformer<RefusalSchemaTransformer>();
    options.AddDocumentTransformer<ShopDocumentTransformer>();
});

// Option<T> reaches a response body through these converters. Result<T, Error> never
// does: an endpoint unwraps it into a status code, so the wire stays ordinary HTTP.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.AddMonadConverters());

builder.AddWaystoneMonads();

// One DbContext and one SQLite file per bounded context. The second registration in
// each pair is what ShopDatabase.OpenAsync enumerates; without it a context's tables
// never exist.
string shop = Option
   .FromNullable(
        builder.Configuration.GetConnectionString(ShopDatabase.ConnectionName))
   .UnwrapOr(ShopDatabase.Default);

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(ShopDatabase.For(shop, "catalog")));
builder.Services.AddScoped<VagrantDbContext>(
    services => services.GetRequiredService<CatalogDbContext>());

builder.Services.AddDbContext<AppraisalDbContext>(options =>
    options.UseSqlite(ShopDatabase.For(shop, "appraisal")));
builder.Services.AddScoped<VagrantDbContext>(
    services => services.GetRequiredService<AppraisalDbContext>());

builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseSqlite(ShopDatabase.For(shop, "ordering")));
builder.Services.AddScoped<VagrantDbContext>(
    services => services.GetRequiredService<OrderingDbContext>());

builder.Services.AddDbContext<StaffingDbContext>(options =>
    options.UseSqlite(ShopDatabase.For(shop, "staffing")));
builder.Services.AddScoped<VagrantDbContext>(
    services => services.GetRequiredService<StaffingDbContext>());

// The shop's clock. Purchase.Settle and Buyback.Settle take a TimeProvider rather than
// reading DateTimeOffset.UtcNow, so what goes on a receipt is testable; this is the one
// registration that decides it is the real time of day.
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IStockLedger, StockLedger>();
builder.Services.AddScoped<ISpecimenShelf, SpecimenShelf>();
builder.Services.AddScoped<IPurchaseBook, PurchaseBook>();
builder.Services.AddScoped<IBuybackBook, BuybackBook>();
builder.Services.AddScoped<IClerkRoster, ClerkRoster>();

WebApplication app = builder.Build();

// The one place a Result reaches the edge of the process rather than the edge of a
// request. A shop whose seed prices do not make sense cannot open, and there is no
// patron to tell, so the reason becomes a log line and a non-zero exit code.
Result<int, Error> opened =
    await ShopDatabase.OpenAsync(app.Services).ConfigureAwait(false);

int exitCode = opened.With(app.Logger).Match(
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

// Mapped in every environment, not behind an IsDevelopment check. The README tells a
// reader to run `dotnet run` and open the reference, and plain `dotnet run` is
// Production — so a check here would answer that reader a 404.
app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => Results.Ok(new { shop = "The Invulnerable Vagrant", city = "Zadash" }))
   .WithTags("Shop")
   .WithSummary("Says which shop this is.")
   .Produces(StatusCodes.Status200OK);
app.MapItems();
app.MapSpecimens();
app.MapPurchases();
app.MapBuybacks();
app.MapClerks();

await app.RunAsync().ConfigureAwait(false);

return 0;
