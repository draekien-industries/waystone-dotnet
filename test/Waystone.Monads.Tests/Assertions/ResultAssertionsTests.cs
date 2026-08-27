namespace Waystone.Monads.Assertions;

using System.Threading.Tasks;
using Results;
using Shouldly;
using Xunit;

/// <remarks>
/// Every failing case checks that the other branch was named, which is the whole
/// point on a result: a test that expected an <c>Ok</c> and got an <c>Err</c>
/// usually has its explanation sitting in the error.
/// </remarks>
public sealed class ResultAssertionsTests
{
    private static Result<int, string> Ok() => Result.Ok<int, string>(3);

    private static Result<int, string> Err() =>
        Result.Err<int, string>("failed");

    [Fact]
    public void GivenAnOk_WhenShouldBeOk_ThenReturnTheValue()
    {
        Result<int, string> result = Ok();

        result.ShouldBeOk().ShouldBe(3);
    }

    [Fact]
    public void GivenAnErr_WhenShouldBeOk_ThenNameTheError()
    {
        Result<int, string> result = Err();

        string message = AssertionFailure.From(() => result.ShouldBeOk());

        message.ShouldBe(
            "result\n    should be Ok\n    but was\nErr(\"failed\")");
    }

    [Fact]
    public void GivenAnErr_WhenShouldBeErr_ThenReturnTheError()
    {
        Result<int, string> result = Err();

        result.ShouldBeErr().ShouldBe("failed");
    }

    [Fact]
    public void GivenAnOk_WhenShouldBeErr_ThenNameTheValue()
    {
        Result<int, string> result = Ok();

        string message = AssertionFailure.From(() => result.ShouldBeErr());

        message.ShouldBe("result\n    should be Err\n    but was\nOk(3)");
    }

    [Fact]
    public void GivenAnOk_WhenShouldBeOkValueMatches_ThenReturnTheValue()
    {
        Result<int, string> result = Ok();

        result.ShouldBeOkValue(3).ShouldBe(3);
    }

    [Fact]
    public void GivenAnErr_WhenShouldBeOkValue_ThenNameBothSides()
    {
        Result<int, string> result = Err();

        string message = AssertionFailure.From(() => result.ShouldBeOkValue(3));

        message.ShouldBe(
            "result\n    should be Ok(3)\n    but was\nErr(\"failed\")");
    }

    [Fact]
    public void GivenAnOk_WhenShouldBeOkValueDiffers_ThenReportBothValues()
    {
        Result<int, string> result = Ok();

        string message = AssertionFailure.From(() => result.ShouldBeOkValue(4));

        message.ShouldContain("3");
        message.ShouldContain("4");
        message.ShouldNotContain("should be Ok(4)");
    }

    [Fact]
    public void GivenAnErr_WhenShouldBeErrValueMatches_ThenReturnTheError()
    {
        Result<int, string> result = Err();

        result.ShouldBeErrValue("failed").ShouldBe("failed");
    }

    [Fact]
    public void GivenAnOk_WhenShouldBeErrValue_ThenNameBothSides()
    {
        Result<int, string> result = Ok();

        string message =
            AssertionFailure.From(() => result.ShouldBeErrValue("failed"));

        message.ShouldBe(
            "result\n    should be Err(\"failed\")\n    but was\nOk(3)");
    }

    [Fact]
    public void GivenAnErr_WhenShouldBeErrValueDiffers_ThenReportBothErrors()
    {
        Result<int, string> result = Err();

        string message =
            AssertionFailure.From(() => result.ShouldBeErrValue("other"));

        message.ShouldContain("failed");
        message.ShouldContain("other");
        message.ShouldNotContain("should be Err(\"other\")");
    }

    [Fact]
    public async Task GivenAnOkTask_WhenShouldBeOkAsync_ThenReturnTheValue()
    {
        Task<Result<int, string>> task = Task.FromResult(Ok());

        (await task.ShouldBeOkAsync()).ShouldBe(3);
    }

    [Fact]
    public async Task GivenAnErrTask_WhenShouldBeOkAsync_ThenMatchTheSyncMessage()
    {
        Task<Result<int, string>> task = Task.FromResult(Err());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeOkAsync());

