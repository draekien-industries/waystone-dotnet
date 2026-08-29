namespace Waystone.Monads.Observability.Sample;

using Configs;
using Extensions.Logging.Configs;
using Microsoft.Extensions.Logging;
using Monads.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Options;
using Results;
using Results.Errors;

internal static class Program
{
    private static void Main()
    {
        using MeterProvider metrics = Sdk.CreateMeterProviderBuilder()
                                         .AddMeter(MonadDiagnostics.MeterName)
                                         .AddConsoleExporter()
                                         .Build()!;

        ILoggerFactory factory = LoggerFactory.Create(
            builder => builder.SetMinimumLevel(LogLevel.Debug)
                              .AddSimpleConsole(
                                   console => console.SingleLine = true));

        MonadOptions.Configure(options => options.UseLoggerFactory(factory));

        MonadDiagnostics.ExceptionHandledEvent.Subscribe(
            static handled => Console.WriteLine(
                $"  event: {handled.Monad} at {handled.Caller.MemberName}:"
              + $"{handled.Caller.LineNumber} caught "
              + $"{handled.Exception.GetType().Name}"));

        WhatEachMonadKeeps();
        RaiseTheLevelForOneFlow(factory);

        factory.Dispose();

        Console.WriteLine();
        Console.WriteLine("-- metrics --");
        metrics.Shutdown();
    }

    private static void WhatEachMonadKeeps()
    {
        Console.WriteLine("-- both swallow, only one keeps the failure --");

        Option<decimal> lost = PriceFeed.Read("MON");
        Result<decimal, Error> kept = PriceFeed.Fetch("MON");

        Console.WriteLine($"  option: {lost}");
        Console.WriteLine($"  result: {kept}");
    }

    private static void RaiseTheLevelForOneFlow(ILoggerFactory factory)
    {
        Console.WriteLine();
        Console.WriteLine("-- one flow logs at Warning, the rest stays at Debug --");

        ILogger logger = factory.CreateLogger("Sample.Reconciliation");

        using (MonadOptions.BeginScope(
                   options => options.UseLogger(logger, LogLevel.Warning)))
        {
            PriceFeed.Read("MISSING");
        }

        PriceFeed.Read("MISSING");
    }
}
