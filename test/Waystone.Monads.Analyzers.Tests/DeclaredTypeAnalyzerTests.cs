namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class DeclaredTypeAnalyzerTests
{
    [Fact]
    public Task FlagsANestedOptionReturnType() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal {|#0:Option<Option<int>>|} Nested() =>
                Option.None<Option<int>>();
            """,
            Verify.Diagnostic(Rules.NestedOption)
               .WithLocation(0)
               .WithArguments("Option<Option<int>>"));

    [Fact]
    public Task FlagsAResultWithIdenticalTypeArguments() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal {|#0:Result<string, string>|} Ambiguous() =>
                Result.Ok<string, string>("value");
            """,
            Verify.Diagnostic(Rules.ResultWithIdenticalTypeArguments)
               .WithLocation(0)
               .WithArguments("Result<string, string>"));

    [Fact]
    public Task FlagsADerivedCaseDeclaredAsAParameter() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal bool Check({|#0:Some<int>|} some) => some.IsSome;
            """,
            Verify.Diagnostic(Rules.DerivedMonadTypeDeclared)
               .WithLocation(0)
               .WithArguments("Some<int>", "Option<int>"));

    [Fact]
    public Task FlagsADerivedCaseDeclaredAsALocal() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal bool Check(Option<int> option)
            {
                {|#0:None<int>|} absent = new None<int>();

                return absent.IsNone;
            }
            """,
            Verify.Diagnostic(Rules.DerivedMonadTypeDeclared)
               .WithLocation(0)
               .WithArguments("None<int>", "Option<int>"));

    [Fact]
    public Task IgnoresADerivedCaseInAPatternTest() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal bool Check(Option<int> option) => option is Some<int>;
            """);

    [Fact]
    public Task IgnoresAnOrdinaryOptionAndResult() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal Option<int> Find() => Option.None<int>();
            internal Result<int, string> Save() => Result.Ok<int, string>(1);
            """);
}
