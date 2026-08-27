namespace Waystone.Monads.Configs;

using System;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[Collection(GlobalMonadOptionsCollection.Name)]
[TestSubject(typeof(MonadOptions))]
public sealed class MonadOptionsResetIsolationTests : IDisposable
{
    public MonadOptionsResetIsolationTests()
    {
        MonadOptions.Reset();
    }

    public void Dispose()
    {
        MonadOptions.Reset();
    }

    [Fact]
    public void WhenConfiguringTheGlobal_ThenStartFromTheDefaults()
    {
        MonadOptions.Global.FallbackErrorCode.ShouldBe("Unspecified");

        MonadOptions.Configure(o => o.UseFallbackErrorCode("first.class"));

        MonadOptions.Global.FallbackErrorCode.ShouldBe("first.class");
    }
}

[Collection(GlobalMonadOptionsCollection.Name)]
[TestSubject(typeof(MonadOptions))]
public sealed class MonadOptionsResetIsolationPairTests : IDisposable
{
    public MonadOptionsResetIsolationPairTests()
    {
        MonadOptions.Reset();
    }

    public void Dispose()
    {
        MonadOptions.Reset();
    }

    [Fact]
    public void WhenConfiguringTheGlobal_ThenStartFromTheDefaults()
    {
        MonadOptions.Global.FallbackErrorCode.ShouldBe("Unspecified");

        MonadOptions.Configure(o => o.UseFallbackErrorCode("second.class"));

        MonadOptions.Global.FallbackErrorCode.ShouldBe("second.class");
    }
}
