namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class NullAndDefaultAnalyzerTests
{
    [Fact]
    public Task FlagsNullAssignedToAnOption() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            "internal Option<int> Make() => {|#0:null|};",
            Verify.Diagnostic(Rules.NullAssignedToMonad)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task FlagsNullSuppressedWithABang() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            "internal Result<int, string> Make() => {|#0:null!|};",
            Verify.Diagnostic(Rules.NullAssignedToMonad)
               .WithLocation(0)
               .WithArguments("Result<int, string>"));

    [Fact]
    public Task IgnoresNullAssignedToAnExplicitlyNullableOption() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            "internal Option<int>? Make() => null;");

    [Fact]
    public Task FlagsNullPassedToAnOptionParameter() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            """
            internal void Take(Option<int> option) { }
            internal void Call() => Take({|#0:null!|});
            """,
            Verify.Diagnostic(Rules.NullAssignedToMonad)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task FlagsNullPassedToAnOptionParameterFromANullableReturningMember() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            """
            internal string? Take(Option<int> option) => null;
            internal string? Call()
            {
                return Take({|#0:null!|});
            }
            """,
            Verify.Diagnostic(Rules.NullAssignedToMonad)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task IgnoresNullPassedToAnExplicitlyNullableOptionParameter() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            """
            internal void Take(Option<int>? option) { }
            internal void Call() => Take(null);
            """);

    [Fact]
    public Task FlagsTheDefaultOfAnOption() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            "internal Option<int> Make() => {|#0:default(Option<int>)|};",
            Verify.Diagnostic(Rules.DefaultOfMonad)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task FlagsTheBareDefaultLiteral() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            "internal Result<int, string> Make() => {|#0:default|};",
            Verify.Diagnostic(Rules.DefaultOfMonad)
               .WithLocation(0)
               .WithArguments("Result<int, string>"));

    [Fact]
    public Task FlagsADefaultValueConvertedToAnOption() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            "internal Option<int> Make() => {|#0:0|};",
            Verify.Diagnostic(Rules.DefaultValueConvertsToNone)
               .WithLocation(0)
               .WithArguments("int", "0"));

    [Fact]
    public Task IgnoresANonDefaultValueConvertedToAnOption() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            "internal Option<int> Make() => 1;");

    [Fact]
    public Task IgnoresADefaultValueConvertedToAResult() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            "internal Result<int, string> Make() => 0;");

    [Fact]
    public Task IgnoresNullForAnUnrelatedType() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            """
            internal string? Text() => null;
            internal int Number() => default;
            """);
}
