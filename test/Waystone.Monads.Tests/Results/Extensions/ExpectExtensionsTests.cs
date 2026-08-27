namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Xunit;

[TestSubject(typeof(ExpectExtensions))]
public sealed class ExpectExtensionsTests
{
    private const string ExpectedOk = "Expected an Ok Result";
    private const string ExpectedErr = "Expected an Err Result";

    private static Task<Result<int, string>> OkTask(int value) =>
        Task.FromResult(Result.Ok<int, string>(value));

    private static ValueTask<Result<int, string>> ErrValueTask(string error) =>
        new ValueTask<Result<int, string>>(Result.Err<int, string>(error));

    [Fact]
    public async Task GivenOkTask_WhenExpectAsync_ThenReturnTheValue()
    {
        int result = await OkTask(10).ExpectAsync(ExpectedOk);

        result.ShouldBe(10);
    }

    [Fact]
    public async Task GivenOkValueTask_WhenExpectAsync_ThenReturnTheValue()
    {
        int result =
            await new ValueTask<Result<int, string>>(
                Result.Ok<int, string>(20)).ExpectAsync(ExpectedOk);

        result.ShouldBe(20);
    }

    [Fact]
    public async Task GivenErrTask_WhenExpectAsync_ThenThrowCarryingTheMessage()
    {
        UnmetExpectationException exception =
            await Should.ThrowAsync<UnmetExpectationException>(
                async () =>
                    await Task.FromResult(
                            Result.Err<int, string>("Error occurred"))
                       .ExpectAsync(ExpectedOk));

        exception.Message.ShouldContain(ExpectedOk);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenExpectAsync_ThenThrowCarryingTheMessage()
    {
        UnmetExpectationException exception =
            await Should.ThrowAsync<UnmetExpectationException>(
                async () => await ErrValueTask("Error occurred")
                   .ExpectAsync(ExpectedOk));

        exception.Message.ShouldContain(ExpectedOk);
    }

    [Fact]
    public async Task GivenErrTask_WhenExpectErrAsync_ThenReturnTheError()
    {
        string result =
            await Task.FromResult(Result.Err<int, string>("Error occurred"))
               .ExpectErrAsync(ExpectedErr);

        result.ShouldBe("Error occurred");
    }

    [Fact]
    public async Task GivenErrValueTask_WhenExpectErrAsync_ThenReturnTheError()
    {
        string result = await ErrValueTask("Critical Error")
           .ExpectErrAsync(ExpectedErr);

        result.ShouldBe("Critical Error");
    }

    [Fact]
    public async Task
        GivenOkTask_WhenExpectErrAsync_ThenThrowCarryingTheMessage()
    {
        UnmetExpectationException exception =
            await Should.ThrowAsync<UnmetExpectationException>(
                async () => await OkTask(10).ExpectErrAsync(ExpectedErr));

        exception.Message.ShouldContain(ExpectedErr);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenExpectErrAsync_ThenThrowCarryingTheMessage()
    {
        UnmetExpectationException exception =
            await Should.ThrowAsync<UnmetExpectationException>(
                async () =>
                    await new ValueTask<Result<int, string>>(
                            Result.Ok<int, string>(10))
                       .ExpectErrAsync(ExpectedErr));

        exception.Message.ShouldContain(ExpectedErr);
    }
}
