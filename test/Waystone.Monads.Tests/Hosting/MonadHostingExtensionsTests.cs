namespace Waystone.Monads.Hosting;

using System;
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

[TestSubject(typeof(MonadServicesBuilderExtensions))]
[TestSubject(typeof(MonadHostApplicationBuilderExtensions))]
[Collection(GlobalMonadOptionsCollection.Name)]
public sealed class MonadHostingExtensionsTests : IDisposable
{
    public MonadHostingExtensionsTests()
    {
        MonadOptions.Reset();
    }

    public void Dispose()
    {
        MonadOptions.Reset();
    }

    [Fact]
    public void GivenANullBuilder_WhenEnablingTheInstall_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((MonadServicesBuilder)null!)
                      .EnableInstallOnStart())
              .ParamName.ShouldBe("builder");
    }

    [Fact]
    public void GivenABuilder_WhenEnablingTheInstall_ThenReturnItForChaining()
    {
        MonadServicesBuilder builder =
            new ServiceCollection().AddWaystoneMonads();

        builder.EnableInstallOnStart().ShouldBeSameAs(builder);
    }

    [Fact]
    public void GivenTheInstallIsEnabledTwice_WhenBuilding_ThenRegisterOneInstaller()
    {
        using ServiceProvider provider =
            new ServiceCollection().AddWaystoneMonads()
                                   .EnableInstallOnStart()
                                   .EnableInstallOnStart()
                                   .Services.BuildServiceProvider();

        provider.GetServices<IHostedService>().ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenANullHostApplicationBuilder_WhenRegistering_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((IHostApplicationBuilder)null!).AddWaystoneMonads())
              .ParamName.ShouldBe("builder");
    }

    [Fact]
    public async Task GivenAHostApplicationBuilder_WhenItStarts_ThenInstallWithoutASecondCall()
    {
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(null);
        builder.AddWaystoneMonads(
            options => options.UseFallbackErrorCode("FromHostBuilder"));

        using IHost host = builder.Build();
        await host.StartAsync(TestContext.Current.CancellationToken);

        MonadOptions.Current.FallbackErrorCode.ShouldBe("FromHostBuilder");

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenAHost_WhenItStarts_ThenInstallTheRegisteredConfiguration()
    {
        using IHost host = NewHost(
            services => services
                       .AddWaystoneMonads(
                            options =>
                                options.UseFallbackErrorCode("FromHost"))
                       .EnableInstallOnStart());

        await host.StartAsync(TestContext.Current.CancellationToken);

        MonadOptions.Current.FallbackErrorCode.ShouldBe("FromHost");

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenAHost_WhenItStarts_ThenDisarmTheMissingInstallReport()
    {
        using IHost host = NewHost(
            services => services.AddWaystoneMonads()
                                .EnableInstallOnStart());

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
                       .EnableInstallOnStart());

        await host.StartAsync(TestContext.Current.CancellationToken);

        probe.SeenAtStart.ShouldBe("FromHost");

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenNoConfigurationDelegate_WhenTheHostStarts_ThenInstallTheDefaults()
    {
        using IHost host = NewHost(
            services => services.AddWaystoneMonads()
                                .EnableInstallOnStart());

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
                       .EnableInstallOnStart());

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
