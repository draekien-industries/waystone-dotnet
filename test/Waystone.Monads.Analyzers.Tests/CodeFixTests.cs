namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class CodeFixTests
{
    [Fact]
    public Task ReplacesSomeOfANullWithNone() =>
        Verify.CodeFixAsync<OptionCreationAnalyzer, UseNoneCodeFix>(
            "internal Option<string> Make() => Option.Some({|#0:default(string)|}!);",
            "internal Option<string> Make() => Option.None<string>();",
            Verify.Diagnostic(Rules.SomeFromDefaultValue)
               .WithLocation(0)
               .WithArguments("string"));

    [Fact]
    public Task ReplacesNullWithNone() =>
        Verify.CodeFixAsync<NullAndDefaultAnalyzer, UseNoneCodeFix>(
            "internal Option<string> Make() => {|#0:null|};",
            "internal Option<string> Make() => Option.None<string>();",
            Verify.Diagnostic(Rules.NullAssignedToMonad)
               .WithLocation(0)
               .WithArguments("Option<string>"));

    [Fact]
    public Task ReplacesTheDefaultOfAnOptionWithNone() =>
        Verify.CodeFixAsync<NullAndDefaultAnalyzer, UseNoneCodeFix>(
            "internal Option<int> Make() => {|#0:default(Option<int>)|};",
            "internal Option<int> Make() => Option.None<int>();",
            Verify.Diagnostic(Rules.DefaultOfMonad)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task QualifiesNoneWhenTheNamespaceIsNotImported() =>
        Verify.RawCodeFixAsync<NullAndDefaultAnalyzer, UseNoneCodeFix>(
            """
            internal class Subject
            {
                internal Waystone.Monads.Options.Option<int> Make() =>
                    {|#0:null|};
            }
            """,
            """
            internal class Subject
            {
                internal Waystone.Monads.Options.Option<int> Make() =>
                    Waystone.Monads.Options.Option.None<int>();
            }
            """,
            Verify.Diagnostic(Rules.NullAssignedToMonad)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task ReplacesSomeWithFromNullable() =>
        Verify.CodeFixAsync<OptionCreationAnalyzer, UseFromNullableCodeFix>(
            """
            internal Option<string> Make(string? value) =>
                Option.Some({|#0:value|});
            """,
            """
            internal Option<string> Make(string? value) =>
                Option.FromNullable(value);
            """,
            Verify.Diagnostic(Rules.PossiblyNullPassedToSome).WithLocation(0));

    [Fact]
    public Task ReplacesUnwrapWithUnwrapOrDefault() =>
        Verify.CodeFixAsync<PanickingCallAnalyzer, UseUnwrapOrDefaultCodeFix>(
            "internal int Value(Option<int> option) => option.{|#0:Unwrap|}();",
            "internal int Value(Option<int> option) => option.UnwrapOrDefault();",
            Verify.Diagnostic(Rules.UnwrapUsed)
               .WithLocation(0)
               .WithArguments("Unwrap"));

    [Fact]
    public Task ReplacesExpectWithUnwrapOrDefaultAndDropsTheMessage() =>
        Verify.CodeFixAsync<PanickingCallAnalyzer, UseUnwrapOrDefaultCodeFix>(
            """
            internal int Value(Option<int> option) =>
                option.{|#0:Expect|}("missing");
            """,
            """
            internal int Value(Option<int> option) =>
                option.UnwrapOrDefault();
            """,
            Verify.Diagnostic(Rules.ExpectUsed)
               .WithLocation(0)
               .WithArguments("Expect"));

    [Fact]
    public Task ReplacesUnwrapOrOfADefaultWithUnwrapOrDefault() =>
        Verify.CodeFixAsync<SimplificationAnalyzer, UseUnwrapOrDefaultCodeFix>(
            "internal int Value(Option<int> option) => option.{|#0:UnwrapOr|}(0);",
            "internal int Value(Option<int> option) => option.{|#1:UnwrapOrDefault|}();",
            [
                Verify.Diagnostic(Rules.UnwrapOrWithDefault)
                   .WithLocation(0)
                   .WithArguments("int"),
            ],
            [
                Verify.Diagnostic(Rules.OrDefaultOnAValueType)
                   .WithLocation(1)
                   .WithArguments(
                        "UnwrapOrDefault",
                        "int",
                        "UnwrapOrNull",
                        "0"),
            ]);

    [Fact]
    public Task ReplacesMapThenFlattenWithAndThen() =>
        Verify.CodeFixAsync<SimplificationAnalyzer, UseAndThenCodeFix>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.Map(value => Option.Some(value * 2)).{|#0:Flatten|}();
            """,
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.AndThen(value => Option.Some(value * 2));
            """,
            Verify.Diagnostic(Rules.MapThenFlatten).WithLocation(0));

    [Fact]
    public Task ReplacesANullComparisonWithIsNone() =>
        Verify.CodeFixAsync<SimplificationAnalyzer, UseStateCheckCodeFix>(
            "internal bool Missing(Option<int> option) => {|#0:option == null|};",
            "internal bool Missing(Option<int> option) => option.IsNone;",
            Verify.Diagnostic(Rules.MonadComparedToNull)
               .WithLocation(0)
               .WithArguments("Option<int>", "IsNone"));

    [Fact]
    public Task ReplacesAnIsNotNullPatternWithIsOk() =>
        Verify.CodeFixAsync<SimplificationAnalyzer, UseStateCheckCodeFix>(
            """
            internal bool Fine(Result<int, string> result) =>
                {|#0:result is not null|};
            """,
            """
            internal bool Fine(Result<int, string> result) =>
                result.IsOk;
            """,
            Verify.Diagnostic(Rules.MonadComparedToNull)
               .WithLocation(0)
               .WithArguments("Result<int, string>", "IsOk"));

    [Fact]
    public Task WidensADerivedCaseToItsBase() =>
        Verify.CodeFixAsync<DeclaredTypeAnalyzer, UseBaseMonadTypeCodeFix>(
            "internal bool Check({|#0:Some<int>|} some) => some.IsSome;",
            "internal bool Check(Option<int> some) => some.IsSome;",
            Verify.Diagnostic(Rules.DerivedMonadTypeDeclared)
               .WithLocation(0)
               .WithArguments("Some<int>", "Option<int>"));

    [Fact]
    public Task StripsTheNullableAnnotationFromAnOption() =>
        Verify.CodeFixAsync<DeclaredTypeAnalyzer,
            RemoveNullableAnnotationCodeFix>(
            "internal bool Check({|#0:Option<int>?|} option) => option is null;",
            "internal bool Check(Option<int> option) => option is null;",
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));
}
