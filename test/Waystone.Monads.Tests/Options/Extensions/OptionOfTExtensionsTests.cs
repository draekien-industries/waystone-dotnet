namespace Waystone.Monads.Options.Extensions
{
    using JetBrains.Annotations;
    using Results;
    using Shouldly;
    using Xunit;

    [TestSubject(typeof(OptionOfTExtensions))]
    public sealed class OptionOfTExtensionsTests
    {
        #region unzip

        [Fact]
        public void GivenSomeWithTuple_WhenUnzip_ThenReturnSome()
        {
            Option<int> some1 = Option.Some(1);
            Option<int> some2 = Option.Some(2);
            Option<(int, int)> zipped = some1.Zip(some2);
            (Option<int>, Option<int>) result = zipped.Unzip();
            result.ShouldBe((some1, some2));
        }

        [Fact]
        public void GivenNoneNoneTuple_WhenUnzip_ThenReturnNone()
        {
            Option<int> none1 = Option.None<int>();
            Option<int> none2 = Option.None<int>();
            Option<(int, int)> zipped = none1.Zip(none2);
            (Option<int>, Option<int>) result = zipped.Unzip();
            result.ShouldBe((none1, none2));
        }

        [Fact]
        public void GivenNoneSomeTuple_WhenUnzip_ThenReturnNoneNone()
        {
            Option<int> none = Option.None<int>();
            Option<int> some = Option.Some(1);
            Option<(int, int)> zipped = none.Zip(some);
            (Option<int>, Option<int>) result = zipped.Unzip();
            result.ShouldBe((none, none));
        }

        #endregion unzip

        #region flatten

        [Fact]
        public void GivenSomeOfSome_WhenFlatten_ThenReturnOption()
        {
            Option<Option<int>> some = Option.Some(Option.Some(1));
            Option<int> result = some.Flatten();
            result.ShouldBe(Option.Some(1));
        }

        [Fact]
        public void GivenNoneOfSome_WhenFlatten_ThenReturnNone()
        {
            Option<int> none = Option.None<int>();
            Option<Option<int>> nested = none.Map(Option.Some);
            Option<int> result = nested.Flatten();
            result.ShouldBe(Option.None<int>());
        }

        #endregion flatten

        #region transpose

        [Fact]
        public void GivenSomeOfOk_WhenTranspose_ThenReturnOkOfSome()
        {
            Option<Result<int, int>> option =
                Option.Some(Result.Ok<int, int>(1));
            Result<Option<int>, int> result = option.Transpose();
            result.ShouldBe(Result.Ok<Option<int>, int>(Option.Some(1)));
        }

        [Fact]
        public void GivenSomeOfErr_WhenTranspose_ThenReturnErrOfSome()
        {
            Option<Result<int, int>>
                option = Option.Some(Result.Err<int, int>(1));
            Result<Option<int>, int> result = option.Transpose();
            result.ShouldBe(Result.Err<Option<int>, int>(1));
        }

        [Fact]
        public void GivenNoneOfOk_WhenTranspose_ThenReturnOkOfNone()
        {
            Option<Result<int, int>> option =
                Option.None<int>().Map(Result.Ok<int, int>);

            Result<Option<int>, int> result = option.Transpose();

            result.ShouldBe(Result.Ok<Option<int>, int>(Option.None<int>()));
        }

        [Fact]
        public void GivenNoneOfErr_WhenTranspose_ThenReturnOkOfNone()
        {
            Option<Result<int, int>> option =
                Option.None<int>().Map(Result.Err<int, int>);

            Result<Option<int>, int> result = option.Transpose();

            result.ShouldBe(Result.Ok<Option<int>, int>(Option.None<int>()));
        }

        #endregion transpose
    }
}
