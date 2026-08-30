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
    public Task IgnoresAnOptionOfBool() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal Option<bool> Flag() => Option.None<bool>();
            """);

    [Fact]
    public Task IgnoresAnOptionOfAnEnumWithAZeroMember() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal enum Colour { Red = 0, Green = 1 }

            internal class Subject
            {
                internal Option<Colour> Chosen() => Option.None<Colour>();
            }
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

    [Fact]
    public Task FlagsANullableOptionInATupleReturnType() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal ({|#0:Option<int>?|} a, int b) Make() =>
                (Option.None<int>(), 1);
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableOptionInATupleParameter() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal int Take(({|#0:Option<int>?|} a, int b) pair) => pair.b;
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableOptionInATupleLocal() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal int Take()
            {
                ({|#0:Option<int>?|} a, int b) pair = (Option.None<int>(), 1);

                return pair.b;
            }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Theory]
    [InlineData("({|#0:Option<int>?|} a, int b, string c)")]
    [InlineData("(int a, {|#0:Option<int>?|} b, string c)")]
    [InlineData("(int a, string b, {|#0:Option<int>?|} c)")]
    public Task FlagsANullableOptionInAnyTupleElement(string tuple) =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            $"internal void Take({tuple} pair) {{ }}",
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsEveryNullableMonadInATuple() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take(
                ({|#0:Option<int>?|} a, int b, {|#1:Option<string>?|} c) pair)
            {
            }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"),
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(1)
               .WithArguments("Option<string>", "None"));

    [Fact]
    public Task FlagsANullableOptionInANestedTuple() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take((int a, ({|#0:Option<int>?|} b, int c) d) pair)
            {
            }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableOptionInAnArrayElement() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal {|#0:Option<int>?|}[] Absent() => [];
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableOptionArrayInATupleElement() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take(({|#0:Option<int>?|}[] a, int b) pair) { }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableResultInATupleReturnType() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal ({|#0:Result<int, string>?|} a, int b) Make() =>
                (Result.Ok<int, string>(1), 2);
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Result<int, string>", "Err"));

    [Fact]
    public Task FlagsANullableResultInATupleParameter() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal int Take(({|#0:Result<int, string>?|} a, int b) pair) =>
                pair.b;
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Result<int, string>", "Err"));

    [Theory]
    [InlineData("({|#0:Result<int, string>?|} a, int b, string c)")]
    [InlineData("(int a, {|#0:Result<int, string>?|} b, string c)")]
    [InlineData("(int a, string b, {|#0:Result<int, string>?|} c)")]
    public Task FlagsANullableResultInAnyTupleElement(string tuple) =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            $"internal void Take({tuple} pair) {{ }}",
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Result<int, string>", "Err"));

    [Fact]
    public Task FlagsANullableResultArrayInATupleElement() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take(({|#0:Result<int, string>?|}[] a, int b) pair)
            {
            }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Result<int, string>", "Err"));

    [Fact]
    public Task FlagsANullableDerivedCaseInATupleTwice() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take(({|#0:{|#1:Some<int>|}?|} a, int b) pair) { }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Some<int>", "None"),
            Verify.Diagnostic(Rules.DerivedMonadTypeDeclared)
               .WithLocation(1)
               .WithArguments("Some<int>", "Option<int>"));

    [Fact]
    public Task FlagsANullableOkCaseInATupleTwice() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take(
                ({|#0:{|#1:Ok<int, string>|}?|} a, int b) pair)
            {
            }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Ok<int, string>", "Err"),
            Verify.Diagnostic(Rules.DerivedMonadTypeDeclared)
               .WithLocation(1)
               .WithArguments("Ok<int, string>", "Result<int, string>"));

    [Fact]
    public Task FlagsANullableResultInATuple() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take(({|#0:Result<int, string>?|} a, int b) pair)
            {
            }
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Result<int, string>", "Err"));

    [Fact]
    public Task FlagsANullableOptionInANestedTypeArgument() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal Task<System.Collections.Generic.List<{|#0:Option<int>?|}>>
                AbsentAsync() =>
                Task.FromResult(
                    new System.Collections.Generic.List<Option<int>?>());
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task FlagsANullableOptionInADelegateTypeArgument() =>
        Verify.AnalyzerAsync<DeclaredTypeAnalyzer>(
            """
            internal System.Func<{|#0:Option<int>?|}> Absent() => () => null;
            """,
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task IgnoresATupleWithNoMonad() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take((int a, string b) pair) { }
            """);

    [Fact]
    public Task IgnoresAnUnannotatedOptionInATuple() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take((Option<int> a, int b) pair) { }
            """);

    [Fact]
    public Task IgnoresAnUnannotatedResultInATuple() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal void Take((Result<int, string> a, int b) pair) { }
            """);

    [Fact]
    public Task IgnoresANullableArrayOfOptions() =>
        Verify.NoDiagnosticAsync<DeclaredTypeAnalyzer>(
            """
            internal Option<int>[]? Absent() => null;
            """);
}
