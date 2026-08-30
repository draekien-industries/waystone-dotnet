namespace Waystone.Monads.Shouldly.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class UseAwaitedAssertionCodeFixTests
{
    [Theory]
    [InlineData("Task<Option<int>>", "(await task).ShouldBeSome()", "await task.ShouldBeSomeAsync()", "ShouldBeSome")]
    [InlineData("Task<Option<int>>", "(await task).ShouldBeNone()", "await task.ShouldBeNoneAsync()", "ShouldBeNone")]
    [InlineData("Task<Option<int>>", "(await task).ShouldBeSomeValue(3)", "await task.ShouldBeSomeValueAsync(3)", "ShouldBeSomeValue")]
    [InlineData("Task<Result<int, string>>", "(await task).ShouldBeOk()", "await task.ShouldBeOkAsync()", "ShouldBeOk")]
    [InlineData("Task<Result<int, string>>", "(await task).ShouldBeErr()", "await task.ShouldBeErrAsync()", "ShouldBeErr")]
    [InlineData("Task<Result<int, string>>", "(await task).ShouldBeOkValue(3)", "await task.ShouldBeOkValueAsync(3)", "ShouldBeOkValue")]
    [InlineData("Task<Result<int, string>>", "(await task).ShouldBeErrValue(\"failed\")", "await task.ShouldBeErrValueAsync(\"failed\")", "ShouldBeErrValue")]
    [InlineData("ValueTask<Option<int>>", "(await task).ShouldBeSome()", "await task.ShouldBeSomeAsync()", "ShouldBeSome")]
    [InlineData("ValueTask<Result<int, string>>", "(await task).ShouldBeOk()", "await task.ShouldBeOkAsync()", "ShouldBeOk")]
    public Task GivenAParenthesisedAwait_ThenMoveTheAwaitOutward(
        string receiver,
        string before,
        string after,
        string assertion) =>
        Verify.CodeFixAsync<AwaitedAssertionAnalyzer,
            UseAwaitedAssertionCodeFix>(
            Subject(receiver, "{|#0:" + before + "|}"),
            Subject(receiver, after),
            Verify.Diagnostic(Rules.ParenthesisedAwaitAssertion)
               .WithLocation(0)
               .WithArguments(assertion, assertion + "Async"));

    /// <summary>
    /// A custom message transfers with the rest of the argument list.
    /// </summary>
    [Fact]
    public Task GivenACustomMessage_ThenCarryItOver() =>
        Verify.CodeFixAsync<AwaitedAssertionAnalyzer,
            UseAwaitedAssertionCodeFix>(
            Subject(
                "Task<Option<int>>",
                "{|#0:(await task).ShouldBeSome(\"while loading\")|}"),
            Subject(
                "Task<Option<int>>",
                "await task.ShouldBeSomeAsync(\"while loading\")"),
            Verify.Diagnostic(Rules.ParenthesisedAwaitAssertion)
               .WithLocation(0)
               .WithArguments("ShouldBeSome", "ShouldBeSomeAsync"));

    /// <summary>
    /// An assigned assertion keeps its value through the rewrite.
    /// </summary>
    /// <remarks>
    /// The awaited assertions return the unwrapped value just as the synchronous ones
    /// do, so the declaration's type is unchanged. This is the second of the two
    /// positions the rule reports, and the only one where the rewritten expression is
    /// read rather than discarded.
    /// </remarks>
    [Fact]
    public Task GivenTheValueIsAssigned_ThenKeepIt() =>
        Verify.CodeFixAsync<AwaitedAssertionAnalyzer,
            UseAwaitedAssertionCodeFix>(
            """
            private async Task Check(Task<Option<int>> task)
            {
                int value = {|#0:(await task).ShouldBeSome()|};

                value.ShouldBe(3);
            }
            """,
            """
            private async Task Check(Task<Option<int>> task)
            {
                int value = await task.ShouldBeSomeAsync();

                value.ShouldBe(3);
            }
            """,
            Verify.Diagnostic(Rules.ParenthesisedAwaitAssertion)
               .WithLocation(0)
               .WithArguments("ShouldBeSome", "ShouldBeSomeAsync"));

    [Fact]
    public Task GivenSeveralAwaitsInOneFile_ThenFixThemTogether() =>
        Verify.FixAllAsync<AwaitedAssertionAnalyzer,
            UseAwaitedAssertionCodeFix>(
            """
            private async Task Check(
                Task<Option<int>> option,
                ValueTask<Result<int, string>> result)
            {
                {|#0:(await option).ShouldBeSomeValue(3)|};
                {|#1:(await result).ShouldBeErrValue("failed")|};
            }
            """,
            """
            private async Task Check(
                Task<Option<int>> option,
                ValueTask<Result<int, string>> result)
            {
                await option.ShouldBeSomeValueAsync(3);
                await result.ShouldBeErrValueAsync("failed");
            }
            """,
            Verify.Diagnostic(Rules.ParenthesisedAwaitAssertion)
               .WithLocation(0)
               .WithArguments("ShouldBeSomeValue", "ShouldBeSomeValueAsync"),
            Verify.Diagnostic(Rules.ParenthesisedAwaitAssertion)
               .WithLocation(1)
               .WithArguments("ShouldBeErrValue", "ShouldBeErrValueAsync"));

    private static string Subject(string receiver, string expression) =>
        $$"""
          private async Task Check({{receiver}} task)
          {
              {{expression}};
          }
          """;
}
