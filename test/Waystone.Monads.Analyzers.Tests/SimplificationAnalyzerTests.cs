namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class SimplificationAnalyzerTests
{
    [Fact]
    public Task FlagsMapFollowedByFlatten() =>
        Verify.AnalyzerAsync<SimplificationAnalyzer>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.Map(value => Option.Some(value * 2)).{|#0:Flatten|}();
            """,
            Verify.Diagnostic(Rules.MapThenFlatten).WithLocation(0));

    [Fact]
    public Task IgnoresFlattenOnItsOwn() =>
        Verify.NoDiagnosticAsync<SimplificationAnalyzer>(
            """
            internal Option<int> Inner(Option<Option<int>> option) =>
                option.Flatten();
            """);

    [Fact]
    public Task IgnoresMapOnItsOwn() =>
        Verify.NoDiagnosticAsync<SimplificationAnalyzer>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.Map(value => value * 2);
            """);

    [Theory]
    [InlineData("0")]
    [InlineData("default")]
    [InlineData("default(int)")]
    public Task FlagsUnwrapOrGivenADefault(string fallback) =>
        Verify.AnalyzerAsync<SimplificationAnalyzer>(
            $"internal int Value(Option<int> option) => option.{{|#0:UnwrapOr|}}({fallback});",
            Verify.Diagnostic(Rules.UnwrapOrWithDefault)
               .WithLocation(0)
               .WithArguments("int"));

    [Fact]
    public Task IgnoresUnwrapOrGivenARealFallback() =>
        Verify.NoDiagnosticAsync<SimplificationAnalyzer>(
            "internal int Value(Option<int> option) => option.UnwrapOr(1);");

    [Fact]
    public Task FlagsAnOptionComparedToNull() =>
        Verify.AnalyzerAsync<SimplificationAnalyzer>(
            "internal bool Missing(Option<int> option) => {|#0:option == null|};",
            Verify.Diagnostic(Rules.MonadComparedToNull)
               .WithLocation(0)
               .WithArguments("Option<int>", "IsNone"));

    [Fact]
    public Task FlagsAResultComparedToNullTheOtherWayRound() =>
        Verify.AnalyzerAsync<SimplificationAnalyzer>(
            """
            internal bool Present(Result<int, string> result) =>
                {|#0:null != result|};
            """,
            Verify.Diagnostic(Rules.MonadComparedToNull)
               .WithLocation(0)
               .WithArguments("Result<int, string>", "IsOk"));

    [Fact]
    public Task FlagsTheIsNullPattern() =>
        Verify.AnalyzerAsync<SimplificationAnalyzer>(
            "internal bool Missing(Option<int> option) => {|#0:option is null|};",
            Verify.Diagnostic(Rules.MonadComparedToNull)
               .WithLocation(0)
               .WithArguments("Option<int>", "IsNone"));

    [Fact]
    public Task FlagsTheIsNotNullPattern() =>
        Verify.AnalyzerAsync<SimplificationAnalyzer>(
            """
            internal bool Present(Option<int> option) =>
                {|#0:option is not null|};
            """,
            Verify.Diagnostic(Rules.MonadComparedToNull)
               .WithLocation(0)
               .WithArguments("Option<int>", "IsSome"));

    [Fact]
    public Task IgnoresANullComparisonOnAnUnrelatedType() =>
        Verify.NoDiagnosticAsync<SimplificationAnalyzer>(
            "internal bool Missing(string? text) => text == null;");
}
