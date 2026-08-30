namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

public sealed class WrapAsyncStepCodeFixTests
{
    private const string ResultStep = """
                                      private static Task<Result<int, Error>> StepAsync(int value) =>
                                          Task.FromResult(Result.Ok<int, Error>(value));
                                      """;

    [Fact]
    public Task WrapsAStepInAnAsyncLambda() =>
        Fixed(
            $$"""
              {{ResultStep}}

              internal static ValueTask<Result<int, Error>> Chain(
                  Result<int, Error> result) =>
                  result.AndThenAsync({|#0:StepAsync|});
              """,
            $$"""
              {{ResultStep}}

              internal static ValueTask<Result<int, Error>> Chain(
                  Result<int, Error> result) =>
                  result.AndThenAsync(async value => await StepAsync(value));
              """);

    /// <summary>
    /// The lambda's parameter is named from the step's own, so a step that spells its
    /// parameter differently produces a lambda that reads like the method it calls.
    /// </summary>
    [Fact]
    public Task NamesTheLambdaParameterAfterTheStep() =>
        Fixed(
            """
            private static Task<Option<int>> PriceAsync(int order) =>
                Task.FromResult(Option.Some(order));

            internal static ValueTask<Option<int>> Chain(Option<int> option) =>
                option.AndThenAsync({|#0:PriceAsync|});
            """,
            """
            private static Task<Option<int>> PriceAsync(int order) =>
                Task.FromResult(Option.Some(order));

            internal static ValueTask<Option<int>> Chain(Option<int> option) =>
                option.AndThenAsync(async order => await PriceAsync(order));
            """,
            group: "PriceAsync",
            returned: "Task<Option<int>>",
            step: "AndThenAsync",
            wanted: "ValueTask<Option<int>>",
            parameter: "order");

    /// <summary>
    /// <c>Option.OrElseAsync</c> takes a step with no parameter, which is the only
    /// shape that needs a parenthesized lambda rather than a simple one.
    /// </summary>
    [Fact]
    public Task WrapsAStepThatTakesNoArgument() =>
        Fixed(
            """
            private static Task<Option<int>> RecoverAsync() =>
                Task.FromResult(Option.Some(0));

            internal static ValueTask<Option<int>> Chain(Option<int> option) =>
                option.OrElseAsync({|#0:RecoverAsync|});
            """,
            """
            private static Task<Option<int>> RecoverAsync() =>
                Task.FromResult(Option.Some(0));

            internal static ValueTask<Option<int>> Chain(Option<int> option) =>
                option.OrElseAsync(async () => await RecoverAsync());
            """,
            group: "RecoverAsync",
            returned: "Task<Option<int>>",
            step: "OrElseAsync",
            wanted: "ValueTask<Option<int>>",
            parameter: "");

    [Fact]
    public Task WrapsAStepOnAnAwaitedReceiver() =>
        Fixed(
            $$"""
              {{ResultStep}}

              private static ValueTask<Result<int, Error>> FetchAsync() =>
                  new ValueTask<Result<int, Error>>(Result.Ok<int, Error>(1));

              internal static ValueTask<Result<int, Error>> Chain() =>
                  FetchAsync().AndThenAsync({|#0:StepAsync|});
              """,
            $$"""
              {{ResultStep}}

              private static ValueTask<Result<int, Error>> FetchAsync() =>
                  new ValueTask<Result<int, Error>>(Result.Ok<int, Error>(1));

              internal static ValueTask<Result<int, Error>> Chain() =>
                  FetchAsync().AndThenAsync(async value => await StepAsync(value));
              """);

    /// <summary>
    /// An overloaded step, where the lambda's parameter name is not determined by the
    /// group alone.
    /// </summary>
    [Fact]
    public Task LeavesAnOverloadedStepAlone() =>
        Unfixed(
            """
            private static Task<Result<int, Error>> StepAsync(int value) =>
                Task.FromResult(Result.Ok<int, Error>(value));

            private static Task<Result<int, Error>> StepAsync(string text) =>
                Task.FromResult(Result.Ok<int, Error>(0));

            internal static ValueTask<Result<int, Error>> Chain(
                Result<int, Error> result) =>
                result.AndThenAsync({|#0:StepAsync|});
            """);

    private static Task Fixed(
        string source,
        string fixedSource,
        string group = "StepAsync",
        string returned = "Task<Result<int, Error>>",
        string step = "AndThenAsync",
        string wanted = "ValueTask<Result<int, Error>>",
        string parameter = "value") =>
        Verify.BrokenCodeFixAsync<AsyncStepAnalyzer, WrapAsyncStepCodeFix>(
            source,
            fixedSource,
            Reported(group, returned, step, wanted, parameter));

    private static Task Unfixed(string source) =>
        Verify.BrokenCodeFixAsync<AsyncStepAnalyzer, WrapAsyncStepCodeFix>(
            source,
            source,
            Reported(
                "StepAsync",
                "Task<Result<int, Error>>",
                "AndThenAsync",
                "ValueTask<Result<int, Error>>",
                "value"));

    private static DiagnosticResult Reported(
        string group,
        string returned,
        string step,
        string wanted,
        string parameter) =>
        Verify.Diagnostic(Rules.TaskReturningAsyncStep)
           .WithLocation(0)
           .WithArguments(
                group,
                returned,
                step,
                wanted,
                parameter.Length == 0
                    ? $"async () => await {group}()"
                    : $"async {parameter} => await {group}({parameter})");
}
