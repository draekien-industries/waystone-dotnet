namespace Waystone.Monads.Assertions;

using Options;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// The failure text is the reason this package exists, so these assert the whole
/// message rather than only that <see cref="ShouldAssertException" /> was thrown.
/// Each receiver is a local named <c>option</c> or <c>task</c> because Shouldly
/// recovers the caller's expression from the source line, so the local's name is
/// part of the expected string — which is also what proves the expression reaches
/// the message at all.
/// </remarks>
public sealed class OptionAssertionsTests
{
    private static Option<int> Some() => Option.Some(3);

    private static Option<int> None() => Option.None<int>();

    [Fact]
    public void GivenASome_WhenShouldBeSome_ThenReturnTheValue()
    {
        Option<int> option = Some();

        option.ShouldBeSome().ShouldBe(3);
    }

    [Fact]
    public void GivenANone_WhenShouldBeSome_ThenNameTheStateFound()
    {
        Option<int> option = None();

        string message = AssertionFailure.From(() => option.ShouldBeSome());

        message.ShouldBe("option\n    should be Some\n    but was\nNone");
    }

    [Fact]
    public void GivenANone_WhenShouldBeNone_ThenPass()
    {
        Option<int> option = None();

        option.ShouldBeNone();
    }

    [Fact]
    public void GivenASome_WhenShouldBeNone_ThenNameTheValueFound()
    {
        Option<int> option = Some();

        string message = AssertionFailure.From(() => option.ShouldBeNone());

        message.ShouldBe("option\n    should be None\n    but was\nSome(3)");
    }

    [Fact]
    public void GivenAStringSome_WhenShouldBeNone_ThenQuoteTheValue()
    {
        Option<string> option = Option.Some("failed");

        string message = AssertionFailure.From(() => option.ShouldBeNone());

        message.ShouldBe(
            "option\n    should be None\n    but was\nSome(\"failed\")");
    }

    [Fact]
    public void GivenACustomMessage_WhenTheAssertionFails_ThenAppendIt()
    {
        Option<int> option = None();

        string message =
            AssertionFailure.From(() => option.ShouldBeSome("while loading"));

        message.ShouldBe(
            "option\n    should be Some\n    but was\nNone\n\nAdditional Info:\n    while loading");
    }

    /// <summary>
    /// The caller's own expression reaches the message, rather than the receiver
    /// parameter's name. This is what the <c>CallerArgumentExpression</c> plumbing
    /// buys, and it fails silently if the attribute is dropped.
    /// </summary>
    [Fact]
    public void GivenAnInlineReceiver_WhenTheAssertionFails_ThenNameTheExpression()
    {
        string message =
            AssertionFailure.From(() => Option.None<int>().ShouldBeSome());

        message.ShouldStartWith("Option.None<int>()");
        message.ShouldNotContain("actual");
    }

    [Fact]
    public void GivenASome_WhenShouldBeSomeValueMatches_ThenReturnTheValue()
    {
        Option<int> option = Some();

        option.ShouldBeSomeValue(3).ShouldBe(3);
    }

    [Fact]
    public void GivenANone_WhenShouldBeSomeValue_ThenNameTheExpectedValue()
    {
        Option<int> option = None();

        string message = AssertionFailure.From(() => option.ShouldBeSomeValue(3));

        message.ShouldBe("option\n    should be Some(3)\n    but was\nNone");
    }

    /// <summary>
    /// A wrong value is deliberately left to Shouldly, so this asserts that its
    /// comparison ran and reported both values rather than asserting a string this
    /// package does not own.
    /// </summary>
    [Fact]
    public void GivenASome_WhenShouldBeSomeValueDiffers_ThenReportBothValues()
    {
        Option<int> option = Some();

        string message = AssertionFailure.From(() => option.ShouldBeSomeValue(4));

        message.ShouldContain("3");
        message.ShouldContain("4");
        message.ShouldNotContain("should be Some(4)");
    }

    [Fact]
    public async Task GivenASomeTask_WhenShouldBeSomeAsync_ThenReturnTheValue()
    {
        Task<Option<int>> task = Task.FromResult(Some());

        (await task.ShouldBeSomeAsync()).ShouldBe(3);
    }

