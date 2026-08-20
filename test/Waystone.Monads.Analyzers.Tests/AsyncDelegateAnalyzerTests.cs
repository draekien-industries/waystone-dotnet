namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class AsyncDelegateAnalyzerTests
{
    [Fact]
    public Task FlagsAnAsyncFactoryPassedToOptionTry() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<Task<int>> Make() =>
                Option.{|#0:Try|}(() => Task.FromResult(42));
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments("Try", "Option<Task<int>>", "TryAsync"));

    [Fact]
    public Task FlagsAnAsyncFactoryPassedToResultTry() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Result<Task<int>, string> Make() =>
                Result.{|#0:Try|}(
                    () => Task.FromResult(42),
                    error => error.Message);
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments("Try", "Result<Task<int>, string>", "TryAsync"));

    [Fact]
    public Task FlagsAnAsyncFactoryPassedToTheErrorReturningResultTry() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Result<Task<int>, Error> Make() =>
                Result.{|#0:Try|}(() => Task.FromResult(42));
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments("Try", "Result<Task<int>, Error>", "TryAsync"));

    [Fact]
    public Task FlagsAValueTaskFactory() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<ValueTask<int>> Make() =>
                Option.{|#0:Try|}(() => new ValueTask<int>(42));
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments("Try", "Option<ValueTask<int>>", "TryAsync"));

    [Fact]
    public Task FlagsAnAsyncMethodGroup() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Task<int> LoadAsync() => Task.FromResult(42);

            internal Option<Task<int>> Make() => Option.{|#0:Try|}(LoadAsync);
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments("Try", "Option<Task<int>>", "TryAsync"));

    [Fact]
    public Task FlagsAnAsyncDelegatePassedToMap() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<Task<int>> Doubled(Option<int> option) =>
                option.{|#0:Map|}(value => Task.FromResult(value * 2));
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments("Map", "Option<Task<int>>", "MapAsync"));

    [Fact]
    public Task FlagsAnAsyncDelegatePassedToMapErr() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Result<int, Task<string>> Tag(Result<int, string> result) =>
                result.{|#0:MapErr|}(error => Task.FromResult(error));
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments(
                    "MapErr",
                    "Result<int, Task<string>>",
                    "MapErrAsync"));

    [Fact]
    public Task FlagsAnAsyncDelegatePassedToTheStateOverloadOfMap() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<Task<int>> Shift(Option<int> option, int offset) =>
                option.{|#0:Map|}(
                    offset,
                    static (value, state) => Task.FromResult(value + state));
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments("Map", "Option<Task<int>>", "MapAsync"));

    /// <remarks>
    /// Match hands the task straight back rather than trapping it in a monad,
    /// so <c>await option.Match(...)</c> works and nothing is lost. The rule
    /// tests where the awaitable ends up, not whether a delegate produced one.
    /// </remarks>
    [Fact]
    public Task IgnoresMatchWhichHandsTheTaskBack() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
            """
            internal Task<int> Read(Option<int> option) =>
                option.Match(
                    value => Task.FromResult(value),
                    () => Task.FromResult(0));
            """);

    [Fact]
    public Task IgnoresMapOrWhichHandsTheTaskBack() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
            """
            internal Task<int> Read(Option<int> option) =>
                option.MapOr(
                    Task.FromResult(0),
                    value => Task.FromResult(value));
            """);

    /// <remarks>
    /// A task passed as a value rather than produced by a delegate is the
    /// caller doing it on purpose. Requiring a delegate parameter is what keeps
    /// the rule off it.
    /// </remarks>
    [Fact]
    public Task IgnoresATaskHandedStraightToSome() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<Task<int>> Make() =>
                Option.Some(Task.FromResult(42));
            """);

    [Fact]
    public Task IgnoresASynchronousFactory() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<int> Make() => Option.Try(() => 42);

            internal Result<int, string> Fallible() =>
                Result.Try(() => 42, error => error.Message);
            """);

    [Fact]
    public Task IgnoresASynchronousDelegatePassedToMap() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.Map(value => value * 2);
            """);

    [Fact]
    public Task IgnoresTryAsync() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
            """
            internal Task<Option<int>> Make() =>
                Option.TryAsync(() => Task.FromResult(42));

            internal Task<Result<int, string>> Fallible() =>
                Result.TryAsync(
                    () => Task.FromResult(42),
                    error => error.Message);
            """);

    [Fact]
    public Task IgnoresMapAsync() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
            """
            internal ValueTask<Option<int>> Doubled(Option<int> option) =>
                option.MapAsync(value => Task.FromResult(value * 2));
            """);

    [Fact]
    public Task IgnoresAnUnrelatedTryMethod() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
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

    /// <remarks>
    /// The state overloads put <c>TState</c> at type argument zero, so a rule
    /// that read the method's first type argument would test the state rather
    /// than the value. These two pin both halves of that: a state that happens
    /// to be a task is not the hazard, and a state factory that returns one
    /// still is.
    /// </remarks>
    [Fact]
    public Task IgnoresATaskPassedAsState() =>
        Verify.NoDiagnosticAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<int> Make(Task<int> pending) =>
                Option.Try(pending, static task => task.Result);
            """);

    [Fact]
    public Task FlagsAStateFactoryThatReturnsATask() =>
        Verify.AnalyzerAsync<AsyncDelegateAnalyzer>(
            """
            internal Option<Task<int>> Make(int seed) =>
                Option.{|#0:Try|}(seed, static value => Task.FromResult(value));
            """,
            Verify.Diagnostic(Rules.AsyncDelegatePassedToSyncMethod)
               .WithLocation(0)
               .WithArguments("Try", "Option<Task<int>>", "TryAsync"));
}
