namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Testing;
using Shouldly;
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

    [Fact]
    public Task FixesAValueTaskHeldInALocal() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task<int> FromLocal()
            {
                ValueTask<int> pending = new ValueTask<int>(1);

                Task<int> task = {|#0:pending|};

                return task;
            }
            """,
            """
            internal Task<int> FromLocal()
            {
                ValueTask<int> pending = new ValueTask<int>(1);

                Task<int> task = pending.AsTask();

                return task;
            }
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    [Fact]
    public Task FixesAValueTaskReadFromAField() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal sealed class Holder
            {
                internal ValueTask<int> Pending;
            }

            internal class Subject
            {
                internal Task<int> FromField(Holder holder) =>
                    {|#0:holder.Pending|};
            }
            """,
            """
            internal sealed class Holder
            {
                internal ValueTask<int> Pending;
            }

            internal class Subject
            {
                internal Task<int> FromField(Holder holder) =>
                    holder.Pending.AsTask();
            }
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    [Fact]
    public Task FixesAValueTaskReadFromAnArray() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task<int> FromArray(ValueTask<int>[] pending) =>
                {|#0:pending[0]|};
            """,
            """
            internal Task<int> FromArray(ValueTask<int>[] pending) =>
                pending[0].AsTask();
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    /// <remarks>
    /// The receiver is parenthesised only where appending <c>.AsTask()</c>
    /// would otherwise bind to part of the expression. A conditional binds
    /// looser than member access, so it needs the parentheses that an
    /// identifier or an element access does not.
    /// </remarks>
    [Fact]
    public Task WrapsAConditionalValueTaskInParentheses() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task<int> Either(
                bool flag,
                ValueTask<int> first,
                ValueTask<int> second) =>
                {|#0:flag ? first : second|};
            """,
            """
            internal Task<int> Either(
                bool flag,
                ValueTask<int> first,
                ValueTask<int> second) =>
                (flag ? first : second).AsTask();
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    /// <remarks>
    /// A named argument reaches the parameter by name rather than by position,
    /// so the target type cannot be read from the index. Reordering the call
    /// puts the ValueTask at an index whose parameter is not a Task, which
    /// fails the fix if the name is ignored.
    /// </remarks>
    [Fact]
    public Task FixesAValueTaskPassedByName() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task<int> ByName(ValueTask<int> pending) =>
                Accept(task: {|#0:pending|}, label: "x");

            private static Task<int> Accept(string label, Task<int> task) =>
                task;
            """,
            """
            internal Task<int> ByName(ValueTask<int> pending) =>
                Accept(task: pending.AsTask(), label: "x");

            private static Task<int> Accept(string label, Task<int> task) =>
                task;
            """,
            DiagnosticResult.CompilerError("CS1503").WithLocation(0));

    /// <remarks>
    /// The name alone is not the test. A type called ValueTask outside
    /// System.Threading.Tasks has no AsTask, so a fix keyed on the name would
    /// produce code that does not compile.
    /// </remarks>
    [Fact]
    public Task LeavesAForeignValueTaskAlone() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal struct ValueTask<T>
            {
            }

            internal class Subject
            {
                internal Task<int> Foreign(ValueTask<int> pending) =>
                    {|#0:pending|};
            }
            """,
            """
            internal struct ValueTask<T>
            {
            }

            internal class Subject
            {
                internal Task<int> Foreign(ValueTask<int> pending) =>
                    {|#0:pending|};
            }
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));

    [Fact]
    public void OffersTheBatchFixAllProvider() =>
        new AsTaskCodeFix().GetFixAllProvider()
                          .ShouldBe(WellKnownFixAllProviders.BatchFixer);

    /// <remarks>
    /// The mismatch case reached through an assignment rather than an
    /// argument. The converted type is present and does not bridge, so the
    /// fixer falls through to the overload-candidate path and finds no call to
    /// read a parameter from.
    /// </remarks>
    [Fact]
    public Task LeavesAMismatchedAssignmentAlone() =>
        Verify.CompilerCodeFixAsync<AsTaskCodeFix>(
            """
            internal Task<string> Mismatched(ValueTask<int> pending) =>
                {|#0:pending|};
            """,
            """
            internal Task<string> Mismatched(ValueTask<int> pending) =>
                {|#0:pending|};
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0));
}
