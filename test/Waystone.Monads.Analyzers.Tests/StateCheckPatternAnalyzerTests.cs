namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class StateCheckPatternAnalyzerTests
{
    [Fact]
    public Task FlagsIsSomeInAnIsExpression() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal bool Read(Option<int> option) =>
                option is { {|#0:IsSome: true|} };
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsSome", "Option<int>", "IsSomeAnd"));

    [Fact]
    public Task FlagsIsNone() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal bool Read(Option<int> option) =>
                option is { {|#0:IsNone: true|} };
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsNone", "Option<int>", "IsNoneOr"));

    [Fact]
    public Task FlagsIsOkOnAResult() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal bool Read(Result<int, string> result) =>
                result is { {|#0:IsOk: true|} };
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsOk", "Result<int, string>", "IsOkAnd"));

    [Fact]
    public Task FlagsIsErrOnAResult() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal bool Read(Result<int, string> result) =>
                result is { {|#0:IsErr: true|} };
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsErr", "Result<int, string>", "IsErrAnd"));

    /// <remarks>
    /// The value the pattern tests for is irrelevant — <c>false</c> asks the
    /// same question as <c>true</c> and hides it the same way.
    /// </remarks>
    [Fact]
    public Task FlagsAFalseTest() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal bool Read(Option<int> option) =>
                option is { {|#0:IsSome: false|} };
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsSome", "Option<int>", "IsSomeAnd"));

    [Fact]
    public Task FlagsASwitchArm() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal int Read(Option<int> option) => option switch
            {
                { {|#0:IsSome: true|} } => 1,
                _ => 0,
            };
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsSome", "Option<int>", "IsSomeAnd"));

    [Fact]
    public Task FlagsAGuardedUnwrapWrittenAsAPattern() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal int Read(Option<int> option)
            {
                if (option is { {|#0:IsSome: true|} })
                {
                    return option.Unwrap();
                }

                return 0;
            }
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsSome", "Option<int>", "IsSomeAnd"));

    [Fact]
    public Task FlagsANegatedPattern() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal bool Read(Option<int> option) =>
                option is not { {|#0:IsSome: true|} };
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsSome", "Option<int>", "IsSomeAnd"));

    /// <remarks>
    /// A subpattern nested inside another type's pattern reaches the same node,
    /// which is why the rule is keyed on the subpattern rather than on the
    /// enclosing <c>is</c>.
    /// </remarks>
    [Fact]
    public Task FlagsANestedSubpattern() =>
        Verify.AnalyzerAsync<StateCheckPatternAnalyzer>(
            """
            internal class Basket
            {
                public Option<int> Discount { get; set; } = Option.None<int>();
            }

            internal class Subject
            {
                internal bool Read(Basket basket) =>
                    basket is { Discount: { {|#0:IsSome: true|} } };
            }
            """,
            Verify.Diagnostic(Rules.StateCheckedThroughPattern)
               .WithLocation(0)
               .WithArguments("IsSome", "Option<int>", "IsSomeAnd"));

    [Fact]
    public Task IgnoresAPropertyRead() =>
        Verify.NoDiagnosticAsync<StateCheckPatternAnalyzer>(
            """
            internal bool Read(Option<int> option) => option.IsSome;
            """);

    [Fact]
    public Task IgnoresACombinator() =>
        Verify.NoDiagnosticAsync<StateCheckPatternAnalyzer>(
            """
            internal bool Read(Option<int> option) =>
                option.IsSomeAnd(value => value > 2);
            """);

    [Fact]
    public Task IgnoresAnIdenticallyNamedPropertyOnAnotherType() =>
        Verify.NoDiagnosticAsync<StateCheckPatternAnalyzer>(
            """
            internal class Slot
            {
                public bool IsSome { get; set; }
            }

            internal class Subject
            {
                internal bool Read(Slot slot) => slot is { IsSome: true };
            }
            """);
}
