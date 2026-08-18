namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class DeprecationAnalyzerTests
{
    [Fact]
    public Task FlagsFlatMapOnAnOption() =>
        Verify.AnalyzerAsync<DeprecationAnalyzer>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.{|#0:FlatMap|}(value => Option.Some(value * 2));
            """,
            Verify.Diagnostic(Rules.RenamedToAndThen)
               .WithLocation(0)
               .WithArguments("FlatMap", "AndThen"));

    [Fact]
    public Task FlagsFlatMapAsyncOnAnOption() =>
        Verify.AnalyzerAsync<DeprecationAnalyzer>(
            """
            internal ValueTask<Option<int>> Doubled(Option<int> option) =>
                option.{|#0:FlatMapAsync|}(
                    value => Task.FromResult(Option.Some(value * 2)));
            """,
            Verify.Diagnostic(Rules.RenamedToAndThen)
               .WithLocation(0)
               .WithArguments("FlatMapAsync", "AndThenAsync"));

    [Fact]
    public Task FlagsFlatMapAsyncOnAnOptionTask() =>
        Verify.AnalyzerAsync<DeprecationAnalyzer>(
            """
            internal Task<Option<int>> Doubled(Task<Option<int>> option) =>
                option.{|#0:FlatMapAsync|}(value => Option.Some(value * 2));
            """,
            Verify.Diagnostic(Rules.RenamedToAndThen)
               .WithLocation(0)
               .WithArguments("FlatMapAsync", "AndThenAsync"));

    [Fact]
    public Task FlagsFlatMapAsyncOnAnOptionValueTask() =>
        Verify.AnalyzerAsync<DeprecationAnalyzer>(
            """
            internal Task<Option<int>> Doubled(ValueTask<Option<int>> option) =>
                option.{|#0:FlatMapAsync|}(value => Option.Some(value * 2));
            """,
            Verify.Diagnostic(Rules.RenamedToAndThen)
               .WithLocation(0)
               .WithArguments("FlatMapAsync", "AndThenAsync"));

    [Fact]
    public Task IgnoresAndThen() =>
        Verify.NoDiagnosticAsync<DeprecationAnalyzer>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.AndThen(value => Option.Some(value * 2));
            """);

    [Fact]
    public Task IgnoresAFlatMapThatIsNotOnAMonad() =>
        Verify.NoDiagnosticAsync<DeprecationAnalyzer>(
            """
            internal class Box
            {
                internal Box FlatMap(Func<Box, Box> map) => map(this);

                internal Box Doubled(Box box) => box.FlatMap(value => value);
            }
            """);
}
