namespace Waystone.Monads.Tests.Linq;

using Monads.Linq;

public sealed class OptionsLinqExtensionsTests
{
    private static readonly List<Option<int>> Values =
    [
        11,
        12,
        Option.None<int>(),
        2,
    ];

    [Fact]
    public void GivenCollectionOfOptions_WhenInvokingFilter_ThenApplyFilter()
    {
        List<Option<int>> result = Values.Filter(x => x > 10).ToList();

        result.Should().HaveCount(4);
        result.Count(x => x.IsSome).Should().Be(2);
        result.Count(x => x.IsNone).Should().Be(2);
    }

    [Fact]
    public void
        GivenCollectionOfOptions_AndMatchingPredicate_WhenInvokingFirstOrNone_ThenReturnMatch()
    {
        Option<int> result = Values.FirstOrNone(x => x > 10);

        result.IsSome.Should().BeTrue();
        result.Should().Be(Option.Some(11));
    }

    [Fact]
    public void
        GivenCollectionOfOptions_AndNoMatch_WhenInvokingFirstOrNone_ThenReturnMatch()
    {
        Option<int> result = Values.FirstOrNone(x => x > 20);

        result.IsNone.Should().BeTrue();
        result.Should().Be(Option.None<int>());
    }

    [Theory]
    [InlineData(10, 20, 11)]
    [InlineData(20, 20, 20)]
    public void
        GivenCollectionOfOptions_WhenInvokingFirstOr_ThenReturnExpected(
            int minValue,
            int @default,
            int expected)
    {
        int results = Values.FirstOr(x => x > minValue, @default);

        results.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 20, 11)]
    [InlineData(20, 20, 20)]
    public void
        GivenCollectionOfOptions_WhenInvokingFirstOrElse_ThenReturnExpected(
            int minValue,
            int @default,
            int expected)
    {
        int results = Values.FirstOrElse(x => x > minValue, () => @default);

        results.Should().Be(expected);
    }

    [Fact]
    public void
        GivenCollectionOfOptions_AndMatchingPredicate_WhenInvokingLastOrNone_ThenReturnMatch()
    {
        Option<int> result = Values.LastOrNone(x => x > 10);

        result.IsSome.Should().BeTrue();
        result.Should().Be(Option.Some(12));
    }

    [Fact]
    public void
        GivenCollectionOfOptions_AndNoMatch_WhenInvokingLastOrNone_ThenReturnMatch()
    {
        Option<int> result = Values.LastOrNone(x => x > 20);

        result.IsNone.Should().BeTrue();
        result.Should().Be(Option.None<int>());
    }

    [Theory]
    [InlineData(10, 20, 12)]
    [InlineData(20, 20, 20)]
    public void
        GivenCollectionOfOptions_WhenInvokingLastOr_ThenReturnExpected(
            int minValue,
            int @default,
            int expected)
    {
        int results = Values.LastOr(x => x > minValue, @default);

        results.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 20, 12)]
    [InlineData(20, 20, 20)]
    public void
        GivenCollectionOfOptions_WhenInvokingLastOrElse_ThenReturnExpected(
            int minValue,
            int @default,
            int expected)
    {
        int results = Values.LastOrElse(x => x > minValue, () => @default);

        results.Should().Be(expected);
    }

    [Fact]
    public void
        GivenCollectionOfOptions_WhenInvokingMap_ThenReturnMappedOptions()
    {
        List<Option<string>> results = Values.Map(x => x.ToString()).ToList();

        results.Where(x => x.IsSome).Should().HaveCount(3);
        results.First(x => x.IsSome).Unwrap().Should().Be("11");
    }
}
