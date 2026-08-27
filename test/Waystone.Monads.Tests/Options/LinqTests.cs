namespace Waystone.Monads.Options;

using JetBrains.Annotations;
using Linq;
using Shouldly;
using Xunit;

[TestSubject(typeof(OptionQueryExtensions))]
public sealed class LinqTests
{
    [Fact]
    public void GivenASome_WhenSelect_ThenProjectTheValue()
    {
        Option.Some(21).Select(value => value * 2).ShouldBe(Option.Some(42));
    }

    [Fact]
    public void GivenANone_WhenSelect_ThenStayNone()
    {
        Option.None<int>()
              .Select(value => value * 2)
              .ShouldBe(Option.None<int>());
    }

    [Fact]
    public void GivenASome_WhenSelectAgreesWithMap_ThenBothProduceTheSame()
    {
        Option<int> option = Option.Some(21);

        option.Select(value => value * 2).ShouldBe(option.Map(value => value * 2));
    }

    [Fact]
    public void GivenASome_WhenSelectMany_ThenChainTheNextOption()
    {
        Option.Some(21)
              .SelectMany(value => Option.Some(value * 2))
              .ShouldBe(Option.Some(42));
    }

    [Fact]
    public void GivenANone_WhenSelectMany_ThenDoNotCallTheSelector()
    {
        var called = false;

        Option<int> result = Option.None<int>()
                                   .SelectMany(
                                        value =>
                                        {
                                            called = true;

                                            return Option.Some(value);
                                        });

        result.ShouldBe(Option.None<int>());
        called.ShouldBeFalse();
    }

    [Fact]
    public void GivenASomeSatisfyingThePredicate_WhenWhere_ThenKeepIt()
    {
        Option.Some(42).Where(value => value > 0).ShouldBe(Option.Some(42));
    }

    [Fact]
    public void GivenASomeFailingThePredicate_WhenWhere_ThenDiscardIt()
    {
        Option.Some(42)
              .Where(value => value < 0)
              .ShouldBe(Option.None<int>());
    }

    [Fact]
    public void GivenThreeSomes_WhenQueried_ThenCombineEveryValue()
    {
        Combine(Option.Some(1), Option.Some(2), Option.Some(3))
           .ShouldBe(Option.Some("1-2-3"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GivenANoneInAnyClause_WhenQueried_ThenShortCircuit(int position)
    {
        Option<int> first = position == 1 ? Option.None<int>() : Option.Some(1);
        Option<int> second = position == 2 ? Option.None<int>() : Option.Some(2);
        Option<int> third = position == 3 ? Option.None<int>() : Option.Some(3);

        Combine(first, second, third).ShouldBe(Option.None<string>());
    }

    [Fact]
    public void GivenAQueryWithAWhereClause_WhenTheValueFails_ThenShortCircuit()
    {
        Describe(Option.Some(4)).ShouldBe(Option.Some("even 4"));
        Describe(Option.Some(5)).ShouldBe(Option.None<string>());
        Describe(Option.None<int>()).ShouldBe(Option.None<string>());
    }

    [Fact]
    public void GivenANoneFirstClause_WhenQueried_ThenCallNeitherSelector()
    {
        var collectionCalled = false;
        var resultCalled = false;

        Option<int> query = from outer in Option.None<int>()
                           from inner in TrackOption(outer, () => collectionCalled = true)
                           select TrackValue(outer + inner, () => resultCalled = true);

        query.ShouldBe(Option.None<int>());
        collectionCalled.ShouldBeFalse();
        resultCalled.ShouldBeFalse();
    }

    [Fact]
    public void GivenANoneSecondClause_WhenQueried_ThenDoNotProjectTheResult()
    {
        var resultCalled = false;

        Option<int> query = from outer in Option.Some(1)
                           from inner in Option.None<int>()
                           select TrackValue(outer + inner, () => resultCalled = true);

        query.ShouldBe(Option.None<int>());
        resultCalled.ShouldBeFalse();
    }

    private static Option<int> TrackOption(int value, System.Func<bool> record)
    {
        record();

        return Option.Some(value);
    }

    private static int TrackValue(int value, System.Func<bool> record)
    {
        record();

        return value;
    }

    private static Option<string> Combine(
        Option<int> first,
        Option<int> second,
        Option<int> third) =>
        from x in first
        from y in second
        from z in third
        select $"{x}-{y}-{z}";

    private static Option<string> Describe(Option<int> option) =>
        from value in option
        where value % 2 == 0
        select $"even {value}";
}
