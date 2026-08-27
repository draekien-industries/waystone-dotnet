namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public sealed class AsyncStepAnalyzerTests
{
    private const string ResultStep = """
                                      private static Task<Result<int, Error>> StepAsync(int value) =>
                                          Task.FromResult(Result.Ok<int, Error>(value));
                                      """;

    private const string OptionStep = """
                                      private static Task<Option<int>> StepAsync(int value) =>
                                          Task.FromResult(Option.Some(value));
                                      """;

    [Fact]
    public Task ReportsAStepOnAResult() =>
        Reported(
            $$"""
              {{ResultStep}}

              internal static ValueTask<Result<int, Error>> Chain(
                  Result<int, Error> result) =>
                  result.AndThenAsync({|#0:StepAsync|});
              """,
            "Task<Result<int, Error>>",
            "AndThenAsync",
            "ValueTask<Result<int, Error>>");

    [Fact]
    public Task ReportsAStepOnAnOption() =>
        Reported(
            $$"""
              {{OptionStep}}

              internal static ValueTask<Option<int>> Chain(Option<int> option) =>
                  option.AndThenAsync({|#0:StepAsync|});
              """,
            "Task<Option<int>>",
            "AndThenAsync",
            "ValueTask<Option<int>>");

    /// <summary>
    /// The shape the previous-major sample actually breaks on, where the receiver is
    /// an awaited chain rather than a monad, so the member comes from the generated
    /// awaited-receiver extension and its first parameter is the receiver.
    /// </summary>
    [Fact]
    public Task ReportsAStepOnAnAwaitedReceiver() =>
        Reported(
            $$"""
              {{ResultStep}}

              private static ValueTask<Result<int, Error>> FetchAsync() =>
                  new ValueTask<Result<int, Error>>(Result.Ok<int, Error>(1));

              internal static ValueTask<Result<int, Error>> Chain() =>
                  FetchAsync().AndThenAsync({|#0:StepAsync|});
              """,
            "Task<Result<int, Error>>",
            "AndThenAsync",
            "ValueTask<Result<int, Error>>");

    [Fact]
    public Task ReportsAStepOnOrElseAsync() =>
        Reported(
            """
            private static Task<Result<int, string>> RecoverAsync(Error error) =>
                Task.FromResult(Result.Ok<int, string>(0));

            internal static ValueTask<Result<int, string>> Chain(
                Result<int, Error> result) =>
                result.OrElseAsync({|#0:RecoverAsync|});
            """,
            returned: "Task<Result<int, string>>",
            step: "OrElseAsync",
            wanted: "ValueTask<Result<int, string>>",
            group: "RecoverAsync",
            parameter: "error");

    /// <summary>
    /// The correction the message names, which has to stay silent or the rule reports
    /// on the code it just told the caller to write.
    /// </summary>
    [Fact]
    public Task LeavesALambdaAlone() =>
        NoDiagnostic(
            $$"""
              {{ResultStep}}

              internal static ValueTask<Result<int, Error>> Chain(
                  Result<int, Error> result) =>
                  result.AndThenAsync(async value => await StepAsync(value));
              """);

    [Fact]
    public Task LeavesABindingCallAlone() =>
        Verify.BrokenAnalyzerAsync<AsyncStepAnalyzer>(
            """
            private static ValueTask<Result<int, Error>> StepAsync(int value) =>
                new ValueTask<Result<int, Error>>(Result.Ok<int, Error>(value));

            internal static ValueTask<Result<int, Error>> Chain(
                Result<int, Error> result) =>
                result.AndThenAsync(StepAsync);
            """);

    /// <summary>
    /// A step returning a task of something that is not a monad, which cannot be the
    /// break this rule describes however the call fails.
    /// </summary>
    [Fact]
    public Task LeavesATaskOfANonMonadAlone() =>
        NoDiagnostic(
            """
            private static Task<int> StepAsync(int value) => Task.FromResult(value);

            internal static ValueTask<Result<int, Error>> Chain(
                Result<int, Error> result) =>
                result.AndThenAsync(StepAsync);
            """);

    [Fact]
    public Task LeavesANonStepMemberAlone() =>
        NoDiagnostic(
            $$"""
              {{ResultStep}}

              internal static ValueTask<Result<Option<int>, Error>> Chain(
                  Result<int, Error> result) =>
                  result.MapAsync(StepAsync);
              """);

    /// <summary>
    /// Somebody else's <c>AndThenAsync</c>, declared with no parameters so that the
    /// receiver clause of the gate has an empty parameter list to read.
    /// </summary>
    [Fact]
    public Task LeavesAnUnrelatedAndThenAsyncAlone() =>
        NoDiagnostic(
            $$"""
              {{ResultStep}}

              private static void AndThenAsync() { }

              internal static void Chain() => AndThenAsync(StepAsync);
              """);

    /// <summary>
    /// A lambda in a call that does <em>not</em> bind, which is the only way to reach
    /// the syntax gate at all — the well-formed lambda above binds, so the rule
    /// returns before ever looking at its argument.
    /// </summary>
    [Fact]
    public Task LeavesALambdaInABrokenCallAlone() =>
        NoDiagnostic(
            $$"""
              {{ResultStep}}

              internal static ValueTask<Result<int, Error>> Chain(
                  Result<int, Error> result) =>
                  result.AndThenAsync(
                      async (int value, int other) => await StepAsync(value));
              """);

    /// <summary>
    /// An invocation whose callee is itself an invocation, so there is no name to read
    /// off the syntax at all. The hot-path filter has to survive one.
    /// </summary>
    [Fact]
    public Task LeavesAnInvocationWithNoNameAlone() =>
        NoDiagnostic(
            """
            internal static int Chain(Func<Func<int>> outer) => outer()();
            """);

    /// <summary>
    /// A step taking two arguments, which no chaining delegate accepts, so returning
    /// a <c>ValueTask</c> would not make the call bind and the message's advice would
    /// be wrong.
    /// </summary>
    [Fact]
    public Task LeavesAStepTakingTwoArgumentsAlone() =>
        NoDiagnostic(
            """
            private static Task<Result<int, Error>> StepAsync(int value, int other) =>
                Task.FromResult(Result.Ok<int, Error>(value + other));

            internal static ValueTask<Result<int, Error>> Chain(
                Result<int, Error> result) =>
                result.AndThenAsync(StepAsync);
            """);

    private static Task Reported(
        string source,
        string returned,
        string step,
        string wanted,
        string group = "StepAsync",
        string parameter = "value") =>
        Verify.BrokenAnalyzerAsync<AsyncStepAnalyzer>(
            source,
            Verify.Diagnostic(Rules.TaskReturningAsyncStep)
               .WithLocation(0)
               .WithArguments(
                   group,
                   returned,
                   step,
                   wanted,
                   $"async {parameter} => await {group}({parameter})"));

    private static Task NoDiagnostic(string source) =>
        Verify.BrokenAnalyzerAsync<AsyncStepAnalyzer>(source);
}
