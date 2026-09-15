namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class LiftedAndThenAnalyzerTests
{
    [Fact]
    public Task FlagsAnAndThenThatLiftsAProjection() =>
        Verify.AnalyzerAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.{|#0:AndThen|}(value => Option.Some(Net(value)));

            private static int Net(int value) => value - 1;
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task FlagsAnAndThenThatLiftsAnInlineProjection() =>
        Verify.AnalyzerAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.{|#0:AndThen|}(value => Option.Some(value + 1));
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task FlagsAResultAndThenThatLiftsWithOk() =>
        Verify.AnalyzerAsync<LiftedAndThenAnalyzer>(
            """
            internal Result<int, string> Project(Result<int, string> result) =>
                result.{|#0:AndThen|}(
                    value => Result.Ok<int, string>(value + 1));
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task FlagsABlockBodiedDelegate() =>
        Verify.AnalyzerAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.{|#0:AndThen|}(
                    value =>
                    {
                        return Option.Some(value + 1);
                    });
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task FlagsTheStateOverload() =>
        Verify.AnalyzerAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option, int offset) =>
                option.{|#0:AndThen|}(
                    offset,
                    static (value, state) => Option.Some(value + state));
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task FlagsABinderMember() =>
        Verify.AnalyzerAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option, int offset) =>
                option.With(offset)
                   .{|#0:AndThen|}(
                        static (value, state) => Option.Some(value + state));
            """,
            Verify.Diagnostic(Rules.LiftedAndThen)
               .WithLocation(0)
               .WithArguments("AndThen", "Map"));

    [Fact]
    public Task DoesNotFlagABranchingDelegate() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.AndThen(
                    value => value > 0
                        ? Option.Some(value)
                        : Option.None<int>());
            """);

    [Fact]
    public Task DoesNotFlagAMethodGroup() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.AndThen(Lookup);

            private static Option<int> Lookup(int value) => Option.Some(value);
            """);

    /// <summary>
    /// The factory produces the absent case for some inputs, so the delegate has a
    /// second case and Map is not its rewrite.
    /// </summary>
    [Fact]
    public Task DoesNotFlagFromNullable() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<string> Project(Option<int> option) =>
                option.AndThen(value => Option.FromNullable(Describe(value)));

            private static string? Describe(int value) =>
                value > 0 ? value.ToString() : null;
            """);

    [Fact]
    public Task DoesNotFlagTry() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<string> option) =>
                option.AndThen(value => Option.Try(() => int.Parse(value)));
            """);

    [Fact]
    public Task DoesNotFlagAConstantNone() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.AndThen(value => Option.None<int>());
            """);

    [Fact]
    public Task DoesNotFlagADelegateCallingIntoAnotherOption() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option, Option<int> next) =>
                option.AndThen(value => next.Filter(inner => inner > value));
            """);

    [Fact]
    public Task DoesNotFlagADelegateThatDoesMoreThanReturn() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.AndThen(
                    value =>
                    {
                        int doubled = value * 2;

                        return Option.Some(doubled);
                    });
            """);

    [Fact]
    public Task DoesNotFlagMap() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.Map(value => value + 1);
            """);

    /// <summary>
    /// A two-argument factory is not the lift this rule reads, whatever it is
    /// named.
    /// </summary>
    [Fact]
    public Task DoesNotFlagASameNamedFactoryOfSomebodyElses() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal class Subject
            {
                internal Option<int> Project(Option<int> option) =>
                    option.AndThen(value => Lifts.Some(value, 1));
            }

            internal static class Lifts
            {
                public static Option<int> Some(int value, int offset) =>
                    Option.Some(value + offset);
            }
            """);

    /// <summary>
    /// WM2005 points at AndThen from the other direction. Neither rule sees the
    /// other's shape.
    /// </summary>
    [Fact]
    public Task DoesNotFlagTheShapeWM2005Reports() =>
        Verify.NoDiagnosticAsync<LiftedAndThenAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.Map(value => Option.Some(value + 1)).Flatten();
            """);

    [Fact]
    public Task WM2005DoesNotFlagTheShapeThisRuleReports() =>
        Verify.NoDiagnosticAsync<SimplificationAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.AndThen(value => Option.Some(value + 1));
            """);
}
