namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class OptionCreationAnalyzerTests
{
    [Theory]
    [InlineData("0", "int")]
    [InlineData("false", "bool")]
    [InlineData("0m", "decimal")]
    [InlineData("default(int)", "int")]
    [InlineData("Guid.Empty", "Guid")]
    [InlineData("DateTime.MinValue", "DateTime")]
    [InlineData("TimeSpan.Zero", "TimeSpan")]
    public Task IgnoresTheDefaultOfAValueType(string value, string type) =>
        Verify.NoDiagnosticAsync<OptionCreationAnalyzer>(
            $"internal Option<{type}> Make() => Option.Some({value});");

    [Theory]
    [InlineData("default(string)", "string")]
    [InlineData("default(object)", "object")]
    public Task FlagsTheDefaultOfAReferenceType(string value, string type) =>
        Verify.AnalyzerAsync<OptionCreationAnalyzer>(
            $"internal Option<{type}> Make() => Option.Some({{|#0:{value}|}}!);",
            Verify.Diagnostic(Rules.SomeFromDefaultValue)
               .WithLocation(0)
               .WithArguments(type));

    [Theory]
    [InlineData("1", "int")]
    [InlineData("true", "bool")]
    [InlineData("\"\"", "string")]
    [InlineData("\"value\"", "string")]
    public Task IgnoresAValueThatIsNotTheDefault(string value, string type) =>
        Verify.NoDiagnosticAsync<OptionCreationAnalyzer>(
            $"internal Option<{type}> Make() => Option.Some({value});");

    [Fact]
    public Task FlagsAPossiblyNullValue() =>
        Verify.AnalyzerAsync<OptionCreationAnalyzer>(
            """
            internal Option<string> Make(string? value) =>
                Option.Some({|#0:value|});
            """,
            Verify.Diagnostic(Rules.PossiblyNullPassedToSome).WithLocation(0));

    [Fact]
    public Task IgnoresAValueTheCompilerKnowsIsNotNull() =>
        Verify.NoDiagnosticAsync<OptionCreationAnalyzer>(
            """
            internal Option<string> Make(string? value) =>
                value is null ? Option.None<string>() : Option.Some(value);
            """);

    [Fact]
    public Task IgnoresNoneAndTheNullableFactory() =>
        Verify.NoDiagnosticAsync<OptionCreationAnalyzer>(
            """
            internal Option<int> Absent() => Option.None<int>();
            internal Option<string> FromNullable(string? value) =>
                Option.FromNullable(value);
            """);

    [Fact]
    public Task IgnoresAnUnrelatedSomeMethod() =>
        Verify.NoDiagnosticAsync<OptionCreationAnalyzer>(
            """
            internal static class Maybe
            {
                internal static int Some<T>(T value) => 0;
            }

            internal class Subject
            {
                internal int Make() => Maybe.Some(0);
            }
            """);
}
