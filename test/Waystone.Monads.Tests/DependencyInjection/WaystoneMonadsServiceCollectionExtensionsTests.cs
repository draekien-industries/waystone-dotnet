namespace Waystone.Monads.DependencyInjection;

using System;
using Configs;
using Diagnostics;
using Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

[TestSubject(typeof(WaystoneMonadsServiceCollectionExtensions))]
[Collection(GlobalMonadOptionsCollection.Name)]
public sealed class WaystoneMonadsServiceCollectionExtensionsTests : IDisposable
{
    public WaystoneMonadsServiceCollectionExtensionsTests()
    {
        MonadOptions.Reset();
    }

    public void Dispose()
    {
        MonadOptions.Reset();
    }

    [Fact]
    public void GivenANullCollection_WhenRegistering_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((IServiceCollection)null!).AddWaystoneMonads())
              .ParamName.ShouldBe("services");
    }

    [Fact]
    public void GivenACollection_WhenRegistering_ThenReturnItForChaining()
    {
        var services = new ServiceCollection();

        services.AddWaystoneMonads().ShouldBeSameAs(services);
    }

    [Fact]
    public void GivenNoFactoryRegistered_WhenRegistering_ThenSupplyTheDefaultOne()
    {
        using ServiceProvider provider = new ServiceCollection()
                                        .AddWaystoneMonads()
                                        .BuildServiceProvider();

        provider.GetRequiredService<ErrorCodeFactory>()
                .ShouldBeOfType<ErrorCodeFactory>();
    }

    [Fact]
    public void GivenAFactoryAlreadyRegistered_WhenRegistering_ThenLeaveItAlone()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ErrorCodeFactory, ProbeErrorCodeFactory>();

        using ServiceProvider provider = services.AddWaystoneMonads()
                                                 .BuildServiceProvider();

        provider.GetRequiredService<ErrorCodeFactory>()
                .ShouldBeOfType<ProbeErrorCodeFactory>();
    }

    [Fact]
    public void GivenRegistrationHasRun_WhenOptionsAreRead_ThenReportTheMissingInstall()
    {
        using var recorder =
            new EventRecorder<ConfigurationNotApplied>(
                MonadDiagnostics.ConfigurationNotAppliedEventName);

        new ServiceCollection().AddWaystoneMonads();
        _ = MonadOptions.Current;

        recorder.Recorded().ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenRegistrationHasNotRun_WhenOptionsAreRead_ThenReportNothing()
    {
        using var recorder =
            new EventRecorder<ConfigurationNotApplied>(
                MonadDiagnostics.ConfigurationNotAppliedEventName);

        _ = MonadOptions.Current;

        recorder.Recorded().ShouldBeEmpty();
    }

    private sealed class ProbeErrorCodeFactory : ErrorCodeFactory;
}
