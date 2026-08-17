namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class GuardAnalyzerTests
{
    [Fact]
    public Task FlagsAnIsSomeGuardAroundAnUnwrap() =>
        Verify.AnalyzerAsync<GuardAnalyzer>(
            """
            internal int Value(Option<int> option)
            {
                if ({|#0:option.IsSome|})
                {
                    return option.Unwrap();
                }

                return 0;
            }
            """,
            Verify.Diagnostic(Rules.GuardedUnwrap)
               .WithLocation(0)
               .WithArguments("IsSome"));

    [Fact]
    public Task FlagsAnIsNoneGuardWithTheUnwrapInTheElse() =>
        Verify.AnalyzerAsync<GuardAnalyzer>(
            """
            internal int Value(Option<int> option)
            {
                if ({|#0:option.IsNone|})
                {
                    return 0;
                }
                else
                {
                    return option.Unwrap();
                }
            }
            """,
            Verify.Diagnostic(Rules.GuardedUnwrap)
               .WithLocation(0)
               .WithArguments("IsNone"));

    [Fact]
    public Task FlagsAGuardedExpectOnAResult() =>
        Verify.AnalyzerAsync<GuardAnalyzer>(
            """
            internal int Value(Result<int, string> result)
            {
                if ({|#0:result.IsOk|})
                {
                    return result.Expect("checked");
                }

                return 0;
            }
            """,
            Verify.Diagnostic(Rules.GuardedUnwrap)
               .WithLocation(0)
               .WithArguments("IsOk"));

    [Fact]
    public Task IgnoresAGuardOnADifferentInstance() =>
        Verify.NoDiagnosticAsync<GuardAnalyzer>(
            """
            internal int Value(Option<int> first, Option<int> second)
            {
                if (first.IsSome)
                {
                    return second.UnwrapOr(0);
                }

                return 0;
            }
            """);

    [Fact]
    public Task IgnoresAGuardWithNoUnwrapInIt() =>
        Verify.NoDiagnosticAsync<GuardAnalyzer>(
            """
            internal int Value(Option<int> option)
            {
                if (option.IsSome)
                {
                    return option.UnwrapOr(0);
                }

                return 0;
            }
            """);

    [Fact]
    public Task FlagsAnIsSomeCheckCombinedWithAnUnwrap() =>
        Verify.AnalyzerAsync<GuardAnalyzer>(
            """
            internal bool Big(Option<int> option) =>
                {|#0:option.IsSome && option.Unwrap() > 2|};
            """,
            Verify.Diagnostic(Rules.CheckCombinedWithUnwrap)
               .WithLocation(0)
               .WithArguments("IsSome", "IsSomeAnd"));

    [Fact]
    public Task FlagsAnIsNoneCheckCombinedWithAnUnwrap() =>
        Verify.AnalyzerAsync<GuardAnalyzer>(
            """
            internal bool BigOrMissing(Option<int> option) =>
                {|#0:option.IsNone || option.Unwrap() > 2|};
            """,
            Verify.Diagnostic(Rules.CheckCombinedWithUnwrap)
               .WithLocation(0)
               .WithArguments("IsNone", "IsNoneOr"));

    [Fact]
    public Task IgnoresAnIsSomeCheckCombinedWithSomethingElse() =>
        Verify.NoDiagnosticAsync<GuardAnalyzer>(
            """
            internal bool Big(Option<int> option, int other) =>
                option.IsSome && other > 2;
            """);

    [Fact]
    public Task FlagsAStateCheckLaterInAConjunction() =>
        Verify.AnalyzerAsync<GuardAnalyzer>(
            """
            internal bool Big(Option<int> option, bool enabled) =>
                {|#0:enabled && option.IsSome && option.Unwrap() > 2|};
            """,
            Verify.Diagnostic(Rules.CheckCombinedWithUnwrap)
               .WithLocation(0)
               .WithArguments("IsSome", "IsSomeAnd"));
}