        message.ShouldBe("task\n    should be Ok\n    but was\nErr(\"failed\")");
    }

    [Fact]
    public async Task GivenAnErrTask_WhenShouldBeErrAsync_ThenReturnTheError()
    {
        Task<Result<int, string>> task = Task.FromResult(Err());

        (await task.ShouldBeErrAsync()).ShouldBe("failed");
    }

    [Fact]
    public async Task GivenAnOkTask_WhenShouldBeErrAsync_ThenNameTheValue()
    {
        Task<Result<int, string>> task = Task.FromResult(Ok());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeErrAsync());

        message.ShouldBe("task\n    should be Err\n    but was\nOk(3)");
    }

    [Fact]
    public async Task
        GivenAnOkTask_WhenShouldBeOkValueAsyncMatches_ThenReturnTheValue()
    {
        Task<Result<int, string>> task = Task.FromResult(Ok());

        (await task.ShouldBeOkValueAsync(3)).ShouldBe(3);
    }

    [Fact]
    public async Task GivenAnErrTask_WhenShouldBeOkValueAsync_ThenNameBothSides()
    {
        Task<Result<int, string>> task = Task.FromResult(Err());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeOkValueAsync(3));

        message.ShouldBe(
            "task\n    should be Ok(3)\n    but was\nErr(\"failed\")");
    }

    [Fact]
    public async Task
        GivenAnErrTask_WhenShouldBeErrValueAsyncMatches_ThenReturnTheError()
    {
        Task<Result<int, string>> task = Task.FromResult(Err());

        (await task.ShouldBeErrValueAsync("failed")).ShouldBe("failed");
    }

    [Fact]
    public async Task GivenAnOkTask_WhenShouldBeErrValueAsync_ThenNameBothSides()
    {
        Task<Result<int, string>> task = Task.FromResult(Ok());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeErrValueAsync("failed"));

        message.ShouldBe(
            "task\n    should be Err(\"failed\")\n    but was\nOk(3)");
    }

    [Fact]
    public async Task GivenAnOkValueTask_WhenShouldBeOkAsync_ThenReturnTheValue()
    {
        ValueTask<Result<int, string>> task =
            new ValueTask<Result<int, string>>(Ok());

        (await task.ShouldBeOkAsync()).ShouldBe(3);
    }

    [Fact]
    public async Task
        GivenAnErrValueTask_WhenShouldBeOkAsync_ThenMatchTheSyncMessage()
    {
        ValueTask<Result<int, string>> task =
            new ValueTask<Result<int, string>>(Err());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeOkAsync());

        message.ShouldBe("task\n    should be Ok\n    but was\nErr(\"failed\")");
    }

    [Fact]
    public async Task
        GivenAnErrValueTask_WhenShouldBeErrAsync_ThenReturnTheError()
    {
        ValueTask<Result<int, string>> task =
            new ValueTask<Result<int, string>>(Err());

        (await task.ShouldBeErrAsync()).ShouldBe("failed");
    }

    [Fact]
    public async Task GivenAnOkValueTask_WhenShouldBeErrAsync_ThenNameTheValue()
    {
        ValueTask<Result<int, string>> task =
            new ValueTask<Result<int, string>>(Ok());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeErrAsync());

        message.ShouldBe("task\n    should be Err\n    but was\nOk(3)");
    }

    [Fact]
    public async Task
        GivenAnOkValueTask_WhenShouldBeOkValueAsyncMatches_ThenReturnTheValue()
    {
        ValueTask<Result<int, string>> task =
            new ValueTask<Result<int, string>>(Ok());

        (await task.ShouldBeOkValueAsync(3)).ShouldBe(3);
    }

    [Fact]
    public async Task
        GivenAnErrValueTask_WhenShouldBeOkValueAsync_ThenNameBothSides()
    {
        ValueTask<Result<int, string>> task =
            new ValueTask<Result<int, string>>(Err());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeOkValueAsync(3));

        message.ShouldBe(
            "task\n    should be Ok(3)\n    but was\nErr(\"failed\")");
    }

    [Fact]
    public async Task
        GivenAnErrValueTask_WhenShouldBeErrValueAsyncMatches_ThenReturnTheError()
    {
        ValueTask<Result<int, string>> task =
            new ValueTask<Result<int, string>>(Err());

        (await task.ShouldBeErrValueAsync("failed")).ShouldBe("failed");
    }

    [Fact]
    public async Task
        GivenAnOkValueTask_WhenShouldBeErrValueAsync_ThenNameBothSides()
    {
        ValueTask<Result<int, string>> task =
            new ValueTask<Result<int, string>>(Ok());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeErrValueAsync("failed"));

        message.ShouldBe(
            "task\n    should be Err(\"failed\")\n    but was\nOk(3)");
    }
}
