namespace Waystone.Monads.Shouldly.Analyzers;

using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// Every reported case is a position the fix can rewrite without adding parentheses of
/// its own, and every unreported one is a position where moving the <c>await</c>
/// outward would change what is awaited. That split is the whole rule, so the
/// unreported cases carry as much weight here as the reported ones.
/// </remarks>
public class AwaitedAssertionAnalyzerTests
{
    [Theory]
    [InlineData("Task<Option<int>>", "(await task).ShouldBeSome()", "ShouldBeSome")]
    [InlineData("Task<Option<int>>", "(await task).ShouldBeNone()", "ShouldBeNone")]
    [InlineData("Task<Option<int>>", "(await task).ShouldBeSomeValue(3)", "ShouldBeSomeValue")]
    [InlineData("Task<Result<int, string>>", "(await task).ShouldBeOk()", "ShouldBeOk")]
    [InlineData("Task<Result<int, string>>", "(await task).ShouldBeErr()", "ShouldBeErr")]
    [InlineData("Task<Result<int, string>>", "(await task).ShouldBeOkValue(3)", "ShouldBeOkValue")]
    [InlineData("Task<Result<int, string>>", "(await task).ShouldBeErrValue(\"failed\")", "ShouldBeErrValue")]
    [InlineData("ValueTask<Option<int>>", "(await task).ShouldBeSome()", "ShouldBeSome")]
    [InlineData("ValueTask<Result<int, string>>", "(await task).ShouldBeOk()", "ShouldBeOk")]
    public Task GivenAParenthesisedAwait_ThenNameTheAwaitedAssertion(
        string receiver,
        string expression,
        string assertion) =>
        Verify.AnalyzerAsync<AwaitedAssertionAnalyzer>(
            Subject(receiver, expression),
            Verify.Diagnostic(Rules.ParenthesisedAwaitAssertion)
               .WithLocation(0)
               .WithArguments(assertion, assertion + "Async"));

    /// <summary>
    /// An assertion whose value is assigned is reported, because <c>await</c> needs no
    /// parentheses there.
    /// </summary>
    [Fact]
    public Task GivenTheValueIsAssigned_ThenStillReport() =>
        Verify.AnalyzerAsync<AwaitedAssertionAnalyzer>(
            """
            private async Task Check(Task<Option<int>> task)
            {
                int value = {|#0:(await task).ShouldBeSome()|};

                value.ShouldBe(3);
            }
            """,
            Verify.Diagnostic(Rules.ParenthesisedAwaitAssertion)
               .WithLocation(0)
               .WithArguments("ShouldBeSome", "ShouldBeSomeAsync"));

    /// <summary>
    /// A chained assertion is deliberately not reported.
    /// </summary>
    /// <remarks>
    /// Member access binds tighter than <c>await</c>, so the rewrite would read the
    /// member off the task rather than off the asserted value and would not compile.
    /// Reporting a case the fix cannot take is worse than staying quiet, because the
    /// consumer sees a suggestion with no way to apply it.
    /// </remarks>
    [Fact]
    public Task GivenTheAssertedValueIsChained_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<AwaitedAssertionAnalyzer>(
            """
            private async Task Check(Task<Option<int>> task)
            {
                (await task).ShouldBeSome().ShouldBe(3);
            }
            """);

    /// <summary>
    /// A configured await is deliberately not reported.
    /// </summary>
    /// <remarks>
    /// <c>ConfigureAwait</c> returns an awaitable that is not a task, and no assertion
    /// is declared on it — so moving the <c>await</c> outward would leave the
    /// replacement called on a receiver that does not have it. This is the shape the
    /// library's own style produces everywhere, which is exactly why it is pinned.
    /// </remarks>
    [Fact]
    public Task GivenAConfiguredAwait_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<AwaitedAssertionAnalyzer>(
            """
            private async Task Check(Task<Option<int>> task)
            {
                (await task.ConfigureAwait(false)).ShouldBeSome();
            }
            """);

    [Fact]
    public Task GivenAConcreteTypeAssertion_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<AwaitedAssertionAnalyzer>(
            """
            private async Task Check(Task<Option<int>> task)
            {
                (await task).ShouldBeOfType<Some<int>>();
            }
            """);

    [Fact]
    public Task GivenATaskOfSomethingElse_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<AwaitedAssertionAnalyzer>(
            """
            private async Task Check(Task<int> task)
            {
                (await task).ShouldBe(3);
            }
            """);

    [Fact]
    public Task GivenTheAwaitedFormIsAlreadyUsed_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<AwaitedAssertionAnalyzer>(
            """
            private async Task Check(Task<Option<int>> task)
            {
                await task.ShouldBeSomeAsync();
            }
            """);

    [Fact]
    public Task GivenASynchronousReceiver_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<AwaitedAssertionAnalyzer>(
            """
            private void Check(Option<int> option)
            {
                option.ShouldBeSome();
            }
            """);

    [Fact]
    public Task GivenTheAssertionsPackageIsAbsent_ThenReportNothing() =>
        Verify.WithoutAssertionsAsync<AwaitedAssertionAnalyzer>(
            """
            private async Task Check(Task<Option<int>> task)
            {
                (await task).ShouldBe(Option.Some(3));
            }
            """);

    private static string Subject(string receiver, string expression) =>
        $$"""
          private async Task Check({{receiver}} task)
          {
              {|#0:{{expression}}|};
          }
          """;
}
