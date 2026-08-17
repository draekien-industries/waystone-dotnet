namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class DiscardedMonadAnalyzerTests
{
    [Fact]
    public Task FlagsADiscardedResult() =>
        Verify.AnalyzerAsync<DiscardedMonadAnalyzer>(
            """
            internal Result<int, string> Save() => Result.Ok<int, string>(1);

            internal void Run()
            {
                {|#0:Save|}();
            }
            """,
            Verify.Diagnostic(Rules.ResultDiscarded)
               .WithLocation(0)
               .WithArguments("Result<int, string>"));

    [Fact]
    public Task FlagsADiscardedOptionAtItsOwnSeverity() =>
        Verify.AnalyzerAsync<DiscardedMonadAnalyzer>(
            """
            internal Option<int> Find() => Option.None<int>();

            internal void Run()
            {
                {|#0:Find|}();
            }
            """,
            Verify.Diagnostic(Rules.OptionDiscarded)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task FlagsADiscardedAwaitedResult() =>
        Verify.AnalyzerAsync<DiscardedMonadAnalyzer>(
            """
            internal Task<Result<int, string>> SaveAsync() =>
                Task.FromResult(Result.Ok<int, string>(1));

            internal async Task RunAsync()
            {
                await {|#0:SaveAsync|}();
            }
            """,
            Verify.Diagnostic(Rules.ResultDiscarded)
               .WithLocation(0)
               .WithArguments("Result<int, string>"));

    [Fact]
    public Task IgnoresAnExplicitDiscard() =>
        Verify.NoDiagnosticAsync<DiscardedMonadAnalyzer>(
            """
            internal Result<int, string> Save() => Result.Ok<int, string>(1);

            internal void Run()
            {
                _ = Save();
            }
            """);

    [Fact]
    public Task IgnoresAResultThatIsUsed() =>
        Verify.NoDiagnosticAsync<DiscardedMonadAnalyzer>(
            """
            internal Result<int, string> Save() => Result.Ok<int, string>(1);

            internal bool Run() => Save().IsOk;
            """);

    [Fact]
    public Task IgnoresADiscardedCallThatReturnsSomethingElse() =>
        Verify.NoDiagnosticAsync<DiscardedMonadAnalyzer>(
            """
            internal int Count() => 1;

            internal void Run()
            {
                Count();
            }
            """);
}
