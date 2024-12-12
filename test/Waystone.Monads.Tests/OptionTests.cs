namespace Waystone.Monads.Tests;

[TestSubject(typeof(Option))]
public class OptionTests
{
    [Fact]
    public void WhenBindingFactoryThatSucceeds_ThenReturnSome()
    {
        var callback = Substitute.For<Action<Exception>>();
        Option<int> option = Option.Bind(() => 1, callback);
        option.Should().Be(Option.Some(1));
        callback.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public void
        GivenFactoryThatThrows_AndOnErrorCallback_WhenBindingFactory_ThenInvokeCallback()
    {
        var callback = Substitute.For<Action<Exception>>();
        Option<int> option = Option.Bind<int>(
            () => throw new Exception(),
            callback);
        option.Should().Be(Option.None<int>());
        callback.Received(1).Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public void GivenSomeWithTuple_WhenUnzip_ThenReturnSome()
    {
        Option<int> some1 = Option.Some(1);
        Option<int> some2 = Option.Some(2);
        Option<(int, int)> zipped = some1.Zip(some2);
        (Option<int>, Option<int>) result = zipped.Unzip();
        result.Should().Be((some1, some2));
    }

    [Fact]
    public void GivenNoneNoneTuple_WhenUnzip_ThenReturnNone()
    {
        Option<int> none1 = Option.None<int>();
        Option<int> none2 = Option.None<int>();
        Option<(int, int)> zipped = none1.Zip(none2);
        (Option<int>, Option<int>) result = zipped.Unzip();
        result.Should().Be((none1, none2));
    }

    [Fact]
    public void GivenNoneSomeTuple_WhenUnzip_ThenReturnNoneNone()
    {
        Option<int> none = Option.None<int>();
        Option<int> some = Option.Some(1);
        Option<(int, int)> zipped = none.Zip(some);
        (Option<int>, Option<int>) result = zipped.Unzip();
        result.Should().Be((none, none));
    }

    [Fact]
    public void GivenSomeOfSome_WhenFlatten_ThenReturnOption()
    {
        Option<Option<int>> some = Option.Some(Option.Some(1));
        Option<int> result = some.Flatten();
        result.Should().Be(Option.Some(1));
    }

    [Fact]
    public void GivenNoneOfSome_WhenFlatten_ThenReturnNone()
    {
        Option<int> none = Option.None<int>();
        Option<Option<int>> nested = none.Map(Option.Some);
        Option<int> result = nested.Flatten();
        result.Should().Be(Option.None<int>());
    }

    [Fact]
    public void GivenSomeOfOk_WhenTranspose_ThenReturnOkOfSome()
    {
        Option<Result<int, int>> option = Option.Some(Result.Ok<int, int>(1));
        Result<Option<int>, int> result = option.Transpose();
        result.Should().Be(Result.Ok<Option<int>, int>(Option.Some(1)));
    }

    [Fact]
    public void GivenSomeOfErr_WhenTranspose_ThenReturnErrOfSome()
    {
        Option<Result<int, int>>
            option = Option.Some(Result.Err<int, int>(1));
        Result<Option<int>, int> result = option.Transpose();
        result.Should().Be(Result.Err<Option<int>, int>(1));
    }

    [Fact]
    public void GivenNoneOfOk_WhenTranspose_ThenReturnOkOfNone()
    {
        Option<Result<int, int>> option =
            Option.None<int>().Map(Result.Ok<int, int>);

        Result<Option<int>, int> result = option.Transpose();

        result.Should().Be(Result.Ok<Option<int>, int>(Option.None<int>()));
    }

    [Fact]
    public void GivenNoneOfErr_WhenTranspose_ThenReturnOkOfNone()
    {
        Option<Result<int, int>> option =
            Option.None<int>().Map(Result.Err<int, int>);

        Result<Option<int>, int> result = option.Transpose();

        result.Should().Be(Result.Ok<Option<int>, int>(Option.None<int>()));
    }

    [Fact]
    public void WhenImplicitlyCreatingOption_ThenReturnExpected()
    {
        Option<int> option1 = 0;
        Option<int> option2 = 1;
        Option<string> option3 = string.Empty;
        Option<string> option4 = default(string)!;
        Option<Guid> option5 = default(Guid)!;
        Option<Guid> option6 = Guid.NewGuid();

        option1.IsSome.Should().BeFalse();
        option2.IsSome.Should().BeTrue();
        option3.IsSome.Should().BeTrue();
        option4.IsSome.Should().BeFalse();
        option5.IsSome.Should().BeFalse();
        option6.IsSome.Should().BeTrue();
    }
}
