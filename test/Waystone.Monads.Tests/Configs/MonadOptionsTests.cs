namespace Waystone.Monads.Configs;

using System;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadOptions))]
public sealed class MonadOptionsTests
{
    [Fact]
    public void GivenDefaultOptions_ThenErrorCodeFactoryShouldBeSet()
    {
        MonadOptions.Global.ErrorCodeFactory.ShouldNotBeNull();
        MonadOptions.Global.ErrorCodeFactory.ShouldBeOfType<ErrorCodeFactory>();
    }
}
