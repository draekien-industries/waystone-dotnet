namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Xunit;

[TestSubject(typeof(ExpectExtensions))]
public sealed class ExpectExtensionsTests
{
    private const string Message = "Expected a Some Option";

    [Fact]
    public async Task GivenSomeTask_WhenExpectAsync_ThenReturnTheValue()
    {
        int result = await Task.FromResult(Option.Some(10))
           .ExpectAsync(Message);

        result.ShouldBe(10);
    }

    [Fact]
    public async Task GivenSomeValueTask_WhenExpectAsync_ThenReturnTheValue()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(20))
           .ExpectAsync(Message);

        result.ShouldBe(20);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenExpectAsync_ThenThrowCarryingTheMessage()
    {
        UnmetExpectationException exception =
            await Should.ThrowAsync<UnmetExpectationException>(
                async () => await Task.FromResult(Option.None<int>())
                   .ExpectAsync(Message));

        exception.Message.ShouldContain(Message);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenExpectAsync_ThenThrowCarryingTheMessage()
    {
        UnmetExpectationException exception =
            await Should.ThrowAsync<UnmetExpectationException>(
                async () =>
                    await new ValueTask<Option<int>>(Option.None<int>())
                       .ExpectAsync(Message));

        exception.Message.ShouldContain(Message);
    }
}
