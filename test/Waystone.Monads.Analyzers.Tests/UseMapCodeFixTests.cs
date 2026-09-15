namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class UseMapCodeFixTests
{
    [Fact]
    public Task UnwrapsTheLiftAndRenamesTheMember() =>
        Verify.CodeFixAsync<LiftedAndThenAnalyzer, UseMapCodeFix>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.{|#0:AndThen|}(value => Option.Some(Net(value)));

            private static int Net(int value) => value - 1;
            """,
            """
            internal Option<int> Project(Option<int> option) =>
                option.Map(value => Net(value));

            private static int Net(int value) => value - 1;
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task UnwrapsAResultLift() =>
        Verify.CodeFixAsync<LiftedAndThenAnalyzer, UseMapCodeFix>(
            """
            internal Result<int, string> Project(Result<int, string> result) =>
                result.{|#0:AndThen|}(
                    value => Result.Ok<int, string>(value + 1));
            """,
            """
            internal Result<int, string> Project(Result<int, string> result) =>
                result.Map(
                    value => value + 1);
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    /// <summary>
    /// A block body becomes an expression body, since the return was the whole of
    /// it.
    /// </summary>
    [Fact]
    public Task RewritesABlockBodiedDelegateToAnExpression() =>
        Verify.CodeFixAsync<LiftedAndThenAnalyzer, UseMapCodeFix>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.{|#0:AndThen|}(
                    value =>
                    {
                        return Option.Some(value + 1);
                    });
            """,
            """
            internal Option<int> Project(Option<int> option) =>
                option.Map(
                    value => value + 1);
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task KeepsTheStateArgument() =>
        Verify.CodeFixAsync<LiftedAndThenAnalyzer, UseMapCodeFix>(
            """
            internal Option<int> Project(Option<int> option, int offset) =>
                option.{|#0:AndThen|}(
                    offset,
                    static (value, state) => Option.Some(value + state));
            """,
            """
            internal Option<int> Project(Option<int> option, int offset) =>
                option.Map(
                    offset,
                    static (value, state) => value + state);
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    /// <summary>
    /// Explicit type arguments survive the rename, because Map and AndThen declare
    /// the same type parameters in the same order.
    /// </summary>
    [Fact]
    public Task KeepsExplicitTypeArguments() =>
        Verify.CodeFixAsync<LiftedAndThenAnalyzer, UseMapCodeFix>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.{|#0:AndThen<int>|}(value => Option.Some(value + 1));
            """,
            """
            internal Option<int> Project(Option<int> option) =>
                option.Map<int>(value => value + 1);
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task RewritesABinderMember() =>
        Verify.CodeFixAsync<LiftedAndThenAnalyzer, UseMapCodeFix>(
            """
            internal Option<int> Project(Option<int> option, int offset) =>
                option.With(offset)
                   .{|#0:AndThen|}(
                        static (value, state) => Option.Some(value + state));
            """,
            """
            internal Option<int> Project(Option<int> option, int offset) =>
                option.With(offset)
                   .Map(
                        static (value, state) => value + state);
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));
}
