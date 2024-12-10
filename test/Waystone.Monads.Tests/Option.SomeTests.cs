namespace Waystone.Monads.Tests;

[TestSubject(typeof(Some<>))]
public class SomeTests
{
    [Fact]
    public void GivenSome_WhenAccessingValue_ThenReturnValue()
    {
        IOption<int> some = Option.Some(1);

        some.IsSome.Should().BeTrue();
        some.IsNone.Should().BeFalse();

        some.Unwrap().Should().Be(1);
        some.UnwrapOr(10).Should().Be(1);
        some.UnwrapOrDefault().Should().Be(1);
        some.UnwrapOrElse(() => 10).Should().Be(1);

        some.Expect("value is 1").Should().Be(1);
    }

    [Fact]
    public void WhenComputingSomeOrOption_ThenReturnSome()
    {
        IOption<int> some = Option.Some(1);

        some.Or(Option.Some(2)).Should().Be(some);
        some.OrElse(() => Option.Some(2)).Should().Be(some);
    }

    [Fact]
    public void WhenComputingSomeXorSome_ThenReturnNone()
    {
        IOption<int> some = Option.Some(1);

        some.Xor(Option.Some(2)).Should().Be(Option.None<int>());
    }

    [Fact]
    public void WhenComputingSomeXorNone_ThenReturnSome()
    {
        IOption<int> some = Option.Some(1);

        some.Xor(Option.None<int>()).Should().Be(some);
    }

    [Fact]
    public void
        GivenTwoOptionsWithTheSameValue_WhenComparingThem_ThenReturnsTrue()
    {
        IOption<int> some = Option.Some(1);
        IOption<int> other = Option.Some(1);

        some.Should().Be(other);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void GivenPredicate_WhenInvokingIsSomeAnd_ThenReturnExpected(
        int value,
        bool expected)
    {
        IOption<int> some = Option.Some(value);

        bool result = some.IsSomeAnd(x => x == 1);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void GivenPredicate_WhenInvokingIsNoneOr_ThenReturnExpected(
        int value,
        bool expected)
    {
        IOption<int> some = Option.Some(value);

        bool result = some.IsNoneOr(x => x == 1);

        result.Should().Be(expected);
    }

    [Fact]
    public void GivenFunc_WhenMatchingOption_ThenInvokeOnSome()
    {
        IOption<int> some = Option.Some(1);

        var onSome = Substitute.For<Func<int, bool>>();
        onSome.Invoke(Arg.Any<int>()).Returns(true);

        var onNone = Substitute.For<Func<bool>>();
        onNone.Invoke().Returns(false);

        bool result = some.Match(onSome, onNone);

        result.Should().BeTrue();
    }

    [Fact]
    public void GivenAction_WhenMatchingOption_ThenInvokeOnSome()
    {
        IOption<int> some = Option.Some(1);

        var onSome = Substitute.For<Action<int>>();

        var onNone = Substitute.For<Action>();

        some.Match(onSome, onNone);

        onSome.Received(1).Invoke(1);
    }

    [Fact]
    public void WhenMap_ThenReturnMappedOption()
    {
        IOption<int> some = Option.Some(1);

        IOption<int> result = some.Map(x => x + 1);

        result.Unwrap().Should().Be(2);
    }

    [Fact]
    public void WhenMapOr_ThenReturnMappedValue()
    {
        IOption<int> some = Option.Some(1);

        int result = some.MapOr(10, x => x + 1);

        result.Should().Be(2);
    }

    [Fact]
    public void WhenMapOrElse_ThenReturnMappedValue()
    {
        IOption<int> some = Option.Some(1);

        int result = some.MapOrElse(() => 10, x => x + 1);

        result.Should().Be(2);
    }

    [Fact]
    public void WhenInspect_ThenInvokeAction()
    {
        IOption<int> some = Option.Some(1);
        var action = Substitute.For<Action<int>>();
        some.Inspect(action);

        action.Received().Invoke(1);
    }

    [Fact]
    public void GivenPredicateEvaluatesToTrue_WhenFilter_ThenReturnSome()
    {
        IOption<int> some = Option.Some(1);
        IOption<int> result = some.Filter(x => x == 1);
        result.Should().Be(some);
    }

    [Fact]
    public void GivenPredicateEvaluatesToFalse_WhenFilter_ThenReturnNone()
    {
        IOption<int> some = Option.Some(1);
        IOption<int> result = some.Filter(x => x == 2);
        result.Should().Be(Option.None<int>());
    }

    [Fact]
    public void GivenSome_AndSome_WhenZip_ThenReturnSome()
    {
        IOption<int> some1 = Option.Some(1);
        IOption<int> some2 = Option.Some(2);
        IOption<(int, int)> result = some1.Zip(some2);
        result.Should().Be(Option.Some((1, 2)));
    }

    [Fact]
    public void GivenSome_AndNone_WhenZip_ThenReturnNone()
    {
        IOption<int> some = Option.Some(1);
        IOption<(int, int)> result = some.Zip(Option.None<int>());
        result.Should().Be(Option.None<(int, int)>());
    }
}
