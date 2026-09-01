namespace Waystone.Monads.Schemas;

using Shouldly;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class SchemaViolationTests
{
    private static SchemaViolation Sut(params string[] messages)
    {
        var violations = new Violation[messages.Length];

        for (var index = 0; index < messages.Length; index++)
        {
            violations[index] = new Violation(
                ViolationPath.Root.Append("field" + index),
                ViolationCodeCatalog.Codes.Malformed,
                messages[index]);
        }

        return new SchemaViolation(new ViolationCollection(violations));
    }

    [Fact]
    public void GivenViolations_WhenConstructing_ThenReportTheFixedCode()
    {
        Sut("Not an email.").Code.Value.ShouldBe("schema_violation");
    }

    [Fact]
    public void GivenSeveralViolations_WhenConstructing_ThenJoinTheirMessages()
    {
        Sut("Not an email.", "Not positive.")
           .Message.ShouldBe("Not an email.; Not positive.");
    }

    [Fact]
    public void GivenViolations_WhenReadingThem_ThenReturnTheSameCollectionEveryTime()
    {
        SchemaViolation sut = Sut("Not an email.");

        sut.Violations.Count.ShouldBe(1);
        sut.Violations.ShouldBeSameAs(sut.Violations);
    }

    [Fact]
    public void GivenViolations_WhenGrouping_ThenForwardToTheCollection()
    {
        SchemaViolation sut = Sut("Not an email.", "Not positive.");

        sut.ByPath().Count.ShouldBe(2);
        sut.ByCode()[ViolationCodeCatalog.Codes.Malformed].Count.ShouldBe(2);
        sut.ToDictionary()["field0"].ShouldBe(new[] { "Not an email." });
    }

    [Fact]
    public void GivenTwoFailuresReportingTheSameThing_WhenComparing_ThenTheyAreEqual()
    {
        SchemaViolation left = Sut("Not an email.");
        SchemaViolation right = Sut("Not an email.");

        left.Equals(right).ShouldBeTrue();
        left.Equals((object)right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void GivenDifferentMessages_WhenComparing_ThenTheyAreNotEqual()
    {
        Sut("Not an email.").Equals(Sut("Not positive.")).ShouldBeFalse();
    }

    [Fact]
    public void GivenNull_WhenComparing_ThenItIsNotEqual()
    {
        Sut("Not an email.").Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GivenAPlainError_WhenComparing_ThenItIsNotEqual()
    {
        Sut("Not an email.")
           .Equals(new Error(new ErrorCode("schema_violation"), "Not an email."))
           .ShouldBeFalse();
    }

    [Fact]
    public void GivenAFailure_WhenRendering_ThenKeepTheErrorRenderingRatherThanTheRecordOne()
    {
        Sut("Not an email.").ToString().ShouldBe("[schema_violation] Not an email.");
    }

    [Fact]
    public void GivenAFailure_WhenUsedAsAnError_ThenItIsOne()
    {
        Error error = Sut("Not an email.");

        (error is SchemaViolation).ShouldBeTrue();
    }
}
