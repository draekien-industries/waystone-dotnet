namespace Waystone.Monads.Configs;

using System;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadOptions))]
public sealed class MonadOptionsTests
{
    public class CustomErrorCodeFactory : ErrorCodeFactory;

    [Fact]
    public void GivenCustomErrorCodeFactory_ThenErrorCodeFactoryShouldBeSet()
    {
        MonadOptions.Configure(option => option.UseErrorCodeFactory(new CustomErrorCodeFactory()));
        MonadOptions.Global.ErrorCodeFactory.ShouldNotBeNull();
        MonadOptions.Global.ErrorCodeFactory.ShouldBeOfType<CustomErrorCodeFactory>();
    }
}
