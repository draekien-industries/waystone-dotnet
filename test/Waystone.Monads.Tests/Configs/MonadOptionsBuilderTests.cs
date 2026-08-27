namespace Waystone.Monads.Configs;

using System;
using FluentValidation.Configs;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadOptionsBuilder))]
public sealed class MonadOptionsBuilderTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenAnUnusableFallbackCode_WhenConfiguring_ThenRefuseIt(
        string? errorCode)
    {
        ArgumentException thrown = Should.Throw<ArgumentException>(
            () => MonadOptions.Create(o => o.UseFallbackErrorCode(errorCode!)));

        thrown.ParamName.ShouldBe("errorCode");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenAnUnusableFallbackMessage_WhenConfiguring_ThenRefuseIt(
        string? errorMessage)
    {
        ArgumentException thrown = Should.Throw<ArgumentException>(
            () => MonadOptions.Create(
                o => o.UseFallbackErrorMessage(errorMessage!)));

        thrown.ParamName.ShouldBe("errorMessage");
    }

    [Fact]
    public void GivenPaddedFallbacks_WhenConfiguring_ThenStoreThemTrimmed()
    {
        MonadOptions options = MonadOptions.Create(
            o => o.UseFallbackErrorCode("  padded.code  ")
               .UseFallbackErrorMessage("  padded message.  "));

        options.FallbackErrorCode.ShouldBe("padded.code");
        options.FallbackErrorMessage.ShouldBe("padded message.");
    }

    [Fact]
    public void GivenCancellationAsFailure_WhenConfiguring_ThenCatchIt()
    {
        MonadOptions options =
            MonadOptions.Create(o => o.UseCancellationAsFailure());

        options.Catches(new OperationCanceledException()).ShouldBeTrue();
    }

    [Fact]
    public void GivenTheDefaults_WhenBuilding_ThenLeaveCancellationAlone()
    {
        MonadOptions options = MonadOptions.Create(_ => { });

        options.Catches(new OperationCanceledException()).ShouldBeFalse();
        options.Catches(new InvalidOperationException()).ShouldBeTrue();
    }

    [Fact]
    public void
        GivenOneSatelliteConfiguredTwice_WhenBuilding_ThenBothCallsLandOnOneBuilder()
    {
        MonadOptions options = MonadOptions.Create(
            o =>
            {
                o.UseValidationErrorCode("first.code");
                o.UseFallbackValidationErrorMessage("Second call.");
            });

        MonadValidationOptions validation =
            MonadValidationOptions.For(options);

        validation.ValidationErrorCode.ShouldBe("first.code");
        validation.FallbackValidationErrorMessage.ShouldBe("Second call.");
    }

    [Fact]
    public void
        GivenASatelliteConfigured_WhenReconfiguringOnlyACoreOption_ThenTheSatelliteSurvives()
    {
        MonadOptions configured =
            MonadOptions.Create(o => o.UseValidationErrorCode("carried.code"));

        MonadOptions rebuilt = configured.ToBuilder()
           .UseFallbackErrorCode("unrelated")
           .Build();

        MonadValidationOptions.For(rebuilt)
           .ValidationErrorCode.ShouldBe("carried.code");
    }

    [Fact]
    public void
        GivenASlotAllocatedAfterASnapshotWasBuilt_WhenReadingIt_ThenItIsNotConfigured()
    {
        MonadOptions snapshot = MonadOptions.Create(_ => { });

        int lateSlot = MonadOptionsSlot.Allocate();

        snapshot.Satellite<object>(lateSlot).ShouldBeNull();
    }
}
