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
    [Fact]
    public void TheConstantCarriesTheDefaultScheme() =>
        ShipmentErrorCatalog.Names.NotFound.ShouldBe("ShipmentError.NotFound");

    [Fact]
    public void TheCodeCarriesTheDefaultScheme() =>
        ShipmentErrorCatalog.Codes.AlreadyShipped.ShouldBe(
            new ErrorCode("ShipmentError.AlreadyShipped"));

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
        ShipmentError.AlreadyShipped.ToErrorCodeName()
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
    /// A value with no declared member still gets the scheme applied rather than
    /// falling back to the enum's own <c>ToString</c>, so a code read off the wire
    /// keeps the same shape whether or not it maps to a member.
    /// </summary>
    [Fact]
    public void AnUndeclaredValueGetsTheSameSchemeApplied()
    {
        var undeclared = (ShipmentError)99;

        undeclared.ToErrorCodeName().ShouldBe("ShipmentError.99");
        undeclared.ToErrorCode().ShouldBe(new ErrorCode("ShipmentError.99"));
    }

    /// <summary>
    /// Installing a custom factory changes nothing a generated member returns, on
    /// either path. This is the test that would have caught the fallback arm
    /// consulting the factory while the declared members did not.
    /// </summary>
    /// <remarks>
    /// The <c>FromException</c> assertion is the control, not an afterthought: it is
    /// the only remaining way to observe that the factory was installed at all.
    /// Without it a scope that silently failed to apply would pass this test while
    /// proving nothing.
    /// </remarks>
    [Fact]
    public void ACustomFactoryChangesNothingTheGeneratedMembersReturn()
    {
        using (MonadOptions.BeginScope(
                   options => options.UseErrorCodeFactory(new PrefixingFactory())))
        {
            ShipmentError.NotFound.ToErrorCodeName()
                         .ShouldBe("ShipmentError.NotFound");

            ((ShipmentError)99).ToErrorCodeName()
                               .ShouldBe("ShipmentError.99");

            ErrorCode.FromException(new System.InvalidOperationException())
                     .Value.ShouldBe("custom.InvalidOperation");
        }
    }

    private sealed class PrefixingFactory : ErrorCodeFactory
    {
        public override ErrorCode FromException(System.Exception exception) =>
            new ErrorCode($"custom.{base.FromException(exception).Value}");
    }
}

[ErrorCodeCatalog]
public enum ShipmentError
{
    NotFound,
    AlreadyShipped,
}
