namespace Waystone.Monads.Results;

using JetBrains.Annotations;
using Linq;
using Shouldly;
using Xunit;

[TestSubject(typeof(ResultQueryExtensions))]
public sealed class LinqTests
{
    [Fact]
    public void GivenAnOk_WhenSelect_ThenProjectTheOkValue()
    {
        Result.Ok<int, string>(21)
              .Select(value => value * 2)
              .ShouldBe(Result.Ok<int, string>(42));
    }

    [Fact]
    public void GivenAnErr_WhenSelect_ThenCarryTheErrorThrough()
    {
        Result.Err<int, string>("broken")
              .Select(value => value * 2)
              .ShouldBe(Result.Err<int, string>("broken"));
    }

    [Fact]
    public void GivenAnOk_WhenSelectAgreesWithMap_ThenBothProduceTheSame()
    {
        Result<int, string> result = Result.Ok<int, string>(21);

        result.Select(value => value * 2)
              .ShouldBe(result.Map(value => value * 2));
    }

    [Fact]
    public void GivenAnOk_WhenSelectMany_ThenChainTheNextResult()
    {
        Result.Ok<int, string>(21)
              .SelectMany(value => Result.Ok<int, string>(value * 2))
              .ShouldBe(Result.Ok<int, string>(42));
    }

    [Fact]
    public void GivenAnErr_WhenSelectMany_ThenDoNotCallTheSelector()
    {
        var called = false;

        Result<int, string> result =
            Result.Err<int, string>("broken")
                  .SelectMany(
                       value =>
                       {
                           called = true;

                           return Result.Ok<int, string>(value);
                       });

        result.ShouldBe(Result.Err<int, string>("broken"));
        called.ShouldBeFalse();
    }

    [Fact]
    public void GivenThreeOks_WhenQueried_ThenCombineEveryValue()
    {
        Combine(
                Result.Ok<int, string>(1),
                Result.Ok<int, string>(2),
                Result.Ok<int, string>(3))
           .ShouldBe(Result.Ok<string, string>("1-2-3"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GivenAnErrInAnyClause_WhenQueried_ThenShortCircuitWithThatError(
        int position)
    {
        Result<int, string> first = At(1, position);
        Result<int, string> second = At(2, position);
        Result<int, string> third = At(3, position);

        Combine(first, second, third)
           .ShouldBe(Result.Err<string, string>($"broken at {position}"));
    }

    [Fact]
    public void GivenTwoErrs_WhenQueried_ThenSurfaceOnlyTheFirst()
    {
        Combine(
                Result.Ok<int, string>(1),
                Result.Err<int, string>("second"),
                Result.Err<int, string>("third"))
           .ShouldBe(Result.Err<string, string>("second"));
    }

    [Fact]
    public void GivenAnErrFirstClause_WhenQueried_ThenCallNeitherSelector()
    {
        var collectionCalled = false;
        var resultCalled = false;

        Result<int, string> query =
            from outer in Result.Err<int, string>("broken")
            from inner in TrackResult(outer, () => collectionCalled = true)
            select TrackValue(outer + inner, () => resultCalled = true);

        query.ShouldBe(Result.Err<int, string>("broken"));
        collectionCalled.ShouldBeFalse();
        resultCalled.ShouldBeFalse();
    }

    [Fact]
    public void GivenAnErrSecondClause_WhenQueried_ThenDoNotProjectTheResult()
    {
        var resultCalled = false;

        Result<int, string> query =
            from outer in Result.Ok<int, string>(1)
            from inner in Result.Err<int, string>("broken")
            select TrackValue(outer + inner, () => resultCalled = true);

        query.ShouldBe(Result.Err<int, string>("broken"));
        resultCalled.ShouldBeFalse();
    }

    private static Result<int, string> At(int clause, int failing) =>
        clause == failing
            ? Result.Err<int, string>($"broken at {failing}")
            : Result.Ok<int, string>(clause);

    private static Result<int, string> TrackResult(
        int value,
        System.Func<bool> record)
    {
        record();

        return Result.Ok<int, string>(value);
    }

    private static int TrackValue(int value, System.Func<bool> record)
    {
        record();

        return value;
    }

    private static Result<string, string> Combine(
        Result<int, string> first,
        Result<int, string> second,
        Result<int, string> third) =>
        from x in first
        from y in second
        from z in third
        select $"{x}-{y}-{z}";
}
