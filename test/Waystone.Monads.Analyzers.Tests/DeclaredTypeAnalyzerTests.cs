namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class DeclaredTypeAnalyzerTests
{
    [Fact]
    public Task FlagsANullableOptionReturnType() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal {|#0:Option<int>?|} Absent() => null;
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableResultParameter() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal bool Check({|#0:Result<int, string>?|} result) =>
                result is null;
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Result<int, string>", "Err"));

    [Fact]
    public Task FlagsANullableOptionProperty() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal {|#0:Option<string>?|} Value { get; set; }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<string>", "None"));

    [Fact]
    public Task FlagsANullableOptionLocal() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal bool Check()
            {
                {|#0:Option<int>?|} absent = null;

                return absent is null;
            }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableOptionInATypeArgument() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal Task<{|#0:Option<int>?|}> AbsentAsync() =>
                Task.FromResult<Option<int>?>(null);
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableDerivedCaseTwice() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal bool Check({|#0:{|#1:Some<int>|}?|} some) => some is null;
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Some<int>", "None"),
            Verify.Diagnostic(Rules.DerivedMonadTypeDeclared)
               .WithLocation(1)
               .WithArguments("Some<int>", "Option<int>"));

    [Fact]
    public Task FlagsANullableOptionInAnUnannotatedContext() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            #nullable disable
            internal {|#0:Option<int>?|} Absent() => null;
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task IgnoresAnUnannotatedOption() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal Option<int> Absent() => Option.None<int>();
            """);

    [Fact]
    public Task IgnoresANullableValueThatIsNotAMonad() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal int? Absent() => null;
            """);

    [Fact]
    public Task FlagsAnOptionOfBool() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal {|#0:Option<bool>|} Flag() => Option.None<bool>();
            """,
            Verify.Diagnostic(Rules.OptionOfZeroValuedType)
               .WithLocation(0)
               .WithArguments(
                    "Option<bool>",
                    "bool",
                    "Use a three-state enum whose zero member carries the state you meant None to express"));

    [Fact]
    public Task FlagsAnOptionOfAnEnumWithAZeroMember() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal enum Colour { Red = 0, Green = 1 }

            internal class Subject
            {
                internal {|#0:Option<Colour>|} Chosen() => Option.None<Colour>();
            }
            """,
            Verify.Diagnostic(Rules.OptionOfZeroValuedType)
               .WithLocation(0)
               .WithArguments(
                    "Option<Colour>",
                    "Colour",
                    "Renumber it so no meaningful member is 0, leaving zero to the state None expresses"));

    [Fact]
    public Task FlagsAnOptionOfAnEnumWhoseZeroMemberIsImplicit() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal enum Colour { Red, Green }

            internal class Subject
            {
                internal {|#0:Option<Colour>|} Chosen() => Option.None<Colour>();
            }
            """,
            Verify.Diagnostic(Rules.OptionOfZeroValuedType)
               .WithLocation(0)
               .WithArguments(
                    "Option<Colour>",
                    "Colour",
                    "Renumber it so no meaningful member is 0, leaving zero to the state None expresses"));

    [Fact]
    public Task IgnoresAnOptionOfAnEnumWithoutAZeroMember() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal enum Colour { Red = 1, Green = 2 }

            internal class Subject
            {
                internal Option<Colour> Chosen() => Option.None<Colour>();
            }
            """);

    [Fact]
    public Task IgnoresAnOptionOfAnEnumWithAnUnsignedUnderlyingType() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal enum Colour : byte { Red = 1, Green = 2 }

            internal class Subject
            {
                internal Option<Colour> Chosen() => Option.None<Colour>();
            }
            """);

    [Fact]
    public Task IgnoresAnOptionOfAnIntEvenThoughZeroIsMeaningful() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal Option<int> Count() => Option.None<int>();
            """);

    [Fact]
    public Task IgnoresAResultWhoseOkIsBool() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal Result<bool, string> Flag() =>
                Result.Ok<bool, string>(true);
            """);

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

    [Fact]
    public Task FlagsACaseDeclaredByALocalFunction() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal void Run()
            {
                {|#0:Some<int>|} Local() => (Some<int>)Option.Some(1);

                Local();
            }
            """,
            Verify.Diagnostic(Rules.DerivedMonadTypeDeclared)
               .WithLocation(0)
               .WithArguments("Some<int>", "Option<int>"));

    [Fact]
    public Task FlagsACaseDeclaredAsATypeArgument() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal System.Collections.Generic.List<{|#0:Some<int>|}> Cases() =>
                new System.Collections.Generic.List<Some<int>>();
            """,
            Verify.Diagnostic(Rules.DerivedMonadTypeDeclared)
               .WithLocation(0)
               .WithArguments("Some<int>", "Option<int>"));
}
