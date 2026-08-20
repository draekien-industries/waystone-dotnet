namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

public class AsTaskCodeFixTests
{
    [Fact]
    public Task FixesAValueTaskAssignedToATaskLocal() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task<Option<int>> Assign(Option<int> option)
            {
                Task<Option<int>> mapped =
                    {|#0:option.MapAsync(value => Task.FromResult(value + 1))|};

                return mapped;
            }
            """,
            """
            internal Task<Option<int>> Assign(Option<int> option)
            {
                Task<Option<int>> mapped =
                    option.MapAsync(value => Task.FromResult(value + 1)).AsTask();

                return mapped;
            }
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    [Fact]
    public Task FixesAValueTaskPassedToATaskParameter() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task<Option<int>> Pass(Option<int> option) =>
                Accept({|#0:option.MapAsync(value => Task.FromResult(value + 1))|});

            private static Task<Option<int>> Accept(Task<Option<int>> task) =>
                task;
            """,
            """
            internal Task<Option<int>> Pass(Option<int> option) =>
                Accept(option.MapAsync(value => Task.FromResult(value + 1)).AsTask());

            private static Task<Option<int>> Accept(Task<Option<int>> task) =>
                task;
            """,
            DiagnosticResult.CompilerError("CS1503").WithLocation(0));

    [Fact]
    public Task FixesAValueTaskInsideTaskWhenAll() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task FanOut(Option<int> first, Option<int> second) =>
                Task.WhenAll(
                    {|#0:first.MapAsync(value => Task.FromResult(value + 1))|},
                    {|#1:second.MapAsync(value => Task.FromResult(value + 1))|});
            """,
            """
            internal Task FanOut(Option<int> first, Option<int> second) =>
                Task.WhenAll(
                    first.MapAsync(value => Task.FromResult(value + 1)).AsTask(),
                    second.MapAsync(value => Task.FromResult(value + 1)).AsTask());
            """,
            DiagnosticResult.CompilerError("CS1503").WithLocation(0),
            DiagnosticResult.CompilerError("CS1503").WithLocation(1));

    [Fact]
    public Task FixesANonGenericValueTaskAssignedToATask() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task Run(Result<int, string> result)
            {
                Task matched = {|#0:result.MatchAsync(
                    ok => Task.CompletedTask,
                    err => Task.CompletedTask)|};

                return matched;
            }
            """,
            """
            internal Task Run(Result<int, string> result)
            {
                Task matched = result.MatchAsync(
                    ok => Task.CompletedTask,
                    err => Task.CompletedTask).AsTask();

                return matched;
            }
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    [Fact]
    public Task LeavesAnUnrelatedConversionErrorAlone() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            "internal string Wrong() => {|#0:1|};",
            "internal string Wrong() => {|#0:1|};",
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    [Fact]
    public Task LeavesANonValueTaskSourceAlone() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            "internal Task<int> NotATask(Option<int> option) => {|#0:option|};",
            "internal Task<int> NotATask(Option<int> option) => {|#0:option|};",
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    [Fact]
    public Task LeavesAMismatchedTypeArgumentAlone() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task<string> Mismatched(Option<int> option) =>
                Accept({|#0:option.MapAsync(value => Task.FromResult(value + 1))|});

            private static Task<string> Accept(Task<string> task) => task;
            """,
            """
            internal Task<string> Mismatched(Option<int> option) =>
                Accept({|#0:option.MapAsync(value => Task.FromResult(value + 1))|});

            private static Task<string> Accept(Task<string> task) => task;
            """,
            DiagnosticResult.CompilerError("CS1503").WithLocation(0));
}
