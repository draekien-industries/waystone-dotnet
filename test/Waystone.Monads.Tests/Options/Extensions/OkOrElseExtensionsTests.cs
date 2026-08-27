namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Results;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class OkOrElseExtensionsTests
{
    [Fact]
    public async Task GivenSome_WhenOkOrElseAsync_ThenReturnOk()
    {
        Result<int, string> result = await Option.Some(10)
           .OkOrElseAsync(() => Task.FromResult("Error occurred"));

        result.ShouldBeOkValue(10);
    }

    [Fact]
    public async Task GivenNone_WhenOkOrElseAsync_ThenReturnTheError()
    {
        Result<int, string> result = await Option.None<int>()
           .OkOrElseAsync(() => Task.FromResult("Error occurred"));

        result.ShouldBeErrValue("Error occurred");
    }

    [Fact]
    public async Task GivenSomeTask_WhenOkOrElseAsync_ThenReturnOk()
    {
        Result<int, string> result = await Task.FromResult(Option.Some(20))
           .OkOrElseAsync(() => Task.FromResult("Task Error occurred"));

        result.ShouldBeOkValue(20);
    }

    [Fact]
    public async Task GivenNoneTask_WhenOkOrElseAsync_ThenReturnTheError()
    {
        Result<int, string> result = await Task.FromResult(Option.None<int>())
           .OkOrElseAsync(() => Task.FromResult("Task Error occurred"));

        result.ShouldBeErrValue("Task Error occurred");
    }

    [Fact]
    public async Task GivenSomeValueTask_WhenOkOrElseAsync_ThenReturnOk()
    {
        Result<int, string> result =
            await new ValueTask<Option<int>>(Option.Some(30))
               .OkOrElseAsync(
                    () => Task.FromResult("ValueTask Error occurred"));

        result.ShouldBeOkValue(30);
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenOkOrElseAsync_ThenReturnTheError()
    {
        Result<int, string> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .OkOrElseAsync(
                    () => Task.FromResult("ValueTask Error occurred"));

        result.ShouldBeErrValue("ValueTask Error occurred");
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenOkOrElseAsyncWithASyncFactory_ThenReturnOk()
    {
        Result<int, string> result = await Task.FromResult(Option.Some(10))
           .OkOrElseAsync(() => "Synchronous Error occurred");

        result.ShouldBeOkValue(10);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenOkOrElseAsyncWithASyncFactory_ThenReturnTheError()
    {
        Result<int, string> result = await Task.FromResult(Option.None<int>())
           .OkOrElseAsync(() => "Synchronous Error occurred");

        result.ShouldBeErrValue("Synchronous Error occurred");
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenOkOrElseAsyncWithASyncFactory_ThenReturnOk()
    {
        Result<int, string> result =
            await new ValueTask<Option<int>>(Option.Some(20))
               .OkOrElseAsync(() => "Synchronous ValueTask Error occurred");

        result.ShouldBeOkValue(20);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenOkOrElseAsyncWithASyncFactory_ThenReturnTheError()
    {
        Result<int, string> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .OkOrElseAsync(() => "Synchronous ValueTask Error occurred");

        result.ShouldBeErrValue("Synchronous ValueTask Error occurred");
    }
}
