namespace Waystone.Monads.Configuration.Sample;

using Configs;
using Extensions.Logging.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Options;

internal static class Program
{
    private static readonly Dictionary<string, Action> Scenarios = new()
    {
        ["static"] = WithoutAContainer,
        ["container"] = ThroughAContainer,
        ["factory"] = WithACustomErrorCodeFactory,
        ["additive"] = AcrossSeveralRegistrations,
        ["logging"] = WithLoggingWiredFromTheContainer,
        ["scope"] = OverriddenForOneFlow,
        ["forgotten"] = WithTheInstallForgotten,
    };

    private static int Main(string[] args)
    {
        if (args.Length != 1 || !Scenarios.TryGetValue(args[0], out Action? run))
        {
            Console.WriteLine("Pass one scenario name:");

            foreach (string name in Scenarios.Keys)
            {
                Console.WriteLine($"  {name}");
            }

            return 1;
        }

        run();
        return 0;
    }

    private static void WithoutAContainer()
    {
        Scenario.Heading("no container: MonadOptions.Configure");

        Scenario.ReportOptionsInEffect("before");

        MonadOptions.Configure(
            options => options.UseFallbackErrorCode("Contoso")
                              .UseFallbackErrorMessage("Something went wrong."));

        Scenario.ReportOptionsInEffect("after");
    }

    private static void ThroughAContainer()
    {
        Scenario.Heading("a container, installed by hand");

        var services = new ServiceCollection();
        services.AddWaystoneMonads(
            options => options.UseFallbackErrorCode("Contoso"));

        using ServiceProvider provider = services.BuildServiceProvider();

        Scenario.ReportOptionsInEffect("registered, not yet installed");

        provider.UseWaystoneMonads();

        Scenario.ReportOptionsInEffect("installed");
    }

    private static void WithACustomErrorCodeFactory()
    {
        Scenario.Heading("a custom ErrorCodeFactory from the container");

        var services = new ServiceCollection();
        services.AddSingleton<ErrorCodeFactory, ShoutingErrorCodeFactory>();
        services.AddWaystoneMonads();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.UseWaystoneMonads();

        Scenario.ReportOptionsInEffect("installed");
    }

    private static void AcrossSeveralRegistrations()
    {
        Scenario.Heading("several registrations, applied in order");

        var services = new ServiceCollection();

        services.AddWaystoneMonads(
            options => options.UseFallbackErrorCode("FromTheLibrary")
                              .UseFallbackErrorMessage("Set by the library."));

        services.AddWaystoneMonads(
            options => options.UseFallbackErrorCode("FromTheApplication"));

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.UseWaystoneMonads();

        Scenario.ReportOptionsInEffect("installed");
    }

    private static void WithLoggingWiredFromTheContainer()
    {
        Scenario.Heading("logging wired from the container");

        var services = new ServiceCollection();
        services.AddLogging(
            logging => logging.SetMinimumLevel(LogLevel.Debug)
                              .AddSimpleConsole(
                                   console => console.SingleLine = true));
        services.AddWaystoneMonads(
            (provider, options) => options.UseLoggerFactoryFrom(provider));

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.UseWaystoneMonads();

        Console.WriteLine("  swallowing an exception, which should now be logged:");
        Option.Try<int>(() => throw new TimeoutException("the feed timed out"));
    }

    private static void OverriddenForOneFlow()
    {
        Scenario.Heading("one flow overridden by a scope");

        MonadOptions.Configure(options => options.UseFallbackErrorCode("Global"));

        Scenario.ReportOptionsInEffect("outside the scope");

        using (MonadOptions.BeginScope(
                   options => options.UseFallbackErrorCode("Scoped")))
        {
            Scenario.ReportOptionsInEffect("inside the scope");
        }

        Scenario.ReportOptionsInEffect("after the scope");
    }

    private static void WithTheInstallForgotten()
    {
        Scenario.Heading("the install forgotten, and reported");

        using var watcher = new ForgottenInstallWatcher();

        var services = new ServiceCollection();
        services.AddWaystoneMonads(
            options => options.UseFallbackErrorCode("NeverInstalled"));

        _ = services.BuildServiceProvider();

        Scenario.ReportOptionsInEffect("read without installing");

        Console.WriteLine($"  diagnostic events seen: {watcher.Seen}");
    }
}
