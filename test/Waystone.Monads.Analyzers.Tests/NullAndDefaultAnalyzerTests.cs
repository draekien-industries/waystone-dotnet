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

    [Theory]
    [InlineData("option == null")]
    [InlineData("option != null")]
    [InlineData("null == option")]
    [InlineData("option is null")]
    [InlineData("option is not null")]
    [InlineData("option == default")]
    public Task IgnoresANullTest(string test) =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            $"internal bool Test(Option<int> option) => {test};");

    [Fact]
    public Task FlagsNullInANullableTupleElement() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            "internal (Option<int>? a, int b) Make() => ({|#0:null|}, 1);",
            Verify.Diagnostic(Rules.NullAssignedToMonad)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task FlagsTheDefaultInANullableTupleElement() =>
        Verify.AnalyzerAsync<NullAndDefaultAnalyzer>(
            "internal (Option<int>? a, int b) Make() => ({|#0:default|}, 1);",
            Verify.Diagnostic(Rules.DefaultOfMonad)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task IgnoresTheDefaultOfAnUnconstrainedTypeParameter() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            "internal T Get<T>() => default!;");

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
    public Task IgnoresADefaultValueConvertedToAnOption() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            "internal Option<int> Make() => 0;");

    [Fact]
    public Task IgnoresTheDefaultOfAReferenceTypeConvertedToAnOption() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            "internal Option<string> Make() => default(string)!;");

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
