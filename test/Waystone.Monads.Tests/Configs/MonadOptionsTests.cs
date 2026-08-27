namespace Waystone.Monads.Configs;

using JetBrains.Annotations;
using Shouldly;
using Xunit;

[Collection(GlobalMonadOptionsCollection.Name)]
[TestSubject(typeof(MonadOptions))]
public sealed class MonadOptionsTests
{
    public class CustomErrorCodeFactory : ErrorCodeFactory;

    [Fact]
    public void GivenCustomErrorCodeFactory_ThenErrorCodeFactoryShouldBeSet()
    {
        ErrorCodeFactory original = MonadOptions.Global.ErrorCodeFactory;

        try
        {
            MonadOptions.Configure(option => option.UseErrorCodeFactory(new CustomErrorCodeFactory()));
            MonadOptions.Global.ErrorCodeFactory.ShouldNotBeNull();
            MonadOptions.Global.ErrorCodeFactory.ShouldBeOfType<CustomErrorCodeFactory>();
        }
        finally
        {
            MonadOptions.Configure(option => option.UseErrorCodeFactory(original));
        }
    }
}
