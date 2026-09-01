namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Xunit;

public sealed class SchemaIdRuleTests
{
    private static readonly ParseContext At = ParseContext.Root.At("orderId");

    [Fact]
    public void GivenASetIdentifier_WhenRequiringOne_ThenReportNothing()
    {
        Schema.Id.NotEmpty()
              .Evaluate(Guid.NewGuid(), At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAnEmptyIdentifier_WhenRequiringOne_ThenReportMismatched()
    {
        Violation violation = Schema.Id.NotEmpty()
                                   .Evaluate(Guid.Empty, At)
                                   .Violations.ShouldHaveSingleItem();

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Mismatched);

        violation.Message.ShouldBe(
            "Expected orderId not to be an empty identifier.");
    }

    [Fact]
    public void GivenNoSchema_WhenRequiringAnIdentifier_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<Guid, Guid>)null!).NotEmpty())
              .ParamName.ShouldBe("schema");
    }
}
