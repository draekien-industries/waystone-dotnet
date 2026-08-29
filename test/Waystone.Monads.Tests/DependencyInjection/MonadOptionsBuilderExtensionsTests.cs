namespace Waystone.Monads.DependencyInjection;

using System;
using System.Collections.Generic;
using Configs;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadOptionsBuilderExtensions))]
public sealed class MonadOptionsBuilderExtensionsTests
{
    [Fact]
    public void GivenANullBuilder_WhenReading_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((MonadOptionsBuilder)null!).ReadFromConfiguration(
                       Configuration()))
              .ParamName.ShouldBe("builder");
    }

    [Fact]
    public void GivenANullConfiguration_WhenReading_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => new MonadOptionsBuilder().ReadFromConfiguration(null!))
              .ParamName.ShouldBe("configuration");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenABlankSectionName_WhenReading_ThenThrow(string sectionName)
    {
        Should.Throw<ArgumentException>(
                   () => new MonadOptionsBuilder().ReadFromConfiguration(
                       Configuration(),
                       sectionName))
              .ParamName.ShouldBe("sectionName");
    }

    [Fact]
    public void GivenABuilder_WhenReading_ThenReturnItForChaining()
    {
        var builder = new MonadOptionsBuilder();

        builder.ReadFromConfiguration(Configuration()).ShouldBeSameAs(builder);
    }

    [Fact]
    public void GivenAnAbsentSection_WhenReading_ThenChangeNothing()
    {
        var builder = new MonadOptionsBuilder();

        builder.ReadFromConfiguration(Configuration());

        builder.FallbackErrorCode.ShouldBe("Unspecified");
        builder.FallbackErrorMessage.ShouldBe("An unexpected error occurred.");
        builder.CatchesCancellation.ShouldBeFalse();
    }

    [Fact]
    public void GivenEveryKey_WhenReading_ThenApplyThemAll()
    {
        var builder = new MonadOptionsBuilder();

        builder.ReadFromConfiguration(
            Configuration(
                ("WaystoneMonads:FallbackErrorCode", "Bound"),
                ("WaystoneMonads:FallbackErrorMessage", "Bound message."),
                ("WaystoneMonads:CatchesCancellation", "true")));

        builder.FallbackErrorCode.ShouldBe("Bound");
        builder.FallbackErrorMessage.ShouldBe("Bound message.");
        builder.CatchesCancellation.ShouldBeTrue();
    }

    [Fact]
    public void GivenOneKey_WhenReading_ThenLeaveTheOthersAsTheyWere()
    {
        var builder = new MonadOptionsBuilder();
        builder.UseFallbackErrorMessage("Set in code.");

        builder.ReadFromConfiguration(
            Configuration(("WaystoneMonads:FallbackErrorCode", "Bound")));

        builder.FallbackErrorCode.ShouldBe("Bound");
        builder.FallbackErrorMessage.ShouldBe("Set in code.");
    }

    [Fact]
    public void GivenANamedSection_WhenReading_ThenReadFromThatOneInstead()
    {
        var builder = new MonadOptionsBuilder();

        builder.ReadFromConfiguration(
            Configuration(("Elsewhere:FallbackErrorCode", "Bound")),
            "Elsewhere");

        builder.FallbackErrorCode.ShouldBe("Bound");
    }

    [Fact]
    public void GivenCancellationIsConfiguredOff_WhenReading_ThenLeaveItOff()
    {
        var builder = new MonadOptionsBuilder();

        builder.ReadFromConfiguration(
            Configuration(("WaystoneMonads:CatchesCancellation", "false")));

        builder.CatchesCancellation.ShouldBeFalse();
    }

    [Fact]
    public void GivenCancellationWasTurnedOnInCode_WhenConfiguredOff_ThenTurnItBackOff()
    {
        var builder = new MonadOptionsBuilder();
        builder.UseCancellationAsFailure();

        builder.ReadFromConfiguration(
            Configuration(("WaystoneMonads:CatchesCancellation", "false")));

        builder.CatchesCancellation.ShouldBeFalse();
    }

    [Fact]
    public void GivenCancellationIsNotABoolean_WhenReading_ThenThrow()
    {
        ArgumentException thrown = Should.Throw<ArgumentException>(
            () => new MonadOptionsBuilder().ReadFromConfiguration(
                Configuration(("WaystoneMonads:CatchesCancellation", "yes"))));

        thrown.ParamName.ShouldBe("configuration");
        thrown.Message.ShouldContain("WaystoneMonads:CatchesCancellation");
        thrown.Message.ShouldContain("yes");
    }

    [Fact]
    public void GivenABlankFallbackErrorCode_WhenReading_ThenLetTheBuilderThrow()
    {
        Should.Throw<ArgumentException>(
                   () => new MonadOptionsBuilder().ReadFromConfiguration(
                       Configuration(
                           ("WaystoneMonads:FallbackErrorCode", " "))))
              .ParamName.ShouldBe("errorCode");
    }

    [Fact]
    public void GivenABlankFallbackErrorMessage_WhenReading_ThenLetTheBuilderThrow()
    {
        Should.Throw<ArgumentException>(
                   () => new MonadOptionsBuilder().ReadFromConfiguration(
                       Configuration(
                           ("WaystoneMonads:FallbackErrorMessage", " "))))
              .ParamName.ShouldBe("errorMessage");
    }

    private static IConfiguration Configuration(
        params (string Key, string Value)[] entries)
    {
        var values = new Dictionary<string, string?>();

        foreach ((string key, string value) in entries)
        {
            values[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
