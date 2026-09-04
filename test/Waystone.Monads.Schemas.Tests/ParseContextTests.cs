namespace Waystone.Monads.Schemas;

using Shouldly;
using Xunit;

public sealed class ParseContextTests
{
    [Fact]
    public void GivenTheRoot_WhenReadingIt_ThenItIsAtTheRootAndNotSensitive()
    {
        ParseContext.Root.Path.IsRoot.ShouldBeTrue();
        ParseContext.Root.IsSensitive.ShouldBeFalse();
    }

    [Fact]
    public void GivenAContext_WhenDescendingIntoAProperty_ThenExtendThePath()
    {
        ParseContext.Root.At("items").At("sku").Path.ToString()
                    .ShouldBe("items.sku");
    }

    [Fact]
    public void GivenASensitiveContext_WhenDescending_ThenStaySensitive()
    {
        ParseContext.Root.AsSensitive().At("password").IsSensitive
                    .ShouldBeTrue();
    }

    [Fact]
    public void GivenASensitiveContext_WhenMarkedAgain_ThenReturnTheSameContext()
    {
        ParseContext sensitive = ParseContext.Root.AsSensitive();

        sensitive.AsSensitive().IsSensitive.ShouldBeTrue();
        sensitive.AsSensitive().Path.ShouldBe(sensitive.Path);
    }

    [Fact]
    public void GivenASensitiveContext_WhenDescending_ThenKeepThePathToo()
    {
        ParseContext.Root.At("user").AsSensitive().At("password").Path
                    .ToString()
                    .ShouldBe("user.password");
    }
}
