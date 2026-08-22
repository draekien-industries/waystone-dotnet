namespace Waystone.Monads.SourceGenerators.Fixtures;

using Shouldly;
using Waystone.Monads.Results.Errors;
using Xunit;

/// <summary>
/// Exercises the generated members as the consumer sees them: this project loads
/// the generator as an analyzer, so <see cref="ShipmentErrorProvider" /> below is
/// emitted rather than written.
/// </summary>
public sealed class GeneratedErrorCodeTests
{
    [Fact]
    public void TheConstantMatchesTheRuntimeFactory() =>
        ShipmentErrorProvider.ErrorCodeStrings.NotFound.ShouldBe(
            ErrorCode.FromEnum(ShipmentError.NotFound).Value);

    [Fact]
    public void TheErrorCodeMatchesTheRuntimeFactory() =>
        ShipmentErrorProvider.ErrorCodes.AlreadyShipped.ShouldBe(
            ErrorCode.FromEnum(ShipmentError.AlreadyShipped));

    [Fact]
    public void TheErrorFactoryCarriesTheCodeAndMessage()
    {
        Error error = ShipmentErrorProvider.Errors.NotFound("no such shipment");

        error.Code.ShouldBe(ShipmentErrorProvider.ErrorCodes.NotFound);
        error.Message.ShouldBe("no such shipment");
    }

    [Fact]
    public void TheExtensionsAgreeWithTheNestedClasses()
    {
        ShipmentError.AlreadyShipped.ToErrorCodeString()
                     .ShouldBe(
                          ShipmentErrorProvider.ErrorCodeStrings.AlreadyShipped);

        ShipmentError.AlreadyShipped.ToErrorCode()
                     .ShouldBe(ShipmentErrorProvider.ErrorCodes.AlreadyShipped);

        ShipmentError.AlreadyShipped.ToError("already gone")
                     .ShouldBe(
                          ShipmentErrorProvider.Errors.AlreadyShipped(
                              "already gone"));
    }

    [Fact]
    public void AnUndeclaredValueFallsThroughToTheRuntimeFactory()
    {
        var undeclared = (ShipmentError)99;

        undeclared.ToErrorCode()
                  .ShouldBe(ErrorCode.FromEnum(undeclared));

        undeclared.ToErrorCodeString().ShouldBe("ShipmentError.99");
    }
}

[ErrorCodeProvider]
public enum ShipmentError
{
    NotFound,
    AlreadyShipped,
}
