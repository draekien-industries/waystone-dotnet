namespace Waystone.Monads.DependencyInjection;

using System;
using Configs;
using Diagnostics;
using Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadServiceCollectionExtensions))]
[Collection(GlobalMonadOptionsCollection.Name)]
public sealed class MonadServiceCollectionExtensionsTests : IDisposable
{
    public MonadServiceCollectionExtensionsTests()
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
    public void GivenANullCollection_WhenRegisteringADelegate_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((IServiceCollection)null!).AddWaystoneMonads(
                       _ => { }))
              .ParamName.ShouldBe("services");
    }

    [Fact]
    public void GivenANullCollection_WhenRegisteringAProviderAwareDelegate_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((IServiceCollection)null!).AddWaystoneMonads(
                       (_, _) => { }))
              .ParamName.ShouldBe("services");
    }

    [Fact]
    public void GivenANullDelegate_WhenRegistering_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => new ServiceCollection().AddWaystoneMonads(
                       (Action<MonadOptionsBuilder>)null!))
              .ParamName.ShouldBe("configure");
    }

    [Fact]
    public void GivenANullProviderAwareDelegate_WhenRegistering_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => new ServiceCollection().AddWaystoneMonads(
                       (Action<IServiceProvider, MonadOptionsBuilder>)null!))
              .ParamName.ShouldBe("configure");
    }

    [Fact]
    public void GivenACollection_WhenRegistering_ThenReturnABuilderOverTheSameCollection()
    {
        var services = new ServiceCollection();

        services.AddWaystoneMonads().Services.ShouldBeSameAs(services);
    }

    [Fact]
    public void GivenACollection_WhenRegisteringAProviderAwareDelegate_ThenReturnABuilderOverTheSameCollection()
    {
        var services = new ServiceCollection();

        services.AddWaystoneMonads((_, _) => { })
                .Services.ShouldBeSameAs(services);
    }

    [Fact]
    public void GivenSeveralRegistrations_WhenBuilding_ThenRegisterOneFactory()
    {
        var services = new ServiceCollection();
        services.AddWaystoneMonads();
        services.AddWaystoneMonads();

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetServices<ErrorCodeFactory>().ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenNoFactoryRegistered_WhenRegistering_ThenSupplyTheDefaultOne()
    {
        using ServiceProvider provider = new ServiceCollection()
                                        .AddWaystoneMonads().Services
                                        .BuildServiceProvider();

        provider.GetRequiredService<ErrorCodeFactory>()
                .ShouldBeOfType<ErrorCodeFactory>();
    }

    [Fact]
    public void GivenAFactoryAlreadyRegistered_WhenRegistering_ThenLeaveItAlone()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ErrorCodeFactory, ProbeErrorCodeFactory>();

        using ServiceProvider provider = services.AddWaystoneMonads().Services
                                                 .BuildServiceProvider();

        provider.GetRequiredService<ErrorCodeFactory>()
                .ShouldBeOfType<ProbeErrorCodeFactory>();
    }

    /// <summary>
    /// Asserts that the signal was written, not that it was written once. The
    /// pending flag is process-wide, so two threads reading options at the same
    /// moment can each write before either disarms it, and a single-item
    /// assertion fails whenever the rest of the suite reads in that window.
    /// </summary>
    [Fact]
    public void GivenRegistrationHasRun_WhenOptionsAreRead_ThenReportTheMissingInstall()
    {
        using var recorder =
            new EventRecorder<ConfigurationNotApplied>(
                MonadDiagnostics.ConfigurationNotAppliedEventName);

        new ServiceCollection().AddWaystoneMonads();
        _ = MonadOptions.Current;

        recorder.Recorded().ShouldNotBeEmpty();
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
}
