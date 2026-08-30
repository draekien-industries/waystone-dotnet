namespace Waystone.Monads.Configs;

using System;
using FluentValidation.Configs;
using JetBrains.Annotations;
using Results.Errors;
using Shouldly;
using Xunit;

[Collection(GlobalMonadOptionsCollection.Name)]
[TestSubject(typeof(MonadOptions))]
public sealed class MonadOptionsResetTests : IDisposable
{
    public void Dispose()
    {
        MonadOptions.Reset();
    }

    [Fact]
    public void GivenEveryOptionConfigured_WhenReset_ThenTheDefaultsAreBack()
    {
        MonadOptions.Configure(
            o => o.UseFallbackErrorCode("configured")
               .UseFallbackErrorMessage("Configured.")
               .UseCancellationAsFailure()
               .UseErrorCodeFactory(
                    new MonadOptionsTests.CustomErrorCodeFactory()));

        MonadOptions.Reset();

        MonadOptions.Global.FallbackErrorCode.ShouldBe("Unspecified");

        MonadOptions.Global.FallbackErrorMessage.ShouldBe(
            "An unexpected error occurred.");

        MonadOptions.Global.Catches(new OperationCanceledException())
           .ShouldBeFalse();

        MonadOptions.Global.ErrorCodeFactory.ShouldBeOfType<ErrorCodeFactory>();
    }

    [Fact]
    public void GivenAConfiguredSatellite_WhenReset_ThenTheSatelliteGoesToo()
    {
        MonadOptions.Configure(o => o.UseValidationErrorCode("configured.code"));

        MonadValidationOptions.Global.ValidationErrorCode.ShouldBe(
            "configured.code");

        MonadOptions.Reset();

        MonadValidationOptions.Global.ValidationErrorCode.ShouldBe(
            "validation.failed");
    }

    [Fact]
    public void GivenAnOpenScope_WhenReset_ThenTheFlowStopsSeeingIt()
    {
        MonadOptions.Configure(o => o.UseFallbackErrorCode("configured"));

        using (MonadOptions.BeginScope(o => o.UseFallbackErrorCode("scoped")))
        {
            new ErrorCode(" ").Value.ShouldBe("scoped");

            MonadOptions.Reset();

            new ErrorCode(" ").Value.ShouldBe("Unspecified");
        }
    }

    [Fact]
    public void WhenResetTwice_ThenTheSecondCallIsHarmless()
    {
        MonadOptions.Configure(o => o.UseFallbackErrorCode("configured"));

        MonadOptions.Reset();
        MonadOptions.Reset();

        MonadOptions.Global.FallbackErrorCode.ShouldBe("Unspecified");
    }
}
