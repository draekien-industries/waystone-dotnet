namespace Waystone.Monads.Results;

using JetBrains.Annotations;
using Shouldly;
using System;
using System.Threading.Tasks;
using Waystone.Monads.Results.Errors;
using Xunit;

/// <remarks>
/// These cover the single-type-argument factories, which default the error type
/// to <see cref="Error" />. The two-argument forms they delegate to are covered
/// by <c>ResultTests</c>; what is load-bearing here is that the defaulted
/// overload picks <see cref="Error" /> and that the <c>Try</c> pair maps a thrown
/// exception onto an error code rather than letting it escape.
/// </remarks>
[TestSubject(typeof(Result))]
public sealed class ErrorResultFactoriesTests
{
    [Fact]
    public void GivenAValue_WhenOk_ThenReturnAnOkDefaultingTheErrorType()
    {
        Result<int, Error> result = Result.Ok<int>(10);

        result.ShouldBeOkValue(10);
    }

    [Fact]
    public void GivenAnError_WhenErr_ThenReturnAnErrCarryingIt()
    {
        var error = new Error("Explicit.Code", "something went wrong");

        Result<int, Error> result = Result.Err<int>(error);

        result.ShouldBeErrValue(error);
        result.UnwrapErr().Code.Value.ShouldBe("Explicit.Code");
        result.UnwrapErr().Message.ShouldBe("something went wrong");
    }

    [Fact]
    public void GivenAFactoryThatSucceeds_WhenTry_ThenReturnOk()
    {
        Result<int, Error> result = Result.Try<int>(() => 10);

        result.ShouldBeOkValue(10);
    }

    [Fact]
    public void GivenAFactoryThatThrows_WhenTry_ThenReturnTheMappedError()
    {
        Result<int, Error> result = Result.Try<int>(
            () => throw new InvalidOperationException("factory failed"));

        result.ShouldBeErr();
        result.UnwrapErr().Code.Value.ShouldBe("InvalidOperation");
        result.UnwrapErr().Message.ShouldBe("factory failed");
    }

    [Fact]
    public async Task GivenAnAsyncFactoryThatSucceeds_WhenTryAsync_ThenReturnOk()
    {
        Result<int, Error> result =
            await Result.TryAsync<int>(() => Task.FromResult(20));

        result.ShouldBeOkValue(20);
    }

    [Fact]
    public async Task
        GivenAnAsyncFactoryThatThrows_WhenTryAsync_ThenReturnTheMappedError()
    {
        Result<int, Error> result = await Result.TryAsync<int>(
            () => Task.FromException<int>(
                new InvalidOperationException("async factory failed")));

        result.ShouldBeErr();
        result.UnwrapErr().Code.Value.ShouldBe("InvalidOperation");
        result.UnwrapErr().Message.ShouldBe("async factory failed");
    }
}
