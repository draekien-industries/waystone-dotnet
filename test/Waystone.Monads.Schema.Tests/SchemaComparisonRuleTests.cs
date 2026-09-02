namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Xunit;

public sealed class SchemaComparisonRuleTests
{
    private static readonly ParseContext At = ParseContext.Root.At("total");

    [Theory]
    [InlineData(5, true)]
    [InlineData(4, true)]
    [InlineData(3, false)]
    public void GivenAnInclusiveLowerBound_WhenChecking_ThenAcceptTheBoundItself(
        int value,
        bool accepted)
    {
        Schema.Number.Int32.AtLeast(4)
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public void GivenAnInclusiveUpperBound_WhenChecking_ThenAcceptTheBoundItself(
        int value,
        bool accepted)
    {
        Schema.Number.Int32.AtMost(4)
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(4, false)]
    [InlineData(3, false)]
    public void GivenAnExclusiveLowerBound_WhenChecking_ThenRejectTheBoundItself(
        int value,
        bool accepted)
    {
        Schema.Number.Int32.GreaterThan(4)
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(5, false)]
    public void GivenAnExclusiveUpperBound_WhenChecking_ThenRejectTheBoundItself(
        int value,
        bool accepted)
    {
        Schema.Number.Int32.LessThan(4)
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);
    }

    [Fact]
    public void GivenABoundedValue_WhenItFails_ThenNameTheBoundAndTheValue()
    {
        Violation violation = Schema.Number.Int32.AtLeast(4)
                                   .Evaluate(3, At)
                                   .Violations.ShouldHaveSingleItem();

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.OutOfRange);

        violation.Message.ShouldBe(
            "Expected total to be at least 4, but got 3.");
    }

    [Fact]
    public void GivenEachBound_WhenItFails_ThenSayWhichOne()
    {
        Message(Schema.Number.Int32.AtMost(4), 5)
           .ShouldBe("Expected total to be at most 4, but got 5.");

        Message(Schema.Number.Int32.GreaterThan(4), 4)
           .ShouldBe("Expected total to be greater than 4, but got 4.");

        Message(Schema.Number.Int32.LessThan(4), 4)
           .ShouldBe("Expected total to be less than 4, but got 4.");
    }

    [Fact]
    public void GivenBothBounds_WhenAValueBreaksEach_ThenReportBoth()
    {
        Schema.Number.Int32.AtLeast(10)
              .AtMost(1)
              .Evaluate(5, At)
              .Violations.Count.ShouldBe(2);
    }

    [Fact]
    public void GivenANonNumericOrderedType_WhenBounding_ThenApplyItsOwnOrdering()
    {
        Schema.Timestamp
              .AtLeast(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero))
              .Evaluate(DateTimeOffset.MinValue, At)
              .Violations.ShouldHaveSingleItem();

        Schema.For<TimeSpan>()
              .AtMost(TimeSpan.FromMinutes(1))
              .Evaluate(TimeSpan.FromSeconds(30), At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAReplacementMessage_WhenABoundFails_ThenLeaveTheTokenInPlace()
    {
        Schema.Number.Int32.AtLeast(4)
              .WithMessage("Needs {Expected}.")
              .Evaluate(3, At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldBe("Needs {Expected}.");
    }

    [Fact]
    public void GivenNoSchema_WhenAddingABound_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<int, int>)null!).AtLeast(1))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<int, int>)null!).AtMost(1))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<int, int>)null!).GreaterThan(1))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<int, int>)null!).LessThan(1))
              .ParamName.ShouldBe("schema");
    }

    private static string Message(Schema<int, int> schema, int value) =>
        schema.Evaluate(value, At).Violations.ShouldHaveSingleItem().Message;
}
