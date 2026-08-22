namespace Waystone.Monads.SourceGenerators.Fixtures;

using Shouldly;
using Waystone.Monads.Configs;
using Waystone.Monads.Results.Errors;
using Xunit;

/// <summary>
/// Exercises the generated members as the consumer sees them: this project loads
/// the generator as an analyzer, so <see cref="ShipmentErrorCatalog" /> below is
/// emitted rather than written.
/// </summary>
public sealed class GeneratedErrorCodeTests
{
#pragma warning disable CS0618
    [Fact]
    public void TheConstantMatchesTheRuntimeFactory() =>
        ShipmentErrorCatalog.Names.NotFound.ShouldBe(
            ErrorCode.FromEnum(ShipmentError.NotFound).Value);

    [Fact]
    public void TheErrorCodeMatchesTheRuntimeFactory() =>
        ShipmentErrorCatalog.Codes.AlreadyShipped.ShouldBe(
            ErrorCode.FromEnum(ShipmentError.AlreadyShipped));
#pragma warning restore CS0618

    [Fact]
    public void TheErrorFactoryCarriesTheCodeAndMessage()
    {
        Error error = ShipmentErrorCatalog.Errors.NotFound("no such shipment");

        error.Code.ShouldBe(ShipmentErrorCatalog.Codes.NotFound);
        error.Message.ShouldBe("no such shipment");
    }

    [Fact]
    public void TheExtensionsAgreeWithTheNestedClasses()
    {
        ShipmentError.AlreadyShipped.ToErrorCodeString()
                     .ShouldBe(
                          ShipmentErrorCatalog.Names.AlreadyShipped);

        ShipmentError.AlreadyShipped.ToErrorCode()
                     .ShouldBe(ShipmentErrorCatalog.Codes.AlreadyShipped);

        ShipmentError.AlreadyShipped.ToError("already gone")
                     .ShouldBe(
                          ShipmentErrorCatalog.Errors.AlreadyShipped(
                              "already gone"));
    }

    /// <summary>
    /// The generated members never consult the configured
    /// <see cref="Waystone.Monads.Configs.ErrorCodeFactory" />, including on the
    /// fallback path. Under the default factory that is indistinguishable from
    /// calling it, which is why this asserts against the literal string rather than
    /// against <see cref="ErrorCode.FromEnum" />.
    /// </summary>
    [Fact]
    public void AnUndeclaredValueGetsTheSameSchemeApplied()
    {
        var undeclared = (ShipmentError)99;

        undeclared.ToErrorCodeString().ShouldBe("ShipmentError.99");
        undeclared.ToErrorCode().ShouldBe(new ErrorCode("ShipmentError.99"));
    }

    /// <summary>
    /// Installing a custom factory changes nothing a generated member returns, on
    /// either path. This is the test that would have caught the fallback arm
    /// consulting the factory while the declared members did not.
    /// </summary>
    [Fact]
    public void ACustomFactoryChangesNothingTheGeneratedMembersReturn()
    {
        using (MonadOptions.BeginScope(
                   options => options.UseErrorCodeFactory(new PrefixingFactory())))
        {
            ShipmentError.NotFound.ToErrorCodeString()
                         .ShouldBe("ShipmentError.NotFound");

            ((ShipmentError)99).ToErrorCodeString()
                               .ShouldBe("ShipmentError.99");

#pragma warning disable CS0618
            ErrorCode.FromEnum(ShipmentError.NotFound)
                     .Value.ShouldBe("custom.ShipmentError.NotFound");
#pragma warning restore CS0618
        }
    }

    private sealed class PrefixingFactory : ErrorCodeFactory
    {
#pragma warning disable CS0672
        public override ErrorCode FromEnum(System.Enum @enum) =>
#pragma warning restore CS0672
            new ErrorCode($"custom.{@enum.GetType().Name}.{@enum}");
    }
}

[ErrorCodeCatalog]
public enum ShipmentError
{
    NotFound,
    AlreadyShipped,
}
