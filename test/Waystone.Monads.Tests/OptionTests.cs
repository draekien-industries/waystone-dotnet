namespace Waystone.Monads.Tests;

[TestSubject(typeof(Option))]
public class OptionTests
{
    [Fact]
    public void GivenSomeWithTuple_WhenUnzip_ThenReturnSome()
    {
        IOption<int> some1 = Option.Some(1);
        IOption<int> some2 = Option.Some(2);
        IOption<(int, int)> zipped = some1.Zip(some2);
        (IOption<int>, IOption<int>) result = zipped.Unzip();
        result.Should().Be((some1, some2));
    }

    [Fact]
    public void GivenNoneNoneTuple_WhenUnzip_ThenReturnNone()
    {
        IOption<int> none1 = Option.None<int>();
        IOption<int> none2 = Option.None<int>();
        IOption<(int, int)> zipped = none1.Zip(none2);
        (IOption<int>, IOption<int>) result = zipped.Unzip();
        result.Should().Be((none1, none2));
    }

    [Fact]
    public void GivenNoneSomeTuple_WhenUnzip_ThenReturnNoneNone()
    {
        IOption<int> none = Option.None<int>();
        IOption<int> some = Option.Some(1);
        IOption<(int, int)> zipped = none.Zip(some);
        (IOption<int>, IOption<int>) result = zipped.Unzip();
        result.Should().Be((none, none));
    }

    [Fact]
    public void GivenSomeOfSome_WhenFlatten_ThenReturnOption()
    {
        IOption<IOption<int>> some = Option.Some(Option.Some(1));
        IOption<int> result = some.Flatten();
        result.Should().Be(Option.Some(1));
    }

    [Fact]
    public void GivenNoneOfSome_WhenFlatten_ThenReturnNone()
    {
        IOption<int> none = Option.None<int>();
        IOption<IOption<int>> nested = none.Map(Option.Some);
        IOption<int> result = nested.Flatten();
        result.Should().Be(Option.None<int>());
    }

    [Fact]
    public void GivenSomeOfOk_WhenTranspose_ThenReturnOkOfSome()
    {
        IOption<IResult<int, int>> option = Option.Some(Result.Ok<int, int>(1));
        IResult<IOption<int>, int> result = option.Transpose();
        result.Should().Be(Result.Ok<IOption<int>, int>(Option.Some(1)));
    }

    [Fact]
    public void GivenSomeOfErr_WhenTranspose_ThenReturnErrOfSome()
    {
        IOption<IResult<int, int>>
            option = Option.Some(Result.Err<int, int>(1));
        IResult<IOption<int>, int> result = option.Transpose();
        result.Should().Be(Result.Err<IOption<int>, int>(1));
    }

    [Fact]
    public void GivenNoneOfOk_WhenTranspose_ThenReturnOkOfNone()
    {
        IOption<IResult<int, int>> option =
            Option.None<int>().Map(Result.Ok<int, int>);

        IResult<IOption<int>, int> result = option.Transpose();

        result.Should().Be(Result.Ok<IOption<int>, int>(Option.None<int>()));
    }

    [Fact]
    public void GivenNoneOfErr_WhenTranspose_ThenReturnOkOfNone()
    {
        IOption<IResult<int, int>> option =
            Option.None<int>().Map(Result.Err<int, int>);

        IResult<IOption<int>, int> result = option.Transpose();

        result.Should().Be(Result.Ok<IOption<int>, int>(Option.None<int>()));
    }
}
