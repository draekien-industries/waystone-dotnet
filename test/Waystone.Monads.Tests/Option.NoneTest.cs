namespace Waystone.Monads.Tests;

using Exceptions;

[TestSubject(typeof(None<>))]
public class NoneTest
{
    [Fact]
    public void GivenNone_WhenAccessingValue_ThenThrow()
    {
        Option<int> none = Option.None<int>();

        none.IsSome.Should().BeFalse();
        none.IsNone.Should().BeTrue();

        none.IsSomeAnd(_ => true).Should().BeFalse();
        none.IsNoneOr(_ => false).Should().BeTrue();

        none.Invoking(x => x.Unwrap()).Should().Throw<UnwrapException>();
        none.Invoking(x => x.Expect("value is 1"))
            .Should()
            .Throw<UnmetExpectationException>()
            .WithMessage("Value is 1");
    }

    [Fact]
    public void GivenNone_WhenAccessingValueWithFallback_ThenReturnFallback()
    {
        Option<int> none = Option.None<int>();

        none.UnwrapOr(10).Should().Be(10);
        none.UnwrapOrDefault().Should().Be(default);
        none.UnwrapOrElse(() => 10).Should().Be(10);
    }

    [Fact]
    public void GivenNone_WhenMatchWithActions_ThenInvokeOnNone()
    {
        Option<int> none = Option.None<int>();

        var onSome = Substitute.For<Action<int>>();
        var onNone = Substitute.For<Action>();

        none.Match(onSome, onNone);

        onSome.DidNotReceive().Invoke(Arg.Any<int>());
        onNone.Received(1).Invoke();
    }

    [Fact]
    public void GivenNone_WhenMatchWithFunc_ThenInvokeOnNone()
    {
        Option<int> none = Option.None<int>();

        var onSome = Substitute.For<Func<int, int>>();
        onSome.Invoke(Arg.Any<int>()).Returns(1);

        var onNone = Substitute.For<Func<int>>();
        onNone.Invoke().Returns(10);

        int output = none.Match(onSome, onNone);

        output.Should().Be(10);
    }

    [Fact]
    public void GivenNone_WhenMapWithFallback_ThenReturnDefault()
    {
        Option<int> none = Option.None<int>();

        none.MapOr(10, x => x + 1).Should().Be(10);
        none.MapOrElse(() => 10, x => x + 1).Should().Be(10);
    }

    [Fact]
    public void GivenNone_WhenInspect_ThenDoNothing()
    {
        Option<int> none = Option.None<int>();

        var action = Substitute.For<Action<int>>();

        Option<int> result = none.Inspect(action);

        result.Should().Be(none);
        action.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public void GivenNone_WhenFilter_ThenDoNothing()
    {
        Option<int> none = Option.None<int>();

        var filter = Substitute.For<Func<int, bool>>();

        Option<int> result = none.Filter(filter);

        result.Should().Be(none);
        filter.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public void GivenNone_WhenOr_ThenReturnOther()
    {
        Option<int> none = Option.None<int>();

        none.Or(Option.None<int>()).Should().Be(none);
        none.Or(Option.Some(1)).Should().Be(Option.Some(1));

        none.OrElse(Option.None<int>).Should().Be(Option.None<int>());
        none.OrElse(() => Option.Some(1)).Should().Be(Option.Some(1));
    }

    [Fact]
    public void GivenNone_WhenXor_ThenReturnExpected()
    {
        Option<int> none = Option.None<int>();

        none.Xor(Option.None<int>()).Should().Be(Option.None<int>());
        none.Xor(Option.Some(1)).Should().Be(Option.Some(1));
    }
}
