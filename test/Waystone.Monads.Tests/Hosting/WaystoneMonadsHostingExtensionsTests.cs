namespace Waystone.Monads.Hosting;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Configs;
using Diagnostics;
using Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

[TestSubject(typeof(WaystoneMonadsHostingExtensions))]
[Collection(GlobalMonadOptionsCollection.Name)]
public sealed class WaystoneMonadsHostingExtensionsTests : IDisposable
{
    public WaystoneMonadsHostingExtensionsTests()
    {
        MonadOptions.Reset();
    }

    public void Dispose()
    {
        MonadOptions.Reset();
    }

    [Fact]
    public void GivenANullCollection_WhenRegisteringTheInstaller_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((IServiceCollection)null!)
                      .InstallWaystoneMonadsOnStart())
              .ParamName.ShouldBe("services");
    }

    [Fact]
    public void GivenACollection_WhenRegisteringTheInstaller_ThenReturnItForChaining()
    {
        var services = new ServiceCollection();

        services.InstallWaystoneMonadsOnStart().ShouldBeSameAs(services);
    }

    [Fact]
    public void GivenTheInstallerIsRegisteredTwice_WhenBuilding_ThenRegisterItOnce()
    {
        using ServiceProvider provider =
            new ServiceCollection().InstallWaystoneMonadsOnStart()
                                   .InstallWaystoneMonadsOnStart()
                                   .BuildServiceProvider();

        provider.GetServices<IHostedService>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task GivenAHost_WhenItStarts_ThenInstallTheRegisteredConfiguration()
    {
        using IHost host = NewHost(
            services => services
                       .AddWaystoneMonads(
                            options =>
                                options.UseFallbackErrorCode("FromHost"))
                       .InstallWaystoneMonadsOnStart());

        await host.StartAsync(TestContext.Current.CancellationToken);

        MonadOptions.Current.FallbackErrorCode.ShouldBe("FromHost");

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenAHost_WhenItStarts_ThenDisarmTheMissingInstallReport()
    {
        using IHost host = NewHost(
            services => services.AddWaystoneMonads()
                                .InstallWaystoneMonadsOnStart());

        await host.StartAsync(TestContext.Current.CancellationToken);

        using var recorder =
            new EventRecorder<ConfigurationNotApplied>(
                MonadDiagnostics.ConfigurationNotAppliedEventName);

        _ = MonadOptions.Current;

        recorder.Recorded().ShouldBeEmpty();

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenAServiceRegisteredBeforeTheInstaller_WhenItStarts_ThenItAlreadySeesTheConfiguration()
    {
        var probe = new OptionsReadingService();

        using IHost host = NewHost(
            services => services
                       .AddSingleton<IHostedService>(probe)
                       .AddWaystoneMonads(
                            options =>
                                options.UseFallbackErrorCode("FromHost"))
                       .InstallWaystoneMonadsOnStart());

        await host.StartAsync(TestContext.Current.CancellationToken);

        probe.SeenAtStart.ShouldBe("FromHost");

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenAHostThatOnlyRegistersTheInstaller_WhenItStarts_ThenInstallTheDefaults()
    {
        using IHost host =
            NewHost(services => services.InstallWaystoneMonadsOnStart());

        await host.StartAsync(TestContext.Current.CancellationToken);

        MonadOptions.Current.FallbackErrorCode.ShouldBe("Unspecified");

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenAHost_WhenItStops_ThenLeaveTheInstalledConfigurationAlone()
    {
        using IHost host = NewHost(
            services => services
                       .AddWaystoneMonads(
                            options =>
                                options.UseFallbackErrorCode("FromHost"))
                       .InstallWaystoneMonadsOnStart());

        await host.StartAsync(TestContext.Current.CancellationToken);
        await host.StopAsync(TestContext.Current.CancellationToken);

        MonadOptions.Current.FallbackErrorCode.ShouldBe("FromHost");
    }

    private static IHost NewHost(Action<IServiceCollection> configure) =>
        new HostBuilder().ConfigureServices(
                              (_, services) => configure(services))
                         .Build();

    private sealed class OptionsReadingService : IHostedService
    {
        public string? SeenAtStart { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SeenAtStart = MonadOptions.Current.FallbackErrorCode;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
