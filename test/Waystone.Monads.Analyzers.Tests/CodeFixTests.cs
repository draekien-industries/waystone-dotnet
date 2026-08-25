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

    [Fact]
    public Task StripsTheNullableAnnotationFromATupleElement() =>
        Verify.CodeFixAsync<DeclaredTypeAnalyzer,
            RemoveNullableAnnotationCodeFix>(
            "internal int Take(({|#0:Option<int>?|} a, int b) pair) => pair.b;",
            "internal int Take((Option<int> a, int b) pair) => pair.b;",
            Verify.Diagnostic(Rules.NullableMonadDeclared)
               .WithLocation(0)
               .WithArguments("Option<int>", "None"));

    [Fact]
    public Task WrapsAnEagerOrArgumentInALambda() =>
        Verify.CodeFixAsync<LazyVariantAnalyzer, UseLazyVariantCodeFix>(
            """
            internal Option<int> Fallback() => Option.Some(0);

            internal Option<int> Pick(Option<int> option) =>
                option.{|#0:Or|}(Fallback());
            """,
            """
            internal Option<int> Fallback() => Option.Some(0);

            internal Option<int> Pick(Option<int> option) =>
                option.OrElse(() => Fallback());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("Or", "Option<int>", "OrElse",
                    "and computing it may be expensive"));

    /// <remarks>
    /// <c>AndThen</c> is the only replacement whose delegate takes the
    /// receiver's value, so the fix has to discard it rather than emit the
    /// parameterless lambda the other four use.
    /// </remarks>
    [Fact]
    public Task DiscardsTheValueWhenWrappingAnEagerAndArgument() =>
        Verify.CodeFixAsync<LazyVariantAnalyzer, UseLazyVariantCodeFix>(
            """
            internal Option<string> Next() => Option.Some("x");

            internal Option<string> Pick(Option<int> option) =>
                option.{|#0:And|}(Next());
            """,
            """
            internal Option<string> Next() => Option.Some("x");

            internal Option<string> Pick(Option<int> option) =>
                option.AndThen(_ => Next());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("And", "Option<int>", "AndThen",
                    "and computing it may be expensive"));

    [Fact]
    public Task WrapsAnEagerUnwrapOrArgumentInALambda() =>
        Verify.CodeFixAsync<LazyVariantAnalyzer, UseLazyVariantCodeFix>(
            """
            internal int Expensive() => 42;

            internal int Read(Option<int> option) =>
                option.{|#0:UnwrapOr|}(Expensive());
            """,
            """
            internal int Expensive() => 42;

            internal int Read(Option<int> option) =>
                option.UnwrapOrElse(() => Expensive());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task KeepsTheMapDelegateWhenWrappingMapOr() =>
        Verify.CodeFixAsync<LazyVariantAnalyzer, UseLazyVariantCodeFix>(
            """
            internal int Expensive() => 42;

            internal int Read(Option<int> option) =>
                option.{|#0:MapOr|}(Expensive(), value => value + 1);
            """,
            """
            internal int Expensive() => 42;

            internal int Read(Option<int> option) =>
                option.MapOrElse(() => Expensive(), value => value + 1);
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("MapOr", "Option<int>", "MapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task WrapsAnEagerOkOrArgumentInALambda() =>
        Verify.CodeFixAsync<LazyVariantAnalyzer, UseLazyVariantCodeFix>(
            """
            internal string Reason() => "missing";

            internal Result<int, string> Convert(Option<int> option) =>
                option.{|#0:OkOr|}(Reason());
            """,
            """
            internal string Reason() => "missing";

            internal Result<int, string> Convert(Option<int> option) =>
                option.OkOrElse(() => Reason());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("OkOr", "Option<int>", "OkOrElse",
                    "and computing it may be expensive"));
}
