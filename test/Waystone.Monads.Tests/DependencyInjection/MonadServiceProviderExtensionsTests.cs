namespace Waystone.Monads.DependencyInjection;

using System;
using Configs;
using Diagnostics;
using Extensions.Logging.Configs;
using Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadServiceProviderExtensions))]
[Collection(GlobalMonadOptionsCollection.Name)]
public sealed class MonadServiceProviderExtensionsTests : IDisposable
{
    public MonadServiceProviderExtensionsTests()
    {
        MonadOptions.Reset();
    }

    public void Dispose()
    {
        MonadOptions.Reset();
    }

    [Fact]
    public void GivenANullProvider_WhenInstalling_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((IServiceProvider)null!).UseWaystoneMonads())
              .ParamName.ShouldBe("provider");
    }

    [Fact]
    public void GivenAProvider_WhenInstalling_ThenReturnItForChaining()
    {
        using ServiceProvider provider = new ServiceCollection()
                                        .AddWaystoneMonads().Services
                                        .BuildServiceProvider();

        provider.UseWaystoneMonads().ShouldBeSameAs(provider);
    }

    [Fact]
    public void GivenRegisteredConfiguration_WhenInstalling_ThenApplyItToTheGlobalOptions()
    {
        using ServiceProvider provider =
            new ServiceCollection()
               .AddWaystoneMonads(
                    options => options.UseFallbackErrorCode("FromContainer")).Services
               .BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadOptions.Current.FallbackErrorCode.ShouldBe("FromContainer");
    }

    [Fact]
    public void GivenNoConfigurationDelegate_WhenInstalling_ThenKeepTheDefaults()
    {
        using ServiceProvider provider = new ServiceCollection()
                                        .AddWaystoneMonads().Services
                                        .BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadOptions.Current.FallbackErrorCode.ShouldBe("Unspecified");
    }

    [Fact]
    public void GivenSeveralRegistrations_WhenInstalling_ThenApplyThemInRegistrationOrder()
    {
        using ServiceProvider provider =
            new ServiceCollection()
               .AddWaystoneMonads(
                    options => options.UseFallbackErrorCode("First")).Services
               .AddWaystoneMonads(
                    options => options.UseFallbackErrorCode("Second")).Services
               .BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadOptions.Current.FallbackErrorCode.ShouldBe("Second");
    }

    [Fact]
    public void GivenEarlierStaticConfiguration_WhenInstalling_ThenCarryForwardWhatTheContainerDoesNotSet()
    {
        MonadOptions.Configure(
            options => options.UseFallbackErrorMessage("Set in code."));

        using ServiceProvider provider =
            new ServiceCollection()
               .AddWaystoneMonads(
                    options => options.UseFallbackErrorCode("FromContainer")).Services
               .BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadOptions.Current.FallbackErrorMessage.ShouldBe("Set in code.");
        MonadOptions.Current.FallbackErrorCode.ShouldBe("FromContainer");
    }

    [Fact]
    public void GivenAnEmptyContainer_WhenInstalling_ThenInstallTheOptionsAlreadyInEffect()
    {
        using ServiceProvider provider =
            new ServiceCollection().BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadOptions.Current.FallbackErrorCode.ShouldBe("Unspecified");
    }

    [Fact]
    public void GivenARegisteredFactory_WhenInstalling_ThenUseIt()
    {
        var factory = new ProbeErrorCodeFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ErrorCodeFactory>(factory);

        using ServiceProvider provider = services.AddWaystoneMonads().Services
                                                 .BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadOptions.Current.ErrorCodeFactory.ShouldBeSameAs(factory);
    }

    [Fact]
    public void GivenARegisteredLoggerFactoryNobodyAskedFor_WhenInstalling_ThenLeaveLoggingUnconfigured()
    {
        var logger = Substitute.For<ILogger>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(MonadLoggingOptions.LoggerCategory)
                     .Returns(logger);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);

        using ServiceProvider provider = services.AddWaystoneMonads().Services
                                                 .BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadLoggingOptions.Current.Logger.ShouldBeSameAs(NullLogger.Instance);
    }

    [Fact]
    public void GivenLoggingAskedForFromServices_WhenInstalling_ThenWireItToTheResolvedFactory()
    {
        var logger = Substitute.For<ILogger>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(MonadLoggingOptions.LoggerCategory)
                     .Returns(logger);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);

        using ServiceProvider provider =
            services.AddWaystoneMonads(
                         (resolver, options) =>
                             options.UseLoggerFactoryFrom(resolver))
                    .Services.BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadLoggingOptions.Current.Logger.ShouldBeSameAs(logger);
    }

    [Fact]
    public void GivenNoLoggerFactory_WhenInstalling_ThenLeaveLoggingUnconfigured()
    {
        using ServiceProvider provider = new ServiceCollection()
                                        .AddWaystoneMonads().Services
                                        .BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadLoggingOptions.Current.Logger.ShouldBeSameAs(NullLogger.Instance);
    }

    [Fact]
    public void GivenLoggingAskedForButNoLoggerFactory_WhenInstalling_ThenThrowFromTheDelegate()
    {
        using ServiceProvider provider =
            new ServiceCollection()
               .AddWaystoneMonads(
                    (resolver, options) => options.UseLoggerFactoryFrom(resolver))
               .Services.BuildServiceProvider();

        Should.Throw<InvalidOperationException>(
            () => provider.UseWaystoneMonads());
    }

    [Fact]
    public void GivenBothOverloads_WhenInstalling_ThenRunThemInRegistrationOrder()
    {
        var first = Substitute.For<ILogger>();
        var last = Substitute.For<ILogger>();

        var services = new ServiceCollection();
        services.AddWaystoneMonads(options => options.UseLogger(first));
        services.AddWaystoneMonads((_, options) => options.UseLogger(last));

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.UseWaystoneMonads();

        MonadLoggingOptions.Current.Logger.ShouldBeSameAs(last);
    }

    [Fact]
    public void GivenAProviderAwareDelegate_WhenInstalling_ThenHandItTheProviderItWasCalledOn()
    {
        IServiceProvider? seen = null;

        using ServiceProvider provider =
            new ServiceCollection()
               .AddWaystoneMonads((resolver, _) => seen = resolver)
               .Services.BuildServiceProvider();

        provider.UseWaystoneMonads();

        seen.ShouldBeSameAs(provider);
    }

    [Fact]
    public void GivenAnOpenScope_WhenInstalling_ThenPublishFromTheGlobalOptionsInstead()
    {
        using ServiceProvider provider =
            new ServiceCollection()
               .AddWaystoneMonads(
                    options => options.UseFallbackErrorCode("FromContainer")).Services
               .BuildServiceProvider();

        using (MonadOptions.BeginScope(
                   options => options.UseFallbackErrorMessage("Scoped.")))
        {
            provider.UseWaystoneMonads();
        }

        MonadOptions.Current.FallbackErrorCode.ShouldBe("FromContainer");
        MonadOptions.Current.FallbackErrorMessage
                    .ShouldBe("An unexpected error occurred.");
    }

    [Fact]
    public void GivenInstallationHasRun_WhenOptionsAreRead_ThenReportNothing()
    {
        using ServiceProvider provider = new ServiceCollection()
                                        .AddWaystoneMonads().Services
                                        .BuildServiceProvider();

        provider.UseWaystoneMonads();

        using var recorder =
            new EventRecorder<ConfigurationNotApplied>(
                MonadDiagnostics.ConfigurationNotAppliedEventName);

        _ = MonadOptions.Current;

        recorder.Recorded().ShouldBeEmpty();
    }
}
