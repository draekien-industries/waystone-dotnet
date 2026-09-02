namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Xunit;

public sealed class SchemaTemporalRuleTests
{
    private static readonly ParseContext At = ParseContext.Root.At("expiresOn");

    private static readonly DateTimeOffset Bound =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GivenAnEarlierInstant_WhenRequiringItBefore_ThenAcceptIt()
    {
        Schema.Timestamp.Before(Bound)
              .Evaluate(Bound.AddDays(-1), At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenTheBoundItself_WhenRequiringItBefore_ThenRejectIt()
    {
        Schema.Timestamp.Before(Bound)
              .Evaluate(Bound, At)
              .Violations.ShouldHaveSingleItem()
              .Code.ShouldBe(ViolationCodeCatalog.Codes.OutOfRange);
    }

    [Fact]
    public void GivenALaterInstant_WhenRequiringItAfter_ThenAcceptIt()
    {
        Schema.Timestamp.After(Bound)
              .Evaluate(Bound.AddDays(1), At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenTheBoundItself_WhenRequiringItAfter_ThenRejectIt()
    {
        Schema.Timestamp.After(Bound)
              .Evaluate(Bound, At)
              .Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenTheSameInstantInAnotherZone_WhenBounding_ThenCompareTheMoment()
    {
        var elsewhere = new DateTimeOffset(
            2026,
            1,
            1,
            11,
            0,
            0,
            TimeSpan.FromHours(11));

        Schema.Timestamp.After(Bound)
              .Evaluate(elsewhere, At)
              .Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenAFailingTemporalBound_WhenReporting_ThenSayWhichDirection()
    {
        Schema.Timestamp.Before(Bound)
              .Evaluate(Bound, At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldStartWith("Expected expiresOn to be before ");

        Schema.Timestamp.After(Bound)
              .Evaluate(Bound, At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldStartWith("Expected expiresOn to be after ");
    }

    [Fact]
    public void GivenNoSchema_WhenAddingATemporalBound_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<DateTimeOffset, DateTimeOffset>)null!)
                      .Before(Bound))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<DateTimeOffset, DateTimeOffset>)null!)
                      .After(Bound))
              .ParamName.ShouldBe("schema");
    }

#if NET8_0_OR_GREATER
    private static readonly DateOnly Day = new(2026, 1, 1);

    [Fact]
    public void GivenAnEarlierDay_WhenRequiringItBefore_ThenAcceptIt()
    {
        Schema.Date.Before(Day)
              .Evaluate(Day.AddDays(-1), At)
              .Violations.ShouldBeEmpty();

        Schema.Date.Before(Day)
              .Evaluate(Day, At)
              .Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenALaterDay_WhenRequiringItAfter_ThenAcceptIt()
    {
        Schema.Date.After(Day)
              .Evaluate(Day.AddDays(1), At)
              .Violations.ShouldBeEmpty();

        Schema.Date.After(Day)
              .Evaluate(Day, At)
              .Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenNoSchema_WhenAddingADateBound_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<DateOnly, DateOnly>)null!).Before(Day))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<DateOnly, DateOnly>)null!).After(Day))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<DateOnly, DateOnly>)null!).OnOrBefore(Day))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<DateOnly, DateOnly>)null!).OnOrAfter(Day))
              .ParamName.ShouldBe("schema");
    }

    /// <summary>
    /// The rule a deadline wants. The closing date itself is still open, which is
    /// the whole difference from <c>Before</c>.
    /// </summary>
    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void GivenADay_WhenRequiringItNoLater_ThenIncludeTheBound(
        int offset,
        bool accepted)
    {
        Schema.Date.OnOrBefore(Day)
              .Evaluate(Day.AddDays(offset), At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, true)]
    [InlineData(-1, false)]
    public void GivenADay_WhenRequiringItNoEarlier_ThenIncludeTheBound(
        int offset,
        bool accepted)
    {
        Schema.Date.OnOrAfter(Day)
              .Evaluate(Day.AddDays(offset), At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);
    }
#endif

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void GivenAnInstant_WhenRequiringItNoLater_ThenIncludeTheBound(
        int offset,
        bool accepted)
    {
        Schema.Timestamp.OnOrBefore(Bound)
              .Evaluate(Bound.AddTicks(offset), At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, true)]
    [InlineData(-1, false)]
    public void GivenAnInstant_WhenRequiringItNoEarlier_ThenIncludeTheBound(
        int offset,
        bool accepted)
    {
        Schema.Timestamp.OnOrAfter(Bound)
              .Evaluate(Bound.AddTicks(offset), At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);
    }

    [Fact]
    public void GivenALateInstant_WhenRequiringItNoLater_ThenNameTheBound()
    {
        Schema.Timestamp.OnOrBefore(Bound)
              .Evaluate(Bound.AddTicks(1), At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldStartWith("Expected expiresOn to be no later than");
    }

    [Fact]
    public void GivenAnEarlyInstant_WhenRequiringItNoEarlier_ThenNameTheBound()
    {
        Schema.Timestamp.OnOrAfter(Bound)
              .Evaluate(Bound.AddTicks(-1), At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldStartWith("Expected expiresOn to be no earlier than");
    }

    [Fact]
    public void GivenNoSchema_WhenAddingAnInclusiveInstantBound_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<DateTimeOffset, DateTimeOffset>)null!)
                      .OnOrBefore(Bound))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<DateTimeOffset, DateTimeOffset>)null!)
                      .OnOrAfter(Bound))
              .ParamName.ShouldBe("schema");
    }
}
