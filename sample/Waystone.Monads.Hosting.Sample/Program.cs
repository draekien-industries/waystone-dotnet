namespace Waystone.Monads.Hosting.Sample;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal static class Program
{
    private static readonly Dictionary<string, Func<Task>> Scenarios = new()
    {
        ["host"] = OnAHostApplicationBuilder,
        ["legacy"] = OnTheOlderHostBuilder,
        ["config"] = FromAppSettings,
        ["section"] = FromANamedSection,
        ["invalid"] = FromUnusableConfiguration,
        ["order"] = RegardlessOfRegistrationOrder,
    };

    private static async Task<int> Main(string[] args)
    {
        if (args.Length != 1
         || !Scenarios.TryGetValue(args[0], out Func<Task>? run))
        {
            Console.WriteLine("Pass one scenario name:");

            foreach (string name in Scenarios.Keys)
            {
                Console.WriteLine($"  {name}");
            }

            return 1;
        }

        await run();
        return 0;
    }

    private static async Task OnAHostApplicationBuilder()
    {
        Report.Heading("a host application builder: one call");

        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(null);

        builder.AddWaystoneMonads(
            options => options.UseFallbackErrorCode("FromTheHost"));

        Report.OptionsInEffect("before the host starts");

        await RunBriefly(builder.Build());

        Report.OptionsInEffect("after the host started");
    }

    private static async Task OnTheOlderHostBuilder()
    {
        Report.Heading("the older IHostBuilder: ConfigureServices");

        IHost host = new HostBuilder()
                    .ConfigureServices(
                         (_, services) => services
                                         .AddWaystoneMonads(
                                              options => options
                                                 .UseFallbackErrorCode(
                                                      "FromConfigureServices"))
                                         .EnableInstallOnStart())
                    .Build();

        await RunBriefly(host);

        Report.OptionsInEffect("after the host started");
    }

    private static async Task FromAppSettings()
    {
        Report.Heading("reading appsettings.json, opted into");

        HostApplicationBuilder builder = NewBuilderRootedAtTheBinaries();

        builder.AddWaystoneMonads(
            options => options.ReadFromConfiguration(builder.Configuration));

        await RunBriefly(builder.Build());

        Report.OptionsInEffect("after the host started");
    }

    private static async Task FromANamedSection()
    {
        Report.Heading("reading a section other than WaystoneMonads");

        HostApplicationBuilder builder = NewBuilderRootedAtTheBinaries();

        builder.AddWaystoneMonads(
            options => options.ReadFromConfiguration(
                builder.Configuration,
                "Contoso"));

        await RunBriefly(builder.Build());

        Report.OptionsInEffect("after the host started");
    }

    private static async Task FromUnusableConfiguration()
    {
        Report.Heading("configuration that cannot be honoured fails fast");

        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(null);

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["WaystoneMonads:CatchesCancellation"] = "sometimes",
            });

        builder.AddWaystoneMonads(
            options => options.ReadFromConfiguration(builder.Configuration));

        try
        {
            await RunBriefly(builder.Build());
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine($"  start-up stopped: {exception.Message}");
        }
    }

    private static async Task RegardlessOfRegistrationOrder()
    {
        Report.Heading("a reader registered before the installer");

        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(null);

        builder.Services.AddSingleton<IHostedService, EarlyReader>();

        builder.AddWaystoneMonads(
            options => options.UseFallbackErrorCode("FromTheHost"));

        await RunBriefly(builder.Build());
    }

    private static HostApplicationBuilder NewBuilderRootedAtTheBinaries() =>
        Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings
            {
                ContentRootPath = AppContext.BaseDirectory,
            });

    private static async Task RunBriefly(IHost host)
    {
        using (host)
        {
            await host.StartAsync();
            await host.StopAsync();
        }
    }
}
