namespace Waystone.Monads.Results.Extensions
{
    using JetBrains.Annotations;
    using Options;
    using Shouldly;
    using Xunit;

    [TestSubject(typeof(Result))]
    public sealed class ResultOfTOkTErrExtensionsTests
    {
#region flatten

        [Fact]
        public void WhenFlatteningResult_ThenReduceNestingByOne()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);
            Result<Result<string, string>, string> nested =
                ok.Map(_ => Result.Ok<string, string>("1"));

            Result<string, string> flattened = nested.Flatten();

            flattened.Unwrap().ShouldBe("1");
        }

#endregion flatten

#region transpose

        [Fact]
        public void GivenOkResultOfSome_WhenTranspose_ThenReturnSomeOfOk()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);
            Result<Option<int>, string> okOfSome = ok.Map(Option.Some);

            Option<Result<int, string>> result = okOfSome.Transpose();

            result.IsSome.ShouldBeTrue();
            result.Unwrap().ShouldBe(ok);
        }

        [Fact]
        public void GivenOkResultOfNone_WhenTranspose_ThenReturnNone()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);
            Result<Option<int>, string> none = ok.Map(_ => Option.None<int>());

            Option<Result<int, string>> result = none.Transpose();

            result.IsNone.ShouldBeTrue();
        }

        [Fact]
        public void GivenErrOfSome_WhenTranspose_ThenReturnSomeOfErr()
        {
            Result<int, string> err = Result.Err<int, string>("failed");
            Result<Option<int>, string> errOfSome = err.Map(Option.Some);

            Option<Result<int, string>> result = errOfSome.Transpose();

            result.IsSome.ShouldBeTrue();
            result.Unwrap().ShouldBe(err);
        }

        [Fact]
        public void GivenErrOfNone_WhenTranspose_ThenReturnSomeOfErr()
        {
            Result<int, string> err = Result.Err<int, string>("failed");
            Result<Option<int>, string> errOfNone =
                err.Map(_ => Option.None<int>());

            Option<Result<int, string>> result = errOfNone.Transpose();

            result.IsSome.ShouldBeTrue();
            result.Unwrap().ShouldBe(err);
        }

#endregion transpose
    }
}
