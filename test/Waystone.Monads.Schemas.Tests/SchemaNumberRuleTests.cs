namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Xunit;

public sealed class SchemaNumberRuleTests
{
    private static readonly ParseContext At = ParseContext.Root.At("amount");

    [Fact]
    public void GivenAValueAboveZero_WhenRequiringPositive_ThenAcceptIt()
    {
        Count(Schema.Number.Int32.Positive(), 1).ShouldBe(0);
        Count(Schema.Number.Int64.Positive(), 1L).ShouldBe(0);
        Count(Schema.Number.Decimal.Positive(), 0.1m).ShouldBe(0);
        Count(Schema.Number.Double.Positive(), 0.1d).ShouldBe(0);
    }

    [Fact]
    public void GivenZero_WhenRequiringPositive_ThenRejectIt()
    {
        Count(Schema.Number.Int32.Positive(), 0).ShouldBe(1);
        Count(Schema.Number.Int64.Positive(), 0L).ShouldBe(1);
        Count(Schema.Number.Decimal.Positive(), 0m).ShouldBe(1);
        Count(Schema.Number.Double.Positive(), 0d).ShouldBe(1);
    }

    [Fact]
    public void GivenAValueBelowZero_WhenRequiringNegative_ThenAcceptIt()
    {
        Count(Schema.Number.Int32.Negative(), -1).ShouldBe(0);
        Count(Schema.Number.Int64.Negative(), -1L).ShouldBe(0);
        Count(Schema.Number.Decimal.Negative(), -0.1m).ShouldBe(0);
        Count(Schema.Number.Double.Negative(), -0.1d).ShouldBe(0);
    }

    [Fact]
    public void GivenZero_WhenRequiringNegative_ThenRejectIt()
    {
        Count(Schema.Number.Int32.Negative(), 0).ShouldBe(1);
        Count(Schema.Number.Int64.Negative(), 0L).ShouldBe(1);
        Count(Schema.Number.Decimal.Negative(), 0m).ShouldBe(1);
        Count(Schema.Number.Double.Negative(), 0d).ShouldBe(1);
    }

    [Fact]
    public void GivenNotANumber_WhenRequiringASign_ThenRejectItEitherWay()
    {
        Count(Schema.Number.Double.Positive(), double.NaN).ShouldBe(1);
        Count(Schema.Number.Double.Negative(), double.NaN).ShouldBe(1);
    }

    [Fact]
    public void GivenInfinity_WhenRequiringASign_ThenAcceptTheMatchingOne()
    {
        Count(Schema.Number.Double.Positive(), double.PositiveInfinity)
           .ShouldBe(0);

        Count(Schema.Number.Double.Negative(), double.NegativeInfinity)
           .ShouldBe(0);
    }

    [Fact]
    public void GivenAFailingSignRule_WhenReporting_ThenSayWhichSign()
    {
        Violation positive = Schema.Number.Decimal.Positive()
                                  .Evaluate(-1m, At)
                                  .Violations.ShouldHaveSingleItem();

        positive.Code.ShouldBe(ViolationCodeCatalog.Codes.OutOfRange);

        positive.Message.ShouldBe(
            "Expected amount to be positive, but got -1.");

        Schema.Number.Decimal.Negative()
              .Evaluate(1m, At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldBe("Expected amount to be negative, but got 1.");
    }

    [Fact]
    public void GivenNoSchema_WhenAddingASignRule_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<int, int>)null!).Positive())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<long, long>)null!).Positive())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<decimal, decimal>)null!).Positive())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<double, double>)null!).Positive())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<int, int>)null!).Negative())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<long, long>)null!).Negative())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<decimal, decimal>)null!).Negative())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<double, double>)null!).Negative())
              .ParamName.ShouldBe("schema");
    }

    private static int Count<T>(Schema<T, T> schema, T value)
        where T : notnull =>
        schema.Evaluate(value, At).Violations.Count;
}
