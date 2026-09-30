namespace Waystone.Monads.Schemas;

using System;
using System.Linq;
using Shouldly;
using Xunit;

public sealed class SchemaTextConversionTests
{
    private static readonly ParseContext At = ParseContext.Root.At("orderId");

    private static readonly Guid OrderId =
        new("0f8fad5b-d9cb-469f-a165-70867728950e");

    [Theory]
    [InlineData("0f8fad5b-d9cb-469f-a165-70867728950e")]
    [InlineData("0f8fad5bd9cb469fa16570867728950e")]
    [InlineData("{0f8fad5b-d9cb-469f-a165-70867728950e}")]
    [InlineData("(0f8fad5b-d9cb-469f-a165-70867728950e)")]
    [InlineData("{0x0f8fad5b,0xd9cb,0x469f,{0xa1,0x65,0x70,0x86,0x77,0x28,0x95,0x0e}}")]
    [InlineData("0F8FAD5B-D9CB-469F-A165-70867728950E")]
    [InlineData("  0f8fad5b-d9cb-469f-a165-70867728950e  ")]
    public void GivenTextInAnyGuidLayout_WhenConvertingToUuid_ThenProduceTheGuid(
        string text)
    {
        Outcome<Guid> outcome = Schema.Text.ToUuid().Evaluate(text, At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(OrderId);
    }

    [Theory]
    [InlineData("blah")]
    [InlineData("")]
    [InlineData("0f8fad5b-d9cb-469f-a165-70867728950")]
    [InlineData("0f8fad5b-d9cb-469f-a165-70867728950g")]
    public void GivenTextThatIsNotAGuid_WhenConvertingToUuid_ThenReportItMalformed(
        string text)
    {
        Outcome<Guid> outcome = Schema.Text.ToUuid().Evaluate(text, At);

        outcome.HasValue.ShouldBeFalse();

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Malformed);
        violation.Message.ShouldBe(
            $"Expected orderId to be a UUID, but got {text}.");
    }

    [Fact]
    public void GivenAnEarlierRefinement_WhenConvertingToUuid_ThenKeepItsViolation()
    {
        Outcome<Guid> outcome = new RefinesAndKeeps<string>()
                               .ToUuid()
                               .Evaluate(OrderId.ToString(), At);

        outcome.Value.ShouldBe(OrderId);
        outcome.Violations.ShouldHaveSingleItem()
               .Code.ShouldBe(ViolationCodeCatalog.Codes.OutOfRange);
    }

    [Fact]
    public void GivenAnEarlierRefinementAndUnreadableText_WhenConvertingToUuid_ThenReportBoth()
    {
        Outcome<Guid> outcome = new RefinesAndKeeps<string>()
                               .ToUuid()
                               .Evaluate("blah", At);

        outcome.HasValue.ShouldBeFalse();

        outcome.Violations.Select(static violation => violation.Code)
               .ShouldBe(
                    new[]
                    {
                        ViolationCodeCatalog.Codes.OutOfRange,
                        ViolationCodeCatalog.Codes.Malformed,
                    });
    }

    [Fact]
    public void GivenNoValue_WhenConvertingToUuid_ThenReportOnlyTheEarlierFailure()
    {
        Outcome<Guid> outcome = new Rejects<string>()
                               .ToUuid()
                               .Evaluate("blah", At);

        outcome.HasValue.ShouldBeFalse();

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Rejected orderId: got blah.");
    }

    [Fact]
    public void GivenADeclaredUuidSchema_WhenPipingConvertedText_ThenApplyItsRules()
    {
        Outcome<Guid> outcome = Schema.Text
                                      .ToUuid()
                                      .Pipe(Schema.Uuid.NotEmpty())
                                      .Evaluate(Guid.Empty.ToString(), At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Expected orderId not to be an empty identifier.");
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("TRUE", true)]
    [InlineData("False", false)]
    [InlineData("  true  ", true)]
    public void GivenTrueOrFalseInAnyCase_WhenConvertingToBool_ThenProduceIt(
        string text,
        bool expected)
    {
        Outcome<bool> outcome = Schema.Text.ToBool().Evaluate(text, At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("yes")]
    [InlineData("on")]
    [InlineData("")]
    public void GivenAnythingElse_WhenConvertingToBool_ThenReportItMalformed(
        string text)
    {
        Outcome<bool> outcome = Schema.Text.ToBool().Evaluate(text, At);

        outcome.HasValue.ShouldBeFalse();

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Malformed);
        violation.Message.ShouldBe(
            $"Expected orderId to be true or false, but got {text}.");
    }

    [Fact]
    public void GivenANullSchema_WhenConverting_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).ToUuid())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).ToBool())
              .ParamName.ShouldBe("schema");
    }
}
