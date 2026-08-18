namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class ThrowAnalyzerTests
{
    [Fact]
    public Task FlagsAThrowInAResultReturningMember() =>
        Verify.AnalyzerAsync<ThrowAnalyzer>(
            """
            internal Result<int, string> Parse(string text)
            {
                {|#0:throw new InvalidOperationException(text);|}
            }
            """,
            Verify.Diagnostic(Rules.ThrowInResultMember)
               .WithLocation(0)
               .WithArguments("Parse", "Result<int, string>"));

    [Fact]
    public Task FlagsAThrowInAnAsyncResultReturningMember() =>
        Verify.AnalyzerAsync<ThrowAnalyzer>(
            """
            internal async Task<Result<int, string>> ParseAsync(string text)
            {
                await Task.Yield();
                {|#0:throw new InvalidOperationException(text);|}
            }
            """,
            Verify.Diagnostic(Rules.ThrowInResultMember)
               .WithLocation(0)
               .WithArguments("ParseAsync", "Result<int, string>"));

    [Fact]
    public Task IgnoresArgumentValidation() =>
        Verify.NoDiagnosticAsync<ThrowAnalyzer>(
            """
            internal Result<int, string> Parse(string text)
            {
                if (text is null)
                {
                    throw new ArgumentNullException(nameof(text));
                }

                return Result.Ok<int, string>(1);
            }
            """);

    [Fact]
    public Task IgnoresARethrow() =>
        Verify.NoDiagnosticAsync<ThrowAnalyzer>(
            """
            internal Result<int, string> Parse(string text)
            {
                try
                {
                    return Result.Ok<int, string>(1);
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
            }
            """);

    [Fact]
    public Task IgnoresAThrowInsideATryFactory() =>
        Verify.NoDiagnosticAsync<ThrowAnalyzer>(
            """
            internal Result<int, Error> Parse(string text) =>
                Result.Try<int>(() => throw new InvalidOperationException(text));
            """);

    [Fact]
    public Task FlagsAThrowInAnOrdinaryMemberUnderTheMigrationRule() =>
        Verify.AnalyzerAsync<ThrowAnalyzer>(
            """
            internal int Parse(string text)
            {
                {|#0:throw new InvalidOperationException(text);|}
            }
            """,
            Verify.Diagnostic(Rules.ThrowCouldBeResult)
               .WithLocation(0)
               .WithArguments("Parse"));
}
