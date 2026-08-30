namespace Waystone.Monads.Configs;

using FluentValidation.Configs;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[Collection(GlobalMonadOptionsCollection.Name)]
[TestSubject(typeof(MonadOptions))]
public sealed class MonadOptionsSnapshotTests
{
    [Fact]
    public void GivenAHeldSnapshot_WhenReconfiguring_ThenTheHeldOneIsUnchanged()
    {
        MonadOptions original = MonadOptions.Global;

        try
        {
            MonadOptions.Configure(o => o.UseFallbackErrorCode("before"));

            MonadOptions held = MonadOptions.Global;

            MonadOptions.Configure(
                o => o.UseFallbackErrorCode("after")
                   .UseFallbackErrorMessage("After."));

            held.FallbackErrorCode.ShouldBe("before");
            MonadOptions.Global.ShouldNotBeSameAs(held);
            MonadOptions.Global.FallbackErrorCode.ShouldBe("after");
        }
        finally
        {
            MonadOptions.Install(original);
        }
    }

    [Fact]
    public void GivenTwoConfigureCalls_WhenEachSetsOneOption_ThenBothStick()
    {
        MonadOptions original = MonadOptions.Global;

        try
        {
            MonadOptions.Configure(o => o.UseFallbackErrorCode("kept.code"));
            MonadOptions.Configure(o => o.UseFallbackErrorMessage("Kept."));

            MonadOptions.Global.FallbackErrorCode.ShouldBe("kept.code");
            MonadOptions.Global.FallbackErrorMessage.ShouldBe("Kept.");
        }
        finally
        {
            MonadOptions.Install(original);
        }
    }

    [Fact]
    public void GivenAPrebuiltSnapshot_WhenInstalled_ThenItBecomesGlobal()
    {
        MonadOptions original = MonadOptions.Global;

        try
        {
            MonadOptions installed = MonadOptions.Create(
                o => o.UseFallbackErrorCode("installed")
                   .UseValidationErrorCode("installed.validation"));

            MonadOptions.Install(installed);

            MonadOptions.Global.ShouldBeSameAs(installed);
            MonadOptions.Global.FallbackErrorCode.ShouldBe("installed");

            MonadValidationOptions.Global.ValidationErrorCode.ShouldBe(
                "installed.validation");
        }
        finally
        {
            MonadOptions.Install(original);
        }
    }

    [Fact]
    public void GivenAnInstalledSnapshot_WhenInstalledAgain_ThenNothingChanges()
    {
        MonadOptions original = MonadOptions.Global;

        try
        {
            MonadOptions installed =
                MonadOptions.Create(o => o.UseFallbackErrorCode("twice"));

            MonadOptions.Install(installed);
            MonadOptions.Install(installed);

            MonadOptions.Global.ShouldBeSameAs(installed);
            MonadOptions.Global.FallbackErrorCode.ShouldBe("twice");
        }
        finally
        {
            MonadOptions.Install(original);
        }
    }

    [Fact]
    public void
        GivenAnInstalledSnapshot_WhenConfiguringOnTopOfIt_ThenBuildOnItRatherThanTheDefaults()
    {
        MonadOptions original = MonadOptions.Global;

        try
        {
            MonadOptions.Install(
                MonadOptions.Create(o => o.UseFallbackErrorCode("installed")));

            MonadOptions.Configure(o => o.UseFallbackErrorMessage("Added."));

            MonadOptions.Global.FallbackErrorCode.ShouldBe("installed");
            MonadOptions.Global.FallbackErrorMessage.ShouldBe("Added.");
        }
        finally
        {
            MonadOptions.Install(original);
        }
    }
}
