namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Xunit;

public sealed class SchemaUuidRuleTests
{
    private static readonly ParseContext At = ParseContext.Root.At("orderId");

    [Fact]
    public void GivenASetIdentifier_WhenRequiringOne_ThenReportNothing()
    {
        Schema.Uuid.NotEmpty()
              .Evaluate(Guid.NewGuid(), At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAnEmptyIdentifier_WhenRequiringOne_ThenReportMismatched()
    {
        Violation violation = Schema.Uuid.NotEmpty()
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

    /// <summary>
    /// The version digits are the first of the third group in the canonical
    /// spelling, which is not the byte the little-endian layout puts them in. These
    /// literals pin that, since every rule below reads the same byte.
    /// </summary>
    [Theory]
    [InlineData("c232ab00-9414-11ec-b3c8-9e6bdeced846", false)]
    [InlineData("000003e8-83f5-21ef-8000-325096b39f47", false)]
    [InlineData("2c5ea4c0-4067-11e9-8bad-9b1deb4d3b7d", false)]
    [InlineData("109156be-c4fb-41ea-b1b4-efe1671c5836", true)]
    [InlineData("0189d6a0-1234-7abc-8def-0123456789ab", false)]
    public void GivenAUuid_WhenRequiringVersion4_ThenJudgeItByItsVersionDigits(
        string value,
        bool passes) =>
        Schema.Uuid.IsVersion4()
              .Evaluate(Guid.Parse(value), At)
              .Violations.Count.ShouldBe(passes ? 0 : 1);

    [Fact]
    public void GivenARandomUuid_WhenRequiringVersion4_ThenReportNothing() =>
        Schema.Uuid.IsVersion4()
              .Evaluate(Guid.NewGuid(), At)
              .Violations.ShouldBeEmpty();

    [Fact]
    public void GivenAnEmptyUuid_WhenRequiringVersion4_ThenReportMismatched()
    {
        Violation violation = Schema.Uuid.IsVersion4()
                                   .Evaluate(Guid.Empty, At)
                                   .Violations.ShouldHaveSingleItem();

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Mismatched);

        violation.Message.ShouldBe("Expected orderId to be a version 4 UUID.");
    }

    [Fact]
    public void GivenNoSchema_WhenRequiringVersion4_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<Guid, Guid>)null!).IsVersion4())
              .ParamName.ShouldBe("schema");

#if NET9_0_OR_GREATER
    [Fact]
    public void GivenATimeOrderedUuid_WhenRequiringVersion7_ThenReportNothing() =>
        Schema.Uuid.IsVersion7()
              .Evaluate(Guid.CreateVersion7(), At)
              .Violations.ShouldBeEmpty();

    [Fact]
    public void GivenARandomUuid_WhenRequiringVersion7_ThenReportMismatched()
    {
        Violation violation = Schema.Uuid.IsVersion7()
                                   .Evaluate(Guid.NewGuid(), At)
                                   .Violations.ShouldHaveSingleItem();

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Mismatched);

        violation.Message.ShouldBe("Expected orderId to be a version 7 UUID.");
    }

    /// <summary>
    /// The two rules read the same byte, so a version 7 value passing 4 — or the
    /// reverse — would mean the digits were being read from the wrong place.
    /// </summary>
    [Fact]
    public void GivenATimeOrderedUuid_WhenRequiringVersion4_ThenReportMismatched() =>
        Schema.Uuid.IsVersion4()
              .Evaluate(Guid.CreateVersion7(), At)
              .Violations.Count.ShouldBe(1);

    [Fact]
    public void GivenNoSchema_WhenRequiringVersion7_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<Guid, Guid>)null!).IsVersion7())
              .ParamName.ShouldBe("schema");
#endif
}
