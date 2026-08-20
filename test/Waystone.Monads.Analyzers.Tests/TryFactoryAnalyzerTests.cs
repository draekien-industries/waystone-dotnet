namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class TryFactoryAnalyzerTests
{
    [Fact]
    public Task FlagsAnAsyncFactoryPassedToOptionTry() =>
        Verify.AnalyzerAsync<TryFactoryAnalyzer>(
            """
            internal Option<Task<int>> Make() =>
                Option.{|#0:Try|}(() => Task.FromResult(42));
            """,
            Verify.Diagnostic(Rules.AsyncFactoryPassedToTry)
               .WithLocation(0)
               .WithArguments("Task<int>"));

    [Fact]
    public Task FlagsAnAsyncFactoryPassedToResultTry() =>
        Verify.AnalyzerAsync<TryFactoryAnalyzer>(
            """
            internal Result<Task<int>, string> Make() =>
                Result.{|#0:Try|}(
                    () => Task.FromResult(42),
                    error => error.Message);
            """,
            Verify.Diagnostic(Rules.AsyncFactoryPassedToTry)
               .WithLocation(0)
               .WithArguments("Task<int>"));

    [Fact]
    public Task FlagsAnAsyncFactoryPassedToTheErrorReturningResultTry() =>
        Verify.AnalyzerAsync<TryFactoryAnalyzer>(
            """
            internal Result<Task<int>, Error> Make() =>
                Result.{|#0:Try|}(() => Task.FromResult(42));
            """,
            Verify.Diagnostic(Rules.AsyncFactoryPassedToTry)
               .WithLocation(0)
               .WithArguments("Task<int>"));

    [Fact]
    public Task FlagsAValueTaskFactory() =>
        Verify.AnalyzerAsync<TryFactoryAnalyzer>(
            """
            internal Option<ValueTask<int>> Make() =>
                Option.{|#0:Try|}(() => new ValueTask<int>(42));
            """,
            Verify.Diagnostic(Rules.AsyncFactoryPassedToTry)
               .WithLocation(0)
               .WithArguments("ValueTask<int>"));

    [Fact]
    public Task FlagsAnAsyncMethodGroup() =>
        Verify.AnalyzerAsync<TryFactoryAnalyzer>(
            """
            internal Task<int> LoadAsync() => Task.FromResult(42);

            internal Option<Task<int>> Make() => Option.{|#0:Try|}(LoadAsync);
            """,
            Verify.Diagnostic(Rules.AsyncFactoryPassedToTry)
               .WithLocation(0)
               .WithArguments("Task<int>"));

    [Fact]
    public Task IgnoresASynchronousFactory() =>
        Verify.NoDiagnosticAsync<TryFactoryAnalyzer>(
            """
            internal Option<int> Make() => Option.Try(() => 42);

            internal Result<int, string> Fallible() =>
                Result.Try(() => 42, error => error.Message);
            """);

    [Fact]
    public Task IgnoresTryAsync() =>
        Verify.NoDiagnosticAsync<TryFactoryAnalyzer>(
            """
            internal Task<Option<int>> Make() =>
                Option.TryAsync(() => Task.FromResult(42));

            internal Task<Result<int, string>> Fallible() =>
                Result.TryAsync(
                    () => Task.FromResult(42),
                    error => error.Message);
            """);

    [Fact]
    public Task IgnoresAnUnrelatedTryMethod() =>
        Verify.NoDiagnosticAsync<TryFactoryAnalyzer>(
            """
            internal static class Attempt
            {
                internal static T Try<T>(Func<T> factory) => factory();
            }

            internal class Subject
            {
                internal Task<int> Make() =>
                    Attempt.Try(() => Task.FromResult(42));
            }
            """);
}
