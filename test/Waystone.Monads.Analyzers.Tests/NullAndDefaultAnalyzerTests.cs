namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
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

    /// <summary>
    /// Replaces the four non-triggers that pinned WM1003 staying quiet on a value
    /// converted to a monad.
    /// </summary>
    /// <remarks>
    /// Those four asserted an analyzer decision — <c>AnalyzeDefault</c> reads
    /// <c>info.Type</c> before <c>info.ConvertedType</c>, so a conversion reported
    /// the source type and failed the monad test. DRA-119 removed the conversions,
    /// so the position no longer compiles and there is no decision left to pin. What
    /// is worth pinning instead is that the compiler, not a rule, is what now
    /// rejects it: a consumer sees <c>CS0029</c> and reaches for the migration fix.
    /// </remarks>
    [Fact]
    public Task AValueNoLongerConvertsToAMonad() =>
        Verify.CompilerDiagnosticsAsync(
            """
            using Waystone.Monads.Options;
            using Waystone.Monads.Results;

            internal class Subject
            {
                internal Option<int> MakeOption() => {|#0:0|};

                internal Result<int, string> MakeResult() => {|#1:0|};
            }
            """,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0),
            DiagnosticResult.CompilerError("CS0029").WithLocation(1));

    [Fact]
    public Task IgnoresNullForAnUnrelatedType() =>
        Verify.NoDiagnosticAsync<NullAndDefaultAnalyzer>(
            """
            internal string? Text() => null;
            internal int Number() => default;
            """);
}
