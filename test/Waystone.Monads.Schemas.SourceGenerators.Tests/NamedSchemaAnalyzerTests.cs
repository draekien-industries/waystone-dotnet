namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

/// <summary>
/// <c>WMSC0009</c>, which reports a <c>Schema.For&lt;T&gt;()</c> that has a named
/// spelling. The rule is the reason an analyzer ships beside the generator at all:
/// every subject here sits in a static field rather than in a <c>Configure</c>
/// body, which is where the documentation puts a schema and where a generator would
/// never look.
/// </summary>
public sealed class NamedSchemaAnalyzerTests
{
    [Theory]
    [InlineData("string", "Schema.Text")]
    [InlineData("bool", "Schema.Bool")]
    [InlineData("System.Guid", "Schema.Uuid")]
    [InlineData("System.DateTimeOffset", "Schema.Timestamp")]
    [InlineData("System.DateOnly", "Schema.Date")]
    [InlineData("int", "Schema.Number.Int32")]
    [InlineData("long", "Schema.Number.Int64")]
    [InlineData("decimal", "Schema.Number.Decimal")]
    [InlineData("double", "Schema.Number.Double")]
    public void ATypeWithANamedSpellingIsReported(string type, string named)
    {
        Diagnostic diagnostic = Analyze(
                $"public static class Rules {{ public static readonly Schema<{type}, {type}> Value = Schema.For<{type}>(); }}")
           .ShouldHaveSingleItem();

        diagnostic.Id.ShouldBe("WMSC0009");

        diagnostic.GetMessage().ShouldContain($"prefer '{named}'");
    }

    [Fact]
    public void TheRuleSuggestsRatherThanWarns() =>
        Analyze(
                "public static class Rules { public static readonly Schema<string, string> Value = Schema.For<string>(); }")
           .ShouldHaveSingleItem()
           .Severity.ShouldBe(DiagnosticSeverity.Info);

    /// <summary>
    /// <c>For&lt;T&gt;()</c> over a type with no named spelling is the documented
    /// starting point for a rule of one's own, so reporting it would fire on the
    /// design working.
    /// </summary>
    [Fact]
    public void ATypeWithNoNamedSpellingIsLeftAlone() =>
        Analyze(
                "public sealed class Quest { } public static class Rules { public static readonly Schema<Quest, Quest> Value = Schema.For<Quest>(); }")
           .ShouldBeEmpty();

    /// <summary>
    /// An enumeration has <c>Schema.Enum&lt;T&gt;()</c>, which checks membership
    /// rather than aliasing <c>For</c>. The two are not interchangeable, so this rule
    /// has no business naming one.
    /// </summary>
    [Fact]
    public void AnEnumerationIsLeftAlone() =>
        Analyze(
                "public enum Rank { Novice } public static class Rules { public static readonly Schema<Rank, Rank> Value = Schema.For<Rank>(); }")
           .ShouldBeEmpty();

    [Fact]
    public void ANamedSpellingIsLeftAlone() =>
        Analyze(
                "public static class Rules { public static readonly Schema<string, string> Value = Schema.Text; }")
           .ShouldBeEmpty();

    /// <summary>
    /// The rule reads the receiver's type rather than its name, so somebody else's
    /// static <c>For</c> is not this one.
    /// </summary>
    [Fact]
    public void AForOnAnotherTypeIsLeftAlone() =>
        Analyze(
                "public static class Mine { public static string For<T>() => null!; } public static class Rules { public static readonly string Value = Mine.For<string>(); }")
           .ShouldBeEmpty();

    /// <summary>
    /// A chained call is still the call, and the report has to land on the part a
    /// reader replaces rather than on the whole chain.
    /// </summary>
    [Fact]
    public void AChainedCallIsReportedAtTheForCall()
    {
        Diagnostic diagnostic = Analyze(
                "public static class Rules { public static readonly Schema<string, string> Value = Schema.For<string>().NotEmpty(); }")
           .ShouldHaveSingleItem();

        diagnostic.Location.SourceTree!.ToString()
                  .Substring(
                       diagnostic.Location.SourceSpan.Start,
                       diagnostic.Location.SourceSpan.Length)
                  .ShouldBe("Schema.For<string>()");
    }

    private static ImmutableArray<Diagnostic> Analyze(string source) =>
        Verify.Analyze(new NamedSchemaAnalyzer(), source)
              .Where(diagnostic => diagnostic.Id == "WMSC0009")
              .ToImmutableArray();
}
