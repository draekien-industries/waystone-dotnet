namespace Waystone.Monads.Tests.Linq;

using Monads.Linq;

public sealed class OptionsLinqExtensionsTests
{
    [Fact]
    public void GivenCollectionOfOptions_WhenInvokingFilter_ThenApplyFilter()
    {
        List<Option<int>> values =
        [
            11,
            12,
            -1,
            2,
        ];

        List<Option<int>> result = values.Filter(x => x > 10).ToList();

        result.Should().HaveCount(4);
        result.Count(x => x.IsSome).Should().Be(2);
        result.Count(x => x.IsNone).Should().Be(2);
    }

    [Fact]
    public void
        GivenCollectionOfOptions_AndMatchingPredicate_WhenInvokingFirstOrNone_ThenReturnMatch()
    {
        List<Option<int>> values =
        [
            11,
            12,
            -1,
            2,
        ];

        Option<int> result = values.FirstOrNone(x => x > 10);

        result.IsSome.Should().BeTrue();
        result.Should().Be(Option.Some(11));
    }

    [Fact]
    public void
        GivenCollectionOfOptions_AndNoMatch_WhenInvokingFirstOrNone_ThenReturnMatch()
    {
        List<Option<int>> values =
        [
            11,
            12,
            -1,
            2,
        ];

        Option<int> result = values.FirstOrNone(x => x > 20);

        result.IsNone.Should().BeTrue();
        result.Should().Be(Option.None<int>());
    }
}