    /// <summary>
    /// The awaited overload forwards the caller's expression to the synchronous
    /// assertion, so its message is the synchronous one with the receiver's name
    /// swapped in. A reader cannot tell which receiver shape failed.
    /// </summary>
    [Fact]
    public async Task GivenANoneTask_WhenShouldBeSomeAsync_ThenMatchTheSyncMessage()
    {
        Task<Option<int>> task = Task.FromResult(None());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeSomeAsync());

        message.ShouldBe("task\n    should be Some\n    but was\nNone");
    }

    [Fact]
    public async Task GivenANoneTask_WhenShouldBeNoneAsync_ThenPass()
    {
        Task<Option<int>> task = Task.FromResult(None());

        await task.ShouldBeNoneAsync();
    }

    [Fact]
    public async Task GivenASomeTask_WhenShouldBeNoneAsync_ThenNameTheValue()
    {
        Task<Option<int>> task = Task.FromResult(Some());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeNoneAsync());

        message.ShouldBe("task\n    should be None\n    but was\nSome(3)");
    }

    [Fact]
    public async Task
        GivenASomeTask_WhenShouldBeSomeValueAsyncMatches_ThenReturnTheValue()
    {
        Task<Option<int>> task = Task.FromResult(Some());

        (await task.ShouldBeSomeValueAsync(3)).ShouldBe(3);
    }

    [Fact]
    public async Task
        GivenANoneTask_WhenShouldBeSomeValueAsync_ThenNameTheExpectedValue()
    {
        Task<Option<int>> task = Task.FromResult(None());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeSomeValueAsync(3));

        message.ShouldBe("task\n    should be Some(3)\n    but was\nNone");
    }

    [Fact]
    public async Task
        GivenASomeTask_WhenShouldBeSomeValueAsyncDiffers_ThenReportBothValues()
    {
        Task<Option<int>> task = Task.FromResult(Some());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeSomeValueAsync(4));

        message.ShouldContain("3");
        message.ShouldContain("4");
    }

    [Fact]
    public async Task
        GivenASomeValueTask_WhenShouldBeSomeAsync_ThenReturnTheValue()
    {
        ValueTask<Option<int>> task = new ValueTask<Option<int>>(Some());

        (await task.ShouldBeSomeAsync()).ShouldBe(3);
    }

    [Fact]
    public async Task
        GivenANoneValueTask_WhenShouldBeSomeAsync_ThenMatchTheSyncMessage()
    {
        ValueTask<Option<int>> task = new ValueTask<Option<int>>(None());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeSomeAsync());

        message.ShouldBe("task\n    should be Some\n    but was\nNone");
    }

    [Fact]
    public async Task GivenANoneValueTask_WhenShouldBeNoneAsync_ThenPass()
    {
        ValueTask<Option<int>> task = new ValueTask<Option<int>>(None());

        await task.ShouldBeNoneAsync();
    }

    [Fact]
    public async Task
        GivenASomeValueTask_WhenShouldBeNoneAsync_ThenNameTheValue()
    {
        ValueTask<Option<int>> task = new ValueTask<Option<int>>(Some());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeNoneAsync());

        message.ShouldBe("task\n    should be None\n    but was\nSome(3)");
    }

    [Fact]
    public async Task
        GivenASomeValueTask_WhenShouldBeSomeValueAsyncMatches_ThenReturnTheValue()
    {
        ValueTask<Option<int>> task = new ValueTask<Option<int>>(Some());

        (await task.ShouldBeSomeValueAsync(3)).ShouldBe(3);
    }

    [Fact]
    public async Task
        GivenANoneValueTask_WhenShouldBeSomeValueAsync_ThenNameTheExpected()
    {
        ValueTask<Option<int>> task = new ValueTask<Option<int>>(None());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeSomeValueAsync(3));

        message.ShouldBe("task\n    should be Some(3)\n    but was\nNone");
    }

    [Fact]
    public async Task
        GivenASomeValueTask_WhenShouldBeSomeValueAsyncDiffers_ThenReportBoth()
    {
        ValueTask<Option<int>> task = new ValueTask<Option<int>>(Some());

        string message = await AssertionFailure.FromAsync(
            async () => await task.ShouldBeSomeValueAsync(4));

        message.ShouldContain("3");
        message.ShouldContain("4");
    }
}
